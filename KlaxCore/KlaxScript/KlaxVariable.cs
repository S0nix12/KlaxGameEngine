using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using KlaxCore.KlaxScript.Serialization;
using Newtonsoft.Json;

namespace KlaxCore.KlaxScript
{
	public class CKlaxVariable
	{
		[JsonConverter(typeof(CUseScriptSerializerConverter))]
		public Type Type { get; set; }

		[JsonConverter(typeof(CUseScriptSerializerConverter))]
		public object Value { get; set; }
		public string Name { get; set; }
		[JsonProperty]
		public Guid Guid { get; private set; } = Guid.NewGuid();

	}
}
