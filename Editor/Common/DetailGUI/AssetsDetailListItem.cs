namespace UnityEditor.GameFoundation
{
    internal struct AssetsDetailListItem
    {
        public readonly int indexInOriginalList;
        public readonly SerializedProperty nameProperty;
        public readonly SerializedProperty valueProperty;

        public AssetsDetailListItem(int indexInOriginalList, SerializedProperty nameProperty, SerializedProperty valueProperty)
        {
            this.indexInOriginalList = indexInOriginalList;
            this.nameProperty = nameProperty;
            this.valueProperty = valueProperty;
        }
    }
}
