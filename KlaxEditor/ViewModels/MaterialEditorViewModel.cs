using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using KlaxCore.Core;
using KlaxCore.EditorHelper;
using KlaxEditor.UserControls.InspectorControls;
using KlaxEditor.Utility;
using KlaxEditor.ViewModels.EditorWindows;
using KlaxEditor.Views;
using KlaxIO.AssetManager;
using KlaxIO.AssetManager.Assets;
using KlaxMath;
using KlaxRenderer;
using KlaxRenderer.Graphics;
using KlaxRenderer.Lights;
using KlaxRenderer.RenderNodes;
using KlaxRenderer.Scene;
using KlaxShared;
using KlaxShared.Definitions.Graphics;
using SharpDX;
using WpfSharpDxControl;

namespace KlaxEditor.ViewModels
{
	interface IMaterialValueConverter
	{
		object ToEditorValue(object materialValue);
		object ToMaterialValue(object editorValue);
		object ToMaterialAssetValue(object editorValue);
	}

	class CEmptyValueConverter : IMaterialValueConverter
	{
		public object ToEditorValue(object materialValue)
		{
			return materialValue;
		}

		public object ToMaterialValue(object editorValue)
		{
			return editorValue;
		}

		public object ToMaterialAssetValue(object editorValue)
		{
			return editorValue;
		}
	}
	class CColorValueConverter : IMaterialValueConverter
	{
		public object ToEditorValue(object materialValue)
		{
			if (materialValue == null)
			{
				return null;
			}

			Vector4 vectorValue = (Vector4)materialValue;
			return new Color4(vectorValue);
		}

		public object ToMaterialValue(object editorValue)
		{
			Color4? colorValue = (Color4?)editorValue;
			return colorValue?.ToVector4();
		}

		public object ToMaterialAssetValue(object editorValue)
		{
			return ToMaterialValue(editorValue);
		}
	}
	class CTextureValueConverter : IMaterialValueConverter
	{
		public object ToEditorValue(object materialValue)
		{
			if (materialValue == null)
			{
				return null;
			}

			CTextureSampler textureValue = (CTextureSampler)materialValue;
			CAssetReference<CTextureAsset> assetReference = CAssetRegistry.Instance.GetAsset<CTextureAsset>(textureValue.Guid);
			return assetReference;
		}

		public object ToMaterialValue(object editorValue)
		{
			if (editorValue == null)
			{
				return null;
			}

			CAssetReference<CTextureAsset> assetReference = (CAssetReference<CTextureAsset>)editorValue;
			return CRenderer.Instance.ResourceManager.RequestResourceFromAsset<CTextureSampler>(assetReference.GetAsset());
		}

		public object ToMaterialAssetValue(object editorValue)
		{
			return editorValue;
		}
	}

	class MaterialParameterViewModel
	{
		public MaterialParameterViewModel(MaterialEditorViewModel viewModel, SShaderParameterTarget parameterTarget, object value, IMaterialValueConverter valueConverter)
		{
			m_valueConverter = valueConverter ?? new CEmptyValueConverter();
			m_viewModel = viewModel;
			HashedName = parameterTarget.parameterName;
			Name = HashedName.GetString();
			ParameterType = parameterTarget.parameterType;
			m_bLockValue = true;
			Value = m_valueConverter.ToEditorValue(value);
			m_bLockValue = false;
		}

		public MaterialParameterViewModel(MaterialEditorViewModel viewModel, SShaderTextureTarget parameterTarget, object value, IMaterialValueConverter valueConverter)
		{
			m_valueConverter = valueConverter ?? new CEmptyValueConverter();
			m_viewModel = viewModel;
			HashedName = parameterTarget.parameterName;
			Name = HashedName.GetString();
			ParameterType = EShaderParameterType.Texture;
			m_bLockValue = true;
			Value = m_valueConverter.ToEditorValue(value);
			m_bLockValue = false;
		}

		public object GetMaterialValue()
		{
			return m_valueConverter.ToMaterialValue(Value);
		}

		public object GetAssetValue()
		{
			return m_valueConverter.ToMaterialAssetValue(Value);
		}

		private void OnValueChanged()
		{
			if (!m_bLockValue)
			{
				m_viewModel.MaterialParameterChanged(this);
			}
		}

		public SHashedName HashedName { get; private set; }
		public string Name { get; private set; }
		public EShaderParameterType ParameterType { get; private set; }

		private object m_value;
		public object Value
		{
			get { return m_value; }
			set
			{
				if (value != m_value)
				{
					m_value = value;
					OnValueChanged();
				}
			}

		}

		private readonly MaterialEditorViewModel m_viewModel;
		private readonly IMaterialValueConverter m_valueConverter;
		private bool m_bLockValue;
	}

	class MaterialEditorViewModel : CEditorWindowViewModel
	{
		public MaterialEditorViewModel() : base("MaterialEditor")
		{
			SetIconSourcePath("Resources/Images/Tabs/materialeditor.png");

			MaterialEditorView view = new MaterialEditorView();
			Content = view;
			//m_viewPortControl = view.PreviewViewport;
			//m_viewPortControl.Loaded += OnViewPortLoaded;
			m_propertyInspector = view.PropertyInspector;

			m_materialValueConverters.Add(EShaderParameterType.Color, new CColorValueConverter());
			m_materialValueConverters.Add(EShaderParameterType.Texture, new CTextureValueConverter());
		}

		private void OnViewPortLoaded(object sender, RoutedEventArgs e)
		{
			//if (m_bWorldLoaded && !m_previewScene.IsCreated)
			//{
			//	IRenderSurface renderSurface = m_viewPortControl.GetRenderSurface();
			//	m_previewScene.EditorThread_CreateScene(renderSurface);
			//}
		}

		public override void PostWorldLoad()
		{
			base.PostWorldLoad();

			//m_bWorldLoaded = true;
			//if (m_viewPortControl.IsLoaded && !m_previewScene.IsCreated)
			//{
			//	IRenderSurface renderSurface = m_viewPortControl.GetRenderSurface();
			//	m_previewScene.EditorThread_CreateScene(renderSurface);
			//}
		}

		public void OnTargetAssetChanged()
		{
			MaterialName = m_targetMaterialAsset.Name;
			PreviewScene previewScene = CWorkspace.Instance.GetTool<CAssetPreviewerViewModel>().PreviewScene;
			CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
			{
				m_targetMaterial = CRenderer.Instance.ResourceManager.RequestResourceFromAsset<CMaterial>(m_targetMaterialAsset);
				previewScene.ShowMeshSelection = true;
				previewScene.EngineThread_SetLastDefaultMesh(false);
				previewScene.EngineThread_SetPreviewMeshMaterial(m_targetMaterial);
				Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)WaitTargetMaterialLoaded);
			});
		}

		public void MaterialParameterChanged(MaterialParameterViewModel changedParameter)
		{
			if (m_targetMaterial != null)
			{
				m_targetMaterial.m_activeParameters[changedParameter.HashedName] = new SShaderParameter(changedParameter.ParameterType, changedParameter.GetMaterialValue());
			}

			if (m_targetMaterialAsset != null)
			{
				bool bFoundParameter = false;
				var shaderParameter = new SShaderParameter(changedParameter.ParameterType, changedParameter.GetAssetValue());
				var materialParameter = new SMaterialParameterEntry(changedParameter.HashedName, shaderParameter);
				for (var index = 0; index < m_targetMaterialAsset.MaterialParameters.Count; index++)
				{
					SMaterialParameterEntry parameterEntry = m_targetMaterialAsset.MaterialParameters[index];
					if (parameterEntry.name == changedParameter.HashedName)
					{
						m_targetMaterialAsset.MaterialParameters[index] = materialParameter;
						bFoundParameter = true;
						break;
					}
				}

				if (!bFoundParameter)
				{
					m_targetMaterialAsset.MaterialParameters.Add(materialParameter);
				}
				CAssetRegistry.Instance.SaveAsset(m_targetMaterialAsset);
			}

			CreateMaterialProperties();
			Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action) (() => { m_propertyInspector.ShowInspectors(MaterialProperties); }));
		}

		private void WaitTargetMaterialLoaded()
		{
			m_targetMaterial.WaitUntilLoaded();
			m_targetMaterial.ShaderResource.WaitUntilLoaded();
			
			List<SShaderParameterTarget> parameterTargets = new List<SShaderParameterTarget>();
			List<SShaderTextureTarget> textureTargets = new List<SShaderTextureTarget>();

			m_targetMaterial.ShaderResource.Shader.GetShaderParameterTargets(parameterTargets, textureTargets);
			ParameterEntries.Clear();

			foreach (var parameterTarget in parameterTargets)
			{
				m_materialValueConverters.TryGetValue(parameterTarget.parameterType, out IMaterialValueConverter valueConverter);
				if (m_targetMaterial.m_activeParameters.TryGetValue(parameterTarget.parameterName, out SShaderParameter materialValue))
				{
					ParameterEntries.Add(new MaterialParameterViewModel(this, parameterTarget, materialValue.parameterData, valueConverter));
				}
				else
				{
					ParameterEntries.Add(new MaterialParameterViewModel(this, parameterTarget, null, valueConverter));
				}
			}

			m_materialValueConverters.TryGetValue(EShaderParameterType.Texture, out IMaterialValueConverter textureConverter);
			foreach (var textureTarget in textureTargets)
			{
				if (m_targetMaterial.m_activeParameters.TryGetValue(textureTarget.parameterName, out SShaderParameter materialValue))
				{
					ParameterEntries.Add(new MaterialParameterViewModel(this, textureTarget, materialValue.parameterData, textureConverter));
				}
				else
				{
					ParameterEntries.Add(new MaterialParameterViewModel(this, textureTarget, null, textureConverter));
				}
			}

			CreateMaterialProperties();
		}

		private void CreateMaterialProperties()
		{
			MaterialProperties.Clear();
			CCategoryInfo category = new CCategoryInfo() { Name = "Material", Priority = 0 };
			foreach (MaterialParameterViewModel parameterEntry in ParameterEntries)
			{
				Type paramType = ShaderHelpers.GetTypeFromParameterType(parameterEntry.ParameterType);
				if (parameterEntry.ParameterType == EShaderParameterType.Texture)
				{
					paramType = typeof(CAssetReference<CTextureAsset>);
				}
				PropertyInfo valuePropertyInfo = typeof(MaterialParameterViewModel).GetProperty("Value");
				CObjectProperty objectProperty = new CObjectProperty(parameterEntry.Name, category, parameterEntry, parameterEntry.Value, paramType, valuePropertyInfo, null);
				MaterialProperties.Add(objectProperty);
			}

			m_propertyInspector.ShowInspectors(MaterialProperties);
		}

		private CMaterial m_targetMaterial;
		private CMaterialAsset m_targetMaterialAsset;
		public CMaterialAsset TargetMaterialAsset
		{
			get { return m_targetMaterialAsset; }
			set
			{
				if (m_targetMaterialAsset != value)
				{
					m_targetMaterialAsset = value;
					RaisePropertyChanged();
					OnTargetAssetChanged();
				}
			}
		}

		private string m_materialName;
		public string MaterialName
		{
			get { return m_materialName; }
			set { m_materialName = value; RaisePropertyChanged(); }
		}

		public ObservableCollection<MaterialParameterViewModel> ParameterEntries { get; private set; } = new ObservableCollection<MaterialParameterViewModel>();
		public List<CObjectBase> MaterialProperties = new List<CObjectBase>();
		private readonly PropertyInspector m_propertyInspector;

		private Dictionary<EShaderParameterType, IMaterialValueConverter> m_materialValueConverters = new Dictionary<EShaderParameterType, IMaterialValueConverter>();
	}
}
