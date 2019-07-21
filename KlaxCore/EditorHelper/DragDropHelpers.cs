using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KlaxCore.Core;
using KlaxCore.GameFramework;
using KlaxCore.GameFramework.Assets;

namespace KlaxCore.EditorHelper
{
	public interface IDropHandler
	{
		void HandleDrop(object dropData);
	}

	public class CEntityDropHandler : IDropHandler
	{
		public void HandleDrop(object dropData)
		{
			CEntityAsset<CEntity> entityAsset = (CEntityAsset<CEntity>) dropData;
			CEngine.Instance.CurrentWorld.InitWithWorld(entityAsset.GetEntity());
		}
	}

	public class CLevelDropHandler : IDropHandler
	{
		public void HandleDrop(object dropData)
		{
			CLevelAsset levelAsset = (CLevelAsset) dropData;

			Stopwatch timer = new Stopwatch();
			timer.Start();
			CLevel newLevel = levelAsset.GetLevel();
			timer.Stop();
			LogUtility.Log("Level deserialize took {0} ms", timer.Elapsed.TotalMilliseconds);
			CEngine.Instance.CurrentWorld.ChangeLevel(levelAsset, newLevel);
		}
	}

	public static class DragDropHelpers
	{
		static DragDropHelpers()
		{
			// Register DropHandlers
			DropHandlers.Add(typeof(CEntityAsset<CEntity>), new CEntityDropHandler());
			DropHandlers.Add(typeof(CLevelAsset), new CLevelDropHandler());
		}

		public static bool CanHandleDragDrop(object dragData)
		{
			if (DropHandlers.ContainsKey(dragData.GetType()))
			{
				return true;
			}
			return false;
		}

		public static void HandleDrop(object dropData)
		{
			if (DropHandlers.TryGetValue(dropData.GetType(), out IDropHandler handler))
			{
				handler.HandleDrop(dropData);
			}
		}

		private static Dictionary<Type, IDropHandler> DropHandlers = new Dictionary<Type, IDropHandler>();
	}
}
