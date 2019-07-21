public abstract class CUndoItem
{
    public abstract void Undo();
    public abstract void Redo();

    public virtual bool CanUndo()
    {
        return true;
    }

    public virtual bool CanRedo()
    {
        return true;
    }

    public IUndoContext UndoContext { get; set; }
}