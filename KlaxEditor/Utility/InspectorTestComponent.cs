using KlaxCore.GameFramework;
using KlaxCore.GameFramework.Assets;
using KlaxCore.KlaxScript;
using KlaxIO.AssetManager.Assets;
using KlaxShared;
using KlaxShared.Attributes;
using Newtonsoft.Json;
using SharpDX;
using System.Collections.Generic;

namespace KlaxEditor.Utility
{
	[KlaxEnum]
	enum ETestEnumeration
	{
		First, 
		Second, 
		Third
	}

#pragma warning disable 0169
	[KlaxComponent(Category = "Editor")]
	class CInspectorTestComponent : CSceneComponent
	{
		[KlaxProperty(Category = "Values", DisplayName = "Bool")]
		[JsonProperty]
		private bool m_bool;
		[KlaxProperty(Category = "Values", DisplayName = "Float")]
		[JsonProperty]
		private float m_float;
		[KlaxProperty(Category = "Values", DisplayName = "Int")]
		[JsonProperty]
		private int m_int;
		[KlaxProperty(Category = "Values", DisplayName = "Color")]
		[JsonProperty]
		private Color4 m_color;
		[KlaxProperty(Category = "Values", DisplayName = "String")]
		[JsonProperty]
		private string m_string;
		[KlaxProperty(Category = "Values", DisplayName = "Type")]
		[JsonProperty]
		private CKlaxScriptTypeInfo m_testType;
		[KlaxProperty(Category = "Values", DisplayName = "Enumeration")]
		[JsonProperty]
		private ELogVerbosity m_verbosity;
		[KlaxProperty(Category = "Values", DisplayName = "HashName")]
		[JsonProperty]
		private SHashedName m_hashedName;
		//[KlaxProperty(Category = "Values", DisplayName = "IntArray")]
		//[JsonProperty]
		//private int[] m_intArray = new int[3];
		[KlaxProperty(Category = "Values", DisplayName = "MaterialList")]
		[JsonProperty]
		private List<CAssetReference<CMaterialAsset>> m_materialList = new List<CAssetReference<CMaterialAsset>>();
		[KlaxProperty(Category = "Values", DisplayName = "EnumList")]
		[JsonProperty]
		private List<ELogVerbosity> m_enumList = new List<ELogVerbosity>();
		[KlaxProperty(Category = "Values", DisplayName = "FloatList")]
		[JsonProperty]
		private List<float> m_floatList = new List<float>();
		[KlaxProperty(Category = "Values", DisplayName = "StringIntDictionary")]
		[JsonProperty]
		private Dictionary<string, int> m_stringIntDictionary = new Dictionary<string, int>();
		[KlaxProperty(Category = "Values", DisplayName = "EntityAssetStringDictionary")]
		[JsonProperty]
		private Dictionary<CAssetReference<CEntityAsset<CEntity>>, string> m_entityAssetStringDictionary = new Dictionary<CAssetReference<CEntityAsset<CEntity>>, string>();

		public override void Init()
		{
			base.Init();

			m_stringIntDictionary.Add("First", 0);
			m_stringIntDictionary.Add("Second", 1);
			m_stringIntDictionary.Add("Third", 2);
			World.UpdateScheduler.Connect((delta) =>
			{
				m_stringIntDictionary["First"] = m_stringIntDictionary["First"] + 1;
				m_stringIntDictionary["Second"] = m_stringIntDictionary["Second"] + 2;
				m_stringIntDictionary["Third"] = m_stringIntDictionary["Third"] + 3;
			}, EUpdatePriority.Default);
		}
	}

	[KlaxComponent(Category = "Editor")]
	class CSimpleTypesComponent : CEntityComponent
	{
		[KlaxProperty(Category = "Values", DisplayName = "EnumList")]
		[JsonProperty]
		private List<ELogVerbosity> m_verbosities;
		[KlaxProperty(Category = "Values", DisplayName = "HealthDict")]
		[JsonProperty]
		private Dictionary<string, float> m_verbosities2;

	}
#pragma warning restore 0169
}
