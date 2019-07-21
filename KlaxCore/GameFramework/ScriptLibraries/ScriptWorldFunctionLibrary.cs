using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxCore.Core;
using KlaxCore.GameFramework.Assets;
using KlaxIO.AssetManager.Assets;
using KlaxShared.Attributes;
using SharpDX;

namespace KlaxCore.GameFramework.ScriptLibraries
{
	[KlaxLibrary]
	public static class ScriptWorldFunctionLibrary
	{
		[KlaxFunction(Category = "Game", Tooltip = "Get the world that is currently used by the engine")]
		public static CWorld GetCurrentWorld()
		{
			return CEngine.Instance.CurrentWorld;
		}

		[KlaxFunction(Category = "Game", Tooltip = "Spawn an empty entity in the world")]
		public static CEntity SpawnEntity()
		{
			CWorld world = GetCurrentWorld();			
			return world.SpawnEntity<CEntity>();
		}

		[KlaxFunction(Category = "Game", Tooltip = "Spawn an entity defined by an asset in the world")]
		public static CEntity SpawnEntityFromAsset(CAssetReference<CEntityAsset<CEntity>> entityAsset)
		{
			if (entityAsset != null && entityAsset.GetAsset() != null)
			{
				CEntity newEntity = entityAsset.GetAsset().GetEntity();
				GetCurrentWorld().InitWithWorld(newEntity);
				return newEntity;
			}

			return null;
		}

		[KlaxFunction(Category = "Game", Tooltip = "Spawn an empty entity in the world at the given position and rotation")]
		public static CEntity SpawnEntityAtPosition(Vector3 worldPosition, Quaternion worldRotation)
		{
			CEntitySpawnParameters spawnParams = new CEntitySpawnParameters();
			spawnParams.position = worldPosition;
			spawnParams.rotation = worldRotation;
			return GetCurrentWorld().SpawnEntity<CEntity>(spawnParams);
		}

		[KlaxFunction(Category = "Game", Tooltip = "Spawn an entity defined by an asset in the world at the given position and rotation")]
		public static CEntity SpawnEntityAssetAtPosition(CAssetReference<CEntityAsset<CEntity>> entityAsset, Vector3 worldPosition, Quaternion worldRotation)
		{
			if (entityAsset != null && entityAsset.GetAsset() != null)
			{
				CEntity newEntity = entityAsset.GetAsset().GetEntity();
				newEntity.SetWorldPosition(in worldPosition);
				newEntity.SetWorldRotation(in worldRotation);
				GetCurrentWorld().InitWithWorld(newEntity);
				return newEntity;
			}

			return null;
		}
	}
}
