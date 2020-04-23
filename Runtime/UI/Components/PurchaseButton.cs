using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Promise;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.GameFoundation.CatalogManagement;
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
        /// The id of the Transaction Item being purchased.
        /// </summary>
        public string transactionId => m_TransactionId;
        
        [SerializeField] private string m_TransactionId;
        
        /// <summary>
        /// The sprite name for price icon that will be displayed on the button.
        /// </summary>
        public string priceIconSpriteName => m_PriceIconSpriteName;
        
        [SerializeField] private string m_PriceIconSpriteName = "purchase_button_icon";

        /// <summary>
        /// Use to enable or disable the button.
        /// </summary>
        public bool interactable { get => m_Interactable; set => SetInteractable(value); }
        
        [SerializeField] private bool m_Interactable = true;

        /// <summary>
        /// The Text component to assign the price text to.
        /// </summary>
        public Text priceTextField => m_PriceTextField;
        
        [SerializeField] private Text m_PriceTextField;
        
        /// <summary>
        /// The Image component to assign the Transaction Item's icon image to.
        /// </summary>
        public Image priceIconImageField => m_PriceIconImageField;
        
        [SerializeField] public Image m_PriceIconImageField;

        /// <summary>
        /// The string to display if the Transaction Item has no cost.
        /// </summary>
        [Space]
        [Tooltip("String to display when there is no cost defined in the Transaction Item.")]
        public string noPriceString = "FREE";
        
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
        [Serializable] public class TransactionSuccessEvent : UnityEvent<BaseTransaction> {}
        
        /// <summary>
        /// A callback for when a transaction is failed. Wraps UnityEvent and accepts a BaseTransaction and Exception as a parameter.
        /// </summary>
        [Serializable] public class TransactionFailureEvent : UnityEvent<BaseTransaction, Exception> {}

        /// <summary>
        /// The Transaction being purchased. 
        /// </summary>
        private BaseTransaction m_Transaction;
        
        /// <summary>
        /// The Button component attached to this PurchaseButton.
        /// </summary>
        private Button m_Button;
        
        /// <summary>
        /// Specifies whether the item is available to purchase.
        /// </summary>
        public bool availableToPurchaseState { get; private set; }

        /// <summary>
        /// Specifies whether the button is interactable internally.
        /// </summary>
        private bool m_InteractableInternal = true;
        
        /// <summary>
        /// Specifies whether the button is driven by Transaction Item View.
        /// </summary>
        private bool m_IsDrivenByTransactionItemView = false;

        /// <summary>
        /// Specifies whether the debug logs is visible.
        /// </summary>
        private bool m_ShowDebugLogs = false;
        
        /// <summary>
        /// Specifies whether purchasing adapter will be initialized.
        /// </summary>
        private bool m_WillPurchasingAdapterInitialized;
        
        
        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                InventoryManager.itemAdded += OnInventoryChanged;
                InventoryManager.itemRemoved += OnInventoryChanged;
                WalletManager.balanceChanged += OnWalletChanged;

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
            }
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                InventoryManager.itemAdded -= OnInventoryChanged;
                InventoryManager.itemRemoved -= OnInventoryChanged;
                WalletManager.balanceChanged -= OnWalletChanged;
                
                if (m_WillPurchasingAdapterInitialized)
                {
                    TransactionManager.purchasingAdapterInitializeSucceeded -= OnPurchasingAdapterInitializeSucceeded;
                    TransactionManager.purchasingAdapterInitializeFailed -= OnPurchasingAdapterInitializeFailed;
                }
            }
        }

        /// <summary>
        /// Gets the Button component of the PurchaseButton and sets the onClick listener to call <see cref="Purchase"/>.
        /// </summary>
        private void Awake()
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
        /// <param name="transactionId">The BaseTransaction id to be displayed.</param>
        /// <param name="priceIconSpriteName">The sprite name for price icon that will be displayed on the button. Default value is null.</param>
        internal void Init(string transactionId, string priceIconSpriteName)
        {
            if (string.IsNullOrEmpty(transactionId))
            {
                return;
            }

            m_TransactionId = transactionId;
            m_PriceIconSpriteName = priceIconSpriteName;

            SetTransactionId(transactionId);
        }

        /// <summary>
        /// Initializes PurchaseButton before the first frame update.
        /// If the it's already initialized by TransactionItemView no action will be taken.
        /// </summary>
        private void Start()
        {
            if (Application.isPlaying)
            {
                ThrowIfNotInitialized();
                UpdateButtonStatus();
            }

            m_IsDrivenByTransactionItemView = IsDrivenByTransactionItemView();

            if (!m_IsDrivenByTransactionItemView)
            {
                SetTransactionId(m_TransactionId);
            }
        }
        
        /// <summary>
        /// Updates which Transaction Item should be displayed by this button.
        /// </summary>
        /// <param name="transactionId">The BaseTransaction id that is displayed by the button.</param>
        /// <remarks>If the transactionId param is null or empty, or is not found in the transaction catalog no action will be taken.</remarks>
        public void SetTransactionId(string transactionId)
        {
            if (string.IsNullOrEmpty(transactionId))
            {
                Debug.LogError($"{nameof(PurchaseButton)} Given transaction Id shouldn't be empty or null.");
                return;
            }

            if (Application.isPlaying)
            {
                m_Transaction = GameFoundation.catalogs.transactionCatalog?.FindItem(transactionId);
                if (m_Transaction == null)
                {
                    Debug.LogError($"{nameof(PurchaseButton)} Requested transaction \"{transactionId}\" doesn't exist in Transaction Catalog.");
                    return;
                }
            }
            
            m_TransactionId = transactionId;
            
            UpdateContent();
        }

        /// <summary>
        /// Gets the BaseTransaction that is attached to the PurchaseButton.
        /// </summary>
        /// <returns>BaseTransaction currently attached to the PurchaseButton.</returns>
        public BaseTransaction GetTransaction()
        {
            return GameFoundation.catalogs.transactionCatalog.FindItem(m_TransactionId);
        }

        /// <summary>
        /// Calls <see cref="O:TransactionManager.BeginTransaction"/> with the purchase detail of the Transaction Item displayed in the button.
        /// Is automatically attached to the onClick event of the PurchaseButton.
        /// </summary>
        public void Purchase()
        {
            if (string.IsNullOrEmpty(m_TransactionId))
            {
                Debug.LogError($"{nameof(PurchaseButton)} Transaction Item is not defined.");
                return;
            }
            
            SetInteractableInternal(false);   
            
            StartCoroutine(ExecuteTransaction(m_Transaction));
        }
        
        /// <summary>
        ///  Execute the transaction with Coroutine since transaction uses deferred objects. 
        /// </summary>
        /// <param name="transaction">The Transaction being purchased.</param>
        /// <returns></returns>
        IEnumerator ExecuteTransaction(BaseTransaction transaction)
        {
            Deferred<TransactionResult> deferred = TransactionManager.BeginTransaction(transaction);

            if (m_ShowDebugLogs)
            {
                Debug.Log($"{nameof(PurchaseButton)} Now processing purchase: {transaction.displayName}");
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
                        Debug.Log($"{nameof(PurchaseButton)} Transaction is now on step {currentStep} of {deferred.totalSteps}");
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
                    Debug.LogError($"{nameof(PurchaseButton)} - Transaction Id:  {transaction.id} - Error Message: {deferred.error}");
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
                    Debug.Log($"player was awarded {currencyReward.amount} " + $"of currency '{currencyReward.currency.displayName}'");
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
        private void SetAvailableToPurchaseState(bool state)
        {
            if (availableToPurchaseState != state)
            {
                availableToPurchaseState = state;
                UpdateButtonStatus();
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
        
        private void SetInteractableInternal(bool active)
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
        private void UpdateButtonStatus()
        {
            m_Button.interactable = availableToPurchaseState && m_Interactable && m_InteractableInternal;
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
        private void SetContent(Sprite priceIcon, string priceText)
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
                Debug.LogWarning($"{nameof(PurchaseButton)} Icon Image Field is not defined.");
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
                Debug.LogWarning($"{nameof(PurchaseButton)} Price Text Field is not defined.");
            }
        }

        /// <summary>
        /// Updates the price icon and the price amount field on the PurchaseButton.
        /// </summary>
        private void UpdateContent()
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
        private void UpdateContentAtRuntime()
        {
            if (string.IsNullOrEmpty(m_TransactionId) || m_Transaction == null)
            {
                SetContent(null, null);
                
                SetAvailableToPurchaseState(false);
                return;
            }

            if (m_Transaction is VirtualTransaction vTransaction)
            {
                if (DoesHaveMultipleCost(vTransaction))
                {
                    Debug.LogWarning($"Transaction item \"{m_TransactionId}\" has multiple exchange item. {nameof(PurchaseButton)} can only show the first item on UI.");
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
                        Debug.LogWarning($"{nameof(PurchaseButton)} {item.displayName} doesn't have Sprite called {m_PriceIconSpriteName}");
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
                
                if (string.IsNullOrEmpty(iapTransaction.productId))
                {
                    Debug.LogError($"{nameof(PurchaseButton)} Transaction Item \"{transactionId}\" shouldn't have empty or null product id.");
                    SetAvailableToPurchaseState(false);
                }
                else if (TransactionManager.purchasingAdapterIsInitialized)
                {
                    SetIAPTransactionPrice(iapTransaction);
                }
                else
                {
                    m_WillPurchasingAdapterInitialized = true;
                    SetAvailableToPurchaseState(false);

                    TransactionManager.purchasingAdapterInitializeSucceeded += OnPurchasingAdapterInitializeSucceeded;
                    TransactionManager.purchasingAdapterInitializeFailed += OnPurchasingAdapterInitializeFailed;
                }
            }
        }

        private void GetVirtualCurrencyCost(VirtualTransaction transaction, int indexOfCost, out long amount, out CatalogItem costItem)
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
        
        private void GetVirtualItemCost(VirtualTransaction transaction, int indexOfCost, out long amount, out CatalogItem costItem)
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

        private bool DoesHaveMultipleCost(VirtualTransaction transaction)
        {
            var costs = transaction?.costs;
            if (costs != null)
            {
                return (costs.m_Items?.Length ?? 0) + (costs.m_Currencies?.Length ?? 0) > 1;
            }

            return false;
        }

        /// <summary>
        /// Sets price text on the button according to localized price 
        /// </summary>
        /// <param name="iapTransaction"></param>
        private void SetIAPTransactionPrice(IAPTransaction iapTransaction)
        {
            var productMetadata = TransactionManager.GetLocalizedIAPProductInfo(iapTransaction.productId);
            if (!string.IsNullOrEmpty(productMetadata.price))
            {
                SetContent(null, productMetadata.price);
                SetAvailableToPurchaseState(true);
            }
            else if (m_ShowDebugLogs)
            {
                Debug.LogError($"{nameof(PurchaseButton)} Transaction Item \"{transactionId}\" localized price is empty or null.");
            }
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// To update the price icon image and the price amount field on the PurchaseButton at editor time.
        /// </summary>
        private void UpdateContentAtEditor()
        {
            if (string.IsNullOrEmpty(m_TransactionId))
            {
                return;
            }
            
            var transactionAsset = GameFoundationDatabaseSettings.database.transactionCatalog.FindItem(m_TransactionId);
            if (transactionAsset == null)
            {
                SetContent(null, null);
            }
            
            if (transactionAsset is VirtualTransactionAsset vTransactionAsset)
            {
                if (DoesHaveMultipleCost(vTransactionAsset))
                {
                    Debug.LogWarning($"Transaction item \"{m_TransactionId}\" has multiple exchange item. {nameof(PurchaseButton)} can only show the first item on UI.");
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
                        Debug.LogWarning($"{nameof(PurchaseButton)} {itemAsset.displayName} transaction item doesn't have Sprite called {m_PriceIconSpriteName}");
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
                    Debug.LogWarning($"{nameof(PurchaseButton)} Transaction Item \"{transactionId}\" shouldn't have empty or null product id.");
                }
                
                SetContent(null, "N/A");
            }
        }
        
        private void GetVirtualCurrencyCostAsset(VirtualTransactionAsset transaction, int indexOfCost, out long amount, out CatalogItemAsset costItemAsset)
        {
            var costs = transaction?.costs;
            if (costs?.m_Currencies != null && costs.m_Currencies.Count > 0)
            {
                var cost = costs.GetCurrency(indexOfCost);
                amount = cost.amount;
                costItemAsset = cost.currency;
                return;
            }

            amount = 0;
            costItemAsset = null;
        }
        
        private void GetVirtualItemCostAsset(VirtualTransactionAsset transaction, int indexOfCost, out long amount, out CatalogItemAsset costItemAsset)
        {
            var costs = transaction?.costs;
            if (costs?.m_Items != null && costs.m_Items.Count > 0)
            {
                var cost = costs.GetItem(indexOfCost);
                amount = cost.amount;
                costItemAsset = cost.item;
                return;
            }

            amount = 0;
            costItemAsset = null;
        }

        private bool DoesHaveMultipleCost(VirtualTransactionAsset transaction)
        {
            var costs = transaction?.costs;
            if (costs != null)
            {
                return (costs.m_Items?.Count ?? 0) + (costs.m_Currencies?.Count ?? 0) > 1;
            }

            return false;
        }
#endif
        
        /// <summary>
        /// Checks the cost items and currencies to see whether there is enough quantity to complete the purchase.
        /// </summary>
        /// <returns>True if there is enough of the items in inventory and/or wallet for the purchase,
        /// false if there is not enough items.</returns>
        private bool IsAffordable(BaseTransaction transaction)
        {
            if (transaction == null)
            {
                return false;
            }

            if (transaction is IAPTransaction)
            {
                return true;
            }
            
            ICollection<Exception> costExceptions = new List<Exception>();
            ((VirtualTransaction)transaction).VerifyCost(costExceptions);

            return costExceptions.Count == 0;
        }

        /// <summary>
        /// Looks at the parent GameObjects to see if this button is a child of a TransactionItemView.
        /// </summary>
        /// <returns>Whether the this button is a child of a TransactionItemView or not.</returns>
        internal bool IsDrivenByTransactionItemView()
        {
            var currentParent = transform.parent;
            while(currentParent != null)
            {
                var itemView = currentParent.GetComponent<TransactionItemView>();
                if (itemView != null)
                {
                    return true;
                }
                currentParent = currentParent.parent;
            }

            return false;
        }
        
        private void ThrowIfNotInitialized()
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
        private void OnWalletChanged(Currency currency, long oldAmount, long newAmount)
        {
            SetAvailableToPurchaseState(IsAffordable(m_Transaction));
        }

        /// <summary>
        /// Listens to updates to the inventory
        /// and updates the button enabled state based on the new information.
        /// </summary>
        private void OnInventoryChanged(GameItem item)
        {
            SetAvailableToPurchaseState(IsAffordable(m_Transaction));
        }

        /// <summary>
        /// Gets triggered when the Purchasing Adapter is initialized successfully, 
        /// and update the button price label and enabled state based on the information
        /// </summary>
        private void OnPurchasingAdapterInitializeSucceeded()
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
        private void OnPurchasingAdapterInitializeFailed(Exception exception)
        {
            if (m_ShowDebugLogs)
            {
                Debug.LogError($"{nameof(PurchaseButton)} - Transaction Id:  {m_TransactionId} - Error Message: {exception.Message}");
            }

            m_WillPurchasingAdapterInitialized = false;
        }
        
    }
}
