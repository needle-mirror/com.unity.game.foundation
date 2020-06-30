using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.DefaultCatalog.Details;
using UnityEngine.Serialization;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.GameFoundation.DefaultCatalog;

#endif

namespace UnityEngine.GameFoundation.UI
{
    /// <summary>
    /// Component that manages displaying a Inventory Item's icon and quantity.
    /// When attached to a game object, it will display the Inventory Item's icon and quantity.
    /// </summary>
    [AddComponentMenu("Game Foundation/Inventory Item Hud", 5)]
    [ExecuteInEditMode]
    public class InventoryItemHudView : MonoBehaviour
    {
        /// <summary>
        /// The identifier of the Inventory Item to display.
        /// </summary>
        public string itemDefinitionKey => m_ItemDefinitionKey;

        /// <inheritdoc cref="itemDefinitionKey"/>
        [Obsolete("Use 'itemDefinitionKey' instead", false)]
        public string itemDefinitionId => itemDefinitionKey;

        [SerializeField, FormerlySerializedAs("m_ItemDefinitionId")]
        internal string m_ItemDefinitionKey;

        /// <summary>
        /// The sprite name for Inventory item icon that will be displayed on this view.
        /// </summary>
        public string iconSpriteName => m_IconSpriteName;

        [SerializeField]
        internal string m_IconSpriteName = "hud_icon";

        /// <summary>
        /// The Image component to assign the Inventory item icon image to.
        /// </summary>
        public Image iconImageField => m_IconImageField;

        [SerializeField]
        internal Image m_IconImageField;

        /// <summary>
        /// The Text component to assign the Inventory item quantity to.
        /// </summary>
        public Text quantityTextField => m_QuantityTextField;

        [SerializeField]
        internal Text m_QuantityTextField;

        /// <summary>
        /// The List of Inventory items <see cref="InventoryItem"/> that has the same definition key to cache them.
        /// </summary>
        readonly List<InventoryItem> m_TempItemList = new List<InventoryItem>();

        /// <summary>
        /// The Quantity of the Inventory item.
        /// </summary>
        long m_Quantity;
        
        /// <summary>
        /// Specifies whether the GameObject fields on the editor is visible.
        /// </summary>
        [SerializeField]
        internal bool showGameObjectEditorFields = true;

        void OnEnable()
        {
            if (Application.isPlaying)
            {
                InventoryManager.itemAdded += OnItemAdded;
                InventoryManager.itemRemoved += OnItemRemoved;
            }
        }

        void OnDisable()
        {
            if (Application.isPlaying)
            {
                InventoryManager.itemAdded -= OnItemAdded;
                InventoryManager.itemRemoved -= OnItemRemoved;
            }
        }

        /// <summary>
        /// Initializes InventoryItemHudView with needed info.
        /// </summary>
        /// <param name="inventoryItemKey">The InventoryItem key to be displayed.</param>
        /// <param name="priceIconSpriteName">The sprite name for item icon that will be displayed on this view.</param>
        internal void Init(string inventoryItemKey, string priceIconSpriteName)
        {
            if (string.IsNullOrEmpty(inventoryItemKey))
            {
                return;
            }
            
            m_IconSpriteName = priceIconSpriteName;

            SetItemDefinitionKey(inventoryItemKey);
        }

        /// <summary>
        /// Initializes InventoryItemHudView before the first frame update.
        /// </summary>
        void Start()
        {
            if (Application.isPlaying)
            {
                ThrowIfNotInitialized();

                UpdateContent();
            }
        }

        /// <summary>
        /// Sets InventoryItem should be displayed by this view.
        /// </summary>
        /// <param name="itemDefinitionKey">The InventoryItem key that should be displayed.</param>
        /// <remarks>If the itemId param is null or empty, or is not found in the inventory catalog no action will be taken.</remarks>
        public void SetItemDefinitionKey(string itemDefinitionKey)
        {
            if (string.IsNullOrEmpty(itemDefinitionKey))
            {
                Debug.LogError($"{nameof(InventoryItemHudView)} - Given Inventory Item key shouldn't be empty or null.");
                return;
            }

            if (m_ItemDefinitionKey == itemDefinitionKey)
            {
                return;
            }

            if (Application.isPlaying)
            {
                var inventoryItem = GameFoundation.catalogs.inventoryCatalog?.FindItem(itemDefinitionKey);
                if (inventoryItem == null)
                {
                    Debug.LogError($"{nameof(InventoryItemHudView)} - Requested Inventory Item \"{itemDefinitionKey}\" key doesn't exist in Inventory Catalog.");
                    return;
                }
            }

            m_ItemDefinitionKey = itemDefinitionKey;

            UpdateContent();
        }

        /// <inheritdoc cref="SetItemDefinitionKey(string)"/>
        [Obsolete("Use 'SetItemDefinitionKey' instead", false)]
        public void SetItemDefinitionId(string itemDefinitionKey) => SetItemDefinitionKey(itemDefinitionKey);

        /// <summary>
        /// Sets sprite name for item icon that will be displayed on this view.
        /// </summary>
        /// <param name="spriteName">The sprite name that is defined on Assets Detail for Inventory item icon sprite.</param>
        public void SetIconSpriteName(string spriteName)
        {
            if (string.IsNullOrEmpty(spriteName) || m_IconSpriteName == spriteName)
            {
                return;
            }

            m_IconSpriteName = spriteName;

            UpdateIconSprite();
        }

        /// <summary>
        /// Sets the Image component to display Inventory item icon sprite on this view.
        /// </summary>
        /// <param name="image">The Image component to display Inventory item icon sprite.</param>
        public void SetIconImageField(Image image)
        {
            if (m_IconImageField == image)
            {
                return;
            }

            m_IconImageField = image;

            UpdateIconSprite();
        }

        /// <summary>
        /// Sets the Text component to display the Inventory item quantity on this view.
        /// </summary>
        /// <param name="text">The Text component to display the Inventory item quantity</param>
        public void SetQuantityTextField(Text text)
        {
            if (m_QuantityTextField == text)
            {
                return;
            }

            m_QuantityTextField = text;

            if (Application.isPlaying)
            {
                UpdateQuantity();
            }
        }

        /// <summary>
        /// Updates the Inventory item icon and quantity on this view.
        /// </summary>
        internal void UpdateContent()
        {
#if UNITY_EDITOR
            // To avoid updating the content the prefab selected in the Project window
            if (!Application.isPlaying && PrefabUtility.IsPartOfPrefabAsset(gameObject))
            {
                return;
            }
#endif
            
            UpdateIconSprite();

            if (Application.isPlaying)
            {
                UpdateQuantity();
            }
        }

        /// <summary>
        /// Updates the Inventory item icon on this view.
        /// </summary>
        void UpdateIconSprite()
        {
            if (string.IsNullOrEmpty(m_ItemDefinitionKey))
            {
                return;
            }

            if (Application.isPlaying)
            {
                var item = GameFoundation.catalogs.inventoryCatalog?.FindItem(m_ItemDefinitionKey);
                if (item == null)
                {
                    return;
                }

                var sprite = item.GetDetail<AssetsDetail>()?.GetAsset<Sprite>(m_IconSpriteName);
                SetIconSprite(sprite);
            }
#if UNITY_EDITOR
            else
            {
                var inventoryCatalog = GameFoundationDatabaseSettings.database.inventoryCatalog;
                if (inventoryCatalog == null)
                {
                    return;
                }

                var itemDefinition = inventoryCatalog.FindItem(m_ItemDefinitionKey);
                if (itemDefinition == null)
                {
                    return;
                }

                var sprite = itemDefinition.GetDetail<AssetsDetailAsset>()?.GetAsset<Sprite>(m_IconSpriteName);
                SetIconSprite(sprite);
            }
#endif
        }

        /// <summary>
        /// Sets sprite of Inventory item in display.
        /// </summary>
        /// <param name="iconSprite">The the sprite to display.</param>
        void SetIconSprite(Sprite iconSprite)
        {
            if (iconImageField == null)
            {
                Debug.LogWarning($"{nameof(InventoryItemHudView)} - Icon Image field is not defined.");
                return;
            }

            if (iconImageField.sprite == iconSprite)
            {
                return;
            }

            iconImageField.sprite = iconSprite;

            if (iconSprite != null)
            {
                iconImageField.SetNativeSize();
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(iconImageField);
#endif
        }

        /// <summary>
        /// Updates the Inventory item quantity on this view.
        /// </summary>
        void UpdateQuantity()
        {
            SetQuantity(GetQuantity());
        }

        /// <summary>
        /// Updates quantity of Inventory item in label.
        /// </summary>
        /// <param name="quantity">The new quantity to display.</param>
        void SetQuantity(long quantity)
        {
            if (m_QuantityTextField == null)
            {
                Debug.LogWarning($"{nameof(InventoryItemHudView)} - Item Quantity Text field is not defined.");
                return;
            }

            if (m_Quantity == quantity)
            {
                return;
            }

            m_Quantity = quantity;

            m_QuantityTextField.text = quantity.ToString();

#if UNITY_EDITOR
            EditorUtility.SetDirty(m_QuantityTextField);
#endif
        }

        /// <summary>
        /// Find all items that have the same definition key to calculate the quantity.
        /// </summary>
        /// <returns>The quantity of the item</returns>
        int GetQuantity()
        {
            if (string.IsNullOrEmpty(m_ItemDefinitionKey))
            {
                return 0;
            }

            var quantity = InventoryManager.FindItemsByDefinition(m_ItemDefinitionKey, m_TempItemList);
            m_TempItemList.Clear();

            return quantity;
        }

        void ThrowIfNotInitialized()
        {
            if (!GameFoundation.IsInitialized)
            {
                throw new InvalidOperationException("Error: GameFoundation.Initialize() must be called before the InventoryItemHudView is used.");
            }
        }

        /// <summary>
        /// Listens to updates from the inventory that contains the item being displayed.
        /// If the item that has changed is the one being displayed it updates the quantity.
        /// </summary>
        void OnItemAdded(InventoryItem addedItem)
        {
            if (addedItem == null || m_ItemDefinitionKey == null || m_ItemDefinitionKey != addedItem.definition.key)
            {
                return;
            }

            UpdateContent();
        }

        /// <summary>
        /// Listens to updates from the inventory that contains the item being displayed.
        /// If the item that was removed is the one being displayed it sets the items quantity display to 0.
        /// </summary>
        void OnItemRemoved(InventoryItem removedItem)
        {
            if (removedItem == null || m_ItemDefinitionKey == null || m_ItemDefinitionKey != removedItem.definition.key)
            {
                return;
            }

            UpdateContent();
        }
    }
}
