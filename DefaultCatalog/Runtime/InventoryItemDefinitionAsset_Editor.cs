#if UNITY_EDITOR

using UnityEditor;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public partial class InventoryItemDefinitionAsset
    {
        /// <inheritdoc/>
        internal override string Editor_AssetPrefix => "Item";

        protected override void OnItemDestroy()
        {
            if (catalog is InventoryCatalogAsset inventoryCatalogAsset)
            {
                //Debug.Log($"Removing {displayName} ({GetType().Name}) from {catalog}");

                inventoryCatalogAsset.Editor_RemoveItem(this);
            }
        }

        /// <summary>
        ///     Adds a property to the definition of this <see cref="InventoryItemDefinitionAsset"/> instance.
        /// </summary>
        /// <param name="name">
        ///     The name of the new property.
        /// </param>
        /// <param name="defaultValue">
        ///     The default value of this new property.
        ///     Also defines its type.
        /// </param>
        /// <returns>
        ///     <c>true</c> if created, <c>false</c> otherwise.
        /// </returns>
        internal bool Editor_AddProperty(string name, Property defaultValue)
        {
            if (properties.ContainsKey(name))
            {
                return false;
            }

            properties.Add(name, defaultValue);
            EditorUtility.SetDirty(this);
            return true;
        }
    }
}

#endif
