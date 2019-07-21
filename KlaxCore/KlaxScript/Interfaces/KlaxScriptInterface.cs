using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxIO.AssetManager.Assets;
using Newtonsoft.Json;

namespace KlaxCore.KlaxScript.Interfaces
{
	public class CKlaxScriptInterfaceFunction
	{
		[JsonProperty]
		public List<CKlaxVariable> InputParameters { get; private set; } = new List<CKlaxVariable>();

		[JsonProperty]
		public List<CKlaxVariable> OutputParameters { get; private set; } = new List<CKlaxVariable>();

		public string Name { get; set; }
		[JsonProperty]
		public Guid Guid { get; private set; } = Guid.NewGuid();
	}

	public class CKlaxScriptInterface
	{
		[JsonProperty]
		public List<CKlaxScriptInterfaceFunction> Functions { get; private set; } = new List<CKlaxScriptInterfaceFunction>();
		[JsonProperty]
		public Guid Guid { get; private set; } = Guid.NewGuid();
	}

	public class CKlaxScriptInterfaceReference
	{
		public CAssetReference<CKlaxScriptInterfaceAsset> InterfaceAsset { get; set; }

		public CKlaxScriptInterface GetInterface()
		{
			if (InterfaceAsset == null)
			{
				return null;
			}

			CKlaxScriptInterfaceAsset asset = InterfaceAsset.GetAsset();
			if (asset == null)
			{
				return null;
			}

			asset.WaitUntilLoaded();
			return asset.Interface;
		}
	}
}
