using System;
using System.Collections.Generic;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.GameFoundation.CatalogManagement;
#endif

namespace UnityEngine.GameFoundation.UI
{
    /// <summary>
    /// Component that manages displaying a Inventory Item's icon and quantity.
    /// When attached to a game object, it will display the Inventory Item's icon and quantity.
    /// </summary>
    [AddComponentMenu("Game Foundation/Inventory Item View", 5)]
    [ExecuteInEditMode]
    public class InventoryItemView : MonoBehaviour
    {
        /// <summary>
        /// The id of the Inventory Item to display.
        /// </summary>
        public string itemDefinitionId => m_ItemDefinitionId;
        
        [SerializeField] private string m_ItemDefinitionId = null;

        /// <summary>
        /// The sprite name for Inventory item icon that will be displayed on this view.
        /// </summary>
        public string iconSpriteName => m_IconSpriteName;
        
        [SerializeField] private string m_IconSpriteName = "item_icon";
        
        /// <summary>
        /// The Image component to assign the Inventory item icon image to.
        /// </summary>
        public Image iconImageField => m_IconImageField;
        
        [SerializeField] private Image m_IconImageField;
        
        /// <summary>
        /// The Text component to assign the Inventory item quantity to.
        /// </summary>
        public Text quantityTextField => m_QuantityTextField;
        
        [SerializeField] private Text m_QuantityTextField;
        
        /// <summary>
        /// The List of Inventory items <see cref="InventoryItem"/> that has the same definition id to cache them.
        /// </summary>
        private readonly List<InventoryItem> m_TempItemList = new List<InventoryItem>();
        
        /// <summary>
        /// The Quantity of the Inventory item.
        /// </summary>
        private long m_Quantity = 0;
        
        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                InventoryManager.itemAdded += OnItemAdded;
                InventoryManager.itemRemoved += OnItemRemoved;
            }
        }
        
        
        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                InventoryManager.itemAdded -= OnItemAdded;
                InventoryManager.itemRemoved -= OnItemRemoved;
            }
        }

        /// <summary>
        /// Initializes InventoryItemView with needed info.
        /// </summary>
        /// <param name="inventoryItemId">The InventoryItem id to be displayed.</param>
        /// <param name="priceIconSpriteName">The sprite name for item icon that will be displayed on this view.</param>
        internal void Init(string inventoryItemId, string priceIconSpriteName)
        {
            if (string.IsNullOrEmpty(inventoryItemId))
            {
                return;
            }

            m_ItemDefinitionId = inventoryItemId;
            m_IconSpriteName = priceIconSpriteName;

            SetItemDefinitionId(inventoryItemId);
        }

        /// <summary>
        /// Initializes InventoryItemView before the first frame update.
        /// </summary>
        private void Start()
        {
            if (Application.isPlaying)
            {
                ThrowIfNotInitialized();
            }

            UpdateContent();
        }

        /// <summary>
        /// Sets InventoryItem should be displayed by this view.
        /// </summary>
        /// <param name="itemDefinitionId">The InventoryItem id that should be displayed.</param>
        /// <remarks>If the itemId param is null or empty, or is not found in the inventory catalog no action will be taken.</remarks>
        public void SetItemDefinitionId(string itemDefinitionId)
        {
            if (string.IsNullOrEmpty(itemDefinitionId))
            {
                Debug.LogError($"{nameof(InventoryItemView)} Given InventoryItem Id shouldn't be empty or null.");
                return;
            }
            
            if (Application.isPlaying)
            {
                var inventoryItem = GameFoundation.catalogs.inventoryCatalog?.FindItem(itemDefinitionId);
                if (inventoryItem == null)
                {
                    Debug.LogError($"{nameof(InventoryItemView)} Requested InventoryItem \"{itemDefinitionId}\" doesn't exist in Inventory Catalog.");
                    return;
                }
            }
            
            m_ItemDefinitionId = itemDefinitionId;

            UpdateContent();
        }

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
        private void UpdateContent()
        {
            UpdateIconSprite();
            
            if (Application.isPlaying)
            {
                UpdateQuantity();
            }
        }
        
        /// <summary>
        /// Updates the Inventory item icon on this view.
        /// </summary>
        private void UpdateIconSprite()
        {
            if (string.IsNullOrEmpty(m_ItemDefinitionId))
            {
                return;
            }
            
            if (Application.isPlaying)
            {
                var item = GameFoundation.catalogs.inventoryCatalog?.FindItem(m_ItemDefinitionId);
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
            
                var itemDefinition = inventoryCatalog.FindItem(m_ItemDefinitionId);
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
        private void SetIconSprite(Sprite iconSprite)
        {
            if (iconImageField == null)
            {
                Debug.LogWarning($"{nameof(InventoryItemView)} Icon Image field is not defined.");
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
        private void UpdateQuantity()
        {
            SetQuantity(GetQuantity());
        }

        /// <summary>
        /// Updates quantity of Inventory item in label.
        /// </summary>
        /// <param name="quantity">The new quantity to display.</param>
        private void SetQuantity(long quantity)
        {
            if (m_QuantityTextField == null)
            {
                Debug.LogWarning($"{nameof(InventoryItemView)} Item Quantity Text field is not defined.");
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
        /// Find all items that have the same definition id to calculate the quantity.
        /// </summary>
        /// <returns>The quantity of the item</returns>
        private int GetQuantity()
        {
            if (string.IsNullOrEmpty(m_ItemDefinitionId))
            {
                return 0;
            }

            var quantity = InventoryManager.FindItemsByDefinition(m_ItemDefinitionId, m_TempItemList);
            m_TempItemList.Clear();
            
            return quantity;
        }
        
        private void ThrowIfNotInitialized()
        {
            if (!GameFoundation.IsInitialized)
            {
                throw new InvalidOperationException("Error: GameFoundation.Initialize() must be called before the InventoryItemView is used.");
            }
        }

        /// <summary>
        /// Listens to updates from the inventory that contains the item being displayed.
        /// If the item that has changed is the one being displayed it updates the quantity.
        /// </summary>
        private void OnItemAdded(GameItem addedItem)
        {
            if (addedItem == null || m_ItemDefinitionId == null || m_ItemDefinitionId != addedItem.definition.id)
            {
                return;
            }

            UpdateContent();
        }

        /// <summary>
        /// Listens to updates from the inventory that contains the item being displayed.
        /// If the item that was removed is the one being displayed it sets the items quantity display to 0.
        /// </summary>
        private void OnItemRemoved(GameItem removedItem)
        {
            if (removedItem == null || m_ItemDefinitionId == null || m_ItemDefinitionId != removedItem.definition.id)
            {
                return;
            }

            UpdateContent();
        }
    }
}
