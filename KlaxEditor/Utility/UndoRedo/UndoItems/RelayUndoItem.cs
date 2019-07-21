using System;

namespace KlaxEditor.Utility.UndoRedo
{
    public class CRelayUndoItem : CUndoItem
    {
        public CRelayUndoItem(Action undo, Action redo, Func<bool> canUndo = null, Func<bool> canRedo = null)
        {
            m_undo = undo;
            m_redo = redo;
            m_canRedo = canRedo;
            m_canUndo = canUndo;
        }

        public override bool CanRedo()
        {
            if (m_canRedo != null)
            {
                return m_canRedo();
            }

            return true;
        }

        public override bool CanUndo()
        {
            if (m_canUndo != null)
            {
                return m_canUndo();
            }

            return true;
        }

        public override void Undo()
        {
            m_undo?.Invoke();
        }

        public override void Redo()
        {
            m_redo?.Invoke();
        }

        private readonly Action m_undo;
        private readonly Action m_redo;
        private readonly Func<bool> m_canUndo;
        private readonly Func<bool> m_canRedo;
    }
}
