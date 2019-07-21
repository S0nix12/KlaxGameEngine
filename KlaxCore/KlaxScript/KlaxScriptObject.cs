using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using KlaxCore.KlaxScript.Interfaces;
using KlaxCore.KlaxScript.Serialization;
using KlaxIO.AssetManager.Assets;

namespace KlaxCore.KlaxScript
{
	public class CKlaxScriptObject
	{
		public void Init(object outer)
		{
			Outer = outer;
			foreach (CEventGraph eventGraph in EventGraphs)
			{
				eventGraph.Subscribe(outer);
				eventGraph.Compile();
			}

			foreach (CInterfaceFunctionGraph interfaceGraph in InterfaceGraphs)
			{
				interfaceGraph.Compile();
			}

			foreach (CCustomFunctionGraph customFunctionGraph in FunctionGraphs)
			{
				customFunctionGraph.Compile();
			}
		}

		[OnSerializing]
		internal void OnSerializing(StreamingContext context)
		{
			foreach (var eventGraph in EventGraphs)
			{
				eventGraph.PrepareNodesForSerialization(this);
			}

			foreach (var interfaceGraph in InterfaceGraphs)
			{
				interfaceGraph.PrepareNodesForSerialization(this);
			}

			foreach (var customFunctionGraph in FunctionGraphs)
			{
				customFunctionGraph.PrepareNodesForSerialization(this);
			}
		}

		[OnDeserialized]
		internal void OnDeserialized(StreamingContext context)
		{
			foreach (var eventGraph in EventGraphs)
			{
				eventGraph.ResolveNodesAfterSerialization(this);
			}

			foreach (var interfaceGraph in InterfaceGraphs)
			{
				interfaceGraph.ResolveNodesAfterSerialization(this);
			}

			foreach (var customFunctionGraph in FunctionGraphs)
			{
				customFunctionGraph.ResolveNodesAfterSerialization(this);
			}
		}

		[JsonProperty]
		public List<CEventGraph> EventGraphs { get; } = new List<CEventGraph>();

		[JsonProperty]
		public List<CInterfaceFunctionGraph> InterfaceGraphs { get; } = new List<CInterfaceFunctionGraph>();

		[JsonProperty]
		public List<CCustomFunctionGraph> FunctionGraphs { get; } = new List<CCustomFunctionGraph>();

		[JsonProperty]
		public List<CKlaxVariable> KlaxVariables { get; } = new List<CKlaxVariable>();

		[JsonProperty]
		public List<CKlaxScriptInterfaceReference> IncludedInterfaces { get; } = new List<CKlaxScriptInterfaceReference>();

		public object Outer { get; private set; }
	}
}
