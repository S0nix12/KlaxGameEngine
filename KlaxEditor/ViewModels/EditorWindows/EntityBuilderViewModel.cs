using KlaxCore.GameFramework;
using KlaxCore.GameFramework.Assets;
using KlaxEditor.Views;
using System;

namespace KlaxEditor.ViewModels.EditorWindows
{
	class CEntityBuilderViewModel : CEditorWindowViewModel
	{
		public event Action<CEntityAsset<CEntity>, CEntity> OnAssetOpened;

		public CEntityBuilderViewModel()
			: base("Entity Builder")
		{
			SetIconSourcePath("Resources/Images/Tabs/entitybuilder.png");

			Content = new EntityBuilderInspector();
			IsAlwaysHidden = true;
		}

		public void OpenAsset(CEntityAsset<CEntity> asset)
		{
			Entity = asset.GetEntity();
			OnAssetOpened?.Invoke(asset, Entity);
		}

		CEntity m_entity;
		public CEntity Entity
		{
			get { return m_entity; }
			set
			{
				m_entity = value;
				RaisePropertyChanged();
			}
		}
	}
}
