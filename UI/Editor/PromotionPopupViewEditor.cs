using UnityEngine;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.GameFoundation.UI;
using UnityEngine.UI;

namespace UnityEditor.GameFoundation.UI
{
    [CustomEditor(typeof(PromotionPopupView))]
    public class PromotionPopupViewEditor : Editor
    {
        PromotionPopupView m_PromotionPopupView;

        string[] m_TransactionNames;
        string[] m_TransactionKeys;

        int m_SelectedTransactionIndex = -1;

        UISpriteNameSelector m_RewardImageAssetNameSelector;
        UISpriteNameSelector m_PromotionImageAssetNameSelector;
        UISpriteNameSelector m_PromoDescriptionPropertyKeySelector;
        UISpriteNameSelector m_BadgePropertyKeySelector;
        UISpriteNameSelector m_PriceIconAssetNameSelector;

        SerializedProperty m_TransactionKey_SerializedProperty;
        SerializedProperty m_AutoGeneratePromoImage_SerializedProperty;
        SerializedProperty m_ItemRewardCountPrefix_SerializedProperty;
        SerializedProperty m_CurrencyRewardCountPrefix_SerializedProperty;
        SerializedProperty m_RewardItemIconSpriteName_SerializedProperty;
        SerializedProperty m_PromoImageSpriteName_SerializedProperty;
        SerializedProperty m_DescriptionPropertyKey_SerializedProperty;
        SerializedProperty m_BadgePropertyKey_SerializedProperty;
        SerializedProperty m_PriceIconSpriteName_SerializedProperty;
        SerializedProperty m_TransactionNameTextField_SerializedProperty;
        SerializedProperty m_PromoDescriptionTextField_SerializedProperty;
        SerializedProperty m_AutoGeneratedImageContainer_SerializedProperty;
        SerializedProperty m_PromoImageField_SerializedProperty;
        SerializedProperty m_BadgeField_SerializedProperty;
        SerializedProperty m_PurchaseButton_SerializedProperty;
        SerializedProperty m_RewardItemPrefab_SerializedProperty;
        SerializedProperty m_SeparatorPrefab_SerializedProperty;
        SerializedProperty m_ShowGameObjectEditorFields_SerializedProperty;

        readonly string[] kExcludedFields =
        {
            "m_Script",
            nameof(PromotionPopupView.m_TransactionKey),
            nameof(PromotionPopupView.m_AutoGeneratePromoImage),
            nameof(PromotionPopupView.m_ItemRewardCountPrefix),
            nameof(PromotionPopupView.m_CurrencyRewardCountPrefix),
            nameof(PromotionPopupView.m_RewardItemIconSpriteName),
            nameof(PromotionPopupView.m_PromoImageSpriteName),
            nameof(PromotionPopupView.m_descriptionPropertyKey),
            nameof(PromotionPopupView.m_BadgeTextPropertyKey),
            nameof(PromotionPopupView.m_PriceIconSpriteName),
            nameof(PromotionPopupView.m_TitleTextField),
            nameof(PromotionPopupView.m_DescriptionTextField),
            nameof(PromotionPopupView.m_AutoGeneratedImageContainer),
            nameof(PromotionPopupView.m_PromoImageField),
            nameof(PromotionPopupView.m_BadgeField),
            nameof(PromotionPopupView.m_PurchaseButton),
            nameof(PromotionPopupView.m_RewardItemPrefab),
            nameof(PromotionPopupView.m_SeparatorPrefab),
            nameof(PromotionPopupView.showGameObjectEditorFields)
        };

        void OnEnable()
        {
            m_PromotionPopupView = target as PromotionPopupView;

            m_TransactionKey_SerializedProperty = serializedObject.FindProperty(nameof(m_PromotionPopupView.m_TransactionKey));
            m_AutoGeneratePromoImage_SerializedProperty = serializedObject.FindProperty(nameof(m_PromotionPopupView.m_AutoGeneratePromoImage));
            m_ItemRewardCountPrefix_SerializedProperty = serializedObject.FindProperty(nameof(m_PromotionPopupView.m_ItemRewardCountPrefix));
            m_CurrencyRewardCountPrefix_SerializedProperty = serializedObject.FindProperty(nameof(m_PromotionPopupView.m_CurrencyRewardCountPrefix));
            m_RewardItemIconSpriteName_SerializedProperty = serializedObject.FindProperty(nameof(m_PromotionPopupView.m_RewardItemIconSpriteName));
            m_PromoImageSpriteName_SerializedProperty = serializedObject.FindProperty(nameof(m_PromotionPopupView.m_PromoImageSpriteName));
            m_DescriptionPropertyKey_SerializedProperty = serializedObject.FindProperty(nameof(m_PromotionPopupView.m_descriptionPropertyKey));
            m_BadgePropertyKey_SerializedProperty = serializedObject.FindProperty(nameof(m_PromotionPopupView.m_BadgeTextPropertyKey));
            m_PriceIconSpriteName_SerializedProperty = serializedObject.FindProperty(nameof(m_PromotionPopupView.m_PriceIconSpriteName));
            m_TransactionNameTextField_SerializedProperty = serializedObject.FindProperty(nameof(m_PromotionPopupView.m_TitleTextField));
            m_PromoDescriptionTextField_SerializedProperty = serializedObject.FindProperty(nameof(m_PromotionPopupView.m_DescriptionTextField));
            m_AutoGeneratedImageContainer_SerializedProperty = serializedObject.FindProperty(nameof(m_PromotionPopupView.m_AutoGeneratedImageContainer));
            m_PromoImageField_SerializedProperty = serializedObject.FindProperty(nameof(m_PromotionPopupView.m_PromoImageField));
            m_BadgeField_SerializedProperty = serializedObject.FindProperty(nameof(m_PromotionPopupView.m_BadgeField));
            m_PurchaseButton_SerializedProperty = serializedObject.FindProperty(nameof(m_PromotionPopupView.m_PurchaseButton));
            m_RewardItemPrefab_SerializedProperty = serializedObject.FindProperty(nameof(m_PromotionPopupView.m_RewardItemPrefab));
            m_SeparatorPrefab_SerializedProperty = serializedObject.FindProperty(nameof(m_PromotionPopupView.m_SeparatorPrefab));
            m_ShowGameObjectEditorFields_SerializedProperty = serializedObject.FindProperty(nameof(m_PromotionPopupView.showGameObjectEditorFields));

            m_RewardImageAssetNameSelector = new UISpriteNameSelector();
            m_PromotionImageAssetNameSelector = new UISpriteNameSelector();
            m_PromoDescriptionPropertyKeySelector = new UISpriteNameSelector();
            m_BadgePropertyKeySelector = new UISpriteNameSelector();
            m_PriceIconAssetNameSelector = new UISpriteNameSelector();

            UpdateTransactions();
            
            // To update the content when the GameObject is selected
            m_PromotionPopupView.UpdateContent();
        }

        public override void OnInspectorGUI()
        {
            // Pull the information from the target into the serializedObject.
            serializedObject.Update();

            var transactionContent = new GUIContent("Transaction Item", "The Transaction Item to display in this view.");
            if (m_TransactionNames != null && m_TransactionNames.Length > 0)
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var selectedTransactionIndex = EditorGUILayout.Popup(transactionContent, m_SelectedTransactionIndex, m_TransactionNames);
                    if (check.changed)
                    {
                        ChangeSelectedTransaction(selectedTransactionIndex);
                    }
                }
            }
            else
            {
                EditorGUILayout.Popup(transactionContent, 0, new[] {"None"});
            }
            
            EditorGUILayout.Space();

            var changed = false;
            var autoGenerateImage = EditorGUILayout.Toggle("Generate Promo Image", m_PromotionPopupView.autoGeneratePromoImage);
            if (autoGenerateImage != m_PromotionPopupView.autoGeneratePromoImage)
            {
                changed = true;
            }

            if (autoGenerateImage)
            {
                var rewardItemIconSpriteName = m_RewardImageAssetNameSelector.Draw(
                    m_PromotionPopupView.rewardItemIconSpriteName,
                    "Reward Item Icon Sprite Name",
                    "Sprite name that is defined on the Assets Details of each of the transaction's reward items. Used when auto generating promotion image.");
                if (rewardItemIconSpriteName != m_PromotionPopupView.rewardItemIconSpriteName)
                {
                    changed = true;
                }

                string itemRewardCountPrefix = null;
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var itemRewardCountPrefixContent = new GUIContent("Item Count Prefix", "The string to add as a prefix to each item's reward count.");
                    itemRewardCountPrefix = EditorGUILayout.TextField(itemRewardCountPrefixContent, m_PromotionPopupView.itemRewardCountPrefix);
                    
                    if (check.changed)
                    {
                        changed = true;
                    }
                }

                string currencyRewardCountPrefix = null;
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var currencyRewardCountPrefixContent = new GUIContent("Currency Count Prefix", "The string to add as a prefix to each currency's reward count.");
                    currencyRewardCountPrefix = EditorGUILayout.TextField(currencyRewardCountPrefixContent, m_PromotionPopupView.currencyRewardCountPrefix);

                    if (check.changed)
                    {
                        changed = true;
                    }
                }

                if (changed)
                {
                    m_PromotionPopupView.SetAutoGeneratePromotionImage(rewardItemIconSpriteName, itemRewardCountPrefix, currencyRewardCountPrefix);
                    
                    m_AutoGeneratePromoImage_SerializedProperty.boolValue = true;
                    m_RewardItemIconSpriteName_SerializedProperty.stringValue = rewardItemIconSpriteName;
                    m_ItemRewardCountPrefix_SerializedProperty.stringValue = itemRewardCountPrefix;
                    m_CurrencyRewardCountPrefix_SerializedProperty.stringValue = currencyRewardCountPrefix;
                }
            }
            else
            {
                var promotionImageSpriteName = m_PromotionImageAssetNameSelector.Draw(
                    m_PromotionPopupView.promoImageSpriteName,
                    "Promo Image Sprite Name",
                    "Sprite name that is defined on transaction's Assets Detail to display the promotion image.");
                if (promotionImageSpriteName != m_PromotionPopupView.promoImageSpriteName)
                {
                    changed = true;
                }

                if (changed)
                {
                    m_PromotionPopupView.SetPromotionImage(promotionImageSpriteName);
                    m_AutoGeneratePromoImage_SerializedProperty.boolValue = false;
                    m_PromoImageSpriteName_SerializedProperty.stringValue = promotionImageSpriteName;
                }
            }
                
            EditorGUILayout.Space();
            
            var descriptionPropertyKey = m_PromoDescriptionPropertyKeySelector.Draw(
                m_PromotionPopupView.descriptionPropertyKey,
                "Description Property Key",
                "Property key attached to the transaction which provides the promotion's description.");
            if (descriptionPropertyKey != m_PromotionPopupView.descriptionPropertyKey)
            {
                m_PromotionPopupView.SetDescriptionPropertyKey(descriptionPropertyKey);
                m_DescriptionPropertyKey_SerializedProperty.stringValue = descriptionPropertyKey;
            }
            
            var badgeTextPropertyKey = m_BadgePropertyKeySelector.Draw(
                m_PromotionPopupView.badgeTextPropertyKey,
                "Badge Text Property Key",
                "Property key attached to the transaction which provides the promotion's badge text.");
            if (badgeTextPropertyKey != m_PromotionPopupView.badgeTextPropertyKey)
            {
                m_PromotionPopupView.SetBadgeTextPropertyKey(badgeTextPropertyKey);
                m_BadgePropertyKey_SerializedProperty.stringValue = badgeTextPropertyKey;
            }
            
            EditorGUILayout.Space();
            var priceIconSpriteName = m_PriceIconAssetNameSelector.Draw(
                m_PromotionPopupView.priceIconSpriteName,
                "Price Icon Sprite Name",
                "Sprite name that is defined on Assets Detail to display the price icon.");
            if (priceIconSpriteName != m_PromotionPopupView.priceIconSpriteName)
            {
                m_PromotionPopupView.SetPriceIconSpriteName(priceIconSpriteName);
                m_PriceIconSpriteName_SerializedProperty.stringValue = priceIconSpriteName;
            }

            EditorGUILayout.Space();
            
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var rewardItemContent = new GUIContent("Reward Item Prefab", "Prefab to use for an individual reward item when auto generating the promotion image.");
                var rewardItem = (ImageInfoView) EditorGUILayout.ObjectField(rewardItemContent, m_PromotionPopupView.rewardItemPrefab, typeof(ImageInfoView), true);

                if (check.changed)
                {
                    m_PromotionPopupView.SetRewardItemPrefab(rewardItem);
                    m_RewardItemPrefab_SerializedProperty.objectReferenceValue = rewardItem;
                }
            }
            
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var separatorContent = new GUIContent("Separator Prefab", "");
                var separator = (GameObject) EditorGUILayout.ObjectField(separatorContent, m_PromotionPopupView.separatorPrefab, typeof(GameObject), true);

                if (check.changed)
                {
                    m_PromotionPopupView.SetSeparatorPrefab(separator);
                    m_SeparatorPrefab_SerializedProperty.objectReferenceValue = separator;
                }
            }

            EditorGUILayout.Space();
            
            m_ShowGameObjectEditorFields_SerializedProperty.boolValue = EditorGUILayout.Foldout(m_ShowGameObjectEditorFields_SerializedProperty.boolValue, "GameObject Fields");
            if (m_ShowGameObjectEditorFields_SerializedProperty.boolValue)
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var titleTextFieldContent = new GUIContent("Title Text",
                        "Text component in which to display the transaction display name.");
                    var transactionNameTextField = (Text) EditorGUILayout.ObjectField(titleTextFieldContent,
                        m_PromotionPopupView.titleTextField, typeof(Text), true);

                    if (check.changed)
                    {
                        m_PromotionPopupView.SetTitleTextField(transactionNameTextField);
                        m_TransactionNameTextField_SerializedProperty.objectReferenceValue = transactionNameTextField;
                    }
                }

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var descriptionTextFieldContent = new GUIContent("Description Text",
                        "Text component in which to display the promotion's description.");
                    var promoDescriptionTextField = (Text) EditorGUILayout.ObjectField(descriptionTextFieldContent,
                        m_PromotionPopupView.descriptionTextField, typeof(Text), true);

                    if (check.changed)
                    {
                        m_PromotionPopupView.SetDescriptionTextField(promoDescriptionTextField);
                        m_PromoDescriptionTextField_SerializedProperty.objectReferenceValue = promoDescriptionTextField;
                    }
                }

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var autoGenImageContent = new GUIContent("Auto-Generated Image Container",
                        "The Game Object in which to display the promotion image.");
                    var autoGenImageContainer = (Transform) EditorGUILayout.ObjectField(autoGenImageContent,
                        m_PromotionPopupView.autoGeneratedImageContainer, typeof(Transform), true);

                    if (check.changed)
                    {
                        m_PromotionPopupView.SetAutoGeneratedImageContainer(autoGenImageContainer);
                        m_AutoGeneratedImageContainer_SerializedProperty.objectReferenceValue = autoGenImageContainer;
                    }
                }

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var promotionImageFieldContent = new GUIContent("Promo Image",
                        "The Game Object in which to display the promotion image.");
                    var promotionImageField = (Image) EditorGUILayout.ObjectField(promotionImageFieldContent,
                        m_PromotionPopupView.promoImageField, typeof(Image), true);

                    if (check.changed)
                    {
                        m_PromotionPopupView.SetPromoImageField(promotionImageField);
                        m_PromoImageField_SerializedProperty.objectReferenceValue = promotionImageField;
                    }
                }

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var badgeFieldContent = new GUIContent("Badge",
                        "GameObject in which to display the promotion's badge.");
                    var badgeField =
                        (ImageInfoView) EditorGUILayout.ObjectField(badgeFieldContent,
                            m_PromotionPopupView.badgeField, typeof(ImageInfoView), true);

                    if (check.changed)
                    {
                        m_PromotionPopupView.SetBadgeField(badgeField);
                        m_BadgeField_SerializedProperty.objectReferenceValue = badgeField;
                    }
                }

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var purchaseButtonContent = new GUIContent("Purchase Button",
                        "PurchaseButton component to use when generating a button for purchasing item in this view.");
                    var purchaseButton = (PurchaseButton) EditorGUILayout.ObjectField(purchaseButtonContent,
                        m_PromotionPopupView.purchaseButton,
                        typeof(PurchaseButton), true);

                    if (check.changed)
                    {
                        m_PromotionPopupView.SetPurchaseButton(purchaseButton);
                        m_PurchaseButton_SerializedProperty.objectReferenceValue = purchaseButton;
                    }
                }
                
                EditorGUILayout.Space();
            }

            // Use the default object field GUI for these properties.
            DrawPropertiesExcluding(serializedObject, kExcludedFields);

            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();
        }

        void UpdateTransactions()
        {
            m_SelectedTransactionIndex = -1;
            
            string selectedStoreKey = m_TransactionKey_SerializedProperty.stringValue;

            var transactionCount = 0;
            var transactionAssets = GameFoundationDatabaseSettings.database.transactionCatalog.GetItems();
            if (transactionAssets != null)
            {
                transactionCount = transactionAssets.Length;
            }

            if (m_TransactionNames == null || m_TransactionNames.Length != transactionCount)
            {
                m_TransactionNames = new string[transactionCount];
            }

            if (m_TransactionKeys == null || m_TransactionKeys.Length != transactionCount)
            {
                m_TransactionKeys = new string[transactionCount];
            }

            if (transactionAssets != null)
            {
                for (int i = 0; i < transactionCount; i++)
                {
                    var transaction = transactionAssets[i];
                    m_TransactionNames[i] = transaction.displayName;
                    m_TransactionKeys[i] = transaction.key;

                    if (!string.IsNullOrEmpty(selectedStoreKey) && selectedStoreKey == transaction.key)
                    {
                        m_SelectedTransactionIndex = i;
                    }
                }
            }

            if (m_SelectedTransactionIndex == -1)
            {
                ChangeSelectedTransaction(0);
            }
        }
        string GetTransactionKey(int index)
        {
            if (m_TransactionKeys == null || m_TransactionKeys.Length <= index || index < -1)
            {
                return null;
            }

            return m_TransactionKeys[index];
        }

        void ChangeSelectedTransaction(int index)
        {
            if (m_SelectedTransactionIndex == index)
            {
                return;
            }

            var key = GetTransactionKey(index);
            if (key == null)
            {
                return;
            }

            m_SelectedTransactionIndex = index;

            // Update the serialized value
            m_TransactionKey_SerializedProperty.stringValue = key;
            m_PromotionPopupView.SetTransactionKey(key);
            
            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();
        }
    }
}
