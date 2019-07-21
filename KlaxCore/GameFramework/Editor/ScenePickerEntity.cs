using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxCore.Core;
using KlaxShared.Attributes;
using Newtonsoft.Json;

namespace KlaxCore.GameFramework.Editor
{
	[KlaxScriptType(Name = "ScenePickerEntity")]
	public class CScenePickerEntity : CEntity
	{
		public CScenePickerEntity()
		{
			ShowInOutliner = false;
		}

		public override void Init(CWorld world, object userData)
		{
			base.Init(world, userData);
			if (GetComponent<CScenePickingComponent>() == null)
			{
				ScenePickingComponent = AddComponent<CScenePickingComponent>(true, true);
			}
		}

		[JsonProperty]
		public CScenePickingComponent ScenePickingComponent { get; private set; }
	}
}
