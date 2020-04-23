using System;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.GameFoundation.CatalogManagement;
#endif

namespace UnityEngine.GameFoundation.UI
{
    /// <summary>
    /// Component that manages displaying a Currency's icon and quantity.
    /// When attached to a game object, it will display the Currency's icon and quantity.
    /// </summary>
    [AddComponentMenu("Game Foundation/Currency View", 4)]
    [ExecuteInEditMode]
    public class CurrencyView : MonoBehaviour
    {
        /// <summary>
        /// The id of the Currency to display.
        /// </summary>
        public string currencyId => m_CurrencyId;
        
        [SerializeField] private string m_CurrencyId = null;

        /// <summary>
        /// The sprite name for Currency icon that will be displayed on this view.
        /// </summary>
        public string iconSpriteName => m_IconSpriteName;
        
        [SerializeField] private string m_IconSpriteName = "item_icon";
        
        /// <summary>
        /// The Image component to assign the Currency icon image to.
        /// </summary>
        public Image iconImageField => m_IconImageField;
        
        [SerializeField] private Image m_IconImageField;
        
        /// <summary>
        /// The Text component to assign the Currency quantity to.
        /// </summary>
        public Text quantityTextField => m_QuantityTextField;
        
        [SerializeField] private Text m_QuantityTextField;


        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                WalletManager.balanceChanged += OnCurrencyChanged;
            }
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                WalletManager.balanceChanged -= OnCurrencyChanged;
            }
        }
        
        /// <summary>
        /// Initializes Currency with needed info.
        /// </summary>
        /// <param name="currencyId">The Currency id to be displayed.</param>
        /// <param name="priceIconSpriteName">The sprite name for item icon that will be displayed on this view.</param>
        internal void Init(string currencyId, string priceIconSpriteName)
        {
            if (string.IsNullOrEmpty(currencyId))
            {
                return;
            }
            
            m_CurrencyId = currencyId;
            m_IconSpriteName = priceIconSpriteName;
            
            UpdateContent();
        }

        /// <summary>
        /// Initializes CurrecnyView before the first frame update.
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
        /// Sets Currency should be displayed by this view.
        /// </summary>
        /// <param name="currencyId">The Currency id that should be displayed.</param>
        /// <remarks>If the currencyId param is null or empty, or is not found in the currency catalog no action will be taken.</remarks>
        public void SetCurrencyId(string currencyId)
        {
            if (string.IsNullOrEmpty(currencyId))
            {
                Debug.LogError($"{nameof(CurrencyView)} Given currency Id shouldn't be empty or null.");
                return;
            }
            
            if (Application.isPlaying)
            {
                var currency = GameFoundation.catalogs.currencyCatalog?.FindItem(currencyId);
                if (currency == null)
                {
                    Debug.LogError($"{nameof(CurrencyView)} Requested currency \"{currencyId}\" doesn't exist in Currency Catalog.");
                    return;
                }
            }

            m_CurrencyId = currencyId;

            UpdateContent();
        }

        /// <summary>
        /// Sets sprite name for item icon that will be displayed on this view.
        /// </summary>
        /// <param name="spriteName">The sprite name that is defined on Assets Detail for Currency icon sprite.</param>
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
        /// Sets the Image component to display Currency icon sprite on this view.
        /// </summary>
        /// <param name="image">The Image component to display Currency icon sprite.</param>
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
        /// Sets the Text component to display the Currency quantity on this view.
        /// </summary>
        /// <param name="text">The Text component to display the Currency quantity</param>
        public void SetQuantityTextField(Text text)
        {
            if (m_QuantityTextField == text)
            {
                return;
            }

            m_QuantityTextField = text;

            UpdateQuantity();
        }
        
        /// <summary>
        /// Updates the Currency icon and quantity on this view.
        /// </summary>
        private void UpdateContent()
        {
            UpdateIconSprite();
            UpdateQuantity();
        }
        
        /// <summary>
        /// Updates the Currency icon on this view.
        /// </summary>
        private void UpdateIconSprite()
        {
            if (string.IsNullOrEmpty(m_CurrencyId))
            {
                return;
            }
            
            if (Application.isPlaying)
            {
                var currency = GameFoundation.catalogs.currencyCatalog?.FindItem(m_CurrencyId);
                if (currency == null)
                {
                    return;
                }
                
                var sprite = currency.GetDetail<AssetsDetail>()?.GetAsset<Sprite>(m_IconSpriteName);
                SetIconSprite(sprite);
            }
#if UNITY_EDITOR
            else
            {
                var currencyCatalog = GameFoundationDatabaseSettings.database.currencyCatalog;
                if (currencyCatalog == null)
                {
                    return;
                }
            
                var currency = currencyCatalog.FindItem(m_CurrencyId);
                if (currency == null)
                {
                    return;
                }

                var sprite = currency.GetDetail<AssetsDetailAsset>()?.GetAsset<Sprite>(m_IconSpriteName);
                SetIconSprite(sprite);
            }
#endif
        }

        /// <summary>
        /// Sets sprite of item in display.
        /// </summary>
        /// <param name="iconSprite">The new sprite to display.</param>
        private void SetIconSprite(Sprite iconSprite)
        {
            if (iconImageField == null)
            {
                Debug.LogWarning($"{nameof(CurrencyView)} Icon Image field is not defined.");
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
        /// Updates the Currency quantity on this view.
        /// </summary>
        private void UpdateQuantity()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            
            var currency = GameFoundation.catalogs.currencyCatalog?.FindItem(m_CurrencyId);
            SetQuantity(currency != null ? WalletManager.GetBalance(currency) : 0);
        }

        /// <summary>
        /// Updates quantity of Currency item in label.
        /// </summary>
        /// <param name="quantity">The new quantity to display.</param>
        private void SetQuantity(long quantity)
        {
            if (m_QuantityTextField == null)
            {
                Debug.LogWarning($"{nameof(CurrencyView)} Item Quantity Text field is not defined.");
                return;
            }

            m_QuantityTextField.text = quantity.ToString();
            
#if UNITY_EDITOR
            EditorUtility.SetDirty(m_QuantityTextField);
#endif
        }
        
        private void ThrowIfNotInitialized()
        {
            if (!GameFoundation.IsInitialized)
            {
                throw new InvalidOperationException("Error: GameFoundation.Initialize() must be called before the CurrencyView is used.");
            }
        }

        /// <summary>
        /// Listens to updates from the Wallet that contains the item being displayed.
        /// If the item that has changed is the one being displayed it updates the quantity.
        /// </summary>
        private void OnCurrencyChanged(Currency currency, long oldBalance, long newBalance)
        {
            if (currency == null || currency.id != m_CurrencyId)
            {
                return;
            }

            UpdateQuantity();
        }
    }
}
