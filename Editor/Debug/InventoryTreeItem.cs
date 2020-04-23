using UnityEditor.IMGUI.Controls;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    internal sealed class InventoryItemView : TreeViewItem
    {
        public readonly InventoryItem inventoryItem;

        public InventoryItemView
            (int id, int depth, string displayName, InventoryItem inventoryItem)
            : base(id, depth, displayName)
        {
            this.inventoryItem = inventoryItem;
        }

        public override string ToString()
        {
            return $"{nameof(InventoryItemView)} item:{inventoryItem.definition.id}";
        }
    }

    internal sealed class StatView : TreeViewItem
    {
        public readonly InventoryItem inventoryItem;

        public readonly StatDefinition statDefinition;

        public StatView(
            int id, int depth, string displayName,
            InventoryItem inventoryItem,
            StatDefinition statDefinition)
            : base(id, depth, displayName)
        {
            this.inventoryItem = inventoryItem;
            this.statDefinition = statDefinition;
        }

        public override string ToString()
        {
            return $"{nameof(StatView)} item:{inventoryItem.definition.id}, stat:{statDefinition.id}";
        }
    }

    internal sealed class CurrencyView : TreeViewItem
    {
        public readonly Currency currency;

        public CurrencyView(
            int id, int depth, string displayName,
            Currency currency)
            : base(id, depth, displayName)
        {
            this.currency = currency;
        }

        public override string ToString()
        {
            return $"{nameof(CurrencyView)} id:{currency.id}";
        }
    }
}
