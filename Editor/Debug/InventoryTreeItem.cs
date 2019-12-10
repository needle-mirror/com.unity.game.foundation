using UnityEditor.IMGUI.Controls;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    internal class InventoryTreeItem : TreeViewItem
    {
        public StatDefinition.StatValueType statType { get; set; }
        public int itemHash { get; set; }
        public object statValue { get; set; }

        internal InventoryTreeItem(int id, int depth, string displayName) : base(id, depth, displayName)
        {
        }
    }
}
