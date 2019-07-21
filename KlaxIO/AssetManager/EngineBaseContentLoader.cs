using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxIO.AssetManager.Assets;
using KlaxIO.AssetManager.Loaders;

namespace KlaxIO.AssetManager
{
	public static class EngineBaseContentLoader
	{
		public static event Action BaseContentLoading;

		public static void LoadBaseContent()
		{
			DefaultCapsule = CImportManager.Instance.MeshImporter.LoadMeshAsset("EngineResources/DefaultMeshes/DefaultCapsule.obj");
			DefaultCone = CImportManager.Instance.MeshImporter.LoadMeshAsset("EngineResources/DefaultMeshes/DefaultCone.obj");
			DefaultCube = CImportManager.Instance.MeshImporter.LoadMeshAsset("EngineResources/DefaultMeshes/DefaultCube.obj");
			DefaultCylinder = CImportManager.Instance.MeshImporter.LoadMeshAsset("EngineResources/DefaultMeshes/DefaultCylinder.obj");
			DefaultSphere = CImportManager.Instance.MeshImporter.LoadMeshAsset("EngineResources/DefaultMeshes/DefaultSphere.obj");

			BaseContentLoading?.Invoke();
		}

		public static CMeshAsset DefaultCapsule { get; private set; }
		public static CMeshAsset DefaultCone { get; private set; }
		public static CMeshAsset DefaultCube { get; private set; }
		public static CMeshAsset DefaultCylinder { get; private set; }
		public static CMeshAsset DefaultSphere { get; private set; }
	}
}
