using UnityEngine;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.GameFoundation.UI;
using UnityEngine.UI;

namespace UnityEditor.GameFoundation.UI
{
    [CustomEditor(typeof(PurchaseButton))]
    public class PurchaseButtonEditor : Editor
    {
        PurchaseButton m_PurchaseButton;

        string[] m_TransactionNames;
        string[] m_TransactionKeys;

        int m_SelectedTransactionIndex = -1;

        bool m_IsDrivenByOtherComponent;

        UISpriteNameSelector m_IconSpriteNameSelector;

        SerializedProperty m_TransactionKey_SerializedProperty;
        SerializedProperty m_PriceIconSpriteName_SerializedProperty;
        SerializedProperty m_PriceIconImageField_SerializedProperty;
        SerializedProperty m_NoPriceString_SerializedProperty;
        SerializedProperty m_PriceText_SerializedProperty;
        SerializedProperty m_Interactable_SerializedProperty;
        SerializedProperty m_ShowGameObjectEditorFields_SerializedProperty;

        readonly string[] kExcludedFields =
        {
            "m_Script",
            nameof(PurchaseButton.m_TransactionKey),
            nameof(PurchaseButton.m_PriceIconImageField),
            nameof(PurchaseButton.m_PriceTextField),
            nameof(PurchaseButton.m_NoPriceString),
            nameof(PurchaseButton.m_PriceIconSpriteName),
            nameof(PurchaseButton.m_Interactable),
            nameof(PurchaseButton.showGameObjectEditorFields)
        };

        void OnEnable()
        {
            m_PurchaseButton = target as PurchaseButton;

            m_TransactionKey_SerializedProperty = serializedObject.FindProperty(nameof(m_PurchaseButton.m_TransactionKey));
            m_PriceIconSpriteName_SerializedProperty = serializedObject.FindProperty(nameof(m_PurchaseButton.m_PriceIconSpriteName));
            m_PriceIconImageField_SerializedProperty = serializedObject.FindProperty(nameof(m_PurchaseButton.m_PriceIconImageField));
            m_NoPriceString_SerializedProperty = serializedObject.FindProperty(nameof(m_PurchaseButton.m_NoPriceString));
            m_PriceText_SerializedProperty = serializedObject.FindProperty(nameof(m_PurchaseButton.m_PriceTextField));
            m_Interactable_SerializedProperty = serializedObject.FindProperty(nameof(m_PurchaseButton.m_Interactable));
            m_ShowGameObjectEditorFields_SerializedProperty = serializedObject.FindProperty(nameof(m_PurchaseButton.showGameObjectEditorFields));

            m_IconSpriteNameSelector = new UISpriteNameSelector();

            m_IsDrivenByOtherComponent = m_PurchaseButton.IsDrivenByOtherComponent();

            UpdateTransactionItems();
            
            // To update the content when the GameObject is selected
            m_PurchaseButton.UpdateContent();
        }

        public override void OnInspectorGUI()
        {
            // Pull the information from the target into the serializedObject.
            serializedObject.Update();

            if (!m_IsDrivenByOtherComponent)
            {
                var transactionItemDisplayContent = new GUIContent("Transaction Item", "The Transaction Item to display in this button");
                if (m_TransactionNames != null && m_TransactionNames.Length > 0)
                {
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        var transactionItemIndex = EditorGUILayout.Popup(transactionItemDisplayContent, m_SelectedTransactionIndex, m_TransactionNames);
                        if (check.changed)
                        {
                            ChangeSelectedTransactionItem(transactionItemIndex);
                        }
                    }
                }
                else
                {
                    EditorGUILayout.Popup(transactionItemDisplayContent, 0, new[] {"None"});
                }

                EditorGUILayout.Space();
                
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var noPriceStringContent = new GUIContent("No Price String", "String to display when there is no cost defined in the Transaction Item.");
                    var noPriceString = EditorGUILayout.TextField(noPriceStringContent, m_PurchaseButton.noPriceString);
                    if (check.changed)
                    {
                        m_PurchaseButton.SetNoPriceString(noPriceString);
                        m_NoPriceString_SerializedProperty.stringValue = noPriceString;
                    }
                }
                
                var priceIconName = m_IconSpriteNameSelector.Draw(m_PurchaseButton.priceIconSpriteName, "Price Icon Sprite Name", "Sprite name that defined on Assets Detail to display price icon.");
                if (priceIconName != m_PurchaseButton.priceIconSpriteName)
                {
                    m_PurchaseButton.SetNoPriceString(priceIconName);
                    m_PriceIconSpriteName_SerializedProperty.stringValue = priceIconName;
                }

                EditorGUILayout.Space();

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var interactableContent = new GUIContent("Interactable", "Sets the button's interactable state.");
                    var interactable = EditorGUILayout.Toggle(interactableContent, m_PurchaseButton.interactable);
                
                    if (check.changed)
                    {
                        m_PurchaseButton.interactable = m_Interactable_SerializedProperty.boolValue = interactable;    
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Item settings are driven by Transaction Item View", MessageType.None);
            }

            EditorGUILayout.Space();
            
            m_ShowGameObjectEditorFields_SerializedProperty.boolValue = EditorGUILayout.Foldout(m_ShowGameObjectEditorFields_SerializedProperty.boolValue, "GameObject Fields");
            if (m_ShowGameObjectEditorFields_SerializedProperty.boolValue)
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var priceIconImageContent = new GUIContent("Price Image", "The Image component in witch to display price icon sprite.");
                    var priceIconImage = (Image) EditorGUILayout.ObjectField(priceIconImageContent,
                        m_PurchaseButton.priceIconImageField, typeof(Image), true);

                    if (check.changed)
                    {
                        m_PurchaseButton.SetPriceIconImageField(priceIconImage);
                        m_PriceIconImageField_SerializedProperty.objectReferenceValue = priceIconImage;
                    }
                }

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var priceTextContent = new GUIContent("Price Text", "the Text component in which to display Transaction Item price.");
                    var priceText = (Text) EditorGUILayout.ObjectField(priceTextContent,
                        m_PurchaseButton.priceTextField, typeof(Text), true);

                    if (check.changed)
                    {
                        m_PurchaseButton.SetPriceTextField(priceText);
                        m_PriceText_SerializedProperty.objectReferenceValue = priceText;
                    }
                }

                EditorGUILayout.Space();
            }

            // Use the default object field GUI for these properties.
            DrawPropertiesExcluding(serializedObject, kExcludedFields);

            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();
        }

        void UpdateTransactionItems()
        {
            m_SelectedTransactionIndex = -1;
            
            string selectedTransactionKey = m_TransactionKey_SerializedProperty.stringValue;

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
            m_PurchaseButton.SetTransactionKey(key);
            
            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();
        }
    }
}
