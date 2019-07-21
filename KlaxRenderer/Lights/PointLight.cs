using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assimp;
using KlaxRenderer.Graphics;
using SharpDX;

namespace KlaxRenderer.Lights
{
	public class CPointLight : CPositionalLight
	{
		public override void FillShaderData(ref SPositionalLightShaderData data)
		{
			data.Position = new Vector4(Transform.WorldPosition, 1.0f);
			data.Direction = new Vector4(Transform.Forward, 1.0f);
			data.Color = LightColor;
			data.Range = Range;
			data.ConstantAttenuation = ConstantAttenuation;
			data.LinearAttenuation = LinearAttenuation;
			data.QuadraticAttenuation = QuadraticAttenuation;
			data.LightType = LightType;
			data.bEnabled = Enabled ? 1 : 0;
		}

		public override ELightType GetLightType()
		{
			return ELightType.Point;
		}

		public bool IsAffectedByLight(CModel model)
		{
			BoundingSphere lightSphere = new BoundingSphere(Transform.Position, Range);
			BoundingBox meshBox = new BoundingBox(model.AABoxMin, model.AABoxMin);

			return lightSphere.Intersects(meshBox);
		}

		public const int LightType = 1;

		public Vector4 LightColor { get; set; }
		public float Range { get; set; }
		public float ConstantAttenuation { get; set; }
		public float LinearAttenuation { get; set; }
		public float QuadraticAttenuation { get; set; }
		public bool Enabled { get; set; } = true;
	}
}
