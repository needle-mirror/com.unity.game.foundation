using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.GameFoundation.DefaultCatalog.Details;
using UnityEngine.Promise;
using UnityEngine.Serialization;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
using UnityEngine.Purchasing;
#endif

namespace UnityEngine.GameFoundation.UI
{
    /// <summary>
    /// Component for completing a purchase using the TransactionManager.
    /// </summary>
    [AddComponentMenu("Game Foundation/Purchase Button", 3)]
    [RequireComponent(typeof(Button))]
    [ExecuteInEditMode]
    public class PurchaseButton : MonoBehaviour
    {
        /// <summary>
        /// The identifier of the Transaction Item being purchased.
        /// </summary>
        public string transactionKey => m_TransactionKey;

        /// <inheritdoc cref="transactionKey"/>
        [Obsolete("Use 'tranasctionKey' instead", false)]
        public string transactionId => transactionKey;

        /// <inheritdoc cref="transactionKey"/>
        [SerializeField, FormerlySerializedAs("m_TransactionId")]
        internal string m_TransactionKey;

        /// <summary>
        /// The sprite name for price icon that will be displayed on the button.
        /// </summary>
        public string priceIconSpriteName => m_PriceIconSpriteName;

        [SerializeField]
        internal string m_PriceIconSpriteName = "purchase_button_icon";

        /// <summary>
        /// Use to enable or disable the button.
        /// </summary>
        public bool interactable
        {
            get => m_Interactable;
            set => SetInteractable(value);
        }

        [SerializeField]
        internal bool m_Interactable = true;

        /// <summary>
        /// The Text component to assign the price text to.
        /// </summary>
        public Text priceTextField => m_PriceTextField;

        [SerializeField]
        internal Text m_PriceTextField;

        /// <summary>
        /// The Image component to assign the Transaction Item's icon image to.
        /// </summary>
        public Image priceIconImageField => m_PriceIconImageField;

        [SerializeField]
        internal Image m_PriceIconImageField;

        /// <summary>
        /// The string to display if the Transaction Item has no cost.
        /// </summary>
        public string noPriceString => m_NoPriceString;
        
        [SerializeField]
        internal string m_NoPriceString = kDefaultNoPriceString;

        /// <summary>
        /// Callback that will get triggered if item purchase completes successfully.
        /// </summary>
        [Space]
        public TransactionSuccessEvent onPurchaseSuccess;

        /// <summary>
        /// Callback that will get triggered if item purchase fails.
        /// </summary>
        public TransactionFailureEvent onPurchaseFailure;

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
        /// The Transaction being purchased. 
        /// </summary>
        BaseTransaction m_Transaction;

        /// <summary>
        /// The Button component attached to this PurchaseButton.
        /// </summary>
        Button m_Button;

        /// <summary>
        /// Specifies whether the item is available to purchase.
        /// </summary>
        public bool availableToPurchaseState => m_AvailableToPurchaseState;
        
        private bool m_AvailableToPurchaseState = true;

        /// <summary>
        /// Specifies whether the button is interactable internally.
        /// </summary>
        bool m_InteractableInternal = true;

        /// <summary>
        /// Specifies whether the button is driven by other component.
        /// </summary>
        bool m_IsDrivenByOtherComponent;

        /// <summary>
        /// Specifies whether the debug logs is visible.
        /// </summary>
        bool m_ShowDebugLogs = false;

#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
        /// <summary>
        /// Specifies whether purchasing adapter will be initialized.
        /// </summary>
        bool m_WillPurchasingAdapterInitialized;
#endif

        /// <summary>
        /// List of item exchange definition objects for transaction.
        /// </summary>
        List<ItemExchangeDefinitionObject> m_ItemObjectsList = new List<ItemExchangeDefinitionObject>();

        /// <summary>
        /// List of currency exchange objects for transaction.
        /// </summary>
        List<CurrencyExchangeObject> m_CurrencyObjectsList = new List<CurrencyExchangeObject>();

        /// <summary>
        /// List of item exchange definitions for transaction.
        /// </summary>
        List<ItemExchangeDefinition> m_ItemsList = new List<ItemExchangeDefinition>();

        /// <summary>
        /// List of currency exchange definitions for transaction.
        /// </summary>
        List<CurrencyExchangeDefinition> m_CurrenciesList = new List<CurrencyExchangeDefinition>();
        
        /// <summary>
        /// Default string to display when there is no cost defined in the Transaction Item.
        /// </summary>
        internal static readonly string kDefaultNoPriceString = "FREE";
        
        /// <summary>
        /// Specifies whether the GameObject fields on the editor is visible.
        /// </summary>
        [SerializeField]
        internal bool showGameObjectEditorFields = true;

        void OnEnable()
        {
            if (Application.isPlaying)
            {
                InventoryManager.itemAdded += OnInventoryChanged;
                InventoryManager.itemRemoved += OnInventoryChanged;
                WalletManager.balanceChanged += OnWalletChanged;

#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
                if (m_WillPurchasingAdapterInitialized)
                {
                    if (TransactionManager.purchasingAdapterIsInitialized)
                    {
                        OnPurchasingAdapterInitializeSucceeded();
                    }
                    else
                    {
                        TransactionManager.purchasingAdapterInitializeSucceeded += OnPurchasingAdapterInitializeSucceeded;
                        TransactionManager.purchasingAdapterInitializeFailed += OnPurchasingAdapterInitializeFailed;
                    }
                }
#endif
            }
        }

        void OnDisable()
        {
            if (Application.isPlaying)
            {
                InventoryManager.itemAdded -= OnInventoryChanged;
                InventoryManager.itemRemoved -= OnInventoryChanged;
                WalletManager.balanceChanged -= OnWalletChanged;

#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
                if (m_WillPurchasingAdapterInitialized)
                {
                    TransactionManager.purchasingAdapterInitializeSucceeded -= OnPurchasingAdapterInitializeSucceeded;
                    TransactionManager.purchasingAdapterInitializeFailed -= OnPurchasingAdapterInitializeFailed;
                }
#endif
            }
        }

        /// <summary>
        /// Gets the Button component of the PurchaseButton and sets the onClick listener to call <see cref="Purchase"/>.
        /// </summary>
        void Awake()
        {
            m_Button = GetComponent<Button>();

            if (Application.isPlaying)
            {
                m_Button.onClick.AddListener(Purchase);
            }
        }

        /// <summary>
        /// Initializes PurchaseButton with needed info.
        /// </summary>
        /// <param name="transactionKey">The BaseTransaction identifier to be displayed.</param>
        /// <param name="priceIconSpriteName">The sprite name for price icon that will be displayed on the button.</param>
        internal void Init(string transactionKey, string priceIconSpriteName)
        {
            Init(transactionKey, priceIconSpriteName, m_NoPriceString);
        }
        
        /// <summary>
        /// Initializes PurchaseButton with needed info.
        /// </summary>
        /// <param name="transactionKey">The BaseTransaction identifier to be displayed.</param>
        /// <param name="priceIconSpriteName">The sprite name for price icon that will be displayed on the button.</param>
        /// <param name="noPriceString">"The string to display when there is no cost defined in the Transaction Item.</param>
        internal void Init(string transactionKey, string priceIconSpriteName, string noPriceString)
        {
            if (string.IsNullOrEmpty(transactionKey))
            {
                return;
            }

            m_IsDrivenByOtherComponent = true;

            m_TransactionKey = transactionKey;
            m_PriceIconSpriteName = priceIconSpriteName;
            m_NoPriceString = noPriceString; 

            SetTransactionKey(transactionKey);
        }

        /// <summary>
        /// Initializes PurchaseButton before the first frame update.
        /// If the it's already initialized by TransactionItemView no action will be taken.
        /// </summary>
        void Start()
        {
            if (Application.isPlaying)
            {
                ThrowIfNotInitialized();

                UpdateButtonStatus();
                
                if (!m_IsDrivenByOtherComponent)
                {
                    SetTransactionKey(m_TransactionKey);
                }
            }
        }

        /// <summary>
        /// Updates which Transaction Item should be displayed by this button.
        /// </summary>
        /// <param name="transactionKey">The BaseTransaction identifier that is displayed by the button.</param>
        /// <remarks>If the transactionId param is null or empty, or is not found in the transaction catalog no action will be taken.</remarks>
        public void SetTransactionKey(string transactionKey)
        {
            if (string.IsNullOrEmpty(transactionKey))
            {
                Debug.LogError($"{nameof(PurchaseButton)} - Given transaction Id shouldn't be empty or null.");
                return;
            }

            if (Application.isPlaying)
            {
                m_Transaction = GameFoundation.catalogs.transactionCatalog?.FindItem(transactionKey);
                if (m_Transaction == null)
                {
                    Debug.LogError($"{nameof(PurchaseButton)} - Requested transaction \"{transactionKey}\" doesn't exist in Transaction Catalog.");
                    return;
                }
            }

            m_TransactionKey = transactionKey;

            UpdateContent();
        }

        [Obsolete("Use 'SetTransactionKey' instead", false)]
        public void SetTransactionId(string transactionKey) => SetTransactionKey(transactionKey);

        /// <summary>
        /// Gets the BaseTransaction that is attached to the PurchaseButton.
        /// </summary>
        /// <returns>BaseTransaction currently attached to the PurchaseButton.</returns>
        public BaseTransaction GetTransaction()
        {
            return GameFoundation.catalogs.transactionCatalog.FindItem(m_TransactionKey);
        }

        /// <summary>
        /// Calls <see cref="O:TransactionManager.BeginTransaction"/> with the purchase detail of the Transaction Item displayed in the button.
        /// Is automatically attached to the onClick event of the PurchaseButton.
        /// </summary>
        public void Purchase()
        {
            if (string.IsNullOrEmpty(m_TransactionKey))
            {
                Debug.LogError($"{nameof(PurchaseButton)} - Transaction Item is not defined.");
                return;
            }

            SetInteractableInternal(false);

            StartCoroutine(ExecuteTransaction(m_Transaction));
        }

        /// <summary>
        ///  Execute the transaction with Coroutine since transaction uses deferred objects. 
        /// </summary>
        /// <param name="transaction">The Transaction being purchased.</param>
        IEnumerator ExecuteTransaction(BaseTransaction transaction)
        {
            Deferred<TransactionResult> deferred = TransactionManager.BeginTransaction(transaction);

            if (m_ShowDebugLogs)
            {
                Debug.Log($"{nameof(PurchaseButton)} - Now processing purchase: {transaction.displayName}");
            }

            // wait for the transaction to be processed
            int currentStep = 0;

            while (!deferred.isDone)
            {
                // keep track of the current step and possibly show a progress UI
                if (deferred.currentStep != currentStep)
                {
                    currentStep = deferred.currentStep;

                    if (m_ShowDebugLogs)
                    {
                        Debug.Log($"{nameof(PurchaseButton)} - Transaction is now on step {currentStep} of {deferred.totalSteps}");
                    }
                }

                yield return null;
            }

            // We re-enable the button before the break even if there is an error
            SetInteractableInternal(true);

            // now that the transaction has been processed, check for an error
            if (!deferred.isFulfilled)
            {
                if (m_ShowDebugLogs)
                {
                    Debug.LogError($"{nameof(PurchaseButton)} - Transaction Key: \"{transaction.key}\" - Error Message: {deferred.error}");
                }

                onPurchaseFailure?.Invoke(transaction, deferred.error);

                deferred.Release();
                yield break;
            }

            // here we can assume success
            if (m_ShowDebugLogs)
            {
                Debug.Log("The purchase was successful in both the platform store and the data layer!");

                foreach (var currencyReward in deferred.result.rewards.currencies)
                {
                    Debug.Log($"Player was awarded {currencyReward.amount} " + $"of currency '{currencyReward.currency.displayName}'");
                }
            }

            onPurchaseSuccess?.Invoke(transaction);

            // all done
            deferred.Release();
        }

        /// <summary>
        /// Sets the button's interactable state according to Transaction Item's affordability status.
        /// </summary>
        /// <param name="state">Whether the button should be enabled or not.</param>
        void SetAvailableToPurchaseState(bool state)
        {
            if (m_AvailableToPurchaseState != state)
            {
                m_AvailableToPurchaseState = state;
                UpdateButtonStatus();
            }
        }
        
        /// <summary>
        /// Updates the button's interactable state according to Transaction Item's affordability status.
        /// </summary>
        void UpdateAvailableToPurchaseState()
        {
            if (m_Transaction == null)
            {
                SetAvailableToPurchaseState(false);
            }
            else if (m_Transaction is VirtualTransaction vTransaction)
            {
                SetAvailableToPurchaseState(IsAffordable(vTransaction));
            }
            else if (m_Transaction is IAPTransaction iapTransaction)
            {
                if (string.IsNullOrEmpty(iapTransaction.productId) || !TransactionManager.purchasingAdapterIsInitialized)
                {
                    SetAvailableToPurchaseState(false);
                }
                else
                {
#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
                    bool available = iapTransaction.product.definition.type != ProductType.NonConsumable ||
                                     iapTransaction.product.definition.type == ProductType.NonConsumable &&
                                     !TransactionManager.IsIapProductOwned(iapTransaction.productId);
                
                    SetAvailableToPurchaseState(available);
#else
                    SetAvailableToPurchaseState(false);
#endif
                }
            }
        }

        /// <summary>
        /// Sets the button's interactable state if the state specified is different from the current state. 
        /// </summary>
        /// <param name="interactable">Whether the button should be enabled or not.</param>
        public void SetInteractable(bool interactable)
        {
            if (m_Interactable != interactable)
            {
                m_Interactable = interactable;
                UpdateButtonStatus();
            }
        }

        void SetInteractableInternal(bool active)
        {
            if (m_InteractableInternal != active)
            {
                m_InteractableInternal = active;
                UpdateButtonStatus();
            }
        }

        /// <summary>
        /// Updates button status according to user defined setting and internal status like affordability
        /// of the Transaction Item
        /// </summary>
        void UpdateButtonStatus()
        {
            m_Button.interactable = m_AvailableToPurchaseState && m_Interactable && m_InteractableInternal;
        }

        /// <summary>
        /// Sets the Text component to display price text on the button.
        /// </summary>
        /// <param name="text">The Text component to display the texts on buttons</param>
        public void SetPriceTextField(Text text)
        {
            if (m_PriceTextField == text)
            {
                return;
            }

            m_PriceTextField = text;
            UpdateContent();
        }

        /// <summary>
        /// Sets the Image component to display price icon sprite on the button.
        /// </summary>
        /// <param name="image">The Image component to display price icon sprite.</param>
        public void SetPriceIconImageField(Image image)
        {
            if (m_PriceIconImageField == image)
            {
                return;
            }

            m_PriceIconImageField = image;
            UpdateContent();
        }
        
        /// <summary>
        /// Sets the string to display when there is no cost defined in the Transaction Item.
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
        /// Sets sprite name for price icon that will be displayed on the button.
        /// </summary>
        /// <param name="spriteName">The sprite name that is defined on Assets Detail for price icon sprite.</param>
        public void SetPriceIconSpriteName(string spriteName)
        {
            if (string.IsNullOrEmpty(spriteName) || m_PriceIconSpriteName == spriteName)
            {
                return;
            }

            m_PriceIconSpriteName = spriteName;
            UpdateContent();
        }

        /// <summary>
        /// Sets price text and icon on the button.
        /// </summary>
        /// <param name="priceIcon">Price icon sprite to display</param>
        /// <param name="priceText">Price text to display</param>
        void SetContent(Sprite priceIcon, string priceText)
        {
            if (m_PriceIconImageField != null)
            {
                m_PriceIconImageField.sprite = priceIcon;

                if (priceIcon != null)
                {
                    m_PriceIconImageField.gameObject.SetActive(true);
                    m_PriceIconImageField.SetNativeSize();
                }
                else
                {
                    m_PriceIconImageField.gameObject.SetActive(false);
                }

#if UNITY_EDITOR
                EditorUtility.SetDirty(m_PriceIconImageField);
#endif
            }
            else
            {
                Debug.LogWarning($"{nameof(PurchaseButton)} - Icon Image Field is not defined.");
            }

            if (m_PriceTextField != null)
            {
                if (m_PriceTextField.text != priceText)
                {
                    m_PriceTextField.text = priceText;

#if UNITY_EDITOR
                    EditorUtility.SetDirty(m_PriceTextField);
#endif
                }
            }
            else
            {
                Debug.LogWarning($"{nameof(PurchaseButton)} - Price Text Field is not defined.");
            }
        }

        /// <summary>
        /// Updates the price icon and the price amount field on the PurchaseButton.
        /// </summary>
        internal void UpdateContent()
        {
            if (Application.isPlaying)
            {
                UpdateContentAtRuntime();
            }
#if UNITY_EDITOR
            else
            {
                UpdateContentAtEditor();
            }
#endif
        }

        /// <summary>
        /// To update the price icon image and the price amount field on the PurchaseButton at runtime.
        /// </summary>
        void UpdateContentAtRuntime()
        {
            if (string.IsNullOrEmpty(m_TransactionKey) || m_Transaction == null)
            {
                SetContent(null, null);

                SetAvailableToPurchaseState(false);
                return;
            }

            if (m_Transaction is VirtualTransaction vTransaction)
            {
                if (DoesHaveMultipleCost(vTransaction))
                {
                    Debug.LogWarning($"{nameof(PurchaseButton)} - Transaction item \"{m_Transaction.displayName}\" has multiple exchange item. {nameof(PurchaseButton)} can only show the first item on UI.");
                }

                GetVirtualCurrencyCost(vTransaction, 0, out var cost, out var item);
                if (item == null)
                {
                    GetVirtualItemCost(vTransaction, 0, out cost, out item);
                }

                if (cost > 0 && item != null)
                {
                    var sprite = item.GetDetail<AssetsDetail>()?.GetAsset<Sprite>(m_PriceIconSpriteName);
                    if (sprite == null)
                    {
                        Debug.LogWarning($"{nameof(PurchaseButton)} - \"{item.displayName}\" doesn't have sprite called \"{m_PriceIconSpriteName}\"");
                    }

                    SetContent(sprite, cost.ToString());
                    SetAvailableToPurchaseState(IsAffordable(vTransaction));
                }

                // Item is free
                else
                {
                    SetContent(null, noPriceString);
                    SetAvailableToPurchaseState(true);
                }
            }
            else if (m_Transaction is IAPTransaction iapTransaction)
            {
                SetContent(null, null);

#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
                if (string.IsNullOrEmpty(iapTransaction.productId))
                {
                    Debug.LogError($"{nameof(PurchaseButton)} - Transaction Item \"{m_Transaction.displayName}\" shouldn't have empty or null product id.");
                }
                else if (TransactionManager.purchasingAdapterIsInitialized)
                {
                    SetIAPTransactionPrice(iapTransaction);
                }
                else
                {
                    m_WillPurchasingAdapterInitialized = true;

                    TransactionManager.purchasingAdapterInitializeSucceeded += OnPurchasingAdapterInitializeSucceeded;
                    TransactionManager.purchasingAdapterInitializeFailed += OnPurchasingAdapterInitializeFailed;
                }

                UpdateAvailableToPurchaseState();
#endif
            }
        }

        void GetVirtualCurrencyCost(VirtualTransaction transaction, int indexOfCost, out long amount, out CatalogItem costItem)
        {
            var costs = transaction?.costs;
            if (costs != null && costs.CurrencyExchangeCount > 0)
            {
                var cost = costs.GetCurrencyExchange(indexOfCost);
                amount = cost.amount;
                costItem = cost.currency;

                return;
            }

            amount = 0;
            costItem = null;
        }

        void GetVirtualItemCost(VirtualTransaction transaction, int indexOfCost, out long amount, out CatalogItem costItem)
        {
            var costs = transaction?.costs;
            if (costs != null && costs.ItemExchangeCount > 0)
            {
                var cost = costs.GetItemExchange(indexOfCost);
                amount = cost.amount;
                costItem = cost.item;

                return;
            }

            amount = 0;
            costItem = null;
        }

        bool DoesHaveMultipleCost(VirtualTransaction transaction)
        {
            var costs = transaction?.costs;
            if (costs != null)
            {
                return (costs.GetItemExchanges(m_ItemsList) + costs.GetCurrencyExchanges(m_CurrenciesList)) > 1;
            }

            return false;
        }

#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
        /// <summary>
        /// Sets price text on the button according to localized price 
        /// </summary>
        /// <param name="iapTransaction"></param>
        void SetIAPTransactionPrice(IAPTransaction iapTransaction)
        {
            var productMetadata = TransactionManager.GetLocalizedIAPProductInfo(iapTransaction.productId);
            if (!string.IsNullOrEmpty(productMetadata.price))
            {
                SetContent(null, productMetadata.price);
            }
            else if (m_ShowDebugLogs)
            {
                Debug.LogError($"{nameof(PurchaseButton)} - Transaction Item \"{iapTransaction.displayName}\" localized price is empty or null.");
            }
        }
#endif

#if UNITY_EDITOR
        /// <summary>
        /// To update the price icon image and the price amount field on the PurchaseButton at editor time.
        /// </summary>
        void UpdateContentAtEditor()
        {
            // To avoid updating the content the prefab selected in the Project window
            if (PrefabUtility.IsPartOfPrefabAsset(gameObject))
            {
                return;
            }
            
            if (string.IsNullOrEmpty(m_TransactionKey))
            {
                return;
            }

            var transactionAsset = GameFoundationDatabaseSettings.database.transactionCatalog.FindItem(m_TransactionKey);
            if (transactionAsset == null)
            {
                SetContent(null, null);
            }

            if (transactionAsset is VirtualTransactionAsset vTransactionAsset)
            {
                if (DoesHaveMultipleCost(vTransactionAsset))
                {
                    Debug.LogWarning($"{nameof(PurchaseButton)} - Transaction item \"{transactionAsset.displayName}\" has multiple exchange item. {nameof(PurchaseButton)} can only show the first item on UI.");
                }

                GetVirtualCurrencyCostAsset(vTransactionAsset, 0, out var cost, out var itemAsset);
                if (itemAsset == null)
                {
                    GetVirtualItemCostAsset(vTransactionAsset, 0, out cost, out itemAsset);
                }

                if (cost > 0 && itemAsset != null)
                {
                    var sprite = itemAsset.GetDetail<AssetsDetailAsset>()?.GetAsset<Sprite>(m_PriceIconSpriteName);
                    if (sprite == null)
                    {
                        Debug.LogWarning($"{nameof(PurchaseButton)} - \"{itemAsset.displayName}\" transaction item doesn't have sprite called \"{m_PriceIconSpriteName}\"");
                    }

                    SetContent(sprite, cost.ToString());
                }
                else
                {
                    SetContent(null, noPriceString);
                }
            }
            else if (transactionAsset is IAPTransactionAsset iapTransactionAsset)
            {
                if (string.IsNullOrEmpty(iapTransactionAsset.productId))
                {
                    Debug.LogWarning($"{nameof(PurchaseButton)} - Transaction Item \"{transactionAsset.displayName}\" shouldn't have empty or null product id.");
                }

                SetContent(null, "N/A");
            }
        }

        void GetVirtualCurrencyCostAsset(VirtualTransactionAsset transaction, int indexOfCost, out long amount, out CatalogItemAsset costItemAsset)
        {
            var costs = transaction?.costs;
            if (costs?.GetCurrencies(m_CurrencyObjectsList) > indexOfCost)
            {
                var cost = costs.GetCurrency(indexOfCost);
                amount = cost.amount;
                costItemAsset = cost.currency;
                return;
            }

            amount = 0;
            costItemAsset = null;
        }

        void GetVirtualItemCostAsset(VirtualTransactionAsset transaction, int indexOfCost, out long amount, out CatalogItemAsset costItemAsset)
        {
            var costs = transaction?.costs;
            if (costs.GetItems(m_ItemObjectsList) > indexOfCost)
            {
                var cost = costs.GetItem(indexOfCost);
                amount = cost.amount;
                costItemAsset = cost.item;
                return;
            }

            amount = 0;
            costItemAsset = null;
        }

        bool DoesHaveMultipleCost(VirtualTransactionAsset transaction)
        {
            var costs = transaction?.costs;
            if (costs != null)
            {
                return (costs.GetItems(m_ItemObjectsList) + costs.GetCurrencies(m_CurrencyObjectsList)) > 1;
            }

            return false;
        }
#endif

        /// <summary>
        /// Checks the cost items and currencies to see whether there is enough quantity to complete the purchase.
        /// </summary>
        /// <returns>True if there is enough of the items in inventory and/or wallet for the purchase,
        /// false if there is not enough items.</returns>
        bool IsAffordable(VirtualTransaction transaction)
        {
            if (transaction == null)
            {
                return false;
            }

            ICollection<Exception> costExceptions = new List<Exception>();
            transaction.VerifyCost(costExceptions);

            return costExceptions.Count == 0;
        }

        /// <summary>
        /// Checks if this component's settings driven by other component.
        /// </summary>
        /// <returns>Specifies whether this component is driven by other component.</returns>
        internal bool IsDrivenByOtherComponent()
        {
            return m_IsDrivenByOtherComponent;
        }

        void ThrowIfNotInitialized()
        {
            if (!GameFoundation.IsInitialized)
            {
                throw new InvalidOperationException("Error: GameFoundation.Initialize() must be called before the PurchaseButton is used.");
            }
        }

        /// <summary>
        /// Listens to updates to the wallet
        /// and updates the button enabled state based on the new information.
        /// </summary>
        void OnWalletChanged(BalanceChangedEventArgs args)
        {
            UpdateAvailableToPurchaseState();
        }

        /// <summary>
        /// Listens to updates to the inventory
        /// and updates the button enabled state based on the new information.
        /// </summary>
        void OnInventoryChanged(InventoryItem item)
        {
            UpdateAvailableToPurchaseState();
        }

#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
        /// <summary>
        /// Gets triggered when the Purchasing Adapter is initialized successfully, 
        /// and update the button price label and enabled state based on the information
        /// </summary>
        void OnPurchasingAdapterInitializeSucceeded()
        {
            if (m_Transaction != null && m_Transaction is IAPTransaction iapTransaction)
            {
                SetIAPTransactionPrice(iapTransaction);
            }

            m_WillPurchasingAdapterInitialized = false;
        }

        /// <summary>
        /// Gets triggered when the Purchasing Adapter fails to be initialized.
        /// </summary>
        void OnPurchasingAdapterInitializeFailed(Exception exception)
        {
            if (m_ShowDebugLogs)
            {
                Debug.LogError($"{nameof(PurchaseButton)} - Transaction Key: \"{m_TransactionKey}\" - Error Message: {exception.Message}");
            }

            m_WillPurchasingAdapterInitialized = false;
        }
#endif
    }
}
