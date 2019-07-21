using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxCore.KlaxScript
{

	public delegate void KlaxScriptEventHandler(object[] parameters);
	public abstract class CKlaxScriptEventBase
	{
		public void Subscribe(KlaxScriptEventHandler handler)
		{
			if (!m_handlers.Contains(handler))
			{
				m_handlers.Add(handler);
			}
		}

		public void Unsubscribe(KlaxScriptEventHandler handler)
		{
			m_handlers.Remove(handler);
		}

		public bool HasHandlers()
		{
			return m_handlers.Count > 0;
		}

		protected readonly List<KlaxScriptEventHandler> m_handlers = new List<KlaxScriptEventHandler>();
	}
	public class CKlaxScriptEvent : CKlaxScriptEventBase
	{
		public void Invoke()
		{
			for (int i = 0; i < m_handlers.Count; i++)
			{
				m_handlers[i](null);
			}
		}
	}
	public class CKlaxScriptEvent<T> : CKlaxScriptEventBase
	{
		public void Invoke(T p1)
		{
			m_buffer[0] = p1;
			for (int i = 0; i < m_handlers.Count; i++)
			{
				m_handlers[i](m_buffer);
			}
		}

		private readonly object[] m_buffer = new object[1];
	}
	public class CKlaxScriptEvent<T1,T2> : CKlaxScriptEventBase
	{
		public void Invoke(T1 p1, T2 p2)
		{
			m_buffer[0] = p1;
			m_buffer[1] = p2;
			for (int i = 0; i < m_handlers.Count; i++)
			{
				m_handlers[i](m_buffer);
			}
		}

		private readonly object[] m_buffer = new object[2];
	}
	public class CKlaxScriptEvent<T1, T2, T3> : CKlaxScriptEventBase
	{
		public void Invoke(T1 p1, T2 p2, T3 p3)
		{
			m_buffer[0] = p1;
			m_buffer[1] = p2;
			m_buffer[2] = p3;
			for (int i = 0; i < m_handlers.Count; i++)
			{
				m_handlers[i](m_buffer);
			}
		}

		private readonly object[] m_buffer = new object[3];
	}
	public class CKlaxScriptEvent<T1, T2, T3, T4> : CKlaxScriptEventBase
	{
		public void Invoke(T1 p1, T2 p2, T3 p3, T4 p4)
		{
			m_buffer[0] = p1;
			m_buffer[1] = p2;
			m_buffer[2] = p3;
			m_buffer[3] = p4;
			for (int i = 0; i < m_handlers.Count; i++)
			{
				m_handlers[i](m_buffer);
			}
		}

		private readonly object[] m_buffer = new object[4];
	}
	public class CKlaxScriptEvent<T1, T2, T3, T4, T5> : CKlaxScriptEventBase
	{
		public void Invoke(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
		{
			m_buffer[0] = p1;
			m_buffer[1] = p2;
			m_buffer[2] = p3;
			m_buffer[3] = p4;
			m_buffer[4] = p5;
			for (int i = 0; i < m_handlers.Count; i++)
			{
				m_handlers[i](m_buffer);
			}
		}

		private readonly object[] m_buffer = new object[5];
	}
	public class CKlaxScriptEvent<T1, T2, T3, T4, T5, T6> : CKlaxScriptEventBase
	{
		public void Invoke(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6)
		{
			m_buffer[0] = p1;
			m_buffer[1] = p2;
			m_buffer[2] = p3;
			m_buffer[3] = p4;
			m_buffer[4] = p5;
			m_buffer[5] = p6;
			for (int i = 0; i < m_handlers.Count; i++)
			{
				m_handlers[i](m_buffer);
			}
		}

		private readonly object[] m_buffer = new object[6];
	}
	public class CKlaxScriptEvent<T1, T2, T3, T4, T5, T6, T7> : CKlaxScriptEventBase
	{
		public void Invoke(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7)
		{
			m_buffer[0] = p1;
			m_buffer[1] = p2;
			m_buffer[2] = p3;
			m_buffer[3] = p4;
			m_buffer[4] = p5;
			m_buffer[5] = p6;
			m_buffer[6] = p7;
			for (int i = 0; i < m_handlers.Count; i++)
			{
				m_handlers[i](m_buffer);
			}
		}

		private readonly object[] m_buffer = new object[7];
	}
	public class CKlaxScriptEvent<T1, T2, T3, T4, T5, T6, T7, T8> : CKlaxScriptEventBase
	{
		public void Invoke(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8)
		{
			m_buffer[0] = p1;
			m_buffer[1] = p2;
			m_buffer[2] = p3;
			m_buffer[3] = p4;
			m_buffer[4] = p5;
			m_buffer[5] = p6;
			m_buffer[6] = p7;
			m_buffer[7] = p8;
			for (int i = 0; i < m_handlers.Count; i++)
			{
				m_handlers[i](m_buffer);
			}
		}

		private readonly object[] m_buffer = new object[8];
	}
	public class CKlaxScriptEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9> : CKlaxScriptEventBase
	{
		public void Invoke(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9)
		{
			m_buffer[0] = p1;
			m_buffer[1] = p2;
			m_buffer[2] = p3;
			m_buffer[3] = p4;
			m_buffer[4] = p5;
			m_buffer[5] = p6;
			m_buffer[6] = p7;
			m_buffer[7] = p8;
			m_buffer[8] = p9;
			for (int i = 0; i < m_handlers.Count; i++)
			{
				m_handlers[i](m_buffer);
			}
		}

		private readonly object[] m_buffer = new object[9];
	}
	public class CKlaxScriptEvent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : CKlaxScriptEventBase
	{
		public void Invoke(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10)
		{
			m_buffer[0] = p1;
			m_buffer[1] = p2;
			m_buffer[2] = p3;
			m_buffer[3] = p4;
			m_buffer[4] = p5;
			m_buffer[5] = p6;
			m_buffer[6] = p7;
			m_buffer[7] = p8;
			m_buffer[8] = p9;
			m_buffer[9] = p10;
			for (int i = 0; i < m_handlers.Count; i++)
			{
				m_handlers[i](m_buffer);
			}
		}

		private readonly object[] m_buffer = new object[10];
	}
}
