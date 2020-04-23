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
            a.inventoryItemCount == b.inventoryItemCount &&
            a.storeCount == b.storeCount &&
            a.statCount == b.statCount &&
            a.virtualTransactionCount == b.virtualTransactionCount &&
            a.iapTransactionCount == b.iapTransactionCount;

        public static bool operator !=(CatalogSnapshot a, CatalogSnapshot b) =>
            a.inventoryItemCount != b.inventoryItemCount ||
            a.storeCount != b.storeCount ||
            a.statCount != b.statCount ||
            a.virtualTransactionCount != b.virtualTransactionCount ||
            a.virtualTransactionCount != b.virtualTransactionCount;


        public int inventoryItemCount;
        public int storeCount;
        public int statCount;
        public int virtualTransactionCount;
        public int iapTransactionCount;


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
            (inventoryItemCount      & 0b11111111) << 24 |   //  255 stores           (8 bits)
            (storeCount              & 0b11111111) << 16 |   //  255 inventory items  (8 bits)
            (virtualTransactionCount & 0b1111    ) << 12 |   //   15 v transactions   (4 bits)
            (iapTransactionCount     & 0b1111    ) <<  8 |   //   15 iap transactions (4 bits)
            (statCount               & 0b11111111) <<  0     //  255 stats            (8 bits)
            ;
    }
}
