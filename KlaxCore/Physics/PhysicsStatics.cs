using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUphysics.CollisionShapes;
using BEPUutilities;

namespace KlaxCore.Physics
{
	public static class PhysicsStatics
	{
		public static InstancedMeshShape DummyInstancedMesh { get; } 
			= new InstancedMeshShape(new Vector3[] { new Vector3(0.01f, 0, 0), new Vector3(0, 0.01f, 0), new Vector3(0, 0, 0.01f) }, new int[] { 0, 1, 2 });
	}
}
