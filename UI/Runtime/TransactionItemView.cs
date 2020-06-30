using System;
using UnityEngine.Events;
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
    /// Component that manages displaying a Transaction Item's icon, display name and purchase button.
    /// When attached to a game object, it will display the Transaction Item's icon and displayName and create and display a
    /// PurchaseButton (<see cref="PurchaseButton"/>) to complete purchase transactions for it.
    /// </summary>
    [AddComponentMenu("Game Foundation/Transaction Item View", 2)]
    [ExecuteInEditMode]
    public class TransactionItemView : MonoBehaviour
    {
        /// <summary>
        /// The key of the Transaction Item being displayed.
        /// </summary>
        public string transactionKey => m_TransactionKey;

        [SerializeField, FormerlySerializedAs("m_TransactionId")]
        internal string m_TransactionKey;

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
        /// The Image component to assign the Transaction Item's icon image to.
        /// </summary>
        public Image itemIconImageField => m_ItemIconImageField;

        [SerializeField]
        internal Image m_ItemIconImageField;

        /// <summary>
        /// The Text component to assign the item's display name to.
        /// </summary>
        public Text itemNameTextField => m_ItemNameTextField;

        [SerializeField]
        internal Text m_ItemNameTextField;

        /// <summary>
        /// The PurchaseButton to set with the TransactionItem's purchase info.
        /// </summary>
        public PurchaseButton purchaseButton => m_PurchaseButton;

        [SerializeField]
        internal PurchaseButton m_PurchaseButton;

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
        /// The <see cref="BaseTransaction"/> to display in the view.
        /// </summary>
        BaseTransaction m_Transaction;

        /// <summary>
        /// Specifies whether this view is driven by other component
        /// </summary>
        bool m_IsDrivenByOtherComponent;
        
        /// <summary>
        /// Specifies whether the GameObject fields on the editor is visible.
        /// </summary>
        [SerializeField]
        internal bool showGameObjectEditorFields = true;

        void OnEnable()
        {
            if (Application.isPlaying)
            {
                TransactionManager.transactionSucceeded += OnTransactionSucceeded;
                TransactionManager.transactionFailed += OnTransactionFailed;
            }
        }

        void OnDisable()
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
        /// <param name="transactionKey">The transaction key to be displayed.</param>
        /// <param name="itemIconSpriteName">The sprite name for item icon that will be displayed on this view.</param>
        /// <param name="priceIconSpriteName">The sprite name for price icon that will be displayed on PurchaseButton.</param>
        internal void Init(string transactionKey, string itemIconSpriteName, string priceIconSpriteName)
        {
            Init(transactionKey, itemIconSpriteName, priceIconSpriteName, PurchaseButton.kDefaultNoPriceString);
        }
        
        /// <summary>
        /// Initializes TransactionItemView with needed info.
        /// </summary>
        /// <param name="transactionKey">The transaction key to be displayed.</param>
        /// <param name="itemIconSpriteName">The sprite name for item icon that will be displayed on this view.</param>
        /// <param name="priceIconSpriteName">The sprite name for price icon that will be displayed on PurchaseButton.</param>
        /// <param name="noPriceString">"The string to display on Purchase Button when there is no cost defined in the Transaction Item.</param>
        internal void Init(string transactionKey, string itemIconSpriteName, string priceIconSpriteName, string noPriceString)
        {
            if (string.IsNullOrEmpty(transactionKey))
            {
                return;
            }

            m_IsDrivenByOtherComponent = true;

            m_TransactionKey = transactionKey;
            m_ItemIconSpriteName = itemIconSpriteName;
            m_PriceIconSpriteName = priceIconSpriteName;
            m_NoPriceString = noPriceString;

            UpdateContent();
        }

        /// <summary>
        /// Initializes TransactionItemView before the first frame update.
        /// If the it's already initialized by StoreView no action will be taken.
        /// </summary>
        void Start()
        {
            if (Application.isPlaying)
            {
                ThrowIfNotInitialized();

                if (!m_IsDrivenByOtherComponent)
                {
                    UpdateContent();
                }
            }
        }

        /// <summary>
        /// Sets Transaction Item should be displayed by this view.
        /// </summary>
        /// <param name="key">The transaction identifier that should be displayed.</param>
        /// <remarks>If the key param is null or empty, or is not found in the transaction catalog no action will be taken.</remarks>
        public void SetTransactionKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError($"{nameof(TransactionItemView)} - Given Transaction Item key shouldn't be empty or null.");
                return;
            }

            if (m_TransactionKey == key)
            {
                return;
            }

            if (Application.isPlaying)
            {
                var transaction = GameFoundation.catalogs.transactionCatalog?.FindItem(key);
                if (transaction == null)
                {
                    Debug.LogError($"{nameof(TransactionItemView)} - Requested Transaction Item \"{key}\" key doesn't exist in Transaction Catalog.");
                    return;
                }
            }

            m_TransactionKey = key;

            UpdateContent();
        }

        /// <inheritdoc cref="SetTransactionKey(string)"/>
        [Obsolete("Use 'SetTransactionKey' instead", false)]
        public void SetTransactionId(string key) => SetTransactionKey(key);

        /// <summary>
        /// Gets the transaction that is displayed by the TransactionItemView.
        /// </summary>
        /// <returns>Transaction currently displayed in the view.</returns>
        public BaseTransaction GetTransaction()
        {
            return GameFoundation.catalogs.transactionCatalog.FindItem(m_TransactionKey);
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
        /// To update the item name, item icon, and PurchaseButton at runtime.
        /// </summary>
        void UpdateContentAtRuntime()
        {
            if (string.IsNullOrEmpty(m_TransactionKey))
            {
                return;
            }

            var itemDefinition = GameFoundation.catalogs.transactionCatalog.FindItem(m_TransactionKey);
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
                m_PurchaseButton.Init(m_TransactionKey, m_PriceIconSpriteName, m_NoPriceString);
            }
            else
            {
                Debug.LogWarning($"{nameof(TransactionItemView)} - Purchase Button is not defined.");
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// To update the item name, item icon, and PurchaseButton at editor time.
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

            var transactionCatalog = GameFoundationDatabaseSettings.database.transactionCatalog;
            if (transactionCatalog == null)
            {
                return;
            }

            var transaction = transactionCatalog.FindItem(m_TransactionKey);
            if (transaction == null)
            {
                return;
            }

            var sprite = transaction.GetDetail<AssetsDetailAsset>()?.GetAsset<Sprite>(m_ItemIconSpriteName);
            SetItemContent(sprite, transaction.displayName);

            if (m_PurchaseButton != null)
            {
                m_PurchaseButton.Init(m_TransactionKey, m_PriceIconSpriteName, m_NoPriceString);
            }
            else
            {
                Debug.LogWarning($"{nameof(TransactionItemView)} - Purchase Button is not defined.");
            }
        }
#endif

        private void SetItemContent(Sprite itemSprite, string itemDisplayName)
        {
            if (m_ItemIconImageField == null)
            {
                Debug.LogWarning($"{nameof(TransactionItemView)} - Item Icon Image Field is not defined.");
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
                Debug.LogWarning($"{nameof(TransactionItemView)} - Item Name Text Field is not defined.");
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
                throw new InvalidOperationException("Error: GameFoundation.Initialize() must be called before the TransactionItemView is used.");
            }
        }

        /// <summary>
        /// Gets triggered when any item in the store is successfully purchased. Triggers the
        /// user-specified onTransactionSucceeded callback.
        /// </summary>
        void OnTransactionSucceeded(BaseTransaction transaction, TransactionResult result)
        {
            if (m_TransactionKey == transaction.key)
            {
                onTransactionSucceeded?.Invoke(transaction);
            }
        }

        /// <summary>
        /// Gets triggered when any item in the store is attempted and fails to be purchased. Triggers the
        /// user-specified onTransactionFailed callback.
        /// </summary>
        void OnTransactionFailed(BaseTransaction transaction, Exception exception)
        {
            if (m_TransactionKey == transaction.key)
            {
                onTransactionFailed?.Invoke(transaction, exception);
            }
        }
    }
}
