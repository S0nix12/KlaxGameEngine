namespace KlaxEditor.Utility.UndoRedo
{
    public static class UndoRedoUtility
    {
        public static CUndoRedoModel Instance { get; set; }

        public static void Record(CUndoItem item)
        {
            Instance?.Record(item);
        }

        public static void Undo()
        {
            Instance?.Undo();
        }

        public static void Redo()
        {
            Instance?.Redo();
        }

        public static void Purge(IUndoContext context)
        {
            Instance?.Purge(context);
        }

        public static void PushGroup()
        {
            Instance?.PushGroup();
        }

        public static void PopGroup()
        {
            Instance?.PopGroup();
        }

        public static bool CanRedo
        {
            get { return Instance != null ? Instance.CanRedo : false; }
        }

        public static bool CanUndo
        {
            get { return Instance != null ? Instance.CanUndo : false; }
        }

        public static bool IsRecording
        {
            get { return Instance != null ? Instance.IsRecording : false; }
            set { if (Instance != null) Instance.IsRecording = value; }
        }
    }
}
