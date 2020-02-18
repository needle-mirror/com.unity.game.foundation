namespace UnityEditor.GameFoundation
{
    internal interface ICollectionEditor
    {
        string name { get; }
        bool isCreating { get; }

        void Draw();
        void OnWillEnter();
        void OnWillExit();
        void ValidateSelection();
        void RefreshItems();
    }
}
