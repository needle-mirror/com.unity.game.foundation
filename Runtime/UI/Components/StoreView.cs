using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UnityEngine.GameFoundation.UI
{
    /// <summary>
    /// Component that manages displaying the Transaction Items contained within a given store and category.
    /// When attached to a game object, it will create a TransactionItemView (<see cref="TransactionItemView"/>) for each store item in the designated
    /// list, with the given game object as their parent.
    /// </summary>
    [AddComponentMenu("Game Foundation/Store View", 1)]
    public class StoreView : MonoBehaviour
    {
        /// <summary>
        /// The id of the Store being purchased.
        /// </summary>
        public string storeId => m_StoreId;
        
        [SerializeField] private string m_StoreId;

        /// <summary>
        /// The id of the category items in the specified store should be filtered to for display.
        /// </summary>
        public string categoryId => m_CategoryId;
        
        [SerializeField] private string m_CategoryId;
        
        /// <summary>
        /// The sprite name for item icon that will be displayed on TransactionItemView.
        /// </summary>
        public string itemIconSpriteName => m_ItemIconSpriteName;
        
        [SerializeField] private string m_ItemIconSpriteName = "item_icon";

        /// <summary>
        /// The sprite name for price icon that will be displayed on the PurchaseButton.
        /// </summary>
        public string priceIconSpriteName => m_PriceIconSpriteName;
        
        [SerializeField] private string m_PriceIconSpriteName = "purchase_button_icon";
        
        /// <summary>
        /// Use to enable or disable interaction on the store UI.
        /// </summary>
        public bool interactable { get => m_Interactable; set => SetInteractable(value); }
        
        [SerializeField] private bool m_Interactable = true;
        
        /// <summary>
        /// The Transform in which to generate the list of TransactionItemView items.
        /// </summary>
        [Space]
        [Tooltip("Optionally allows specifying an alternate parent container for automatically rendered Transaction Item Prefabs. If not defined, StoreView's Transform will be the parent by default.")]
        public Transform itemContainer;

        /// <summary>
        /// The prefab with <see cref="TransactionItemView"/> component attached to use for creating the list of TransactionItemView items.
        /// </summary>
        [Tooltip("The prefab to use when generating Transaction Items in the Store View.")]
        public TransactionItemView transactionItemPrefab;

        /// <summary>
        /// Callback that will get triggered if a purchase for any item in the store completes successfully.
        /// </summary>
        [Space]
        public TransactionSuccessEvent onTransactionSucceeded;

        /// <summary>
        /// Callback that will get triggered if a purchase for any item in the store fails.
        /// </summary>
        public TransactionFailureEvent onTransactionFailed;
        
        /// <summary>
        /// A callback for when a transaction is completed. Wraps UnityEvent and accepts a BaseTransaction as a parameter.
        /// </summary>
        [Serializable] public class TransactionSuccessEvent : UnityEvent<BaseTransaction> {}
        
        /// <summary>
        /// A callback for when a transaction is failed. Wraps UnityEvent and accepts a BaseTransaction and Exception as a parameter.
        /// </summary>
        [Serializable] public class TransactionFailureEvent : UnityEvent<BaseTransaction, Exception> {}

        /// <summary>
        /// The list of <see cref="TransactionItemView"/> items that are instantiated using Transaction Item prefab based on the specified store and category.
        /// </summary>
        private readonly List<TransactionItemView> m_TransactionItems = new List<TransactionItemView>();

        /// <summary>
        /// To see if the component is being rendered in the scene. 
        /// </summary>
        private bool m_IsRunning = false;

        /// <summary>
        /// The Game Object with ScrollRect component where scrollable content resides.
        /// </summary>
        private ScrollRect m_ScrollRect;
        
        /// <summary>
        /// The final Transform in which to generate the list of TransactionItemView items.
        /// </summary>
        private Transform itemParentTransform => itemContainer ? itemContainer : transform;

        /// <summary>
        /// Specifies whether the button is interactable internally.
        /// </summary>
        private bool m_InteractableInternal = true;
        
        
        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                TransactionManager.transactionInitiated += OnTransactionInitiated;
                TransactionManager.transactionSucceeded += OnTransactionSucceeded;
                TransactionManager.transactionFailed += OnTransactionFailed;
            }
        }
        
        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                TransactionManager.transactionInitiated -= OnTransactionInitiated;
                TransactionManager.transactionSucceeded -= OnTransactionSucceeded;
                TransactionManager.transactionFailed -= OnTransactionFailed;
            }
        }

        /// <summary>
        /// Initializes the StoreView before the first frame update.
        /// </summary>
        private void Start()
        {
            ThrowIfNotInitialized();

            m_IsRunning = true;
            m_ScrollRect = gameObject.GetComponentInChildren<ScrollRect>(false);

            UpdateContent();
        }

        /// <summary>
        /// Updates which store should be displayed by this view.
        /// </summary>
        /// <param name="storeId">
        /// The id for the store that should be displayed.
        /// </param>
        /// <remarks>If the storeId param is null or empty, or is not found in the store catalog no action will be taken.</remarks>
        public void SetStoreId(string storeId)
        {
            if (string.IsNullOrEmpty(storeId))
            {
                Debug.LogError($"{nameof(StoreView)} Given store Id shouldn't be empty or null.");
                return;
            }
            
            if (m_StoreId == storeId)
            {
                return;
            }
            
            if (Application.isPlaying && GameFoundation.catalogs.storeCatalog.FindItem(storeId) == null)
            {
                Debug.LogError($"{nameof(StoreView)} Requested store \"{storeId}\" doesn't exist in Store Catalog.");
                return;
            }

            m_StoreId = storeId;

            if (Application.isPlaying)
            {
                UpdateContent();
            }
        }

        /// <summary>
        /// Updates which category of items within the store should be displayed by this view.
        /// </summary>
        /// <param name="categoryId">
        /// The id for the category of items that should be displayed.
        /// </param>
        /// <remarks>If the categoryId param is null or empty, or is not found in the store catalog no action will be taken.</remarks>
        public void SetCategoryId(string categoryId)
        {
            if (m_CategoryId == categoryId)
            {
                return;
            }

            if (!string.IsNullOrEmpty(categoryId) && Application.isPlaying && GameFoundation.catalogs.transactionCatalog.FindCategory(categoryId) == null)
            {
                Debug.LogWarning($"{nameof(StoreView)} Requested category \"{categoryId}\" doesn't exist in Store Catalog.");
                return;
            }

            m_CategoryId = categoryId;

            if (Application.isPlaying)
            {
                UpdateContent();
            }
        }
        
        /// <summary>
        /// Sets sprite name for item icon that will be displayed on TransactionItemViews.
        /// </summary>
        /// <param name="spriteName">The sprite name that is defined on Assets Detail for item icon sprite.</param>
        public void SetItemIconSpriteName(string spriteName)
        {
            if (string.IsNullOrEmpty(spriteName) || m_ItemIconSpriteName == spriteName)
            {
                return;
            }

            m_ItemIconSpriteName = spriteName;

            if (Application.isPlaying)
            {
                UpdateContent();
            }
        }
        
        /// <summary>
        /// Sets sprite name for price icon that will be displayed on the PurchaseButton.
        /// </summary>
        /// <param name="spriteName">The sprite name that is defined on Assets Detail for price icon sprite.</param>
        public void SetPriceIconSpriteName(string spriteName)
        {
            if (string.IsNullOrEmpty(spriteName) || m_PriceIconSpriteName == spriteName)
            {
                return;
            }

            m_PriceIconSpriteName = spriteName;

            if (Application.isPlaying)
            {
                UpdateContent();
            }
        }
        
        /// <summary>
        /// Sets the button's interactable state if the state specified is different from the current state. 
        /// </summary>
        /// <param name="interactable">Whether the button should be enabled or not.</param>
        public void SetInteractable(bool interactable)
        {
            if (m_Interactable == interactable)
            {
                return;
            }

            m_Interactable = interactable;
            
            if (m_ScrollRect != null)
            {
                m_ScrollRect.enabled = interactable & m_InteractableInternal;
            }

            foreach (var itemView in m_TransactionItems)
            {
                itemView.interactable = interactable;
            }
        }

        private void SetInteractableInternal(bool active)
        {
            if (m_InteractableInternal == active)
            {
                return;
            }
            
            m_InteractableInternal = active;
            
            if (m_ScrollRect != null)
            {
                m_ScrollRect.enabled = active && m_Interactable;
            }
        }
        
        /// <summary>
        /// Gets the Store that is attached to the StoreView.
        /// </summary>
        /// <returns>Store currently attached to the StoreView.</returns>
        public Store GetStore()
        {
            return GameFoundation.catalogs.storeCatalog.FindItem(m_StoreId);
        }
        
        /// <summary>
        /// Gets the list of TransactionItemViews that represents all items being displayed in this store view for the designated store.
        /// </summary>
        /// <returns>Array of TransactionItemViews objects for items being displayed in the store view.</returns>
        public TransactionItemView[] GetItems()
        {
            return m_TransactionItems.ToArray();
        }

        /// <summary>
        /// Generates and instantiates the list of TransactionItemViews for display in the StoreView.
        /// </summary>
        /// <remarks>
        /// Takes no action under any of the following circumstances:
        /// <list type="bullet">
        /// <item>
        /// <description>The storeId variable is null</description>
        /// </item>
        /// <item>
        /// <description>The storeId isn't a valid store id in the Store Catalog</description>
        /// </item>
        /// <item>
        /// <description>The TransactionItem prefab variable is null</description>
        /// </item>
        /// <item>
        /// <description>The TransactionItem Prefab does not have a TransactionItemViews component attached</description>
        /// </item>
        /// <item>
        /// <description>There are no Transaction Items that match the storeId/visibleItems filter preferences</description>
        /// </item>
        /// </list>
        /// <para>For any given item in the Transaction Items list, that item will not be shown if it does not have a purchasable detail
        /// attached to it's definition</para>
        /// </remarks>
        private void UpdateContent()
        {
            if (!m_IsRunning || string.IsNullOrEmpty(m_StoreId) || transactionItemPrefab == null || transactionItemPrefab.GetComponent<TransactionItemView>() == null)
            {
                return;
            }
            
            var store = GetStore();
            if (store == null)
            {
                return;
            }
            
            var transactions = string.IsNullOrEmpty(m_CategoryId)
                ? store.GetStoreItems()
                : store.GetStoreItemsByCategory(m_CategoryId);
            
            if (transactions == null || transactions.Length == 0)
            {
                return;
            }
            
            RemoveAllItems();

            foreach (var transaction in transactions)
            {
                var item = Instantiate(transactionItemPrefab, itemParentTransform, true).GetComponent<TransactionItemView>();
                item.transform.localScale = Vector3.one;
                item.Init(transaction.id, m_ItemIconSpriteName, m_PriceIconSpriteName);
                if (!m_Interactable)
                {
                    item.interactable = false;
                }

                m_TransactionItems.Add(item);
            }
            
            StartCoroutine(UpdateScrollbarStatus());
        }
        
        /// <summary>
        /// Updates whether vertical scrolling should be enabled.
        /// </summary>
        /// <returns>IEnumerator to wait for end of frame.</returns>
        /// <remarks>Doesn't support runtime screen orientation changes.</remarks>
        private IEnumerator UpdateScrollbarStatus()
        {
            yield return new WaitForEndOfFrame();

            if (!ReferenceEquals(m_ScrollRect, null))
            {
                var scrollTransform = m_ScrollRect.GetComponent<RectTransform>();
                var containerTransform = itemParentTransform.GetComponent<RectTransform>();
                
                if (!ReferenceEquals(scrollTransform, null) && !ReferenceEquals(containerTransform, null))
                {
                    var scrollRect = scrollTransform.rect;
                    var containerRect = containerTransform.rect;
                    
                    m_ScrollRect.vertical = scrollRect.height < containerRect.height;
                    m_ScrollRect.horizontal = scrollRect.width < containerRect.width;
                }
            }
        }

        /// <summary>
        /// Resets the StoreView by removing the listeners and destroying the game object for all items in the view.
        /// </summary>
        private void RemoveAllItems()
        {
            if (m_TransactionItems.Count == 0)
            {
                return;
            }

            foreach (var item in m_TransactionItems)
            {
                Destroy(item.gameObject);
            }
            
            m_TransactionItems.Clear();
        }

        private bool HasStoreContain(BaseTransaction transaction)
        {
            var store = GetStore();
            if (store != null)
            {
                foreach (var item in store.m_Items)
                {
                    if (item.id == transaction.id)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        
        private void ThrowIfNotInitialized()
        {
            if (!GameFoundation.IsInitialized)
            {
                throw new InvalidOperationException("Error: GameFoundation.Initialize() must be called before the StoreView is used.");
            }
        }

        /// <summary>
        /// Gets triggered when a Transaction Item is initiated. Triggers the
        /// user-specified onTransactionInitiated callback.
        /// </summary>
        private void OnTransactionInitiated(BaseTransaction transaction)
        {
            SetInteractableInternal(false);
        }

        /// <summary>
        /// Gets triggered when any item in the store is successfully purchased. Triggers the
        /// user-specified onTransactionSucceeded callback.
        /// </summary>
        private void OnTransactionSucceeded(BaseTransaction transaction, TransactionResult result)
        {
            SetInteractableInternal(true);

            if (HasStoreContain(transaction))
            {
                onTransactionSucceeded?.Invoke(transaction);   
            }
        }

        /// <summary>
        /// Gets triggered when any item in the store is attempted and fails to be purchased. Triggers the
        /// user-specified onTransactionSucceeded callback.
        /// </summary>
        private void OnTransactionFailed(BaseTransaction transaction, Exception exception)
        {
            SetInteractableInternal(true);

            if (HasStoreContain(transaction))
            {
                onTransactionFailed?.Invoke(transaction, exception);
            }
        }
    }
}
