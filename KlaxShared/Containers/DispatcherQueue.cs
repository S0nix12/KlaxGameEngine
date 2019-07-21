using System;
using TActionQueue = System.Collections.Generic.List<System.Action>;

namespace KlaxShared.Containers
{
    public class DispatcherQueue
    {
        public DispatcherQueue(int maxPriority)
        {
            m_lock = new object();
            m_actions = new TActionQueue[maxPriority];
            for (int i = 0; i < m_actions.Length; i++)
            {
                m_actions[i] = new TActionQueue(16);
            }
        }

        public void Add(int priority, Action action)
        {
			TActionQueue queue = m_actions[priority];
            lock (m_lock)
            {
                queue.Add(action);
            }
        }

        public void Execute(int priority)
        {
            TActionQueue queue = m_actions[priority];
            int count = queue.Count;

            if (count > 0)
            {
                lock (m_lock)
                {
                    int lockedCount = queue.Count;
                    for (int i = 0; i < lockedCount; i++)
                    {
                        m_actionsToExecute.Add(queue[i]);
                    }
                    queue.Clear();
                }

                int copyCount = m_actionsToExecute.Count;
                for (int i = 0; i < m_actionsToExecute.Count; i++)
                {
                    m_actionsToExecute[i]();
                }

                m_actionsToExecute.Clear();
            }
        }

        private TActionQueue[] m_actions;
        private TActionQueue m_actionsToExecute = new TActionQueue(16);
        private readonly object m_lock;
    }
}
