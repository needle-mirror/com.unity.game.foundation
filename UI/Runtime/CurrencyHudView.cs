using System;
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
    /// Component that manages displaying a Currency's icon and quantity.
    /// When attached to a game object, it will display the Currency's icon and quantity.
    /// </summary>
    [AddComponentMenu("Game Foundation/Currency Hud", 4)]
    [ExecuteInEditMode]
    public class CurrencyHudView : MonoBehaviour
    {
        /// <summary>
        /// The identifier of the Currency to display.
        /// </summary>
        public string currencyKey => m_CurrencyKey;

        /// <inheritdoc cref="currencyKey"/>
        [Obsolete("Use 'currencyKey' instead", false)]
        public string currencyId => currencyKey;

        /// <inheritdoc cref="currencyKey"/>
        [SerializeField, FormerlySerializedAs("m_CurrencyId")]
        internal string m_CurrencyKey;

        /// <summary>
        /// The sprite name for Currency icon that will be displayed on this view.
        /// </summary>
        public string iconSpriteName => m_IconSpriteName;

        [SerializeField]
        internal string m_IconSpriteName = "hud_icon";

        /// <summary>
        /// The Image component to assign the Currency icon image to.
        /// </summary>
        public Image iconImageField => m_IconImageField;

        [SerializeField]
        internal Image m_IconImageField;

        /// <summary>
        /// The Text component to assign the Currency quantity to.
        /// </summary>
        public Text quantityTextField => m_QuantityTextField;

        [SerializeField]
        internal Text m_QuantityTextField;
        
        /// <summary>
        /// Specifies whether the GameObject fields on the editor is visible.
        /// </summary>
        [SerializeField]
        internal bool showGameObjectEditorFields = true;

        void OnEnable()
        {
            if (Application.isPlaying)
            {
                WalletManager.balanceChanged += OnCurrencyChanged;
            }
        }

        void OnDisable()
        {
            if (Application.isPlaying)
            {
                WalletManager.balanceChanged -= OnCurrencyChanged;
            }
        }

        /// <summary>
        /// Initializes Currency with needed info.
        /// </summary>
        /// <param name="currencyKey">The Currency identifier to be displayed.</param>
        /// <param name="priceIconSpriteName">The sprite name for item icon that will be displayed on this view.</param>
        internal void Init(string currencyKey, string priceIconSpriteName)
        {
            if (string.IsNullOrEmpty(currencyKey))
            {
                return;
            }
            
            m_IconSpriteName = priceIconSpriteName;

            SetCurrencyKey(currencyKey);
        }

        /// <summary>
        /// Initializes CurrencyHudView before the first frame update.
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
        /// Sets Currency should be displayed by this view.
        /// </summary>
        /// <param name="currencyKey">The Currency identifier that should be displayed.</param>
        /// <remarks>If the <paramref name="currencyKey"/> param is null or empty, or is not found in the currency catalog no action will be taken.</remarks>
        public void SetCurrencyKey(string currencyKey)
        {
            if (string.IsNullOrEmpty(currencyKey))
            {
                Debug.LogError($"{nameof(CurrencyHudView)} - Given Currency key shouldn't be empty or null.");
                return;
            }
            
            if (m_CurrencyKey == currencyKey)
            {
                return;
            }

            if (Application.isPlaying)
            {
                var currency = GameFoundation.catalogs.currencyCatalog?.FindItem(currencyKey);
                if (currency == null)
                {
                    Debug.LogError($"{nameof(CurrencyHudView)} - Requested Currency \"{currencyKey}\" key doesn't exist in Currency Catalog.");
                    return;
                }
            }

            m_CurrencyKey = currencyKey;

            UpdateContent();
        }

        /// <inheritdoc cref="SetCurrencyKey(string)"/>
        [Obsolete("Use 'SetCurrencyKey' instead", false)]
        public void SetCurrencyId(string currencyKey) => SetCurrencyKey(currencyKey);

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
            UpdateQuantity();
        }

        /// <summary>
        /// Updates the Currency icon on this view.
        /// </summary>
        void UpdateIconSprite()
        {
            if (string.IsNullOrEmpty(m_CurrencyKey))
            {
                return;
            }

            if (Application.isPlaying)
            {
                var currency = GameFoundation.catalogs.currencyCatalog?.FindItem(m_CurrencyKey);
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

                var currency = currencyCatalog.FindItem(m_CurrencyKey);
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
        void SetIconSprite(Sprite iconSprite)
        {
            if (iconImageField == null)
            {
                Debug.LogWarning($"{nameof(CurrencyHudView)} - Icon Image field is not defined.");
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
        void UpdateQuantity()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            var currency = GameFoundation.catalogs.currencyCatalog?.FindItem(m_CurrencyKey);
            SetQuantity(currency != null ? WalletManager.GetBalance(currency) : 0);
        }

        /// <summary>
        /// Updates quantity of Currency item in label.
        /// </summary>
        /// <param name="quantity">The new quantity to display.</param>
        void SetQuantity(long quantity)
        {
            if (m_QuantityTextField == null)
            {
                Debug.LogWarning($"{nameof(CurrencyHudView)} - Item Quantity Text field is not defined.");
                return;
            }

            m_QuantityTextField.text = quantity.ToString();

#if UNITY_EDITOR
            EditorUtility.SetDirty(m_QuantityTextField);
#endif
        }

        void ThrowIfNotInitialized()
        {
            if (!GameFoundation.IsInitialized)
            {
                throw new InvalidOperationException("Error: GameFoundation.Initialize() must be called before the CurrencyHudView is used.");
            }
        }

        /// <summary>
        /// Listens to updates from the Wallet that contains the item being displayed.
        /// If the item that has changed is the one being displayed it updates the quantity.
        /// </summary>
        void OnCurrencyChanged(BalanceChangedEventArgs args)
        {
            if (args.currency == null || args.currency.key != m_CurrencyKey)
            {
                return;
            }

            UpdateQuantity();
        }
    }
}
