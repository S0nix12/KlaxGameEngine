using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxIO.Input
{
	public enum EButtonEvent
	{
		Pressed,
		Released
	}

	public struct SInputButtonEvent
	{
		public EButtonEvent buttonEvent;
		public EInputButton button;
	}
}
