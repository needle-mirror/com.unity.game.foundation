using UnityEngine;
using UnityEngine.UI;
using UnityEngine.GameFoundation.CatalogManagement;
using UnityEngine.GameFoundation.UI;

#if UNITY_EDITOR
namespace UnityEditor.GameFoundation.UI
{
    [CustomEditor(typeof(TransactionItemView))]
    public class TransactionItemViewEditor : Editor
    {
        private TransactionItemView m_TransactionItemView;
        
        private string[] m_TransactionNames;

        private string[] m_TransactionIds;

        private int m_SelectedTransactionIndex = -1;

        private bool m_IsUnderStoreView = false;

        private UISpriteNameSelector m_ItemSpriteNameSelector;
        private UISpriteNameSelector m_PriceSpriteNameSelector;

        private SerializedProperty m_TransactionId_SerializedProperty;
        private SerializedProperty m_ItemIconSpriteName_SerializedProperty ;
        private SerializedProperty m_PriceIconSpriteName_SerializedProperty;
        private SerializedProperty m_ItemIconImageField_SerializedProperty;
        private SerializedProperty m_ItemNameTextField_SerializedProperty;
        private SerializedProperty m_PurchaseButton_SerializedProperty;
        private SerializedProperty m_Interactable_SerializedProperty;

        private readonly string[] kExcludedFields = {"m_Script", "m_TransactionId", "m_ItemIconSpriteName", "m_PriceIconSpriteName", "m_ItemIconImageField", "m_ItemNameTextField", "m_PurchaseButton", "m_Interactable"};

        private void OnEnable()
        {
            m_TransactionItemView = target as TransactionItemView;
            
            m_TransactionId_SerializedProperty = serializedObject.FindProperty("m_TransactionId");
            m_ItemIconSpriteName_SerializedProperty = serializedObject.FindProperty("m_ItemIconSpriteName");
            m_PriceIconSpriteName_SerializedProperty = serializedObject.FindProperty("m_PriceIconSpriteName");
            m_ItemIconImageField_SerializedProperty = serializedObject.FindProperty("m_ItemIconImageField");
            m_ItemNameTextField_SerializedProperty = serializedObject.FindProperty("m_ItemNameTextField");
            m_PurchaseButton_SerializedProperty = serializedObject.FindProperty("m_PurchaseButton");
            m_Interactable_SerializedProperty = serializedObject.FindProperty("m_Interactable");

            m_IsUnderStoreView = m_TransactionItemView.IsDrivenByStoreView();
            
            m_PriceSpriteNameSelector = new UISpriteNameSelector();
            m_ItemSpriteNameSelector = new UISpriteNameSelector();

            UpdateTransactions();
        }

        public override void OnInspectorGUI()
        {
            // Pull the information from the target into the serializedObject.
            serializedObject.Update();

            if (!m_IsUnderStoreView)
            {
                var transactionDisplayContent =
                    new GUIContent("Transaction Item", "The Transaction Item to display in this view.");
                var selectedTransactionIndex =
                    EditorGUILayout.Popup(transactionDisplayContent, m_SelectedTransactionIndex, m_TransactionNames);
                ChangeSelectedTransaction(selectedTransactionIndex);

                EditorGUILayout.Space();
                
                var interactableContent = new GUIContent("Interactable", "Sets the Purchase Button's interactable state.");
                bool oldInteractable = m_TransactionItemView.interactable;
                var newInteractable = EditorGUILayout.Toggle(interactableContent, oldInteractable);
                if (oldInteractable != newInteractable)
                {
                    m_TransactionItemView.interactable = m_Interactable_SerializedProperty.boolValue = newInteractable;
                }
                
                EditorGUILayout.Space();
                
                var oldItemSpriteName = m_TransactionItemView.itemIconSpriteName;
                var newItemSpriteName = m_ItemSpriteNameSelector.Draw(oldItemSpriteName, "Item Icon Sprite Name", "Sprite name that is defined on Assets Detail to display the item icon.");
                if (newItemSpriteName != oldItemSpriteName)
                {
                    m_TransactionItemView.SetItemIconSpriteName(newItemSpriteName);
                    m_ItemIconSpriteName_SerializedProperty.stringValue = newItemSpriteName;
                }
                
                var oldPriceIconName = m_TransactionItemView.priceIconSpriteName;
                var newPriceIconName = m_PriceSpriteNameSelector.Draw(oldPriceIconName, "Price Icon Sprite Name", "Sprite name that is defined on Assets Detail to display the price icon.");
                if (newPriceIconName != oldPriceIconName)
                {
                    m_TransactionItemView.SetPriceIconSpriteName(newPriceIconName);
                    m_PriceIconSpriteName_SerializedProperty.stringValue = newPriceIconName;
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Item settings are driven by Store View", MessageType.None);
            }
            
            EditorGUILayout.Space();
            
            var imageDisplayContent = new GUIContent("Item Icon Image Field", "The Image component in witch to display item icon sprite.");
            var oldImage = m_TransactionItemView.itemIconImageField;
            var newImage = (Image)EditorGUILayout.ObjectField(imageDisplayContent, oldImage, typeof(Image), true);
            
            if (oldImage != newImage)
            {
                m_TransactionItemView.SetItemIconImageField(newImage);
                m_ItemIconImageField_SerializedProperty.objectReferenceValue = newImage;
            }
            
            var textDisplayContent = new GUIContent("Item Name Text Field", "Text component in which to display Store Item price.");
            var oldText = m_TransactionItemView.itemNameTextField;
            var newText = (Text)EditorGUILayout.ObjectField(textDisplayContent, oldText, typeof(Text), true);
            
            if (oldText != newText)
            {
                m_TransactionItemView.SetItemNameTextField(newText);
                m_ItemNameTextField_SerializedProperty.objectReferenceValue = newText;
            }
            
            var purchaseButtonContent = new GUIContent("Purchase Button", "PurchaseButton component to use when generating a button for purchasing item in this view.");
            var oldPurchase = m_TransactionItemView.purchaseButton;
            var newPurchase = (PurchaseButton)EditorGUILayout.ObjectField(purchaseButtonContent, oldPurchase, typeof(PurchaseButton), true);
            
            if (oldPurchase != newPurchase)
            {
                m_TransactionItemView.SetPurchaseButton(newPurchase);
                m_PurchaseButton_SerializedProperty.objectReferenceValue = newPurchase;
            }

            // Use the default object field GUI for these properties.
            DrawPropertiesExcluding(serializedObject, kExcludedFields);

            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();
        }

        private void UpdateTransactions()
        {
            string selectedStoreId = m_TransactionId_SerializedProperty.stringValue;

            var transactionDefCount = 0;
            var transactionDefs = GameFoundationDatabaseSettings.database.transactionCatalog.GetItems();
            if (transactionDefs != null)
            {
                transactionDefCount = transactionDefs.Length;
            }

            if (m_TransactionNames == null || m_TransactionNames.Length != transactionDefCount)
            {
                m_TransactionNames = new string[transactionDefCount];
            }

            if (m_TransactionIds == null || m_TransactionIds.Length != transactionDefCount)
            {
                m_TransactionIds = new string[transactionDefCount];
            }

            if (transactionDefs != null)
            {
                for (int i = 0; i < transactionDefCount; i++)
                {
                    var def = transactionDefs[i];
                    m_TransactionNames[i] = def.displayName;
                    m_TransactionIds[i] = def.id;

                    if (!string.IsNullOrEmpty(selectedStoreId) && selectedStoreId == def.id)
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

        private void ChangeSelectedTransaction(int index)
        {
            if (m_SelectedTransactionIndex == index || m_TransactionIds == null || m_TransactionIds.Length <= index || index < -1)
            {
                return;
            }

            m_SelectedTransactionIndex = index;

            // Update the serialized value
            m_TransactionId_SerializedProperty.stringValue = m_TransactionIds[index];
            m_TransactionItemView.SetTransactionId(m_TransactionIds[index]);
        }
    }
}
#endif
