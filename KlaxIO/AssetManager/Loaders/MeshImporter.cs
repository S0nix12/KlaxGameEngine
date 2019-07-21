using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Assimp;
using KlaxIO.AssetManager.Assets;
using KlaxShared;
using KlaxShared.Definitions;
using KlaxShared.Definitions.Graphics;
using KlaxShared.Utilities;
using SharpDX;
using SharpDX.Direct3D;

namespace KlaxIO.AssetManager.Loaders
{
	enum EAssetType
	{
		Mesh,
		Model
	}

	struct SAssetLoadRequest
	{
		public CAsset targetAsset;
		public string filename;
		public string assetPath;
		public EAssetType type;
	}

	class CMeshLoadingJob
	{
		public CMeshLoadingJob(Scene scene, string basePath)
		{
			Scene = scene;
			BasePath = basePath;
		}

		public Scene Scene;
		public string BasePath;
		public Dictionary<string, CTextureAsset> LoadedTextures;
		public Dictionary<string, CMaterialAsset> LoadedMaterials;
	}
	public class CMeshImporter : IImporter, IDisposable
	{
		public CMeshImporter()
		{
			m_importer = new AssimpContext();
		}

		public bool Import(string filename, string importPath, bool bAlwaysImport = false)
		{
			// Block new loads and wait for our current load to finish
			m_bIsLoading = true;
			m_currentLoadTask?.Wait();

			// Load the asset synchronous
			Scene scene = m_importer.ImportFile(filename, PostProcessPreset.TargetRealTimeMaximumQuality | PostProcessPreset.ConvertToLeftHanded);
			if (scene.HasMeshes)
			{
				if (scene.MeshCount > 1)
				{
					CModelAsset outModel = new CModelAsset();
					LoadModelInternal(filename, scene, outModel, importPath);
					outModel.LoadFinished();

					outModel.Name = Path.GetFileNameWithoutExtension(filename);
					if (CAssetRegistry.Instance.RequestRegisterAsset(outModel, importPath + "Models/", out CModelAsset existingModel, bAlwaysImport))
					{
						existingModel.WaitUntilLoaded();
					}
				}
				else
				{
					string basePath = Path.GetDirectoryName(filename);
					CMeshLoadingJob loadingJob = new CMeshLoadingJob(scene, basePath);
					CMeshAsset outMesh = new CMeshAsset();
					LoadMeshInternal(0, outMesh, loadingJob, importPath, Path.GetFileNameWithoutExtension(filename), bAlwaysImport);
					outMesh.LoadFinished();
				}
			}

			// Unblock loader
			m_bIsLoading = false;
			// Continue loading request
			StartNextLoadRequest();

			return true;
		}

		public bool ImportAsync(string filename, string importPath, bool bAlwaysImport = false)
		{
			throw new NotImplementedException();
		}

		public string[] GetSupportedFormats()
		{
			return m_importer.GetSupportedImportFormats();
		}

		public CModelAsset LoadModelAsset(string filename, bool bAlwaysImport = false)
		{
			// Block new loads and wait for our current load to finish
			m_bIsLoading = true;
			m_currentLoadTask?.Wait();

			// Load the asset synchronous
			CModelAsset outAsset = new CModelAsset();
			if (!bAlwaysImport && TryGetExistingAsset(filename, "Assets/Models", outAsset, out CModelAsset existingAsset))
			{
				m_bIsLoading = false;
				return existingAsset;
			}

			Scene scene = m_importer.ImportFile(filename, PostProcessPreset.TargetRealTimeMaximumQuality | PostProcessPreset.ConvertToLeftHanded);
			if (scene.HasMeshes)
			{
				LoadModelInternal(filename, scene, outAsset, "Assets/");
			}

			outAsset.Name = Path.GetFileNameWithoutExtension(filename);
			if (CAssetRegistry.Instance.RequestRegisterAsset(outAsset, "Assets/Models/", out CModelAsset existingModel, bAlwaysImport))
			{
				existingModel.WaitUntilLoaded();
				outAsset.CopyFrom(existingModel);
			}

			outAsset.LoadFinished();
			// Unblock loader
			m_bIsLoading = false;
			// Continue loading request
			StartNextLoadRequest();

			return outAsset;
		}

		public CMeshAsset LoadMeshAsset(string filename, bool bAlwaysImport = false)
		{
			// Block new loads and wait for our current load to finish
			m_bIsLoading = true;
			m_currentLoadTask?.Wait();

			// Load the asset synchronous
			CMeshAsset outAsset = new CMeshAsset();
			if (!bAlwaysImport && TryGetExistingAsset(filename, "Assets/", outAsset, out CMeshAsset existingAsset))
			{
				m_bIsLoading = false;
				return existingAsset;
			}

			Scene scene = m_importer.ImportFile(filename, PostProcessPreset.TargetRealTimeMaximumQuality | PostProcessPreset.ConvertToLeftHanded);
			if (scene.HasMeshes)
			{
				string basePath = Path.GetDirectoryName(filename);
				CMeshLoadingJob loadingJob = new CMeshLoadingJob(scene, basePath);
				LoadMeshInternal(0, outAsset, loadingJob, "Assets/", Path.GetFileNameWithoutExtension(filename), bAlwaysImport);
			}

			outAsset.LoadFinished();
			// Unblock loader
			m_bIsLoading = false;
			// Continue loading request
			StartNextLoadRequest();

			return outAsset;
		}

		public CModelAsset LoadModelAsync(string filename, bool bAlwaysImport = false)
		{
			CModelAsset outAsset = new CModelAsset();
			if (!bAlwaysImport && TryGetExistingAsset(filename, "Assets/Models/", outAsset, out CModelAsset existingAsset))
			{
				return existingAsset;
			}

			SAssetLoadRequest loadRequest = new SAssetLoadRequest()
			{
				filename = filename,
				targetAsset = outAsset,
				assetPath = "Assets/",
				type = EAssetType.Model
			};

			m_loadRequests.Enqueue(loadRequest);			
			StartNextLoadRequest();
			return outAsset;
		}

		public CMeshAsset LoadMeshAsync(string filename, bool bAlwaysImport = false)
		{
			CMeshAsset outAsset = new CMeshAsset();
			if (!bAlwaysImport && TryGetExistingAsset(filename, "Assets/", outAsset, out CMeshAsset existingAsset))
			{
				return existingAsset;
			}

			SAssetLoadRequest loadRequest = new SAssetLoadRequest()
			{
				filename = filename,
				targetAsset = outAsset,
				assetPath = "Assets/",
				type = EAssetType.Mesh
			};

			m_loadRequests.Enqueue(loadRequest);
			StartNextLoadRequest();
			return outAsset;
		}

		private void LoadModelInternal(string filename, Scene scene, CModelAsset asset, string assetPath)
		{
			string modelPath = Path.GetDirectoryName(filename);
			Matrix identity = Matrix.Identity;
			CMeshLoadingJob loadingJob = new CMeshLoadingJob(scene, modelPath);
			loadingJob.LoadedTextures = new Dictionary<string, CTextureAsset>();
			loadingJob.LoadedMaterials = new Dictionary<string, CMaterialAsset>();

			AddMeshes(scene.RootNode, asset, assetPath, loadingJob, ref identity);
		}

		private void AddMeshes(Node node, CModelAsset asset, string assetPath, CMeshLoadingJob loadingJob, ref Matrix transform)
		{
			Matrix previousTransform = transform;
			transform = Matrix.Multiply(previousTransform, FromAssimpMatrix(node.Transform));

			if (node.HasMeshes)
			{
				foreach (int meshIndex in node.MeshIndices)
				{
					CMeshAsset meshAsset = new CMeshAsset();

					// We always import all meshes in a file so we use always import here
					LoadMeshInternal(meshIndex, meshAsset, loadingJob, assetPath, null, true);
					meshAsset.LoadFinished();

					SMeshChild modelChild = new SMeshChild()
					{
						meshAsset = meshAsset,
						relativeTransform = transform
					};

					asset.MeshChildren.Add(modelChild);
				}
			}

			for (int i = 0; i < node.ChildCount; i++)
			{
				AddMeshes(node.Children[i], asset, assetPath, loadingJob, ref transform);
			}

			transform = previousTransform;
		}

		private void LoadMeshInternal(int meshIndex, CMeshAsset asset, CMeshLoadingJob loadingJob, string assetPath, string nameOverride = null, bool bAlwaysImport = false)
		{
			Assimp.Mesh assimpMesh = loadingJob.Scene.Meshes[meshIndex];

			// Load texture and material from the file if present
			//todo henning extract more textures
			Material material = loadingJob.Scene.Materials[assimpMesh.MaterialIndex];
			if (material != null && material.GetMaterialTextureCount(TextureType.Diffuse) > 0)
			{
				if (material.GetMaterialTexture(TextureType.Diffuse, 0, out TextureSlot texture))
				{
					if (loadingJob.LoadedMaterials == null || !loadingJob.LoadedMaterials.TryGetValue(material.Name, out CMaterialAsset materialAsset))
					{
						materialAsset = new CMaterialAsset();
						loadingJob.LoadedMaterials?.Add(material.Name, materialAsset);
						// Make sure we only load each referenced texture once
						if (loadingJob.LoadedTextures == null || !loadingJob.LoadedTextures.TryGetValue(texture.FilePath, out CTextureAsset textureAsset))
						{
							textureAsset = CImportManager.Instance.TextureImporter.ImportTextureAsync(loadingJob.BasePath + "\\" + texture.FilePath, assetPath + "Textures/");
							loadingJob.LoadedTextures?.Add(texture.FilePath, textureAsset);
						}

						SShaderParameter textureParameter = new SShaderParameter()
						{
							parameterData = new CAssetReference<CTextureAsset>(textureAsset),
							parameterType = EShaderParameterType.Texture
						};
						materialAsset.MaterialParameters.Add(new SMaterialParameterEntry(new SHashedName("DiffuseTexture"), textureParameter));

						materialAsset.Name = material.Name;
						if (CAssetRegistry.Instance.RequestRegisterAsset(materialAsset, assetPath + "Materials/", out CMaterialAsset existingMaterial))
						{
							existingMaterial.WaitUntilLoaded();
							existingMaterial.CopyFrom(existingMaterial);
						}
						materialAsset.LoadFinished();
					}
					asset.MaterialAsset = materialAsset;
				}
			}

			bool hasTexCoords = assimpMesh.HasTextureCoords(0);
			bool hasColors = assimpMesh.HasVertexColors(0);
			bool hasNormals = assimpMesh.HasNormals;
			bool hasTangents = assimpMesh.Tangents != null && assimpMesh.Tangents.Count > 0;
			bool hasBiTangents = assimpMesh.BiTangents != null && assimpMesh.BiTangents.Count > 0;

			switch (assimpMesh.PrimitiveType)
			{
				case PrimitiveType.Point:
					asset.PrimitiveTopology = PrimitiveTopology.PointList;
					break;
				case PrimitiveType.Line:
					asset.PrimitiveTopology = PrimitiveTopology.LineList;
					break;
				case PrimitiveType.Triangle:
					asset.PrimitiveTopology = PrimitiveTopology.TriangleList;
					break;
				default:
					throw new ArgumentOutOfRangeException("Primtive Type not supported: " + assimpMesh.PrimitiveType.ToString());
			}

			asset.FaceCount = assimpMesh.FaceCount;
			asset.VertexData = new SVertexInfo[assimpMesh.VertexCount];

			Vector3 boundingBoxMin = new Vector3(1e10f, 1e10f, 1e10f);
			Vector3 boundingBoxMax = new Vector3(-1e10f, -1e10f, -1e10f);

			for (int i = 0; i < assimpMesh.VertexCount; i++)
			{
				SVertexInfo vertexInfo = new SVertexInfo();
				vertexInfo.position = FromAssimpVector(assimpMesh.Vertices[i]);
				boundingBoxMin.X = Math.Min(vertexInfo.position.X, boundingBoxMin.X);
				boundingBoxMin.Y = Math.Min(vertexInfo.position.Y, boundingBoxMin.Y);
				boundingBoxMin.Z = Math.Min(vertexInfo.position.Z, boundingBoxMin.Z);

				boundingBoxMax.X = Math.Max(vertexInfo.position.X, boundingBoxMax.X);
				boundingBoxMax.Y = Math.Max(vertexInfo.position.Y, boundingBoxMax.Y);
				boundingBoxMax.Z = Math.Max(vertexInfo.position.Z, boundingBoxMax.Z);

				if (hasColors)
				{
					vertexInfo.color = FromAssimpColor(assimpMesh.VertexColorChannels[0][i]);
				}
				else
				{
					vertexInfo.color = Vector4.One;
				}

				if (hasNormals)
				{
					vertexInfo.normal = FromAssimpVector(assimpMesh.Normals[i]);
				}

				if (hasBiTangents)
				{
					vertexInfo.biTangent = FromAssimpVector(assimpMesh.BiTangents[i]);
				}

				if (hasTangents)
				{
					vertexInfo.tangent = FromAssimpVector(assimpMesh.Tangents[i]);
				}

				if (hasTexCoords)
				{
					Vector3D assimpTexCoord = assimpMesh.TextureCoordinateChannels[0][i];
					vertexInfo.texCoord = new Vector2(assimpTexCoord.X, assimpTexCoord.Y);
				}

				asset.VertexData[i] = vertexInfo;
			}

			asset.AABBMin = boundingBoxMin;
			asset.AABBMax = boundingBoxMax;
			asset.IndexData = assimpMesh.GetIndices();
			asset.Name = nameOverride ?? assimpMesh.Name;
			if(CAssetRegistry.Instance.RequestRegisterAsset(asset, assetPath, out CMeshAsset existingAsset, true))
			{
				existingAsset.WaitUntilLoaded();
				asset.CopyFrom(existingAsset);
			}
		}

		private static bool TryGetExistingAsset<T>(string filename, string importDirectory, T baseAsset, out T existingAsset) where T : CAsset, new()
		{
			string assetExtension = baseAsset.GetFileExtension();
			string assetFilePath = FileUtilities.GetAssetFilenameFromSource(filename, importDirectory, assetExtension);
			return CAssetRegistry.Instance.TryGetAssetByFilename(assetFilePath, out existingAsset);
		}

		private void AssetLoadedCallback(SAssetLoadRequest finishedRequest)
		{
			m_bIsLoading = false;
			finishedRequest.targetAsset.LoadFinished();
		}

		private void ProcessLoadRequest(SAssetLoadRequest loadRequest)
		{
			Scene scene = m_importer.ImportFile(loadRequest.filename, PostProcessPreset.TargetRealTimeMaximumQuality | PostProcessPreset.ConvertToLeftHanded);
			if (loadRequest.type == EAssetType.Mesh)
			{
				if (scene.HasMeshes)
				{
					CMeshAsset meshAsset = (CMeshAsset) loadRequest.targetAsset;
					CMeshLoadingJob loadingJob = new CMeshLoadingJob(scene, Path.GetDirectoryName(loadRequest.filename));
					LoadMeshInternal(0, meshAsset, loadingJob, loadRequest.assetPath, Path.GetFileNameWithoutExtension(loadRequest.filename));
				}
			}
			else if(loadRequest.type == EAssetType.Model)
			{
				if (scene.HasMeshes)
				{
					CModelAsset modelAsset = (CModelAsset) loadRequest.targetAsset;
					LoadModelInternal(loadRequest.filename, scene, modelAsset, loadRequest.assetPath);
					modelAsset.Name = Path.GetFileNameWithoutExtension(loadRequest.filename);
					if (CAssetRegistry.Instance.RequestRegisterAsset(modelAsset, loadRequest.assetPath + "Models/", out CModelAsset existingAsset))
					{
						existingAsset.WaitUntilLoaded();
						modelAsset.CopyFrom(existingAsset);
					}
				}
			}
			AssetLoadedCallback(loadRequest);
		}

		private void StartNextLoadRequest()
		{
			if (!m_bIsLoading && m_loadRequests.TryDequeue(out SAssetLoadRequest request))
			{
				m_bIsLoading = true;
				m_currentLoadTask = Task.Run(() => ProcessLoadRequest(request)).ContinueWith((x) => StartNextLoadRequest());
			}
		}

		public void Dispose()
		{
			m_importer.Dispose();
		}

		private readonly ConcurrentQueue<SAssetLoadRequest> m_loadRequests = new ConcurrentQueue<SAssetLoadRequest>();
		private bool m_bIsLoading;
		private AssimpContext m_importer;
		private Task m_currentLoadTask;


		// Conversion Helpers
		public static Matrix FromAssimpMatrix(Matrix4x4 matrix)
		{
			Matrix outMat;
			outMat.M11 = matrix.A1;
			outMat.M12 = matrix.A2;
			outMat.M13 = matrix.A3;
			outMat.M14 = matrix.A4;
			outMat.M21 = matrix.B1;
			outMat.M22 = matrix.B2;
			outMat.M23 = matrix.B3;
			outMat.M24 = matrix.B4;
			outMat.M31 = matrix.C1;
			outMat.M32 = matrix.C2;
			outMat.M33 = matrix.C3;
			outMat.M34 = matrix.C4;
			outMat.M41 = matrix.D1;
			outMat.M42 = matrix.D2;
			outMat.M43 = matrix.D3;
			outMat.M44 = matrix.D4;

			outMat.Transpose();
			return outMat;
		}

		public static Vector3 FromAssimpVector(Vector3D vec)
		{
			Vector3 outVec;
			outVec.X = vec.X;
			outVec.Y = vec.Y;
			outVec.Z = vec.Z;
			return outVec;
		}

		public static Vector4 FromAssimpColor(Color4D col)
		{
			Vector4 outCol;
			outCol.X = col.R;
			outCol.Y = col.G;
			outCol.Z = col.B;
			outCol.W = col.A;
			return outCol;
		}
	}
}
