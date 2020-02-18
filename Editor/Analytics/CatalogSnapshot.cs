using System;

namespace UnityEditor.GameFoundation
{
    [Serializable]
    class CatalogSnapshotContainer
    {
        public CatalogSnapshot CatalogSnapshot;
    }

    [Serializable]
    struct CatalogSnapshot : IEquatable<CatalogSnapshot>
    {
        public static bool operator ==(CatalogSnapshot a, CatalogSnapshot b) =>
            a.gameItemCount == b.gameItemCount &&
            a.inventoryItemCount == b.inventoryItemCount &&
            a.inventoryCount == b.inventoryCount &&
            a.categoryCount == b.categoryCount &&
            a.statCount == b.statCount;

        public static bool operator !=(CatalogSnapshot a, CatalogSnapshot b) =>
            a.gameItemCount != b.gameItemCount ||
            a.inventoryItemCount != b.inventoryItemCount ||
            a.inventoryCount != b.inventoryCount ||
            a.categoryCount != b.categoryCount ||
            a.statCount != b.statCount;


        public int gameItemCount;
        public int inventoryItemCount;
        public int inventoryCount;
        public int categoryCount;
        public int statCount;


        public bool Equals(CatalogSnapshot other) => this == other;

        public override bool Equals(object obj)
        {
            if(obj is CatalogSnapshot other)
            {
                return this == other;
            }
            return false;
        }

        public override int GetHashCode() =>
            (gameItemCount & 0b111111111) << 21 |      // 511 game items
            (inventoryItemCount & 0b111111111) << 12 | // 511 inventory items
            (inventoryCount & 0b1111) << 10 |          //  15 inventories
            (statCount & 0b111111) << 4 |              //  63 stats
            (categoryCount & 0b1111);                  //  15 categories
    }
}
