using Assimp;
using KlaxShared;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using KlaxRenderer.Debug;
using KlaxRenderer.Graphics.Shader;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace KlaxRenderer.Graphics
{
	class ModelLoader : IDisposable
	{

		[StructLayout(LayoutKind.Sequential)]
		protected struct VertexType
		{
			public Vector3 position;
			public Vector2 texCoord;
		};
		// Fields
		Device m_device;
		AssimpContext m_importer;

		public ModelLoader(Device device)
		{
			m_device = device;
			m_importer = new AssimpContext();			
		}

		public CModel LoadModelFromFile(string fileName, SHashedName shaderName = new SHashedName())
		{
			if (m_device.IsDisposed)
			{
				throw new Exception("Trying to load a model, but our device was disposed");
			}

			Assimp.Scene scene = m_importer.ImportFile(fileName, PostProcessPreset.TargetRealTimeMaximumQuality | PostProcessPreset.ConvertToLeftHanded);
			string modelPath = Path.GetDirectoryName(fileName);
			Matrix identity = Matrix.Identity;

			CModel model = new CModel();
			CShaderResource shaderResource = CRenderer.Instance.ResourceManager.DefaultShader;
			if (!shaderName.IsEmpty())
			{
				shaderResource = CRenderer.Instance.ResourceManager.RequestShaderResource(shaderName);
			}

			AddVertexData(model, scene, scene.RootNode, m_device, ref identity, modelPath, shaderResource);
			ComputeBoundingBox(model, scene);

			return model;
		}

		internal void LoadPrimitveFromFile(string fileName, out DebugVertexType[] vertices, out UInt16[] indices)
		{
			Assimp.Scene scene = m_importer.ImportFile(fileName, PostProcessPreset.TargetRealTimeMaximumQuality | PostProcessPreset.ConvertToLeftHanded);
			// Loaded primitive files should always only contain a single mesh so we load only the first mesh
			if (scene.HasMeshes)
			{
				Assimp.Mesh assimpMesh = scene.Meshes[0];
				vertices = new DebugVertexType[assimpMesh.Vertices.Count];
				indices = new UInt16[assimpMesh.GetIndices().Length];

				for (int i = 0; i < assimpMesh.Vertices.Count; ++i)
				{
					Vector3 position = FromAssimpVector(assimpMesh.Vertices[i]);
					vertices[i].position = position;
				}

				for (int i = 0; i < assimpMesh.GetIndices().Length; i++)
				{
					int index = assimpMesh.GetIndices()[i];
					indices[i] = (UInt16) index;
				}
			}
			else
			{
				vertices = new DebugVertexType[0];
				indices = new UInt16[0];
			}
		}

		private void AddVertexData(CModel model, Assimp.Scene scene, Node node, Device device, ref Matrix transform, string modelPath, CShaderResource shaderResource)
		{
			// TODO henning With this we don't preserve hiearchy in the models meshes, for now this is the wanted behavior but maybe we want to think about this in the future
			Matrix previousTransform = transform;
			transform = Matrix.Multiply(previousTransform, FromAssimpMatrix(node.Transform));			

			if (node.HasMeshes)
			{
				foreach (int index in node.MeshIndices)
				{
					// Get the mesh from the scene
					Assimp.Mesh assimpMesh = scene.Meshes[index];

					// Create a mesh in our engine format
					CMesh mesh = new CMesh();
					mesh.Transform.SetFromMatrix(in transform);
					model.AddMesh(ref mesh);

					//Extract diffuse texture from Assimp Material
					// TODO henning extract other textures if we want
					Material material = scene.Materials[assimpMesh.MaterialIndex];
					if (material != null && material.GetMaterialTextureCount(TextureType.Diffuse) > 0)
					{
						TextureSlot texture;
						if (material.GetMaterialTexture(TextureType.Diffuse, 0, out texture))
						{
							// Create texture asset
							mesh.Material = CMaterial.CreateDefaultMaterial();
							CTextureSampler textureSampler = new CTextureSampler(device, device.ImmediateContext, modelPath + "\\" + texture.FilePath);
							mesh.Material.SetTextureParameter(new SHashedName("DiffuseTexture"), textureSampler);
							mesh.Material.FinishLoading();
						}
						else
						{
							mesh.Material = CRenderer.Instance.ResourceManager.DefaultTextureMaterial;
						}
					}
					else
					{
						mesh.Material = CRenderer.Instance.ResourceManager.DefaultTextureMaterial;
					}
					mesh.Material.SetColorParameter(new SHashedName("tintColor"), new Vector4(1, 1, 1, 1));


					int numInputElements = 6; // We always provide all data to the vertex buffer so shaders can rely on them being available if it is not in the asset we will add a default value

					bool hasTexCoords = assimpMesh.HasTextureCoords(0);
					bool hasColors = assimpMesh.HasVertexColors(0);
					bool hasNormals = assimpMesh.HasNormals;
					bool hasTangents = assimpMesh.Tangents != null && assimpMesh.Tangents.Count > 0;
					bool hasBiTangents = assimpMesh.BiTangents != null && assimpMesh.BiTangents.Count > 0;

					// Create InputElement list
					InputElement[] vertexElements = new InputElement[numInputElements];
					uint elementIndex = 0;
					vertexElements[elementIndex] = new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0);
					elementIndex++;
					int vertexSize = Utilities.SizeOf<Vector3>();

					// TODO henning evaluate if we need 32bit vertex color range
					vertexElements[elementIndex] = new InputElement("COLOR", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 0);
					elementIndex++;
					vertexSize += Utilities.SizeOf<Vector4>();
					vertexElements[elementIndex] = new InputElement("NORMAL", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0);
					elementIndex++;
					vertexSize += Utilities.SizeOf<Vector3>();
					vertexElements[elementIndex] = new InputElement("TANGENT", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0);
					elementIndex++;
					vertexSize += Utilities.SizeOf<Vector3>();
					vertexElements[elementIndex] = new InputElement("BITANGENT", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0);
					elementIndex++;
					vertexSize += Utilities.SizeOf<Vector3>();
					vertexElements[elementIndex] = new InputElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 0);
					elementIndex++;
					vertexSize += Utilities.SizeOf<Vector2>();

					// Set InputElements and Vertex Size on mesh
					mesh.m_sizePerVertex = vertexSize;

					List<Vector3D> positions = assimpMesh.Vertices;
					List<Vector3D> texCoords = assimpMesh.TextureCoordinateChannels[0]; // TODO henning support multiple UV channels if wanted
					List<Vector3D> normals = assimpMesh.Normals;
					List<Vector3D> tangents = assimpMesh.Tangents;
					List<Vector3D> biTangents = assimpMesh.BiTangents;
					List<Color4D> colors = assimpMesh.VertexColorChannels[0];

					switch (assimpMesh.PrimitiveType)
					{
						case PrimitiveType.Point:
							mesh.m_primitiveTopology = SharpDX.Direct3D.PrimitiveTopology.PointList;
							break;
						case PrimitiveType.Line:
							mesh.m_primitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;
							break;
						case PrimitiveType.Triangle:
							mesh.m_primitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
							break;
						default:
							throw new NotImplementedException("Primitive Type not supported: " + assimpMesh.PrimitiveType.ToString());
					}

					DataStream vertexStream = new DataStream(assimpMesh.VertexCount * vertexSize, true, true);

					for (int i = 0; i < assimpMesh.VertexCount; i++)
					{
						//add position, after transforming it with accumulated node transform
						{
							Vector3 pos = FromAssimpVector(positions[i]);
							vertexStream.Write(pos);
						}

						if (hasColors)
						{
							Vector4 vertColor = FromAssimpColor(colors[i]);
							vertexStream.Write(vertColor);
						}
						else
						{
							vertexStream.Write(new Vector4(1,1,1,1));
						}
						if (hasNormals)
						{
							Vector3 normal = FromAssimpVector(normals[i]);
							vertexStream.Write(normal);
						}
						else
						{
							vertexStream.Write(new Vector3(0,0,0));
						}
						if (hasTangents)
						{
							Vector3 tangent = FromAssimpVector(tangents[i]);
							vertexStream.Write(tangent);
						}
						else
						{
							vertexStream.Write(new Vector3(0, 0, 0));
						}
						if (hasBiTangents)
						{
							Vector3 biTangent = FromAssimpVector(biTangents[i]);
							vertexStream.Write(biTangent);
						}
						else
						{
							vertexStream.Write(new Vector3(0, 0, 0));
						}
						if (hasTexCoords)
						{
							vertexStream.Write(new Vector2(texCoords[i].X, texCoords[i].Y));
						}
						else
						{
							vertexStream.Write(new Vector2(0, 0));
						}
					}

					vertexStream.Position = 0;
					BufferDescription vertexDescription = new BufferDescription
					{
						BindFlags = BindFlags.VertexBuffer,
						Usage = ResourceUsage.Default,
						CpuAccessFlags = CpuAccessFlags.None,
						SizeInBytes = assimpMesh.VertexCount * vertexSize,
						OptionFlags = ResourceOptionFlags.None
					};

					mesh.m_vertexBuffer = new Buffer(device, vertexStream, vertexDescription);
					vertexStream.Dispose();

					mesh.m_vertexCount = assimpMesh.VertexCount;
					mesh.m_primitiveCount = assimpMesh.FaceCount;


					int[] indices = assimpMesh.GetIndices();
					mesh.m_indexBuffer = Buffer.Create(device, BindFlags.IndexBuffer, indices);
					mesh.m_indexCount = indices.Length;
				}
			}

			for (int i = 0; i < node.ChildCount; i++)
			{
				AddVertexData(model, scene, node.Children[i], device, ref transform, modelPath, shaderResource);
			}

			transform = previousTransform;
		}

		//calculates the bounding box of the whole model
		private void ComputeBoundingBox(CModel model, Assimp.Scene scene)
		{
			Vector3 sceneMin = new Vector3(1e10f, 1e10f, 1e10f);
			Vector3 sceneMax = new Vector3(-1e10f, -1e10f, -1e10f);
			Matrix transform = Matrix.Identity;

			ComputeBoundingBox(scene, scene.RootNode, ref sceneMin, ref sceneMax, ref transform);

			//set min and max of bounding box
			model.SetAABox(sceneMin, sceneMax);
		}

		//recursively calculates the bounding box of the whole model
		private void ComputeBoundingBox(Assimp.Scene scene, Node node, ref Vector3 min, ref Vector3 max, ref Matrix transform)
		{
			Matrix previousTransform = transform;
			transform = Matrix.Multiply(previousTransform, FromAssimpMatrix(node.Transform));

			if (node.HasMeshes)
			{
				foreach (int index in node.MeshIndices)
				{
					Assimp.Mesh mesh = scene.Meshes[index];
					for (int i = 0; i < mesh.VertexCount; i++)
					{
						Vector3 tmp = FromAssimpVector(mesh.Vertices[i]);
						Vector4 result;
						Vector3.Transform(ref tmp, ref transform, out result);

						min.X = Math.Min(min.X, result.X);
						min.Y = Math.Min(min.Y, result.Y);
						min.Z = Math.Min(min.Z, result.Z);

						max.X = Math.Max(max.X, result.X);
						max.Y = Math.Max(max.Y, result.Y);
						max.Z = Math.Max(max.Z, result.Z);
					}
				}
			}

			//go down the hierarchy if children are present
			for (int i = 0; i < node.ChildCount; i++)
			{
				ComputeBoundingBox(scene, node.Children[i], ref min, ref max, ref transform);
			}
			transform = previousTransform;
		}

		public void Dispose()
		{
			m_importer.Dispose();
		}

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
