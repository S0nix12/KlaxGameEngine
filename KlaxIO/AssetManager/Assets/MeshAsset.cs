using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BEPUphysics.CollisionShapes;
using BEPUutilities;
using KlaxIO.AssetManager.Serialization.TypeConverters;
using KlaxMath;
using Newtonsoft.Json;
using SharpDX;
using SharpDX.Direct3D;
using Vector2 = SharpDX.Vector2;
using Vector3 = SharpDX.Vector3;
using Vector4 = SharpDX.Vector4;

namespace KlaxIO.AssetManager.Assets
{
	[StructLayout(LayoutKind.Sequential)]
	public struct SVertexInfo
	{
		public Vector3 position;
		public Vector4 color;
		public Vector3 normal;
		public Vector3 tangent;
		public Vector3 biTangent;
		public Vector2 texCoord;
	}

	public class CMeshAsset : CAsset
	{
		public const string FILE_EXTENSION = ".klaxmesh";
		public const string TYPE_NAME = "Mesh";
		public static readonly Color4 TYPE_COLOR = new Color(20, 187, 195).ToColor4();

		public override string GetFileExtension()
		{
			return FILE_EXTENSION;
		}

		public override string GetTypeName()
		{
			return TYPE_NAME;
		}

		public override Color4 GetTypeColor()
		{
			return TYPE_COLOR;
		}

		public override void Dispose()
		{

		}

		public override void CopyFrom(CAsset source)
		{
			base.CopyFrom(source);
			CMeshAsset meshSource = (CMeshAsset) source;
			PrimitiveTopology = meshSource.PrimitiveTopology;
			FaceCount = meshSource.FaceCount;
			VertexData = meshSource.VertexData;
			IndexData = meshSource.IndexData;
			MaterialAsset = meshSource.MaterialAsset;
			AABBMin = meshSource.AABBMin;
			AABBMax = meshSource.AABBMax;
		}

		public InstancedMeshShape GetPhysicsInstancedMesh()
		{
			System.Diagnostics.Debug.Assert(IsLoaded);
			if (m_physicsInstancedMeshShape != null)
			{
				return m_physicsInstancedMeshShape;
			}

			// The first time the physics mesh is queried we need to create it from our vertex data	
			CreatePhysics();
			return m_physicsInstancedMeshShape;
		}

		private void CreatePhysics()
		{
			BEPUutilities.Vector3[] vertices = new BEPUutilities.Vector3[VertexData.Length];
			for (int i = 0; i < VertexData.Length; i++)
			{
				vertices[i] = VertexData[i].position.ToBepu();
			}

			m_physicsInstancedMeshShape = new InstancedMeshShape(vertices, IndexData);			
		}

		[JsonProperty]
		public PrimitiveTopology PrimitiveTopology { get; internal set; }
		[JsonProperty]
		public int FaceCount { get; internal set; }
		[JsonProperty]
		public SVertexInfo[] VertexData { get; internal set; }
		[JsonProperty]
		public int[] IndexData { get; internal set; }
		[JsonProperty]
		public CAssetReference<CMaterialAsset> MaterialAsset { get; internal set; }
		[JsonProperty]
		public Vector3 AABBMin { get; internal set; }
		[JsonProperty]
		public Vector3 AABBMax { get; internal set; }

		private InstancedMeshShape m_physicsInstancedMeshShape;
	}
}
