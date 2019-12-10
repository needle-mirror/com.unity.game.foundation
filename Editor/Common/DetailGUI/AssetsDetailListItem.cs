namespace UnityEditor.GameFoundation
{
    internal struct AssetsDetailListItem
    {
        public int indexInOriginalList;
        public SerializedProperty keyProperty;
        public SerializedProperty valueProperty;

        public AssetsDetailListItem(int indexInOriginalList, SerializedProperty keyProperty, SerializedProperty valueProperty)
        {
            this.indexInOriginalList = indexInOriginalList;
            this.keyProperty = keyProperty;
            this.valueProperty = valueProperty;
        }
    }
}
