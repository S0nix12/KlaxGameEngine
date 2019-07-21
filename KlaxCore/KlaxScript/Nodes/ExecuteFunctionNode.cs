using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KlaxCore.KlaxScript
{
	public class CExecuteFunctionNode : CNode
	{
		public CExecuteFunctionNode()
		{
			CExecutionPin execute = new CExecutionPin()
			{
				Name = "In",
				TargetNode = null
			};
			InExecutionPins.Add(execute);

			CExecutionPin done = new CExecutionPin()
			{
				Name = "Out",
				TargetNode = null
			};
			OutExecutionPins.Add(done);
		}

		public override CExecutionPin Execute(CNodeExecutionContext context, List<object> inParameters, List<object> outReturn)
		{
			for (int i = 0; i < inParameters.Count; i++)
			{
				if (InputPins[i].SourceNode == null)
				{
					inParameters[i] = InputPins[i].Literal;
				}
			}

			if (m_functionProxy.bIsCompiled)
			{
				m_functionProxy.compiledMethod(inParameters, outReturn);
			}
			else
			{
				object targetObject;
				object[] parameterArray;
				int parameterStartIndex;
				if (TargetMethod.IsStatic)
				{
					targetObject = null;
					parameterArray = new object[InputPins.Count];
					parameterStartIndex = 0;
				}
				else
				{
					targetObject = inParameters[0];
					parameterArray = new object[InputPins.Count - 1];
					parameterStartIndex = 1;
				}

				for (int i = parameterStartIndex; i < InputPins.Count; i++)
				{
					parameterArray[i - parameterStartIndex] = inParameters[i];
				}

				object returnValue = TargetMethod.Invoke(targetObject, parameterArray);
				outReturn.Add(returnValue);
				foreach (int returnIndex in m_additionalReturns)
				{
					outReturn.Add(parameterArray[returnIndex]);
				}
			}

			return IsImplicit ? null : OutExecutionPins[0];
		}

		private void OnTargetMethodChanged()
		{
			OutputPins.Clear();
			InputPins.Clear();

			CKlaxScriptRegistry registry = CKlaxScriptRegistry.Instance;
			registry.TryGetFunctionInfo(TargetMethod, out CKlaxScriptFunctionInfo outFunctionInfo);

			Name = outFunctionInfo.displayName;

			if (TargetMethod.ReturnType != typeof(void))
			{
				COutputPin returnOutput = new COutputPin()
				{
					Name = "Return",
					Type = TargetMethod.ReturnType,
				};
				OutputPins.Add(returnOutput);
			}

			if (!TargetMethod.IsStatic)
			{
				CInputPin targetObjectInput = new CInputPin()
				{
					Name = "Target",
					Type = TargetMethod.DeclaringType,
					Literal = null,
					SourceNode = null,
					SourceParameterIndex = -1,
					StackIndex = -1,
				};
				InputPins.Add(targetObjectInput);
			}

			ParameterInfo[] methodParameters = TargetMethod.GetParameters();
			for (var index = 0; index < methodParameters.Length; index++)
			{
				ParameterInfo parameter = methodParameters[index];
				Type elementType = parameter.ParameterType;
				if (parameter.ParameterType.IsByRef)
				{
					elementType = parameter.ParameterType.GetElementType();
					if (!parameter.IsIn)
					{
						m_additionalReturns.Add(index);
						COutputPin output = new COutputPin()
						{
							Name = parameter.Name,
							Type = elementType,
						};
						OutputPins.Add(output);
					}
				}

				if (parameter.IsOut)
				{
					continue;
				}

				CInputPin input = new CInputPin()
				{
					Name = outFunctionInfo.inputParameterNames[index],
					Type = elementType,
					SourceNode = null,
					SourceParameterIndex = -1,
					StackIndex = -1,
				};

				input.Literal = input.Type.IsValueType ? Activator.CreateInstance(input.Type) : null;
				InputPins.Add(input);

				m_functionProxy = new SKlaxScriptFunctionProxy(outFunctionInfo);
			}
		}

		protected override void OnImplicitChanged()
		{
			InExecutionPins.Clear();
			OutExecutionPins.Clear();

			if (!IsImplicit)
			{
				CExecutionPin execute = new CExecutionPin()
				{
					Name = "In",
					TargetNode = null
				};
				InExecutionPins.Add(execute);

				CExecutionPin done = new CExecutionPin()
				{
					Name = "Out",
					TargetNode = null
				};
				OutExecutionPins.Add(done);
			}
		}

		private MethodInfo m_targetMethod;
		[JsonProperty]
		public MethodInfo TargetMethod
		{
			get { return m_targetMethod; }
			set
			{
				if (m_targetMethod != value)
				{
					m_targetMethod = value;
					OnTargetMethodChanged();
				}
			}
		}

		SKlaxScriptFunctionProxy m_functionProxy;		

		[JsonProperty]
		private List<int> m_additionalReturns = new List<int>();
	}
}
