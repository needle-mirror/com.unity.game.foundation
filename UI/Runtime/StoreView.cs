using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;
#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
using UnityEngine.Purchasing;
#endif

namespace UnityEngine.GameFoundation.UI
{
    /// <summary>
    /// Component that manages displaying the Transaction Items contained within a given store.
    /// When attached to a game object, it will create a TransactionItemView (<see cref="TransactionItemView"/>) for each store item in the designated
    /// list, with the given game object as their parent.
    /// </summary>
    [AddComponentMenu("Game Foundation/Store View", 1)]
    public class StoreView : MonoBehaviour
    {
        /// <summary>
        /// The identifier of the Store being purchased.
        /// </summary>
        public string storeKey => m_StoreKey;

        /// <inheritdoc cref="storeKey"/>
        [Obsolete("Use 'storeKey' instead", false)]
        public string storeId => storeKey;

        /// <inheritdoc cref="storeKey"/>
        [SerializeField, FormerlySerializedAs("m_StoreId")]
        internal string m_StoreKey;

        /// <summary>
        /// The identifier of the tag items in the specified store should be filtered to for display.
        /// </summary>
        public string tagKey => m_TagKey;

        /// <inheritdoc cref="tagKey"/>
        [SerializeField, FormerlySerializedAs("m_CategoryKey")]
        internal string m_TagKey;

        /// <inheritdoc cref="tagKey"/>
        [Obsolete("Use 'tagKey' instead", false)]
        public string categoryId => tagKey;

        /// <inheritdoc cref="tagKey"/>
        [Obsolete("Use 'tagKey' instead", false)]
        public string categoryKey => tagKey;

        /// <summary>
        /// The sprite name for item icon that will be displayed on TransactionItemView.
        /// </summary>
        public string itemIconSpriteName => m_ItemIconSpriteName;

        [SerializeField]
        internal string m_ItemIconSpriteName = "item_icon";

        /// <summary>
        /// The sprite name for price icon that will be displayed on the PurchaseButton.
        /// </summary>
        public string priceIconSpriteName => m_PriceIconSpriteName;

        [SerializeField]
        internal string m_PriceIconSpriteName = "purchase_button_icon";
        
        /// <summary>
        /// The string to display on Purchase Button if the Transaction Item has no cost.
        /// </summary>
        public string noPriceString => m_NoPriceString;
        
        [SerializeField]
        internal string m_NoPriceString = PurchaseButton.kDefaultNoPriceString;

        /// <summary>
        /// Use to enable or disable interaction on the store UI.
        /// </summary>
        public bool interactable
        {
            get => m_Interactable;
            set => SetInteractable(value);
        }

        [SerializeField]
        internal bool m_Interactable = true;

        /// <summary>
        /// The prefab with <see cref="TransactionItemView"/> component attached to use for creating the list of TransactionItemView items.
        /// </summary>
        [Tooltip("The prefab to use when generating Transaction Items in the Store View.")]
        public TransactionItemView transactionItemPrefab;
        
        /// <summary>
        /// The Transform in which to generate the list of TransactionItemView items.
        /// </summary>
        [Header("GameObject Field")]
        [Space]
        [Tooltip("Optionally allows specifying an alternate parent container for automatically rendered Transaction Item Prefabs. If not defined, StoreView's Transform will be the parent by default.")]
        public Transform itemContainer;

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
        [Serializable]
        public class TransactionSuccessEvent : UnityEvent<BaseTransaction> { }

        /// <summary>
        /// A callback for when a transaction is failed. Wraps UnityEvent and accepts a BaseTransaction and Exception as a parameter.
        /// </summary>
        [Serializable]
        public class TransactionFailureEvent : UnityEvent<BaseTransaction, Exception> { }

        /// <summary>
        /// The list of <see cref="TransactionItemView"/> items that are instantiated using Transaction Item prefab based on the specified store and tag.
        /// </summary>
        readonly List<TransactionItemView> m_TransactionItems = new List<TransactionItemView>();

        /// <summary>
        /// To see if the component is being rendered in the scene. 
        /// </summary>
        bool m_IsRunning;

        /// <summary>
        /// The Game Object with ScrollRect component where scrollable content resides.
        /// </summary>
        ScrollRect m_ScrollRect;

        /// <summary>
        /// The final Transform in which to generate the list of TransactionItemView items.
        /// </summary>
        Transform itemParentTransform => itemContainer ? itemContainer : transform;

        /// <summary>
        /// Specifies whether the button is interactable internally.
        /// </summary>
        bool m_InteractableInternal = true;

        void OnEnable()
        {
            if (Application.isPlaying)
            {
                TransactionManager.transactionInitiated += OnTransactionInitiated;
                TransactionManager.transactionSucceeded += OnTransactionSucceeded;
                TransactionManager.transactionFailed += OnTransactionFailed;
            }
        }

        void OnDisable()
        {
            if (Application.isPlaying)
            {
                TransactionManager.transactionInitiated -= OnTransactionInitiated;
                TransactionManager.transactionSucceeded -= OnTransactionSucceeded;
                TransactionManager.transactionFailed -= OnTransactionFailed;
            }
        }

        /// <summary>
        /// Initializes StoreView with needed info.
        /// </summary>
        /// <param name="storeKey">The key for the store that should be displayed.</param>
        /// <param name="tagKey">The key for the tag of items that should be displayed.</param>
        /// <param name="itemIconSpriteName">The sprite name for item icon that will be displayed on this view.</param>
        /// <param name="priceIconSpriteName">The sprite name for price icon that will be displayed on PurchaseButton.</param>
        /// <param name="noPriceString">"The string to display on Purchase Button when there is no cost defined in the Transaction Item.</param>
        internal void Init(string storeKey, string tagKey, string itemIconSpriteName, string priceIconSpriteName, string noPriceString)
        {
            if (string.IsNullOrEmpty(storeKey))
            {
                return;
            }

            m_StoreKey = storeKey;
            m_TagKey = tagKey;
            m_ItemIconSpriteName = itemIconSpriteName;
            m_PriceIconSpriteName = priceIconSpriteName;
            m_NoPriceString = noPriceString;

            UpdateContent();
        }

        /// <summary>
        /// Initializes the StoreView before the first frame update.
        /// </summary>
        void Start()
        {
            ThrowIfNotInitialized();

            m_IsRunning = true;
            m_ScrollRect = gameObject.GetComponentInChildren<ScrollRect>(false);

            UpdateContent();
        }

        /// <summary>
        /// Updates which store should be displayed by this view.
        /// </summary>
        /// <param name="storeKey">The identifier for the store that should be displayed.</param>
        /// <param name="tagKey">The key for the tag of items that should be displayed.</param>
        /// <remarks>If the <paramref name="storeKey"/> param is null or empty, or is not found in the store catalog no action will be taken.</remarks>
        /// <remarks>If the <paramref name="tagKey"/> param is null or empty, all transactions in a store will be displayed.</remarks>
        public void SetStoreKey(string storeKey, string tagKey = null)
        {
            if (string.IsNullOrEmpty(storeKey))
            {
                Debug.LogError($"{nameof(StoreView)} Given store key shouldn't be empty or null.");
                return;
            }

            if (m_StoreKey == storeKey && m_TagKey == tagKey)
            {
                return;
            }

            if (Application.isPlaying && GameFoundation.catalogs.storeCatalog.FindItem(storeKey) == null)
            {
                Debug.LogError($"{nameof(StoreView)} Requested store \"{storeKey}\" doesn't exist in Store Catalog.");
                return;
            }
            
            if (Application.isPlaying && !string.IsNullOrEmpty(tagKey) && GameFoundation.catalogs.tagCatalog.FindTag(tagKey) == null)
            {
                Debug.LogWarning($"{nameof(StoreView)} Requested tag \"{tagKey}\" doesn't exist in Tag Catalog.");
                return;
            }

            m_StoreKey = storeKey;
            m_TagKey = tagKey;

            if (Application.isPlaying)
            {
                UpdateContent();
            }
        }

        /// <inheritdoc cref="SetStoreKey(string)"/>
        [Obsolete("Use 'SetStoreKey' instead", false)]
        public void SetStoreId(string storeKey) => SetStoreKey(storeKey);

        /// <summary>
        /// Updates which tag of items within the store should be displayed by this view.
        /// </summary>
        /// <param name="tagKey">The key for the tag of items that should be displayed. To show all transactions in a store (no tag filtering) null can be passed as the tagKey.</param>
        /// <remarks>If the <paramref name="storeKey"/> param matches existing tag selection, or is not found in the tag catalog no action will be taken.</remarks>
        public void SetTagKey(string tagKey)
        {
            if (m_TagKey == tagKey)
            {
                return;
            }

            if (!string.IsNullOrEmpty(tagKey) && Application.isPlaying && GameFoundation.catalogs.tagCatalog.FindTag(tagKey) == null)
            {
                Debug.LogWarning($"{nameof(StoreView)} Requested tag \"{tagKey}\" doesn't exist in Tag Catalog.");
                return;
            }

            m_TagKey = tagKey;

            if (Application.isPlaying)
            {
                UpdateContent();
            }
        }

        /// <inheritdoc cref="SetTagKey(string)"/>	
        [Obsolete("Use 'SetTagKey' instead", false)]	
        public void SetCategoryId(string categoryId) => SetTagKey(categoryId);

        /// <inheritdoc cref="SetTagKey(string)"/>	
        [Obsolete("Use 'SetTagKey' instead", false)]	
        public void SetCategoryKey(string categoryKey) => SetTagKey(categoryKey);

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
        /// Sets the string to display on Purchase Button when there is no cost defined in the Transaction Item.
        /// </summary>
        /// <param name="noPriceString">The string to display.</param>
        public void SetNoPriceString(string noPriceString)
        {
            if (m_NoPriceString == noPriceString)
            {
                return;
            }

            m_NoPriceString = noPriceString;
            UpdateContent();
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

        void SetInteractableInternal(bool active)
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
            return GameFoundation.catalogs.storeCatalog.FindItem(m_StoreKey);
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
        /// <description>The storeKey variable is null</description>
        /// </item>
        /// <item>
        /// <description>The storeKey isn't a valid store id in the Store Catalog</description>
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
        void UpdateContent()
        {
            if (!m_IsRunning || string.IsNullOrEmpty(m_StoreKey) || transactionItemPrefab == null || transactionItemPrefab.GetComponent<TransactionItemView>() == null)
            {
                return;
            }

            var store = GetStore();
            if (store == null)
            {
                return;
            }

            var transactions = string.IsNullOrEmpty(m_TagKey)
                ? store.GetStoreItems()
                : store.GetStoreItemsByTag(m_TagKey);

            RemoveAllItems();
            
            if (transactions == null || transactions.Length == 0)
            {
                return;
            }

            foreach (var transaction in transactions)
            {
                if (transaction is VirtualTransaction || transaction is IAPTransaction iapTransaction && IsIAPTransactionPurchasable(iapTransaction))
                {
                    var item = Instantiate(transactionItemPrefab, itemParentTransform, true)
                        .GetComponent<TransactionItemView>();
                    item.transform.localScale = Vector3.one;
                    item.Init(transaction.key, m_ItemIconSpriteName, m_PriceIconSpriteName, m_NoPriceString);
                    if (!m_Interactable)
                    {
                        item.interactable = false;
                    }

                    m_TransactionItems.Add(item);
                }
            }

            StartCoroutine(UpdateScrollbarStatus());
        }

        /// <summary>
        /// Updates whether vertical scrolling should be enabled.
        /// </summary>
        /// <returns>IEnumerator to wait for end of frame.</returns>
        /// <remarks>Doesn't support runtime screen orientation changes.</remarks>
        IEnumerator UpdateScrollbarStatus()
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
        void RemoveAllItems()
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

        bool HasStoreContain(BaseTransaction transaction)
        {
            var store = GetStore();
            if (store != null)
            {
                return store.Contains(transaction);
            }

            return false;
        }
        
        bool IsIAPTransactionPurchasable(IAPTransaction iapTransaction)
        {
#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
            return iapTransaction.product.definition.type != ProductType.NonConsumable || 
                   iapTransaction.product.definition.type == ProductType.NonConsumable && 
                   !TransactionManager.IsIapProductOwned(iapTransaction.productId);
#else
            return false;
#endif
        }

        void ThrowIfNotInitialized()
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
        void OnTransactionInitiated(BaseTransaction transaction)
        {
            if (!HasStoreContain(transaction))
                return;
            
            SetInteractableInternal(false);
        }

        /// <summary>
        /// Gets triggered when any item in the store is successfully purchased. Triggers the
        /// user-specified onTransactionSucceeded callback.
        /// </summary>
        void OnTransactionSucceeded(BaseTransaction transaction, TransactionResult result)
        {
            if (!HasStoreContain(transaction))
                return;
            
#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
            if (transaction is IAPTransaction iapTransaction && iapTransaction.product.definition.type == ProductType.NonConsumable)
            {
                UpdateContent();
            }
#endif
            
            SetInteractableInternal(true);

            onTransactionSucceeded?.Invoke(transaction);
        }

        /// <summary>
        /// Gets triggered when any item in the store is attempted and fails to be purchased. Triggers the
        /// user-specified onTransactionSucceeded callback.
        /// </summary>
        void OnTransactionFailed(BaseTransaction transaction, Exception exception)
        {
            if (!HasStoreContain(transaction))
                return;
            
            SetInteractableInternal(true);

            onTransactionFailed?.Invoke(transaction, exception);
        }
    }
}
