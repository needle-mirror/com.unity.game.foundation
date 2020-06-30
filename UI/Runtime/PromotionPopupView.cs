using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.GameFoundation.DefaultCatalog.Details;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.GameFoundation.DefaultCatalog;
#endif

namespace UnityEngine.GameFoundation.UI
{
    /// <summary>
    /// Component that manages displaying a Transaction Item's promotion popup, including promotion image, display name and purchase button.
    /// When attached to a game object, it will display an image featuring the contents of the Transaction Item's rewards (or a specified image), the Transaction Item's displayName and create and display a
    /// PurchaseButton (<see cref="PurchaseButton"/>) to complete purchase transactions for it.
    /// </summary>
    [AddComponentMenu("Game Foundation/Promotion Popup", 2)]
    [ExecuteInEditMode]
    public class PromotionPopupView : MonoBehaviour
    {
        /// <summary>
        /// The key of the Transaction Item being displayed.
        /// </summary>
        public string transactionKey => m_TransactionKey;

        [SerializeField]
        internal string m_TransactionKey;
        
        /// <summary>
        /// Determines whether or not the promotion's image will be auto-generated.
        /// If true (default), the image will be generated using the sprite asset name's for each inventory or currency item listed in the transaction's Rewards.
        /// If false, the Promotion Sprite Name will be used to get the image from the transaction's Asset detail.
        /// </summary>
        public bool autoGeneratePromoImage => m_AutoGeneratePromoImage;

        [SerializeField, Space]
        internal bool m_AutoGeneratePromoImage = true;

        /// <summary>
        /// The string to prefix the reward inventory item counts when auto generating the promotion image.
        /// </summary>
        public string itemRewardCountPrefix => m_ItemRewardCountPrefix;

        [SerializeField]
        internal string m_ItemRewardCountPrefix = kDefaultCountPrefix;

        /// <summary>
        /// The string to prefix the reward currency counts when auto generating the promotion image.
        /// </summary>
        public string currencyRewardCountPrefix => m_CurrencyRewardCountPrefix;

        [SerializeField]
        internal string m_CurrencyRewardCountPrefix = kDefaultCountPrefix;

        /// <summary>
        /// The sprite asset name for the icon of the inventory or currency items listed in the transaction's Rewards, as specified in their Asset details.
        /// Used only when Auto Generate Promo Image is enabled.
        /// </summary>
        public string rewardItemIconSpriteName => m_RewardItemIconSpriteName;

        [SerializeField]
        internal string m_RewardItemIconSpriteName = "promotion_icon";

        /// <summary>
        /// The sprite asset name for the promotion image that will be displayed on this view, as specified in the transaction's Asset detail.
        /// Used only when Auto Generate Promo Image is disabled.
        /// </summary>
        public string promoImageSpriteName => m_PromoImageSpriteName;

        [SerializeField]
        internal string m_PromoImageSpriteName = "promotion_image";

        /// <summary>
        /// The property key for the promotion description to be displayed.
        /// </summary>
        public string descriptionPropertyKey => m_descriptionPropertyKey;

        [SerializeField, Space]
        internal string m_descriptionPropertyKey = "promotion_description";

        /// <summary>
        /// The property key for the badge to be displayed in callout.
        /// </summary>
        public string badgeTextPropertyKey => m_BadgeTextPropertyKey;

        [SerializeField]
        internal string m_BadgeTextPropertyKey = "promotion_badge";

        /// <summary>
        /// The sprite asset name for price icon that will be displayed on the PurchaseButton, as specified in the asset detail for the inventory item or currency listed in the price.
        /// </summary>
        public string priceIconSpriteName => m_PriceIconSpriteName;

        [SerializeField]
        internal string m_PriceIconSpriteName = "purchase_button_icon";
        
        
        /// <summary>
        /// The RewardItem prefab to use when auto generating promotion image.
        /// </summary>
        public ImageInfoView rewardItemPrefab => m_RewardItemPrefab;

        [SerializeField]
        internal ImageInfoView m_RewardItemPrefab;
        
        /// <summary>
        /// The GameObject to use as a separator between RewardItems when auto generating promotion image.
        /// </summary>
        public GameObject separatorPrefab => m_SeparatorPrefab;

        [SerializeField]
        internal GameObject m_SeparatorPrefab;
        

        /// <summary>
        /// The Text component to assign the transaction's display name to.
        /// </summary>
        public Text titleTextField => m_TitleTextField;

        [SerializeField, Space]
        internal Text m_TitleTextField;

        /// <summary>
        /// The Text component to assign the description to.
        /// </summary>
        public Text descriptionTextField => m_DescriptionTextField;

        [SerializeField]
        internal Text m_DescriptionTextField;

        /// <summary>
        /// The Transform in which to auto generate promotion image.
        /// /// Used only when Auto Generate Promo Image is enabled.
        /// </summary>
        public Transform autoGeneratedImageContainer => m_AutoGeneratedImageContainer;

        [SerializeField]
        internal Transform m_AutoGeneratedImageContainer;
        
        /// <summary>
        /// The Image component to assign the Promotion Image to.
        /// Used only when Auto Generate Promo Image is disabled.
        /// </summary>
        public Image promoImageField => m_PromoImageField;

        [SerializeField]
        internal Image m_PromoImageField;

        /// <summary>
        /// The ImageInfoView to assign the badge to.
        /// </summary>
        public ImageInfoView badgeField => m_BadgeField;

        [SerializeField]
        internal ImageInfoView m_BadgeField;

        /// <summary>
        /// The PurchaseButton to set with the TransactionItem's purchase info.
        /// </summary>
        public PurchaseButton purchaseButton => m_PurchaseButton;

        [SerializeField]
        internal PurchaseButton m_PurchaseButton;
        
        
        /// <summary>
        /// Callback that will get triggered when the Promotion Popup is opened.
        /// </summary>
        [Space]
        public PopupOpenedEvent onPopupOpened;

        /// <summary>
        /// Callback that will get triggered when the Promotion Popup is closed.
        /// </summary>
        public PopupClosedEvent onPopupClosed;

        /// <summary>
        /// A callback for when the Promotion Popup opens. Wraps UnityEvent
        /// </summary>
        [Serializable]
        public class PopupOpenedEvent : UnityEvent { }

        /// <summary>
        /// A callback for when the Promotion Popup closes. Wraps UnityEvent.
        /// </summary>
        [Serializable]
        public class PopupClosedEvent : UnityEvent { }

        /// <summary>
        /// A callback for when the Promotion Popup opens. Wraps UnityEvent
        /// </summary>
        [Serializable]
        public class PopupWillOpenEvent : UnityEvent { }

        /// <summary>
        /// A callback for when the Promotion Popup closes. Wraps UnityEvent.
        /// </summary>
        [Serializable]
        public class PopupWillCloseEvent : UnityEvent { }
        
        /// <summary>
        /// A callback that will get triggered if a purchase for any item in the store completes successfully.
        /// </summary>
        public TransactionSuccessEvent onTransactionSucceeded;

        /// <summary>
        /// A callback that will get triggered if a purchase for any item in the store fails.
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
        /// Default count prefix for inventory item and currency.
        /// </summary>
        private const string kDefaultCountPrefix = "x";
        
        /// <summary>
        /// A name to use when generating RewardItem GameObjects under Auto Generated Image Container.
        /// </summary>
        private const string kRewardItemGameObjectName = "Reward Item";
        
        /// <summary>
        /// A name to use when generating Separator GameObjects under Auto Generated Image Container
        /// </summary>
        private const string kSeparatorGameObjectName = "Separator";

        /// <summary>
        /// Specifies whether the debug logs is visible.
        /// </summary>
        private bool m_ShowDebugLogs = false;
        
        /// <summary>
        /// Specifies whether the GameObject fields on the editor is visible.
        /// </summary>
        [SerializeField]
        internal bool showGameObjectEditorFields = true;


        /// <summary>
        /// Adds listeners to the TransactionManager's transaction success and failure events, if the application is playing.
        /// </summary>
        void OnEnable()
        {
            if (Application.isPlaying)
            {
                TransactionManager.transactionSucceeded += OnTransactionSucceeded;
                TransactionManager.transactionFailed += OnTransactionFailed;
            }
        }

        /// <summary>
        /// Removes listeners to the TransactionManager's transaction success and failure events, if the application is playing.
        /// </summary>
        void OnDisable()
        {
            if (Application.isPlaying)
            {
                TransactionManager.transactionSucceeded -= OnTransactionSucceeded;
                TransactionManager.transactionFailed -= OnTransactionFailed;
            }
        }

        /// <summary>
        /// Initializes PromotionPopupView with needed info.
        /// </summary>
        /// <param name="transactionKey">The transaction key to be displayed.</param>
        /// <param name="descriptionPropertyKey">The property key for the promotion description that will be displayed on this view.</param>
        /// <param name="badgeTextPropertyKey">The property key for the promotion badge that will be displayed on this view.</param>
        /// <param name="priceIconSpriteName">The sprite asset name for the price icon that will be displayed on the PurchaseButton.</param>
        /// <param name="autoGeneratePromoImage">Boolean for whether the promotion image should be auto generated using the reward image sprite name or will be user provided via the promotion image sprite name.</param>
        /// <param name="rewardItemIconSpriteName">The sprite asset name for the item images to be used when auto generating promotion image on this view, can be null if autoGeneratePromoImage param is false.</param>
        /// <param name="promotionImageSpriteName">The sprite asset name for the promotion image that will be displayed on this view, can be null if autoGeneratePromoImage param is true.</param>
        /// <param name="itemRewardCountPrefix">The string to prefix inventory item count with.</param>
        /// <param name="currencyRewardCountPrefix">The string to prefix currency count with.</param>
        internal void Init(string transactionKey, string descriptionPropertyKey, string badgeTextPropertyKey, string priceIconSpriteName, bool autoGeneratePromoImage, string rewardItemIconSpriteName = null, string promotionImageSpriteName = null, string itemRewardCountPrefix = kDefaultCountPrefix, string currencyRewardCountPrefix = kDefaultCountPrefix)
        {
            if (string.IsNullOrEmpty(transactionKey))
            {
                Debug.LogError($"{nameof(PromotionPopupView)} - \"{nameof(transactionKey)}\" shouldn't be empty or null.");
                return;
            }

            m_TransactionKey = transactionKey;
            m_descriptionPropertyKey = descriptionPropertyKey;
            m_BadgeTextPropertyKey = badgeTextPropertyKey;
            m_PriceIconSpriteName = priceIconSpriteName;
            m_AutoGeneratePromoImage = autoGeneratePromoImage;
            m_RewardItemIconSpriteName = rewardItemIconSpriteName;
            m_PromoImageSpriteName = promotionImageSpriteName;
            m_ItemRewardCountPrefix = itemRewardCountPrefix;
            m_CurrencyRewardCountPrefix = currencyRewardCountPrefix;

            UpdateContent();
        }

        /// <summary>
        /// Initializes PromotionPopupView before the first frame update.
        /// Will trigger a PopupOpenedEvent (<see cref="PopupOpenedEvent"/>) if the prefab is active in hierarchy at start.
        /// </summary>
        void Start()
        {
            if (Application.isPlaying)
            {
                ThrowIfNotInitialized();

                UpdateContent();
                
                onPopupOpened?.Invoke();
            }
        }
        /// <summary>
        /// Displays the promotion popup.
        /// Will trigger a PopupOpenedEvent (<see cref="PopupOpenedEvent"/>).
        /// </summary>
        public void Open()
        {
            Open(m_TransactionKey, m_AutoGeneratePromoImage);
        }

        /// <summary>
        /// Displays the promotion popup.
        /// Will trigger a PopupOpenedEvent (<see cref="PopupOpenedEvent"/>).
        /// </summary>
        /// <param name="transactionKey">The key of the Transaction Item being displayed.</param>
        public void Open(string transactionKey)
        {
            Open(transactionKey, m_AutoGeneratePromoImage);
        }

        /// <summary>
        /// Displays the promotion popup.
        /// Will trigger a PopupOpenedEvent (<see cref="PopupOpenedEvent"/>). 
        /// </summary>
        /// <param name="transactionKey">The key of the Transaction Item being displayed.</param>
        /// <param name="autoGeneratePromoImage"></param>
        public void Open(string transactionKey, bool autoGeneratePromoImage)
        {
            if (string.IsNullOrEmpty(transactionKey))
            {
                Debug.LogError($"{nameof(PromotionPopupView)} - \"{nameof(transactionKey)}\" shouldn't be empty or null.");
                return;
            }
            
            if (gameObject.activeInHierarchy)
            {
                return;
            }

            m_TransactionKey = transactionKey;
            m_AutoGeneratePromoImage = autoGeneratePromoImage;

            gameObject.SetActive(true);
            
            UpdateContent();
            
            onPopupOpened?.Invoke();
        }

        /// <summary>
        /// Hides the promotion popup.
        /// Will trigger a PopupClosedEvent (<see cref="PopupClosedEvent"/>) and a PopupWillCloseEvent (<see cref="PopupWillCloseEvent"/>).
        /// </summary>
        public void Close()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            gameObject.SetActive(false);
            onPopupClosed?.Invoke();
        }

        /// <summary>
        /// Sets the Transaction Item that should be displayed by this view.
        /// </summary>
        /// <param name="key">The transaction identifier that should be displayed.</param>
        /// <remarks>If the key param is null or empty, or is not found in the transaction catalog no action will be taken.</remarks>
        public void SetTransactionKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError($"{nameof(PromotionPopupView)} - Given transaction Key shouldn't be empty or null.");
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
                    Debug.LogError($"{nameof(PromotionPopupView)} - Requested transaction \"{key}\" doesn't exist in Transaction Catalog.");
                    return;
                }
            }

            m_TransactionKey = key;

            UpdateContent();
        }

        /// <summary>
        /// Gets the transaction that is displayed by the PromotionPopupView.
        /// </summary>
        /// <returns>Transaction currently displayed in the view.</returns>
        public BaseTransaction GetTransaction()
        {
            return GameFoundation.catalogs.transactionCatalog.FindItem(m_TransactionKey);
        }
        
        /// <summary>
        /// Sets whether the image used for the promotion should be auto generated or use an asset from the transaction's Asset Detail.
        /// </summary>
        /// <param name="autoGenerateImage">If true it will auto generate the promotion image, ignoring the Promotion Image Sprite Name field (<see cref="promoImageSpriteName"/>). Field is true by default.
        /// If false, it will use the key specified in Promotion Image Sprite Name (<see cref="promoImageSpriteName"/>) to get the image from the transaction's Asset Detail.</param>
        public void SetAutoGenerateImage(bool autoGenerateImage)
        {
            m_AutoGeneratePromoImage = true;
            
            UpdateContent();
        }
        
        /// <summary>
        /// Set the image used for the promotion should be auto generated.
        /// </summary>
        /// <param name="itemIconSpriteName">The sprite asset name for the icon of the inventory or currency items listed in the transaction's Rewards, as specified in their Asset details.</param>
        /// <param name="itemCountPrefix">The string to prefix the reward inventory item counts when auto generating the promotion image.</param>
        /// <param name="currencyCountPrefix">The string to prefix the reward currency counts when auto generating the promotion image.</param>
        public void SetAutoGeneratePromotionImage(string itemIconSpriteName, string itemCountPrefix = kDefaultCountPrefix, string currencyCountPrefix = kDefaultCountPrefix)
        {
            if (!m_AutoGeneratePromoImage)
            {
                Debug.Log($"{nameof(PromotionPopupView)} - Auto-Generated Image is enabled");
            }
            
            m_AutoGeneratePromoImage = true;
            m_RewardItemIconSpriteName = itemIconSpriteName;
            m_ItemRewardCountPrefix = itemCountPrefix;
            m_CurrencyRewardCountPrefix = currencyCountPrefix;
            
            UpdateContent();
        }
        
        /// <summary>
        /// Sets the promotion image asset name that will be used when displaying the user-provided promotion image and disable Auto Generate Promo Image (<see cref="autoGeneratePromoImage"/>).
        /// </summary>
        /// <param name="spriteAssetName">The sprite name that is defined on the transaction item's Assets Detail for the promotion image.</param>
        public void SetPromotionImage(string spriteAssetName)
        {
            if (m_AutoGeneratePromoImage)
            {
                Debug.Log($"{nameof(PromotionPopupView)} - Auto-Generated Image is disabled");
            }
            
            m_AutoGeneratePromoImage = false;
            m_PromoImageSpriteName = spriteAssetName;

            UpdateContent();
        }

        /// <summary>
        /// Sets the static property key that will be used when displaying the promotion's description.
        /// </summary>
        /// <param name="propertyKey">The key that is defined in the transaction's static properties for the promotion's description.</param>
        public void SetDescriptionPropertyKey(string propertyKey)
        {
            if (string.IsNullOrEmpty(propertyKey) || m_descriptionPropertyKey == propertyKey)
            {
                return;
            }

            m_descriptionPropertyKey = propertyKey;

            UpdateContent();
        }

        /// <summary>
        /// Sets the static property key that will be used when displaying the promotion's badge.
        /// </summary>
        /// <param name="propertyKey">The key that is defined in the transaction's static properties for the promotion's badge.</param>
        public void SetBadgeTextPropertyKey(string propertyKey)
        {
            if (string.IsNullOrEmpty(propertyKey) || m_BadgeTextPropertyKey == propertyKey)
            {
                return;
            }

            m_BadgeTextPropertyKey = propertyKey;

            UpdateContent();
        }

        /// <summary>
        /// Sets sprite asset name for price icon that will be displayed on Purchase Button (<see cref="PurchaseButton"/>).
        /// </summary>
        /// <param name="spriteAssetName">The sprite asset name that is defined on Assets Detail for price icon sprite.</param>
        public void SetPriceIconSpriteName(string spriteAssetName)
        {
            if (string.IsNullOrEmpty(spriteAssetName) || m_PriceIconSpriteName == spriteAssetName)
            {
                return;
            }

            m_PriceIconSpriteName = spriteAssetName;

            UpdateContent();
        }

        /// <summary>
        /// Sets the Text component to display the transaction's display name on this view.
        /// </summary>
        /// <param name="text">The Text component to display the transaction's name.</param>
        public void SetTitleTextField(Text text)
        {
            if (m_TitleTextField == text)
            {
                return;
            }

            m_TitleTextField = text;

            UpdateContent();
        }

        /// <summary>
        /// Sets the Text component to display the promotion's description on this view.
        /// </summary>
        /// <param name="text">The Text component to display the promotion's description.</param>
        public void SetDescriptionTextField(Text text)
        {
            if (m_DescriptionTextField == text)
            {
                return;
            }

            m_DescriptionTextField = text;

            UpdateContent();
        }

        /// <summary>
        /// Sets the Transform in which to display the promotion's image on this view.
        /// </summary>
        /// <param name="container">The Transform in which to display the promotion's image.</param>
        public void SetAutoGeneratedImageContainer(Transform container)
        {
            if (m_AutoGeneratedImageContainer == container)
            {
                return;
            }

            m_AutoGeneratedImageContainer = container;

            UpdateContent();
        }
        
        /// <summary>
        /// Sets the Image component to display the promotion image on this view.
        /// </summary>
        /// <param name="image">The Image component to display the promotion.</param>
        public void SetPromoImageField(Image image)
        {
            if (m_PromoImageField == image)
            {
                return;
            }

            m_PromoImageField = image;

            UpdateContent();
        }

        /// <summary>
        /// Sets the Text component to display the promotion's badge on this view.
        /// </summary>
        /// <param name="badge">The Text component to display the promotion's badge.</param>
        public void SetBadgeField(ImageInfoView badge)
        {
            if (m_BadgeField == badge)
            {
                return;
            }

            m_BadgeField = badge;

            UpdateContent();
        }

        /// <summary>
        /// Sets PurchaseButton to be able to purchase Transaction Item by UI.
        /// </summary>
        /// <param name="purchaseButton">The PurchaseButton to display price and price icon and
        /// to be able to purchase the TransactionItem by using UI.</param>
        public void SetPurchaseButton(PurchaseButton purchaseButton)
        {
            if (m_PurchaseButton == purchaseButton)
            {
                return;
            }
            
            m_PurchaseButton = purchaseButton;

            UpdateContent();
        }
        
        /// <summary>
        /// The prefab to use for each when auto generating the promotion image.
        /// </summary>
        /// <param name="prefab">Reward Item prefab.</param>
        public void SetRewardItemPrefab(ImageInfoView prefab)
        {
            if (m_RewardItemPrefab == prefab)
            {
                return;
            }
            
            m_RewardItemPrefab = prefab;

            UpdateContent();
        }

        /// <summary>
        /// Sets the prefab to use for as a separator when when auto generating the promotion image.
        /// </summary>
        /// <param name="prefab">Separator prefab</param>
        public void SetSeparatorPrefab(GameObject prefab)
        {
            if (m_SeparatorPrefab == prefab)
            {
                return;
            }
            
            m_SeparatorPrefab = prefab;

            UpdateContent();
        }


        /// <summary>
        /// Updates the content displayed in the promotion Popup.
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
        /// Updates the transaction's display name, description, promotion image, badge, and PurchaseButton at runtime.
        /// </summary>
        void UpdateContentAtRuntime()
        {
            if (string.IsNullOrEmpty(m_TransactionKey))
            {
                return;
            }

            var transaction = GameFoundation.catalogs.transactionCatalog.FindItem(m_TransactionKey);
            if (transaction == null)
            {
                return;
            }
            
            // Get the values for transaction's text fields
            string descriptionText = null;
            string badgeText = null;

            if (transaction.TryGetStaticProperty(m_descriptionPropertyKey, out var descriptionProperty))
            {
                descriptionText = descriptionProperty.AsString();
            }
            else if (m_ShowDebugLogs)
            {
                Debug.LogWarning($"{nameof(PromotionPopupView)} - \"{transaction.displayName}\" transaction doesn't have Static Property called \"{m_descriptionPropertyKey}\"");
            }
            
            if (transaction.TryGetStaticProperty(m_BadgeTextPropertyKey, out var badgeProperty))
            {
                badgeText = badgeProperty.AsString();
            }
            else if (m_ShowDebugLogs)
            {
                Debug.LogWarning($"{nameof(PromotionPopupView)} - \"{transaction.displayName}\" transaction doesn't have Static Property called \"{m_BadgeTextPropertyKey}\"");
            }
            
            if (autoGeneratePromoImage)
            {
                SetContent(m_TransactionKey, transaction.displayName, descriptionText, badgeText, GetRewardIcons(transaction));
            }
            else
            {
                var promotionImage = transaction.GetDetail<AssetsDetail>()?.GetAsset<Sprite>(m_PromoImageSpriteName);
                if (promotionImage == null)
                {
                    Debug.LogWarning($"{nameof(PromotionPopupView)} - \"{transaction.displayName}\" doesn't have sprite called \"{m_PromoImageSpriteName}\"");    
                }
                
                SetContent(m_TransactionKey, transaction.displayName, descriptionText, badgeText, promotionImage);
            }
        }
        
        /// <summary>
        /// Gets a list of sprites, one for each inventory or currency item listed in the transaction's rewards list, at runtime.
        /// </summary>
        /// <param name="transaction">The transaction used for this promotion.</param>
        private List<Tuple<Sprite, string>> GetRewardIcons(BaseTransaction transaction)
        {
            var rewards = new List<Tuple<Sprite, string>>();
            
            var currencyExchanges = new List<CurrencyExchangeDefinition>();
            transaction.rewards.GetCurrencyExchanges(currencyExchanges);
            foreach (var currencyExchange in currencyExchanges)
            {
                var icon = currencyExchange.currency.GetDetail<AssetsDetail>()?.GetAsset<Sprite>(m_RewardItemIconSpriteName);
                if (icon != null)
                {
                    var quantity = (m_CurrencyRewardCountPrefix ?? "") + currencyExchange.amount;
                    rewards.Add(new Tuple<Sprite, string>(icon, quantity));
                }
                else
                {
                    Debug.LogWarning($"{nameof(PromotionPopupView)} - The \"{transaction.displayName}\" transaction's \"{currencyExchange.currency.displayName}\" reward does not have an asset with the name \"{m_RewardItemIconSpriteName}\" so it will not be showed in the promotion");
                }
            }
            
            var itemExchangeDefinitions = new List<ItemExchangeDefinition>();
            transaction.rewards.GetItemExchanges(itemExchangeDefinitions);
            foreach (var itemExchange in itemExchangeDefinitions)
            {
                var icon = itemExchange.item.GetDetail<AssetsDetail>()?.GetAsset<Sprite>(m_RewardItemIconSpriteName);
                if (icon != null)
                {
                    var quantity = (m_ItemRewardCountPrefix ?? "") + itemExchange.amount;
                    rewards.Add(new Tuple<Sprite, string>(icon, quantity));
                }
                else
                {
                    Debug.LogWarning($"{nameof(PromotionPopupView)} - The \"{transaction.displayName}\" transaction's \"{itemExchange.item.displayName}\" reward does not have an asset with the name \"{m_RewardItemIconSpriteName}\" so it will not be showed in the promotion");
                }
            }

            return rewards;
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// Updates the transaction's display name, description, promotion image, badge, and PurchaseButton at editor time.
        /// </summary>
        void UpdateContentAtEditor()
        {
            // Known Issue: It's temporary protection for editor time to avoid generating GameObjects(RewardItem) in Prefab Asset.
            if (!PrefabUtility.IsPartOfPrefabInstance(gameObject))
            {
                return;
            }
            
            if (string.IsNullOrEmpty(m_TransactionKey))
            {
                return;
            }
            
            var transaction = GameFoundationDatabaseSettings.database.transactionCatalog.FindItem(m_TransactionKey);
            if (transaction == null)
            {
                return;
            }
             
            // Get the values for transaction's text fields
            string descriptionText = null;
            string badgeText = null;

            var properties = transaction.GetStaticProperties();

            if (!string.IsNullOrEmpty(m_descriptionPropertyKey))
            {
                var descriptionIndex = properties.FindIndex(x => x.key == m_descriptionPropertyKey);
                if (descriptionIndex >= 0)
                {
                    descriptionText = properties[descriptionIndex].value.AsString();
                }
                else if (m_ShowDebugLogs)
                {
                    Debug.LogWarning(
                        $"{nameof(PromotionPopupView)} - \"{transaction.displayName}\" transaction doesn't have Static Property called \"{m_descriptionPropertyKey}\"");
                }
            }

            if (!string.IsNullOrEmpty(m_BadgeTextPropertyKey))
            {
                var badgeIndex = properties.FindIndex(x => x.key == m_BadgeTextPropertyKey);
                if (badgeIndex >= 0)
                {
                    badgeText = properties[badgeIndex].value.AsString();
                }
                else if (m_ShowDebugLogs)
                {
                    Debug.LogWarning(
                        $"{nameof(PromotionPopupView)} - \"{transaction.displayName}\" transaction doesn't have Static Property called \"{m_BadgeTextPropertyKey}\"");
                }
            }

            if (m_AutoGeneratePromoImage)
            {
                SetContent(m_TransactionKey, transaction.displayName, descriptionText, badgeText, GetRewardIcons(transaction));
            }
            else
            {
                var promotionImage = transaction.GetDetail<AssetsDetailAsset>()?.GetAsset<Sprite>(m_PromoImageSpriteName);
                if (promotionImage == null)
                {
                    Debug.LogWarning($"{nameof(PromotionPopupView)} - \"{transaction.displayName}\" doesn't have sprite called \"{m_PromoImageSpriteName}\"");    
                }
                
                SetContent(m_TransactionKey, transaction.displayName, descriptionText, badgeText, promotionImage);
            }
        }
        
        /// <summary>
        /// Gets a list of sprites, one for each inventory or currency item listed in the transaction's rewards list, at editor time.
        /// </summary>
        /// <param name="transactionAsset">The transaction used for this promotion.</param>
        private List<Tuple<Sprite, string>>  GetRewardIcons(BaseTransactionAsset transactionAsset)
        {
            if (transactionAsset == null)
            {
                return null;
            }
            
            var rewards = new List<Tuple<Sprite, string>>();
            
            var currencyExchangeObjects = new List<CurrencyExchangeObject>();
            transactionAsset.rewards.GetCurrencies(currencyExchangeObjects);
            foreach (var currencyExchangeObject in currencyExchangeObjects)
            {
                var icon = currencyExchangeObject.currency.GetDetail<AssetsDetailAsset>()?.GetAsset<Sprite>(m_RewardItemIconSpriteName);
                if (icon != null)
                {
                    var quantity = (m_CurrencyRewardCountPrefix?? "") + currencyExchangeObject.amount;
                    rewards.Add(new Tuple<Sprite, string>(icon, quantity));
                }
                else
                {
                    Debug.LogWarning($"{nameof(PromotionPopupView)} - The \"{transactionAsset.displayName}\" transaction's \"{currencyExchangeObject.currency.displayName}\" reward does not have an asset with the name \"{m_RewardItemIconSpriteName}\" so it will not be showed in the promotion");
                }
            }
            
            var itemExchangeDefinitionObjects = new List<ItemExchangeDefinitionObject>();
            transactionAsset.rewards.GetItems(itemExchangeDefinitionObjects);
            foreach (var itemExchangeObject in itemExchangeDefinitionObjects)
            {
                var icon = itemExchangeObject.item.GetDetail<AssetsDetailAsset>()?.GetAsset<Sprite>(m_RewardItemIconSpriteName);
                if (icon != null)
                {
                    var quantity = (m_ItemRewardCountPrefix?? "") + itemExchangeObject.amount;
                    rewards.Add(new Tuple<Sprite, string>(icon, quantity));
                }
                else
                {
                    Debug.LogWarning($"{nameof(PromotionPopupView)} - The {transactionAsset.displayName} transaction's \"{itemExchangeObject.item.displayName}\" reward does not have an asset with the name \"{m_RewardItemIconSpriteName}\" so it will not be showed in the promotion");
                }
            }

            return rewards;
        }
#endif

        /// <summary>
        /// Sets popup content with user-provided promotion image.
        /// </summary>
        /// <param name="transactionKey">The key of the Transaction Item being displayed.</param>
        /// <param name="title">Title of the popup</param>
        /// <param name="description">Promotion description</param>
        /// <param name="badgeText">Badge info. If it's empty or null, badge will be hidden.</param>
        /// <param name="promotionImage">Image for promotion</param>
        void SetContent(string transactionKey, string title, string description, string badgeText, Sprite promotionImage)
        {
            SetTextContent(title, description, badgeText);
            SetPurchaseButton(transactionKey, m_PriceIconSpriteName);
            
            ClearPromotionImages();
            
            if (m_AutoGeneratedImageContainer != null)
            {
                m_AutoGeneratedImageContainer.gameObject.SetActive(false);
            }

            if (m_PromoImageField != null)
            {
                m_PromoImageField.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"{nameof(PromotionPopupView)} - Promotion Image Field is not defined.");
            }

            if (promotionImage != null)
            {
                m_PromoImageField.sprite = promotionImage;
                m_PromoImageField.SetNativeSize();
                
                // To force rebuilt Layouts at Editor and Run time
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform) m_PromoImageField.transform);
            }
        }

        /// <summary>
        /// Sets popup content with Transaction Item's rewards
        /// </summary>
        /// <param name="transactionKey">The key of the Transaction Item being displayed.</param>
        /// <param name="title">Title of the popup</param>
        /// <param name="description">Promotion description</param>
        /// <param name="badgeText">Badge info. If it's empty or null, badge will be hidden.</param>
        /// <param name="rewards">Sprite and quantity sets of rewards.</param>
        private void SetContent(string transactionKey, string title, string description, string badgeText, IList<Tuple<Sprite, string>> rewards)
        {
            SetTextContent(title, description, badgeText);
            SetPurchaseButton(transactionKey, m_PriceIconSpriteName);

            ClearPromotionImages();

            if (m_PromoImageField != null)
            {
                m_PromoImageField.gameObject.SetActive(false);
            }

            if (m_AutoGeneratedImageContainer != null)
            {
                m_AutoGeneratedImageContainer.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"{nameof(PromotionPopupView)} - Auto-Generated Image Container should be defined to generate Promo Image");
            }

            if (rewardItemPrefab == null)
            {
                Debug.LogWarning($"{nameof(PromotionPopupView)} - Reward Item Prefab should be defined to generate Promo Image, ");
            }
            else if (m_AutoGeneratedImageContainer != null)
            {
                var count = rewards.Count;
                for (var i = 0; i < count; i++)
                {
                    var reward = rewards[i];

                    if (i > 0 && count > 1 && separatorPrefab != null)
                    {
                        var separator = Instantiate(separatorPrefab, m_AutoGeneratedImageContainer);
                        separator.transform.localScale = Vector3.one;
                        separator.name = kSeparatorGameObjectName;
                    }

                    var rewardItem = Instantiate(rewardItemPrefab, m_AutoGeneratedImageContainer)
                        .GetComponent<ImageInfoView>();
                    rewardItem.transform.localScale = Vector3.one;
                    rewardItem.name = kRewardItemGameObjectName;
                    rewardItem.SetView(reward.Item1, reward.Item2);
                }
                
                // To force rebuilt Layouts at Editor and Run time    
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform) m_AutoGeneratedImageContainer.parent);
            }
        }

        /// <summary>
        /// Clears auto-generated Container and user-provided image
        /// </summary>
        private void ClearPromotionImages()
        {
            if (m_AutoGeneratedImageContainer != null)
            {
                var toRemove = new List<Transform>();
                
                foreach (Transform child in m_AutoGeneratedImageContainer)
                {
                    if (child.name == kRewardItemGameObjectName || child.name == kSeparatorGameObjectName)
                    {
                        toRemove.Add(child);
                    }
                }

                for (int i = 0; i < toRemove.Count; i++)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(toRemove[i].gameObject);
                    }
                    else
                    {
                        DestroyImmediate(toRemove[i].gameObject);
                    }
                }
            }
            
            if (m_PromoImageField != null)
            {
                m_PromoImageField.sprite = null;
            }
        }

        /// <summary>
        /// Set text fields of popup content.
        /// </summary>
        /// <param name="title">Title of the popup</param>
        /// <param name="description">Promotion description</param>
        /// <param name="badgeText">Badge info. If it's empty or null, badge will be hidden.</param>
        void SetTextContent(string title, string description, string badgeText)
        {
            if (m_TitleTextField != null)
            {
                m_TitleTextField.text = title;
            }

            if (m_DescriptionTextField != null)
            {
                m_DescriptionTextField.text = description;
                m_DescriptionTextField.gameObject.SetActive(!string.IsNullOrEmpty(description));
            }

            if (m_BadgeField != null)
            {
                m_BadgeField.SetText(badgeText);
                m_BadgeField.gameObject.SetActive(!string.IsNullOrEmpty(badgeText));
            }
        }

        /// <summary>
        /// Set Purchase Button
        /// </summary>
        /// <param name="transactionKey">The key of the Transaction Item being displayed.</param>
        /// <param name="priceIconSpriteName">Sprite asset name for price icon on Purchase Button</param>
        void SetPurchaseButton(string transactionKey, string priceIconSpriteName)
        {
            // Purchase Item Button
            if (m_PurchaseButton != null)
            {
                m_PurchaseButton.Init(transactionKey, priceIconSpriteName);
            }
            else
            {
                Debug.LogWarning($"{nameof(PromotionPopupView)} - {nameof(PurchaseButton)} is not defined.");
            }
        }

        /// <summary>
        /// Throws an Invalid Operation Exception if GameFoundation has not been initialized before the promotion popup view is used.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        void ThrowIfNotInitialized()
        {
            if (!GameFoundation.IsInitialized)
            {
                throw new InvalidOperationException("Error: GameFoundation.Initialize() must be called before the PromotionPopupView is used.");
            }
        }

        /// <summary>
        /// Gets triggered when the promotion popup transaction is successfully purchased. Triggers the
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
        /// Gets triggered when the promotion popup transaction is attempted and fails to be purchased. Triggers the
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
