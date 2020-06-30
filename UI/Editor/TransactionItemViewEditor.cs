using UnityEngine;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.GameFoundation.UI;
using UnityEngine.UI;

namespace UnityEditor.GameFoundation.UI
{
    [CustomEditor(typeof(TransactionItemView))]
    public class TransactionItemViewEditor : Editor
    {
        TransactionItemView m_TransactionItemView;

        string[] m_TransactionNames;
        string[] m_TransactionKeys;

        int m_SelectedTransactionIndex = -1;

        bool m_IsDrivenByOtherComponent;

        UISpriteNameSelector m_ItemSpriteNameSelector;
        UISpriteNameSelector m_PriceSpriteNameSelector;

        SerializedProperty m_TransactionKey_SerializedProperty;
        SerializedProperty m_ItemIconSpriteName_SerializedProperty;
        SerializedProperty m_PriceIconSpriteName_SerializedProperty;
        SerializedProperty m_NoPriceString_SerializedProperty;
        SerializedProperty m_ItemIconImageField_SerializedProperty;
        SerializedProperty m_ItemNameTextField_SerializedProperty;
        SerializedProperty m_PurchaseButton_SerializedProperty;
        SerializedProperty m_Interactable_SerializedProperty;
        SerializedProperty m_ShowGameObjectEditorFields_SerializedProperty;

        readonly string[] kExcludedFields =
        {
            "m_Script",
            nameof(TransactionItemView.m_TransactionKey),
            nameof(TransactionItemView.m_ItemIconSpriteName),
            nameof(TransactionItemView.m_PriceIconSpriteName),
            nameof(TransactionItemView.m_NoPriceString),
            nameof(TransactionItemView.m_ItemIconImageField),
            nameof(TransactionItemView.m_ItemNameTextField),
            nameof(TransactionItemView.m_PurchaseButton),
            nameof(TransactionItemView.m_Interactable),
            nameof(TransactionItemView.showGameObjectEditorFields)
        };

        void OnEnable()
        {
            m_TransactionItemView = target as TransactionItemView;

            m_TransactionKey_SerializedProperty = serializedObject.FindProperty(nameof(m_TransactionItemView.m_TransactionKey));
            m_ItemIconSpriteName_SerializedProperty = serializedObject.FindProperty(nameof(m_TransactionItemView.m_ItemIconSpriteName));
            m_PriceIconSpriteName_SerializedProperty = serializedObject.FindProperty(nameof(m_TransactionItemView.m_PriceIconSpriteName));
            m_NoPriceString_SerializedProperty = serializedObject.FindProperty(nameof(m_TransactionItemView.m_NoPriceString));
            m_ItemIconImageField_SerializedProperty = serializedObject.FindProperty(nameof(m_TransactionItemView.m_ItemIconImageField));
            m_ItemNameTextField_SerializedProperty = serializedObject.FindProperty(nameof(m_TransactionItemView.m_ItemNameTextField));
            m_PurchaseButton_SerializedProperty = serializedObject.FindProperty(nameof(m_TransactionItemView.m_PurchaseButton));
            m_Interactable_SerializedProperty = serializedObject.FindProperty(nameof(m_TransactionItemView.m_Interactable));
            m_ShowGameObjectEditorFields_SerializedProperty = serializedObject.FindProperty(nameof(m_TransactionItemView.showGameObjectEditorFields));

            m_IsDrivenByOtherComponent = m_TransactionItemView.IsDrivenByOtherComponent();

            m_PriceSpriteNameSelector = new UISpriteNameSelector();
            m_ItemSpriteNameSelector = new UISpriteNameSelector();

            UpdateTransactions();
            
            // To update the content when the GameObject is selected
            m_TransactionItemView.UpdateContent();
        }

        public override void OnInspectorGUI()
        {
            // Pull the information from the target into the serializedObject.
            serializedObject.Update();

            if (!m_IsDrivenByOtherComponent)
            {
                var transactionDisplayContent = new GUIContent("Transaction Item", "The Transaction Item to display in this view.");
                if (m_TransactionNames != null && m_TransactionNames.Length > 0)
                {
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        var transactionItemIndex = EditorGUILayout.Popup(transactionDisplayContent, m_SelectedTransactionIndex, m_TransactionNames);
                        if (check.changed)
                        {
                            ChangeSelectedTransactionItem(transactionItemIndex);
                        }
                    }
                }
                else
                {
                    EditorGUILayout.Popup(transactionDisplayContent, 0, new[] {"None"});
                }
                
                EditorGUILayout.Space();
                
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var noPriceStringContent = new GUIContent("No Price String", "String to display on Purchase Button when there is no cost defined in the Transaction Item.");
                    var noPriceString = EditorGUILayout.TextField(noPriceStringContent, m_TransactionItemView.noPriceString);
                    if (check.changed)
                    {
                        m_TransactionItemView.SetNoPriceString(noPriceString);
                        m_NoPriceString_SerializedProperty.stringValue = noPriceString;
                    }
                }

                EditorGUILayout.Space();
                
                var itemIconName = m_ItemSpriteNameSelector.Draw(m_TransactionItemView.itemIconSpriteName, "Item Icon Sprite Name", "Sprite name that is defined on Assets Detail to display the item icon.");
                if (itemIconName != m_TransactionItemView.itemIconSpriteName)
                {
                    m_TransactionItemView.SetItemIconSpriteName(itemIconName);
                    m_ItemIconSpriteName_SerializedProperty.stringValue = itemIconName;
                }
                var priceIconName = m_PriceSpriteNameSelector.Draw(m_TransactionItemView.priceIconSpriteName, "Price Icon Sprite Name", "Sprite name that is defined on Assets Detail to display the price icon.");
                if (priceIconName != m_TransactionItemView.priceIconSpriteName)
                {
                    m_TransactionItemView.SetNoPriceString(priceIconName);
                    m_PriceIconSpriteName_SerializedProperty.stringValue = priceIconName;
                }

                EditorGUILayout.Space();

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var interactableContent = new GUIContent("Interactable", "Sets the button's interactable state.");
                    var interactable = EditorGUILayout.Toggle(interactableContent, m_TransactionItemView.interactable);
                
                    if (check.changed)
                    {
                        m_TransactionItemView.interactable = m_Interactable_SerializedProperty.boolValue = interactable;    
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Item settings are driven by Store View", MessageType.None);
            }

            EditorGUILayout.Space();
            
            m_ShowGameObjectEditorFields_SerializedProperty.boolValue = EditorGUILayout.Foldout(m_ShowGameObjectEditorFields_SerializedProperty.boolValue, "GameObject Fields");
            if (m_ShowGameObjectEditorFields_SerializedProperty.boolValue)
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var imageIconFieldContent = new GUIContent("Item Icon Image", "The Image component in witch to display item icon sprite.");
                    var itemIconField = (Image) EditorGUILayout.ObjectField(imageIconFieldContent, m_TransactionItemView.itemIconImageField, typeof(Image), true);

                    if (check.changed)
                    {
                        m_TransactionItemView.SetItemIconImageField(itemIconField);
                        m_ItemIconImageField_SerializedProperty.objectReferenceValue = itemIconField;
                    }
                }

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var itemNameFieldContent = new GUIContent("Item Name Text", "Text component in which to display Store Item price.");
                    var itemNameField = (Text) EditorGUILayout.ObjectField(itemNameFieldContent, m_TransactionItemView.itemNameTextField, typeof(Text), true);

                    if (check.changed)
                    {
                        m_TransactionItemView.SetItemNameTextField(itemNameField);
                        m_ItemNameTextField_SerializedProperty.objectReferenceValue = itemNameField;
                    }
                }

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var purchaseButtonContent = new GUIContent("Purchase Button", "PurchaseButton component to use when generating a button for purchasing item in this view.");
                    var purchaseButton = (PurchaseButton) EditorGUILayout.ObjectField(purchaseButtonContent, m_TransactionItemView.purchaseButton, typeof(PurchaseButton), true);

                    if (check.changed)
                    {
                        m_TransactionItemView.SetPurchaseButton(purchaseButton);
                        m_PurchaseButton_SerializedProperty.objectReferenceValue = purchaseButton;
                    }
                }
            }

            // Use the default object field GUI for these properties.
            DrawPropertiesExcluding(serializedObject, kExcludedFields);

            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();
        }

        void UpdateTransactions()
        {
            m_SelectedTransactionIndex = -1;
            
            var selectedTransactionKey = m_TransactionKey_SerializedProperty.stringValue;

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
                    var transactionAsset = transactionAssets[i];
                    m_TransactionNames[i] = transactionAsset.displayName;
                    m_TransactionKeys[i] = transactionAsset.key;

                    if (!string.IsNullOrEmpty(selectedTransactionKey) && selectedTransactionKey == transactionAsset.key)
                    {
                        m_SelectedTransactionIndex = i;
                    }
                }
            }

            if (m_SelectedTransactionIndex == -1)
            {
                ChangeSelectedTransactionItem(0);
            }
        }

        string GetTransactionItemKey(int index)
        {
            if (m_TransactionKeys == null || m_TransactionKeys.Length <= index || index < -1)
            {
                return null;
            }

            return m_TransactionKeys[index];
        }

        void ChangeSelectedTransactionItem(int index)
        {
            var key = GetTransactionItemKey(index);
            if (key == null)
            {
                return;
            }
            
            m_SelectedTransactionIndex = index;

            // Update the serialized value
            m_TransactionKey_SerializedProperty.stringValue = key;
            m_TransactionItemView.SetTransactionKey(key);
            
            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();
        }
    }
}
