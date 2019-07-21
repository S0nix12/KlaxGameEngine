using KlaxCore.GameFramework;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace KlaxCore.KlaxScript
{
	class CKlaxScriptCodeGenerator
	{
		const string ASSEMBLY_NAME = "KlaxScriptAssembly";
		const string ASSEMBLY_PATH = "RuntimeData.dll";
		const string MODULE_NAME = "KlaxScriptModule";
		const string MODULE_PATH = "KlaxScriptModule.dll";
		const string METHOD_NAME = "Execute";
		const string METHOD_LOCATION_NAME = "MethodLocation";
		const string TYPE_INDEX_NAME = "TypeIndex";
		const string FUNCTION_INDEX_NAME = "FunctionIndex";

		public void CompileFunctions(List<CKlaxScriptTypeInfo> types, List<CKlaxScriptFunctionInfo> libraryFunctions)
		{
			m_genericObjectListGetter = m_genericObjectListType.GetMethod("get_Item", BindingFlags.Instance | BindingFlags.Public);
			m_genericObjectListSetter = m_genericObjectListType.GetMethod("Add", BindingFlags.Public | BindingFlags.Instance);

			AssemblyName name = new AssemblyName(ASSEMBLY_NAME);
			AppDomain domain = AppDomain.CurrentDomain;
			m_assemblyBuilder = domain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Save);
			m_moduleBuilder = m_assemblyBuilder.DefineDynamicModule(MODULE_NAME, MODULE_PATH);

			//Generate a wrapper for all editor functions
			for (int i = 0, count = types.Count; i < count; i++)
			{
				for (int k = 0, funcCount = types[i].Functions.Count; k < funcCount; k++)
				{
					GenerateMethod(types[i].Functions[k], EKlaxFunctionLocation.Type, i, k);
				}
			}

			for (int i = 0, count = libraryFunctions.Count; i < count; i++)
			{
				GenerateMethod(libraryFunctions[i], EKlaxFunctionLocation.Library, -1, i);
			}

			m_assemblyBuilder.Save(ASSEMBLY_PATH);
		}

		public void UncompiledFunction(List<object> inputParams, List<object> outputParams)
		{
			Vector3 refVec = (Vector3)inputParams[1];
			((CEntity)inputParams[0]).SetWorldPosition(refVec);
		}

		public void LoadCompiledFunctions(List<CKlaxScriptTypeInfo> types, List<CKlaxScriptFunctionInfo> libraryFunctions)
		{
			DirectoryInfo info = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
			m_compiledAssemblyPath = info + "\\" + ASSEMBLY_PATH;

			if (File.Exists(m_compiledAssemblyPath))
			{
				Assembly newAssembly = Assembly.LoadFrom(m_compiledAssemblyPath);

				foreach (var type in newAssembly.DefinedTypes)
				{
					MethodInfo method = type.GetMethod(METHOD_NAME, BindingFlags.Static | BindingFlags.Public);
					if (method != null)
					{
						KlaxScriptFunctionSignature del = (KlaxScriptFunctionSignature)Delegate.CreateDelegate(typeof(KlaxScriptFunctionSignature), null, method);

						FieldInfo locationInfo = type.GetField(METHOD_LOCATION_NAME, BindingFlags.Public | BindingFlags.Static);
						EKlaxFunctionLocation location = (EKlaxFunctionLocation)locationInfo.GetValue(null);

						switch (location)
						{
							case EKlaxFunctionLocation.Type:
								{
									FieldInfo typeIndexInfo = type.GetField(TYPE_INDEX_NAME, BindingFlags.Public | BindingFlags.Static);
									int typeIndex = (int)typeIndexInfo.GetValue(null);

									FieldInfo functionIndexInfo = type.GetField(FUNCTION_INDEX_NAME, BindingFlags.Public | BindingFlags.Static);
									int functionIndex = (int)functionIndexInfo.GetValue(null);

									types[typeIndex].Functions[functionIndex].compiledMethod = del;
								}
								break;
							case EKlaxFunctionLocation.Library:
								{
									FieldInfo functionIndexInfo = type.GetField(FUNCTION_INDEX_NAME, BindingFlags.Public | BindingFlags.Static);
									int functionIndex = (int)functionIndexInfo.GetValue(null);

									libraryFunctions[functionIndex].compiledMethod = del;
								}
								break;
						}
					}
				}
			}
		}

		private int GetOutParametersBefore(int index, ParameterInfo[] parameters)
		{
			int counter = 0;
			for (int i = 0; i < index; i++)
			{
				if (parameters[i].IsOut)
					counter++;
			}

			return counter;
		}

		private void GenerateMethod(CKlaxScriptFunctionInfo function, EKlaxFunctionLocation location, int typeIndex, int functionIndex)
		{
			MethodInfo methodInfo = function.methodInfo;

			ParameterInfo[] parameters = methodInfo.GetParameters();
			ParameterInfo[] outParameters = parameters.Where(param => param.IsOut || (param.ParameterType.IsByRef && !param.IsIn)).ToArray();
			ParameterInfo[] inRefParameters = parameters.Where(param => param.ParameterType.IsByRef && param.IsIn).ToArray();
			ParameterInfo returnParameter = methodInfo.ReturnParameter;
			
			//Check if any invalid parameters are detected
			TypeBuilder typeBuilder = m_moduleBuilder.DefineType(methodInfo.DeclaringType + "_" + methodInfo.Name + "_Type", TypeAttributes.Public);

			//Build MethodInfo field to map generated type to method info
			FieldBuilder methodLocationFieldBuilder = typeBuilder.DefineField(METHOD_LOCATION_NAME, typeof(EKlaxFunctionLocation), FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.HasDefault | FieldAttributes.Literal);
			methodLocationFieldBuilder.SetConstant(location);

			FieldBuilder typeIndexFieldBuilder = typeBuilder.DefineField(TYPE_INDEX_NAME, typeof(int), FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.HasDefault | FieldAttributes.Literal);
			typeIndexFieldBuilder.SetConstant(typeIndex);

			FieldBuilder functionIndexFieldBuilder = typeBuilder.DefineField(FUNCTION_INDEX_NAME, typeof(int), FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.HasDefault | FieldAttributes.Literal);
			functionIndexFieldBuilder.SetConstant(functionIndex);

			//Build execution method
			MethodBuilder methodBuilder = typeBuilder.DefineMethod(METHOD_NAME, MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, CallingConventions.Standard, null, new Type[] { m_genericObjectListType, m_genericObjectListType });
			ILGenerator generator = methodBuilder.GetILGenerator();

			bool bHasReturnType = returnParameter.ParameterType != typeof(void);
			bool bIsStaticFunction = methodInfo.IsStatic;

			int returnParameterCount = 0;
			int outParameterCount = 0;
			int inRefParameterCount = 0;

			if (returnParameter.ParameterType != typeof(void))
			{
				generator.DeclareLocal(returnParameter.ParameterType);
				returnParameterCount++;
			}

			foreach (var outParam in outParameters)
			{
				generator.DeclareLocal(outParam.ParameterType.IsByRef ? outParam.ParameterType.GetElementType() : outParam.ParameterType);
				outParameterCount = outParameters.Length;
			}

			foreach (var inParam in inRefParameters)
			{
				generator.DeclareLocal(inParam.ParameterType.GetElementType());
				inRefParameterCount = inRefParameters.Length;
			}

			//Fill local variables for in ref parameters of desired function
			for (int i = 0, length = inRefParameters.Length; i < length; i++)
			{
				generator.Emit(OpCodes.Ldarg_0);
				generator.Emit(OpCodes.Ldc_I4_S, (sbyte)(Array.IndexOf(parameters, inRefParameters[i]) + (bIsStaticFunction ? 0 : 1)));
				generator.Emit(OpCodes.Callvirt, m_genericObjectListGetter);

				Type underlyingType = inRefParameters[i].ParameterType.GetElementType();
				if (underlyingType.IsValueType)
				{
					generator.Emit(OpCodes.Unbox_Any, underlyingType);
				}
				generator.Emit(OpCodes.Stloc, (short)(returnParameterCount + outParameterCount + i));
			}

			//If we call a member function, convert the first argument into the target type and put onto evaluation stack
			if (!bIsStaticFunction)
			{
				generator.Emit(OpCodes.Ldarg_0);
				generator.Emit(OpCodes.Ldc_I4_0);
				generator.Emit(OpCodes.Callvirt, m_genericObjectListGetter);
				generator.Emit(OpCodes.Castclass, methodInfo.DeclaringType);
			}

			for (int i = 0, length = parameters.Length; i < length; i++)
			{
				if (parameters[i].IsOut)
				{
					//Put out parameters onto evaluation stack
					generator.Emit(OpCodes.Ldloca_S, (byte)(Array.IndexOf(outParameters, parameters[i]) + (bHasReturnType ? 1 : 0)));
				}
				else if (parameters[i].IsIn && parameters[i].ParameterType.IsByRef)
				{
					generator.Emit(OpCodes.Ldloca_S, (byte)(Array.IndexOf(inRefParameters, parameters[i]) + returnParameterCount + outParameterCount));
				}
				else
				{
					PutObjectOntoEvaluationStack_IL(generator, i - GetOutParametersBefore(i, parameters) + (bIsStaticFunction ? 0 : 1), parameters[i].ParameterType);
				}
			}

			generator.Emit(OpCodes.Call, methodInfo);

			if (returnParameter.ParameterType != typeof(void))
			{
				//Store return value in output list
				generator.Emit(OpCodes.Stloc_0);
				generator.Emit(OpCodes.Ldarg_1);
				generator.Emit(OpCodes.Ldloc_0);
				AddObjectToObjectList_IL(generator, returnParameter.ParameterType);
			}

			if (outParameters.Length > 0)
			{
				for (int i = 0, length = outParameters.Length; i < length; i++)
				{
					generator.Emit(OpCodes.Ldarg_1);

					generator.Emit(OpCodes.Ldloc, (short)(i + (bHasReturnType ? 1 : 0)));
					AddObjectToObjectList_IL(generator, outParameters[i].ParameterType);
				}
			}

			generator.Emit(OpCodes.Ret);

			typeBuilder.CreateType();
		}

		private void PutObjectOntoEvaluationStack_IL(ILGenerator generator, int index, Type type)
		{
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ldc_I4, index);
			generator.Emit(OpCodes.Callvirt, m_genericObjectListGetter);
			MakeObjectTypesafe_IL(generator, type);
		}

		private void MakeObjectTypesafe_IL(ILGenerator generator, Type objectType)
		{
			if (objectType.IsValueType)
			{
				generator.Emit(OpCodes.Unbox_Any, objectType);
			}
			else
			{
				generator.Emit(OpCodes.Castclass, objectType);
			}
		}

		private void AddObjectToObjectList_IL(ILGenerator generator, Type objectType)
		{
			if (objectType.IsByRef)
			{
				Type referencedType = objectType.GetElementType();
				if (referencedType.IsValueType)
				{
					//If the referenced type is a boxed value we need to unbox it into the correct type (objectType is typeof(T&) not typeof(T))
					generator.Emit(OpCodes.Box, referencedType);
				}
			}
			else if (objectType.IsValueType)
			{
				generator.Emit(OpCodes.Box, objectType);
			}

			generator.Emit(OpCodes.Callvirt, m_genericObjectListSetter);
		}

		private TypeBuilder GenerateDelegate(MethodInfo methodInfo)
		{
			List<Type> parameterTypes = methodInfo.GetParameters().Select(obj => obj.ParameterType).ToList();
			Type returnType = methodInfo.ReturnType;

			string name = returnType.Name;
			foreach (var type in parameterTypes)
			{
				name += type.Name;
			}

			TypeBuilder delegateTypeBuilder = m_moduleBuilder.DefineType(name, TypeAttributes.Public, typeof(MulticastDelegate));

			//Define Invoke
			MethodBuilder methodInvoke = delegateTypeBuilder.DefineMethod("Invoke", MethodAttributes.Public |
				MethodAttributes.HideBySig |
				MethodAttributes.NewSlot | MethodAttributes.Virtual, CallingConventions.Standard, returnType, parameterTypes.ToArray());

			//Define BeginInvoke
			parameterTypes.Add(typeof(AsyncCallback));
			parameterTypes.Add(typeof(object));
			MethodBuilder methodBeginInvoke = delegateTypeBuilder.DefineMethod("BeginInvoke", MethodAttributes.Public |
				MethodAttributes.HideBySig |
				MethodAttributes.NewSlot |
				MethodAttributes.Virtual,
				typeof(IAsyncResult), parameterTypes.ToArray());
			methodBeginInvoke.SetImplementationFlags(MethodImplAttributes.Runtime |
			MethodImplAttributes.Managed);

			//Define EndInvoke
			MethodBuilder methodEndInvoke = delegateTypeBuilder.DefineMethod("EndInvoke",
			 MethodAttributes.Public |
				MethodAttributes.HideBySig |
				MethodAttributes.NewSlot |
				MethodAttributes.Virtual, null, new Type[] { typeof(IAsyncResult) });
			methodEndInvoke.SetImplementationFlags(MethodImplAttributes.Runtime |
			MethodImplAttributes.Managed);

			return delegateTypeBuilder;
		}

		//Cached methods
		private MethodInfo m_genericObjectListGetter;
		private MethodInfo m_genericObjectListSetter;

		private Type m_genericObjectListType = typeof(List<object>);
		private string m_compiledAssemblyPath;

		private AssemblyBuilder m_assemblyBuilder;
		private ModuleBuilder m_moduleBuilder;
	}
}
