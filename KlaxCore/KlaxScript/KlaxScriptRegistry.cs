using KlaxCore.GameFramework;
using KlaxCore.KlaxScript;
using KlaxShared.Attributes;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using KlaxCore.GameFramework.Assets;
using KlaxCore.KlaxScript.Interfaces;
using KlaxCore.KlaxScript.Nodes;
using KlaxIO.AssetManager.Assets;

namespace KlaxCore.KlaxScript
{
	enum EKlaxFunctionLocation
	{
		Type,
		Library
	}

	class CKlaxScriptRegistry
	{
		private static CKlaxScriptRegistry s_instance;
		public static CKlaxScriptRegistry Instance => s_instance ?? new CKlaxScriptRegistry();

		public readonly static Color4 DEFAULT_TYPE_COLOR = new Color4(0, 0.58f, 1.0f, 1.0f);
		public const int MINIMUM_SUGGESTION_CAPACITY = 256;

		private CKlaxScriptRegistry()
		{
			AddDefaultTypes();
			FindTypes();
			RegisterCustomNodeFactories();

			m_codeGenerator.CompileFunctions(Types, LibraryFunctions);
			m_codeGenerator.LoadCompiledFunctions(Types, LibraryFunctions);

			s_instance = this;
		}

		private void AddDefaultTypes()
		{
			RegisterType(new CKlaxScriptTypeInfo() { Type = typeof(bool), Color = new Color4(0xFF0000FF), Name = "Boolean" });
			RegisterType(new CKlaxScriptTypeInfo() { Type = typeof(int), Color = new Color4(0.52f, 0.807f, 0.98f, 1.0f), Name = "Integer" });
			RegisterType(new CKlaxScriptTypeInfo() { Type = typeof(float), Color = new Color4(0.34f, 1.0f, 0.552f, 1.0f), Name = "Float" });
			RegisterType(new CKlaxScriptTypeInfo() { Type = typeof(string), Color = new Color4(1.0f, 0.0f, 1.0f, 1.0f), Name = "String" });
			RegisterType(new CKlaxScriptTypeInfo() { Type = typeof(Vector3), Color = new Color4(1.0f, 0.68f, 0.0f, 1.0f), Name = "Vector3" });
			RegisterType(new CKlaxScriptTypeInfo() { Type = typeof(Vector2), Color = new Color4(1.0f, 0.24f, 0.0f, 1.0f), Name = "Vector2" });
			RegisterType(new CKlaxScriptTypeInfo() { Type = typeof(Quaternion), Color = new Color4(0.95f, 0.72f, 1.0f, 1.0f), Name = "Quaternion" });

			RegisterType(new CKlaxScriptTypeInfo() { Type = typeof(CAssetReference<CMeshAsset>), Color = CMeshAsset.TYPE_COLOR, Name = "MeshAsset" });
			RegisterType(new CKlaxScriptTypeInfo() { Type = typeof(CAssetReference<CModelAsset>), Color = CModelAsset.TYPE_COLOR, Name = "ModelAsset" });
			RegisterType(new CKlaxScriptTypeInfo() { Type = typeof(CAssetReference<CMaterialAsset>), Color = CMaterialAsset.TYPE_COLOR, Name = "MaterialAsset" });
			RegisterType(new CKlaxScriptTypeInfo() { Type = typeof(CAssetReference<CTextureAsset>), Color = CTextureAsset.TYPE_COLOR, Name = "TextureAsset" });
			RegisterType(new CKlaxScriptTypeInfo() { Type = typeof(CAssetReference<CLevelAsset>), Color = CLevelAsset.TYPE_COLOR, Name = "LevelAsset" });
			RegisterType(new CKlaxScriptTypeInfo() { Type = typeof(CAssetReference<CShaderAsset>), Color = CShaderAsset.TYPE_COLOR, Name = "ShaderAsset" });
			RegisterType(new CKlaxScriptTypeInfo() { Type = typeof(CAssetReference<CEntityAsset<CEntity>>), Color = CEntityAsset<CEntity>.TYPE_COLOR, Name = "EntityAsset" });
		}

		private void RegisterCustomNodeFactories()
		{
			m_customNodeFactories.Clear();

			CKlaxScriptNodeFactory castNodeFactory = new CKlaxScriptNodeFactory("Cast", "Basic", "", null, false, () => new CExplicitCastNode());
			m_customNodeFactories.Add(castNodeFactory);

			CKlaxScriptNodeFactory implicitCastNodeFactory = new CKlaxScriptNodeFactory("Implicit Cast", "Basic", "", null, false, () => new CImplicitCastNode());
			m_customNodeFactories.Add(implicitCastNodeFactory);

			CKlaxScriptNodeFactory branchNode = new CKlaxScriptNodeFactory("If", "Control Flow", "", null, false, () => new CBranchNode());
			m_customNodeFactories.Add(branchNode);

			CKlaxScriptNodeFactory selfReferenceNodeFactory = new CKlaxScriptNodeFactory("Self", "Basic", "", null, false, () => new CSelfReferenceNode());
			m_customNodeFactories.Add(selfReferenceNodeFactory);

			CKlaxScriptNodeFactory forLoopNodeFactory = new CKlaxScriptNodeFactory("For Loop", "Control Flow", "", null, false, () => new CForLoopNode());
			m_customNodeFactories.Add(forLoopNodeFactory);

			CKlaxScriptNodeFactory forEachLoopNodeFactory = new CKlaxScriptNodeFactory("Foreach Loop", "Control Flow", "", null, false, () => new CForEachLoopNode());
			m_customNodeFactories.Add(forEachLoopNodeFactory);

			CKlaxScriptNodeFactory switchNodeFactory = new CKlaxScriptNodeFactory("Switch", "Control Flow", "", null, false, () => new CSwitchNode());
			m_customNodeFactories.Add(switchNodeFactory);
		}

		private void FindTypes()
		{
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				RegisterAssembly(assembly);
			}

			AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoaded;
		}

		private void OnAssemblyLoaded(object sender, AssemblyLoadEventArgs args)
		{
			RegisterAssembly(args.LoadedAssembly);
		}

		private void RegisterAssembly(Assembly assembly)
		{
			KlaxAssembly klaxAssembly = assembly.GetCustomAttribute<KlaxAssembly>();
			if (klaxAssembly == null)
				return;
			
			foreach (TypeInfo type in assembly.DefinedTypes)
			{
				KlaxScriptTypeAttribute klaxType = type.GetCustomAttribute<KlaxScriptTypeAttribute>();
				if (klaxType != null)
				{
					RegisterType(new CKlaxScriptTypeInfo() { Type = type, Color = klaxType.Color.A == 0 ? DEFAULT_TYPE_COLOR : klaxType.Color, Name = klaxType.Name ?? type.Name });
				}
				else
				{
					KlaxLibraryAttribute klaxLibrary = type.GetCustomAttribute<KlaxLibraryAttribute>();
					if (klaxLibrary != null)
					{
						foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
						{
							KlaxFunctionAttribute klaxFunc = method.GetCustomAttribute<KlaxFunctionAttribute>();
							if (klaxFunc != null)
							{
								LibraryFunctions.Add(CreateFunction(klaxFunc, method));
							}
						}
					}
				}
			}
		}

		private void RegisterType(CKlaxScriptTypeInfo typeInfo)
		{
			Type type = typeInfo.Type;

			Types.Add(typeInfo);
			m_klaxTypeMap.Add(typeInfo.Type, typeInfo);

			foreach (var method in type.GetMethods())
			{
				KlaxFunctionAttribute klaxFunction = method.GetCustomAttribute<KlaxFunctionAttribute>();
				if (klaxFunction != null)
				{
					MethodInfo baseMethod = method.GetBaseDefinition();
					if (baseMethod.DeclaringType == type)
					{
						typeInfo.Functions.Add(CreateFunction(klaxFunction, method));
					}
				}
			}

			foreach (var property in type.GetProperties())
			{
				KlaxPropertyAttribute klaxProperty = property.GetCustomAttribute<KlaxPropertyAttribute>();
				if (klaxProperty != null)
				{
					if (property.DeclaringType == type)
					{
						typeInfo.Properties.Add(CreateProperty(klaxProperty, property));
					}
				}
			}

			foreach (var field in type.GetFields())
			{
				KlaxPropertyAttribute klaxProperty = field.GetCustomAttribute<KlaxPropertyAttribute>();
				if (klaxProperty != null)
				{
					if (field.DeclaringType == type)
					{
						string name = klaxProperty.DisplayName ?? field.Name;
						typeInfo.Properties.Add(CreateProperty(field, name, klaxProperty.Category, klaxProperty.IsReadOnly));
					}
				}
				else
				{
					KlaxEventAttribute klaxEvent = field.GetCustomAttribute<KlaxEventAttribute>();
					if (klaxEvent != null && field.DeclaringType == type)
					{
						typeInfo.Events.Add(CreateEvent(field, klaxEvent));
					}
				}
			}
		}

		private CKlaxScriptPropertyInfo CreateProperty(KlaxPropertyAttribute attribute, PropertyInfo property)
		{
			CKlaxScriptPropertyInfo info = new CKlaxScriptPropertyInfo
			{
				propertyInfo = property,
				displayName = attribute.DisplayName ?? property.Name,
				category = attribute.Category,
				tooltip = attribute.Tooltip,
				declaringType = property.DeclaringType,
				propertyType = property.PropertyType,
				bIsReadOnly = attribute.IsReadOnly || !property.CanWrite,
			};

			return info;
		}

		private CKlaxScriptPropertyInfo CreateProperty(FieldInfo field, string name, string category, bool bIsReadOnly)
		{
			CKlaxScriptPropertyInfo info = new CKlaxScriptPropertyInfo
			{
				fieldInfo = field,
				displayName = name,
				category = category,
				declaringType = field.DeclaringType,
				propertyType = field.FieldType,
				bIsReadOnly = bIsReadOnly || field.IsInitOnly || field.IsLiteral,
			};

			return info;
		}

		private CKlaxScriptFunctionInfo CreateFunction(KlaxFunctionAttribute attribute, MethodInfo method)
		{
			ParameterInfo[] parameters = method.GetParameters();
			CKlaxScriptFunctionInfo info = new CKlaxScriptFunctionInfo
			{
				methodInfo = method,
				displayName = attribute.DisplayName ?? method.Name,
				category = attribute.Category,
				tooltip = attribute.Tooltip,
				inputParameter = parameters.Where(whereInfo => !whereInfo.IsOut).Select(selectInfo => selectInfo.ParameterType).ToArray(),
				inputParametersDefaultValue = method.GetParameters().Where(whereInfo => !whereInfo.IsOut).Select(selectInfo => (selectInfo.HasDefaultValue ? selectInfo.DefaultValue : null)).ToArray(),
				bIsImplicit = attribute.IsImplicit,
			};

			info.inputParameterNames = new string[info.inputParameter.Length];
			for (int i = 0, length = info.inputParameter.Length; i < length; i++)
			{
				string result = parameters[i].Name;
				switch (i)
				{
					case 0:
						if (attribute.ParameterName1 != null)
						{
							result = attribute.ParameterName1;
						}
						break;
					case 1:
						if (attribute.ParameterName2 != null)
						{
							result = attribute.ParameterName2;
						}
						break;
					case 2:
						if (attribute.ParameterName3 != null)
						{
							result = attribute.ParameterName3;
						}
						break;
					case 3:
						if (attribute.ParameterName4 != null)
						{
							result = attribute.ParameterName4;
						}
						break;
					case 4:
						if (attribute.ParameterName5 != null)
						{
							result = attribute.ParameterName5;
						}
						break;
					case 5:
						if (attribute.ParameterName6 != null)
						{
							result = attribute.ParameterName6;
						}
						break;
					case 6:
						if (attribute.ParameterName7 != null)
						{
							result = attribute.ParameterName7;
						}
						break;
					case 7:
						if (attribute.ParameterName8 != null)
						{
							result = attribute.ParameterName8;
						}
						break;
					case 8:
						if (attribute.ParameterName9 != null)
						{
							result = attribute.ParameterName9;
						}
						break;
					case 9:
						if (attribute.ParameterName10 != null)
						{
							result = attribute.ParameterName10;
						}
						break;
				}

				info.inputParameterNames[i] = result;
			}

			m_klaxFunctionMap.Add(method, info);

			return info;
		}
		
		private CKlaxScriptEventInfo CreateEvent(FieldInfo eventField, KlaxEventAttribute eventAttribute)
		{
			CKlaxScriptEventInfo info = new CKlaxScriptEventInfo()
			{
				klaxEventInfo = eventField,
				displayName = eventAttribute.DisplayName ?? eventField.Name,
				category = eventAttribute.Category,
				tooltip = eventAttribute.Tooltip,
				ParameterName1 = eventAttribute.ParameterName1,
				ParameterName2 = eventAttribute.ParameterName2,
				ParameterName3 = eventAttribute.ParameterName3,
				ParameterName4 = eventAttribute.ParameterName4,
				ParameterName5 = eventAttribute.ParameterName5,
				ParameterName6 = eventAttribute.ParameterName6,
				ParameterName7 = eventAttribute.ParameterName7,
				ParameterName8 = eventAttribute.ParameterName8,
				ParameterName9 = eventAttribute.ParameterName9,
				ParameterName10 = eventAttribute.ParameterName10,
			};

			return info;
		}

		public void GetNodeSuggestions(CKlaxScriptNodeQueryContext context, List<CKlaxScriptNodeFactory> suggestions)
		{
			if (suggestions.Capacity < MINIMUM_SUGGESTION_CAPACITY)
			{
				suggestions.Capacity = MINIMUM_SUGGESTION_CAPACITY;
			}

			suggestions.Clear();
			
			for (int i = 0, count = Types.Count; i < count; i++)
			{
				CKlaxScriptTypeInfo type = Types[i];
				bool bIncludeAll = type.Type == context.InputPinType;

				for (int k = 0, funcCount = type.Functions.Count; k < funcCount; k++)
				{
					CKlaxScriptFunctionInfo func = type.Functions[k];
					Type declaringType = func.methodInfo.IsStatic ? null : func.methodInfo.DeclaringType;
					if (bIncludeAll || context.InputPinType == null || func.inputParameter.Any((t) => t.IsAssignableFrom(context.InputPinType)))
					{
						CKlaxScriptNodeFactory factory = new CKlaxScriptNodeFactory(func.displayName, func.category, func.tooltip, declaringType, false, () =>
						{
							return new CExecuteFunctionNode()
							{
								TargetMethod = func.methodInfo,
								IsImplicit = func.bIsImplicit,
							};
						});

						suggestions.Add(factory);
					}
				}

				int propertyCount = type.Properties.Count;
				for (int j = 0; j < propertyCount; j++)
				{
					CKlaxScriptPropertyInfo prop = type.Properties[j];
					if (bIncludeAll || context.InputPinType == null || prop.propertyType.IsAssignableFrom(context.InputPinType) || prop.declaringType.IsAssignableFrom(context.InputPinType))
					{
						CKlaxScriptNodeFactory getFactory = new CKlaxScriptNodeFactory("Get " + prop.displayName, prop.category, prop.tooltip, prop.declaringType, true, () =>
						{
							return prop.fieldInfo != null ? new CGetMemberNode(prop.fieldInfo) : new CGetMemberNode(prop.propertyInfo);
						});

						suggestions.Add(getFactory);

						if (!prop.bIsReadOnly)
						{
							CKlaxScriptNodeFactory setFactory = new CKlaxScriptNodeFactory("Set " + prop.displayName, prop.category, prop.tooltip, prop.declaringType, true, () =>
							{
								return prop.fieldInfo != null ? new CSetMemberNode(prop.fieldInfo) : new CSetMemberNode(prop.propertyInfo);
							});
							suggestions.Add(setFactory);
						}

					}
				}
			}

			for (int i = 0, funcCount = LibraryFunctions.Count; i < funcCount; i++)
			{
				CKlaxScriptFunctionInfo func = LibraryFunctions[i];
				Type declaringType = func.methodInfo.IsStatic ? null : func.methodInfo.DeclaringType;
				if (context.InputPinType == null || func.inputParameter.Any((t) => t.IsAssignableFrom(context.InputPinType)))
				{
					CKlaxScriptNodeFactory factory = new CKlaxScriptNodeFactory(func.displayName, func.category, func.tooltip, declaringType, false, () =>
					{
						return new CExecuteFunctionNode()
						{
							TargetMethod = func.methodInfo,
							IsImplicit = func.bIsImplicit,
						};
					});

					suggestions.Add(factory);
				}
			}

			if (context.QueryObject != null)
			{
				foreach (CKlaxScriptInterfaceReference interfaceReference in context.QueryObject.IncludedInterfaces)
				{
					CKlaxScriptInterface klaxInterface = interfaceReference.GetInterface();
					foreach (var interfaceFunction in klaxInterface.Functions)
					{
						CKlaxScriptNodeFactory factory = new CKlaxScriptNodeFactory(interfaceFunction.Name, "Interfaces", "", null, false, () =>
						{
							return new CExecuteInterfaceFunctionNode(interfaceFunction);
						});
						suggestions.Add(factory);
					}
				}

				foreach (CCustomFunctionGraph functionGraph in context.QueryObject.FunctionGraphs)
				{
					CKlaxScriptNodeFactory factory = new CKlaxScriptNodeFactory(functionGraph.Name, "Script Functions", "", null, false, () =>
					{
						CExecuteCustomFunctionNode functionNode = new CExecuteCustomFunctionNode(functionGraph);
						functionNode.ResolveFunctionReference(context.QueryObject);
						return functionNode;
					});
					suggestions.Add(factory);
				}
			}

			suggestions.AddRange(m_customNodeFactories);
		}

		public bool TryGetTypeInfo(Type nativeType, out CKlaxScriptTypeInfo outTypeInfo)
		{
			if (nativeType == null)
			{
				outTypeInfo = null;
				return false;
			}

			return m_klaxTypeMap.TryGetValue(nativeType, out outTypeInfo);
		}

		public bool TryGetFunctionInfo(MethodInfo methodInfo, out CKlaxScriptFunctionInfo outFunctionInfo)
		{
			if (methodInfo == null)
			{
				outFunctionInfo = null;
				return false;
			}
			return m_klaxFunctionMap.TryGetValue(methodInfo, out outFunctionInfo);
		}

		public List<CKlaxScriptTypeInfo> Types { get; } = new List<CKlaxScriptTypeInfo>(128);
		public List<CKlaxScriptFunctionInfo> LibraryFunctions { get; } = new List<CKlaxScriptFunctionInfo>(128);
		private readonly Dictionary<Type, CKlaxScriptTypeInfo> m_klaxTypeMap = new Dictionary<Type, CKlaxScriptTypeInfo>(128);
		private readonly Dictionary<MethodInfo, CKlaxScriptFunctionInfo> m_klaxFunctionMap = new Dictionary<MethodInfo, CKlaxScriptFunctionInfo>(256);
		private readonly List<CKlaxScriptNodeFactory> m_customNodeFactories = new List<CKlaxScriptNodeFactory>();

		private CKlaxScriptCodeGenerator m_codeGenerator = new CKlaxScriptCodeGenerator();
	}
}
