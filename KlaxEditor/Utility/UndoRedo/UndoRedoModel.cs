using System.Collections.Generic;
using System;
using System.Linq;
using KlaxEditor.Utility.UndoRedo;

public class CUndoRedoModel
{
	public CUndoRedoModel()
	{
		UndoStack = new Stack<CUndoItem>(16);
		RedoStack = new Stack<CUndoItem>(16);

		if (UndoRedoUtility.Instance == null)
		{
			UndoRedoUtility.Instance = this;
		}
	}

	public void Record(CUndoItem item)
	{
		if (!IsRecording)
		{
			return;
		}

		lock (m_lockObject)
		{
			if (m_currentUndoGroup != null)
			{
				m_currentUndoGroup.Items.Add(item);
				return;
			}

			UndoStack.Push(item);
			RedoStack.Clear();

			OnItemRecorded?.Invoke(this, item);
		}
	}

	public bool CanUndo
	{
		get { lock (m_lockObject) { return UndoStack.Count > 0 && UndoStack.Peek().CanUndo(); } }
	}

	public bool CanRedo
	{
		get { lock (m_lockObject) { return RedoStack.Count > 0 && RedoStack.Peek().CanRedo(); } }
	}

	public void PushGroup()
	{
		lock (m_lockObject)
		{
			if (m_currentUndoGroup != null)
			{
				throw new Exception("There is already a pushed group. Make sure to only push one group at once.");
			}

			m_currentUndoGroup = new CUndoItemGroup();
		}
	}

	public void PopGroup()
	{
		lock (m_lockObject)
		{
			CUndoItem groupCache = m_currentUndoGroup;
			m_currentUndoGroup = null;
			Record(groupCache);
		}
	}

	public void Purge(IUndoContext context)
	{
		lock (m_lockObject)
		{
			if (context == null)
			{
				UndoStack = new Stack<CUndoItem>(64);
				RedoStack = new Stack<CUndoItem>(64);
			}
			else
			{
				UndoStack = new Stack<CUndoItem>(UndoStack.Where(item => item.UndoContext != context));
				RedoStack = new Stack<CUndoItem>(RedoStack.Where(item => item.UndoContext != context));
			}
		}
	}

	public void Redo()
	{
		lock (m_lockObject)
		{
			bool bWasRecording = IsRecording;
			IsRecording = false;

			CUndoItem redoItem = RedoStack.Pop();
			redoItem.Redo();
			UndoStack.Push(redoItem);

			IsRecording = bWasRecording;

			OnItemRedone?.Invoke(this, redoItem);
		}
	}

	public void Undo()
	{
		lock (m_lockObject)
		{
			bool bWasRecording = IsRecording;
			IsRecording = false;

			CUndoItem undoItem = UndoStack.Pop();
			undoItem.Undo();
			RedoStack.Push(undoItem);

			IsRecording = bWasRecording;

			OnItemUndone?.Invoke(this, undoItem);
		}
	}

	public Stack<CUndoItem> UndoStack { get; private set; }
	public Stack<CUndoItem> RedoStack { get; private set; }

	public bool IsRecording { get; set; } = true;

	public event EventHandler<CUndoItem> OnItemRecorded;
	public event EventHandler<CUndoItem> OnItemUndone;
	public event EventHandler<CUndoItem> OnItemRedone;

	private CUndoItemGroup m_currentUndoGroup;
	private object m_lockObject = new object();
}
