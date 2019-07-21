using System;

namespace KlaxShared.Attributes
{
	[AttributeUsage(AttributeTargets.Class)]
	public class KlaxComponentAttribute : KlaxScriptTypeAttribute
	{
		public bool HideInEditor { get; set; }
		public string Category { get; set; }
	}
}
