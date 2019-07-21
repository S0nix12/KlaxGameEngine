using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxCore.KlaxScript.Serialization;
using KlaxIO.AssetManager.Assets;
using Newtonsoft.Json;
using SharpDX;

namespace KlaxCore.KlaxScript.Interfaces
{
	public class CKlaxScriptInterfaceAsset : CAsset
	{
		public const string FILE_EXTENSION = ".klaxinterface";
		public const string TYPE_NAME = "Interface";
		public static readonly Color4 TYPE_COLOR = new Color(195, 10, 20).ToColor4();

		public override string GetFileExtension()
		{
			return FILE_EXTENSION;
		}

		public override string GetTypeName()
		{
			return TYPE_NAME;
		}

		public override Color4 GetTypeColor()
		{
			return TYPE_COLOR;
		}

		public override void CopyFrom(CAsset source)
		{
			base.CopyFrom(source);
			CKlaxScriptInterfaceAsset interfaceAsset = (CKlaxScriptInterfaceAsset) source;
			Interface = interfaceAsset.Interface;
		}

		public override void Dispose() {}

		[JsonConverter(typeof(CUseScriptSerializerConverter))]
		[JsonProperty]
		public CKlaxScriptInterface Interface { get; private set; } = new CKlaxScriptInterface();
	}
}
