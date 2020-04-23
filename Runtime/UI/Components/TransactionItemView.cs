using System;
using UnityEngine.UI;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.GameFoundation.CatalogManagement;
#endif

namespace UnityEngine.GameFoundation.UI
{
    /// <summary>
    /// Component that manages displaying a Transaction Item.'s icon, display name and purchase button.
    /// When attached to a game object, it will display the Transaction Item's icon and displayName and create and display a
    /// PurchaseButton (<see cref="PurchaseButton"/>) to complete purchase transactions for it.
    /// </summary>
    [AddComponentMenu("Game Foundation/Transaction Item View", 2)]
    [ExecuteInEditMode]
    public class TransactionItemView : MonoBehaviour
    {
        /// <summary>
        /// The id of the Transaction Item being displayed.
        /// </summary>
        public string transactionId => m_TransactionId;
        
        [SerializeField] private string m_TransactionId;

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
        /// The Image component to assign the Transaction Item's icon image to.
        /// </summary>
        public Image itemIconImageField => m_ItemIconImageField;
        
        [SerializeField] private Image m_ItemIconImageField;
        
        /// <summary>
        /// The Text component to assign the price text to.
        /// </summary>
        public Text itemNameTextField => m_ItemNameTextField;
        
        [SerializeField] private Text m_ItemNameTextField;
        
        /// <summary>
        /// The PurchaseButton to set with the TransactionItem's purchase info.
        /// </summary>
        public PurchaseButton purchaseButton => m_PurchaseButton;
        
        [SerializeField] private PurchaseButton m_PurchaseButton;
        
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
        /// The <see cref="BaseTransaction"/> to display in the view.
        /// </summary>
        private BaseTransaction m_Transaction;
        
        /// <summary>
        /// Specifies whether this view is driven by Store View.
        /// </summary>
        private bool m_IsDrivenByStoreView = false;
        
       
        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                TransactionManager.transactionSucceeded += OnTransactionSucceeded;
                TransactionManager.transactionFailed += OnTransactionFailed;   
            }
        }
        
        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                TransactionManager.transactionSucceeded -= OnTransactionSucceeded;
                TransactionManager.transactionFailed -= OnTransactionFailed;   
            }
        }

        /// <summary>
        /// Initializes TransactionItemView with needed info.
        /// </summary>
        /// <param name="transactionId">The transaction id to be displayed.</param>
        /// <param name="itemIconSpriteName">The sprite name for item icon that will be displayed on this view.</param>
        /// <param name="priceIconSpriteName">The sprite name for price icon that will be displayed on PurchaseButton.</param>
        internal void Init(string transactionId, string itemIconSpriteName, string priceIconSpriteName)
        {
            if (string.IsNullOrEmpty(transactionId))
            {
                return;
            }

            m_IsDrivenByStoreView = true;

            m_TransactionId = transactionId;
            m_ItemIconSpriteName = itemIconSpriteName;
            m_PriceIconSpriteName = priceIconSpriteName;

            UpdateContent();
        }

        /// <summary>
        /// Initializes TransactionItemView before the first frame update.
        /// If the it's already initialized by StoreView no action will be taken.
        /// </summary>
        private void Start()
        {
            if (Application.isPlaying)
            {
                ThrowIfNotInitialized();   
            }

            if (!m_IsDrivenByStoreView)
            {
                UpdateContent();
            }
        }

        /// <summary>
        /// Sets Transaction Item should be displayed by this view.
        /// </summary>
        /// <param name="transactionId">The transaction id that should be displayed.</param>
        /// <remarks>If the transactionId param is null or empty, or is not found in the transaction catalog no action will be taken.</remarks>
        public void SetTransactionId(string transactionId)
        {
            if (string.IsNullOrEmpty(transactionId))
            {
                Debug.LogError($"{nameof(TransactionItemView)} Given transaction Id shouldn't be empty or null.");
                return;
            }

            if (m_TransactionId == transactionId)
            {
                return;
            }

            if (Application.isPlaying)
            {
                var transaction = GameFoundation.catalogs.transactionCatalog?.FindItem(transactionId);
                if (transaction == null)
                {
                    Debug.LogError($"{nameof(TransactionItemView)} Requested transaction \"{transactionId}\" doesn't exist in Transaction Catalog.");
                    return;
                }
            }

            m_TransactionId = transactionId;

            UpdateContent();
        }

        /// <summary>
        /// Gets the transaction that is displayed by the TransactionItemView.
        /// </summary>
        /// <returns>Transaction currently displayed in the view.</returns>
        public BaseTransaction GetTransaction()
        {
            return GameFoundation.catalogs.transactionCatalog.FindItem(m_TransactionId);
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

                if (m_PurchaseButton != null)
                {
                    m_PurchaseButton.interactable = interactable;
                }
            }
        }
        
        /// <summary>
        /// Sets the Text component to display item name on this view.
        /// </summary>
        /// <param name="text">The Text component to display the item name.</param>
        public void SetItemNameTextField(Text text)
        {
            if (m_ItemNameTextField == text)
            {
                return;
            }

            m_ItemNameTextField = text;

            UpdateContent();
        }
        
        /// <summary>
        /// Sets the Image component to display item icon sprite on this view.
        /// </summary>
        /// <param name="image">The Image component to display item icon sprite.</param>
        public void SetItemIconImageField(Image image)
        {
            if (m_ItemIconImageField == image)
            {
                return;
            }

            m_ItemIconImageField = image;

            UpdateContent();
        }
        
        /// <summary>
        /// Sets sprite name for item icon that will be displayed on this view.
        /// </summary>
        /// <param name="spriteName">The sprite name that is defined on Assets Detail for item icon sprite.</param>
        public void SetItemIconSpriteName(string spriteName)
        {
            if (string.IsNullOrEmpty(spriteName) || m_ItemIconSpriteName == spriteName)
            {
                return;
            }

            m_ItemIconSpriteName = spriteName;

            UpdateContent();
        }
        
        /// <summary>
        /// Sets sprite name for price icon that will be displayed on this view.
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
        /// Sets PurchaseButton to be able to purchase Transaction Item by UI.
        /// </summary>
        /// <param name="purchaseButton">The PurchaseButton to display price and price icon and
        /// to be able to purchase the TransactionItem by using UI.</param>
        public void SetPurchaseButton(PurchaseButton purchaseButton)
        {
            m_PurchaseButton = purchaseButton;

            UpdateContent();
        }
        
        /// <summary>
        /// Updates the item name, item icon, and PurchaseButton.
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
        /// To update the item name, item icon, and PurchaseButton at runtime.
        /// </summary>
        private void UpdateContentAtRuntime()
        {
            if (string.IsNullOrEmpty(m_TransactionId))
            {
                return;
            }
            
            var itemDefinition = GameFoundation.catalogs.transactionCatalog.FindItem(m_TransactionId);
            if (itemDefinition == null)
            {
                return;
            }

            // Purchase Item Image
            Sprite itemSprite = itemDefinition.GetDetail<AssetsDetail>()?.GetAsset<Sprite>(m_ItemIconSpriteName);
            SetItemContent(itemSprite, itemDefinition.displayName);

            // Purchase Item Button
            if (m_PurchaseButton != null)
            {
                m_PurchaseButton.Init(m_TransactionId, m_PriceIconSpriteName);
            }
            else
            {
                Debug.LogWarning($"{nameof(TransactionItemView)} Purchase Button is not defined.");
            }
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// To update the item name, item icon, and PurchaseButton at editor time.
        /// </summary>
        private void UpdateContentAtEditor()
        {
            if (string.IsNullOrEmpty(m_TransactionId))
            {
                return;
            }
            
            var transactionCatalog = GameFoundationDatabaseSettings.database.transactionCatalog;
            if (transactionCatalog == null)
            {
                return;
            }
            
            var transaction = transactionCatalog.FindItem(m_TransactionId);
            if (transaction == null)
            {
                return;
            }

            var sprite = transaction.GetDetail<AssetsDetailAsset>()?.GetAsset<Sprite>(m_ItemIconSpriteName);
            SetItemContent(sprite, transaction.displayName);

            if (m_PurchaseButton != null)
            {
                m_PurchaseButton.Init(m_TransactionId, m_PriceIconSpriteName);
            }
            else
            {
                Debug.LogWarning($"{nameof(TransactionItemView)} Purchase Button is not defined.");
            }
        }
#endif
        
        internal void SetItemContent(Sprite itemSprite, string itemDisplayName)
        {
            if (m_ItemIconImageField == null)
            {
                Debug.LogWarning($"{nameof(TransactionItemView)} Item Icon Image Field is not defined.");
            }
            else if (m_ItemIconImageField.sprite != itemSprite)
            {
                m_ItemIconImageField.sprite = itemSprite;
                m_ItemIconImageField.SetNativeSize();
                
#if UNITY_EDITOR
                EditorUtility.SetDirty(m_ItemIconImageField);
#endif
            }

            if (m_ItemNameTextField == null)
            {
                Debug.LogWarning($"{nameof(TransactionItemView)} Item Name Text Field is not defined.");
            }
            else if (m_ItemNameTextField.text != itemDisplayName)
            {
                m_ItemNameTextField.text = itemDisplayName;
#if UNITY_EDITOR
                EditorUtility.SetDirty(m_ItemNameTextField);
#endif
            }
        }
        
        /// <summary>
        /// Looks at the parent GameObjects to see if this view is a child of a StoreView.
        /// </summary>
        /// <returns>Whether this view is a child of a StoreView or not.</returns>
        internal bool IsDrivenByStoreView()
        {
            return m_IsDrivenByStoreView;
        }
        
        private void ThrowIfNotInitialized()
        {
            if (!GameFoundation.IsInitialized)
            {
                throw new InvalidOperationException("Error: GameFoundation.Initialize() must be called before the TransactionItemView is used.");
            }
        }

        /// <summary>
        /// Gets triggered when any item in the store is successfully purchased. Triggers the
        /// user-specified onTransactionSucceeded callback.
        /// </summary>
        private void OnTransactionSucceeded(BaseTransaction transaction, TransactionResult result)
        {
            if (m_TransactionId == transaction.id)
            {
                onTransactionSucceeded?.Invoke(transaction);   
            }
        }

        /// <summary>
        /// Gets triggered when any item in the store is attempted and fails to be purchased. Triggers the
        /// user-specified onTransactionFailed callback.
        /// </summary>
        private void OnTransactionFailed(BaseTransaction transaction, Exception exception)
        {
            if (m_TransactionId == transaction.id)
            {
                onTransactionFailed?.Invoke(transaction, exception);   
            }
        }
    }
}
