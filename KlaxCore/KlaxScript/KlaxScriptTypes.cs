using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KlaxCore.KlaxScript
{
	public delegate CNode NodeCreationSignature();
	public delegate void KlaxScriptFunctionSignature(List<object> input, List<object> output);

	[DebuggerDisplay("{Name}")]
	public class CKlaxScriptTypeInfo
	{
		public Type Type { get; set; }
		public Color4 Color { get; set; }
		public string Name { get; set; }

		[JsonIgnore]
		public List<CKlaxScriptFunctionInfo> Functions { get; } = new List<CKlaxScriptFunctionInfo>(4);
		[JsonIgnore]
		public List<CKlaxScriptPropertyInfo> Properties { get; } = new List<CKlaxScriptPropertyInfo>(4);
		[JsonIgnore]
		public List<CKlaxScriptEventInfo> Events { get; } = new List<CKlaxScriptEventInfo>(4);
	}

	public struct SSubtypeOf<T>
	{
		public SSubtypeOf(Type type)
		{
			Type parentType = typeof(T);
			if (parentType.IsAssignableFrom(type))
			{
				m_type = type;
			}
			else
			{
				throw new Exception(string.Format("The exact type of a CSubtype struct must be assignable to its defined type. {0} is not assignable to {1}", type.Name, parentType.Name));
			}
		}

		[JsonProperty]
		private Type m_type;

		[JsonIgnore]
		public Type Type
		{
			get { return m_type; }
			set
			{
				Type parentType = typeof(T);
				if (parentType.IsAssignableFrom(value))
				{
					m_type = value;
				}
				else
				{
					throw new Exception(string.Format("The exact type of a CSubtype struct must be assignable to its defined type. {0} is not assignable to {1}", value != null ? value.Name : "null", parentType.Name));
				}
			}
		}
	}

	public class CKlaxScriptPropertyInfo
	{
		public PropertyInfo propertyInfo;
		public FieldInfo fieldInfo;
		public string displayName;
		public string category;
		public string tooltip;
		public Type declaringType;
		public Type propertyType;
		public bool bIsReadOnly;
	}

	public struct SKlaxScriptFunctionProxy
	{
		public SKlaxScriptFunctionProxy(CKlaxScriptFunctionInfo method)
		{
			methodInfo = method.methodInfo;
			compiledMethod = method.compiledMethod;
			bIsCompiled = compiledMethod != null;
		}

		public bool bIsCompiled;
		public MethodInfo methodInfo;
		public KlaxScriptFunctionSignature compiledMethod;
	}

	public class CKlaxScriptFunctionInfo
	{
		public MethodInfo methodInfo;
		public KlaxScriptFunctionSignature compiledMethod;
		public string displayName;
		public string category;
		public string tooltip;
		public bool bIsImplicit;

		public Type[] inputParameter;
		public string[] inputParameterNames;
		public object[] inputParametersDefaultValue;
	}

	public class CKlaxScriptEventInfo
	{
		public FieldInfo klaxEventInfo;
		public string displayName;
		public string category;
		public string tooltip;
		public string ParameterName1;
		public string ParameterName2;
		public string ParameterName3;
		public string ParameterName4;
		public string ParameterName5;
		public string ParameterName6;
		public string ParameterName7;
		public string ParameterName8;
		public string ParameterName9;
		public string ParameterName10;
	}

	class CKlaxScriptNodeQueryContext
	{
		public static CKlaxScriptNodeQueryContext Empty = new CKlaxScriptNodeQueryContext();

		public CKlaxScriptNodeQueryContext() { }

		public CKlaxScriptNodeQueryContext(CKlaxScriptObject queryObject)
		{
			QueryObject = queryObject;

		}

		public static bool AreEqual(CKlaxScriptNodeQueryContext a, CKlaxScriptNodeQueryContext b)
		{
			return a?.InputPinType == b?.InputPinType && a?.EnvironmentType == b?.EnvironmentType && a?.IsExecPin == b?.IsExecPin && a.QueryObject == b.QueryObject;
		}

		public Type InputPinType { get; set; }
		public CKlaxScriptTypeInfo EnvironmentType { get; set; }
		public CKlaxScriptObject QueryObject { get; set; }
		public bool IsExecPin { get; set; }
	}

	public class CNodeExecutionContext
	{
		public CGraph graph;
		public CExecutionPin calledPin;
	}

	class CKlaxScriptNodeFactory
	{
		public CKlaxScriptNodeFactory(string name, string category, string tooltip, Type targetType, bool bIsMember, NodeCreationSignature creator)
		{
			Name = name;
			Category = category;
			TargetType = targetType;
			IsMemberNode = bIsMember;
			m_createNode = creator;
			Tooltip = tooltip;
		}

		public string Name { get; }
		public string Category { get; }
		public string Tooltip { get; }
		public Type TargetType { get; }
		public bool IsMemberNode { get; }

		public CNode CreateNode()
		{
			return m_createNode != null ? m_createNode() : null;
		}

		private NodeCreationSignature m_createNode;
	}

	public class CNodeChangeContext
	{
		public List<CNodeAction> Actions { get; set; } = new List<CNodeAction>();
	}

	public enum ENodeAction
	{
		PinTypeChange,
		PinNameChange,
		NodeNameChange,
		SwitchNodeTypeChange,
		AddPinChange
	}

	public abstract class CNodeAction
	{
		public ENodeAction ActionType { get; set; }
	}

	public class CPinTypeChangeAction : CNodeAction
	{
		public CPinTypeChangeAction(CPin pin, Type newType)
		{
			ActionType = ENodeAction.PinTypeChange;

			Pin = pin;
			NewType = newType;
		}

		public CPin Pin { get; }
		public Type NewType { get; }
	}

	public class CPinNameChangeAction : CNodeAction
	{
		public CPinNameChangeAction(CPin pin, string newName)
		{
			ActionType = ENodeAction.PinNameChange;

			Pin = pin;
			NewName = newName;
		}

		public CPin Pin { get; }
		public string NewName { get; }
	}

	public class CNodeNameChangeAction : CNodeAction
	{
		public CNodeNameChangeAction(string newName)
		{
			ActionType = ENodeAction.NodeNameChange;
			NewName = newName;
		}

		public string NewName { get; }
	}

	public class CSwitchNodeTypeChangeAction : CNodeAction
	{
		public CSwitchNodeTypeChangeAction(Type newType)
		{
			ActionType = ENodeAction.SwitchNodeTypeChange;
			NewType = newType;
		}

		public Type NewType { get; }
	}

	public class CAddPinChangeAction : CNodeAction
	{
		public CAddPinChangeAction(CPin pin, int index, bool bIsIn)
		{
			ActionType = ENodeAction.AddPinChange;
			Pin = pin;
			Index = index;
			IsIn = bIsIn;
		}

		public CPin Pin { get; }
		public int Index { get; }
		public bool IsIn { get; }
	}
}
