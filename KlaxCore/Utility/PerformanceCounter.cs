using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxShared.Attributes;
using SharpDX;

namespace KlaxCore.Utility
{
	class CPerformanceCounter
	{
		[CVar("c_FrametimeSmoothing")]
		private static float Smoothing { get; set; } = 0.6f;
		public void UpdateFrametime(float deltaTime)
		{
			float smoothFactor = 1 - MathUtil.Clamp(SmoothedFrametime * 10, 0.0002f, 1);
			float invert = 1 - Smoothing;
			float fullSmoothing = Smoothing + invert * smoothFactor;
			SmoothedFrametime = SmoothedFrametime * fullSmoothing + deltaTime * (1 - fullSmoothing);
		}

		public float SmoothedFrametime { get; set; }
	}
}
