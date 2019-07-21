using System.Collections.Generic;

public class CUndoItemGroup : CUndoItem
{
    public List<CUndoItem> Items { get; }

    public CUndoItemGroup()
    {
        Items = new List<CUndoItem>(4);
    }

    public override bool CanRedo()
    {
        foreach (var item in Items)
        {
            if (!item.CanRedo())
            {
                return false;
            }
        }

        return true;
    }

    public override bool CanUndo()
    {
        foreach (var item in Items)
        {
            if (!item.CanUndo())
            {
                return false;
            }
        }

        return true;
    }

    public override void Redo()
    {
        foreach (var item in Items)
        {
            item.Redo();
        }
    }

    public override void Undo()
    {
        foreach (var item in Items)
        {
            item.Undo();
        }
    }
}
