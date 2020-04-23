using UnityEngine.UI;
using UnityEngine;
using UnityEngine.GameFoundation.CatalogManagement;
using UnityEngine.GameFoundation.UI;

#if UNITY_EDITOR
namespace UnityEditor.GameFoundation.UI
{
    [CustomEditor(typeof(PurchaseButton))]
    public class PurchaseButtonEditor : Editor
    {
        private PurchaseButton m_PurchaseButton;
        
        private string[] m_TransactionItemNames;
        private string[] m_TransactionItemIds;

        private int m_SelectedTransactionItemIndex = -1;
        
        private bool m_IsDrivenByTransactionItemView = false;
        
        private UISpriteNameSelector m_IconSpriteNameSelector;

        private SerializedProperty m_TransactionId_SerializedProperty;
        private SerializedProperty m_IconSpriteName_SerializedProperty;
        private SerializedProperty m_PriceIconImage_SerializedProperty;
        private SerializedProperty m_PriceText_SerializedProperty;
        private SerializedProperty m_Interactable_SerializedProperty;

        private readonly string[] kExcludedFields = {"m_Script", "m_TransactionId", "m_AvailableToPurchaseState", "m_PriceIconImageField", "m_PriceIconImageField", "m_PriceTextField", "m_PriceIconSpriteName", "m_Interactable"};

        private void OnEnable()
        {
            m_PurchaseButton = target as PurchaseButton;
            
            m_TransactionId_SerializedProperty = serializedObject.FindProperty("m_TransactionId");
            m_IconSpriteName_SerializedProperty = serializedObject.FindProperty("m_PriceIconSpriteName");
            m_PriceIconImage_SerializedProperty = serializedObject.FindProperty("m_PriceIconImageField");
            m_PriceText_SerializedProperty = serializedObject.FindProperty("m_PriceTextField");
            m_Interactable_SerializedProperty = serializedObject.FindProperty("m_Interactable");
            
            m_IconSpriteNameSelector = new UISpriteNameSelector();

            m_IsDrivenByTransactionItemView = m_PurchaseButton.IsDrivenByTransactionItemView();

            UpdateTransactionItems();
        }

        public override void OnInspectorGUI()
        {
            // Pull the information from the target into the serializedObject.
            serializedObject.Update();

            if (!m_IsDrivenByTransactionItemView)
            {
                var transactionItemDisplayContent =
                    new GUIContent("Transaction Item", "The Transaction Item to display in this button");
                var selectedTransactionItemIndex =
                    EditorGUILayout.Popup(transactionItemDisplayContent, m_SelectedTransactionItemIndex, m_TransactionItemNames);
                ChangeSelectedTransactionItem(selectedTransactionItemIndex);
                
                EditorGUILayout.Space();
                
                var interactableContent = new GUIContent("Interactable", "Sets the button's interactable state.");
                bool oldInteractable = m_PurchaseButton.interactable;
                var newInteractable = EditorGUILayout.Toggle(interactableContent, oldInteractable);
                if (oldInteractable != newInteractable)
                {
                    m_PurchaseButton.interactable = m_Interactable_SerializedProperty.boolValue = newInteractable;
                }

                EditorGUILayout.Space();
                
                var oldPriceIconSpriteName = m_PurchaseButton.priceIconSpriteName;
                var newPriceIconSpriteName = m_IconSpriteNameSelector.Draw(oldPriceIconSpriteName, "Price Icon Sprite Name", "Sprite name that defined on Assets Detail to display price icon.");
                if (newPriceIconSpriteName != oldPriceIconSpriteName)
                {
                    m_IconSpriteName_SerializedProperty.stringValue = newPriceIconSpriteName;
                    m_PurchaseButton.SetPriceIconSpriteName(newPriceIconSpriteName);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Item settings are driven by Transaction Item View", MessageType.None);
            }

            EditorGUILayout.Space();
            
            var imageDisplayContent = new GUIContent("Price Image Field", "The Image component in witch to display price icon sprite.");
            var oldImage = m_PurchaseButton.priceIconImageField;
            var newImage = (Image)EditorGUILayout.ObjectField(imageDisplayContent, oldImage, typeof(Image), true);
            
            if (oldImage != newImage)
            {
                m_PurchaseButton.SetPriceIconImageField(newImage);
                m_PriceIconImage_SerializedProperty.objectReferenceValue = newImage;
            }
            
            var textDisplayContent = new GUIContent("Price Text Field", "the Text component in which to display Transaction Item price.");
            var oldText = m_PurchaseButton.priceTextField;
            var newText = (Text)EditorGUILayout.ObjectField(textDisplayContent, oldText, typeof(Text), true);
            
            if (oldText != newText)
            {
                m_PurchaseButton.SetPriceTextField(newText);
                m_PriceText_SerializedProperty.objectReferenceValue = newText;
            }
            
            EditorGUILayout.Space();

            // Use the default object field GUI for these properties.
            DrawPropertiesExcluding(serializedObject, kExcludedFields);

            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();
        }

        private void UpdateTransactionItems()
        {
            string selectedTransactionId = m_TransactionId_SerializedProperty.stringValue;

            var transactionItemDefCount = 0;
            var transactionItemDefs = GameFoundationDatabaseSettings.database.transactionCatalog.GetItems();
            if (transactionItemDefs != null)
            {
                transactionItemDefCount = transactionItemDefs.Length;
            }

            if (m_TransactionItemNames == null || m_TransactionItemNames.Length != transactionItemDefCount)
            {
                m_TransactionItemNames = new string[transactionItemDefCount];    
            }

            if (m_TransactionItemIds == null || m_TransactionItemIds.Length != transactionItemDefCount)
            {
                m_TransactionItemIds = new string[transactionItemDefCount];
            }

            if (transactionItemDefs != null)
            {
                for (int i = 0; i < transactionItemDefCount; i++)
                {
                    var def = transactionItemDefs[i];
                    m_TransactionItemNames[i] = def.displayName;
                    m_TransactionItemIds[i] = def.id;

                    if (!string.IsNullOrEmpty(selectedTransactionId) && selectedTransactionId == def.id)
                    {
                        m_SelectedTransactionItemIndex = i;
                    }
                }
            }

            if (m_SelectedTransactionItemIndex == -1)
            {
                ChangeSelectedTransactionItem(0);
            }
        }

        private void ChangeSelectedTransactionItem(int index)
        {
            if (m_SelectedTransactionItemIndex == index || m_TransactionItemIds == null || m_TransactionItemIds.Length <= index || index < -1)
            {
                return;
            }

            m_SelectedTransactionItemIndex = index;

            // Update the serialized value
            m_TransactionId_SerializedProperty.stringValue = m_TransactionItemIds[index];
            m_PurchaseButton.SetTransactionId(m_TransactionItemIds[index]);
        }
    }
}
#endif
