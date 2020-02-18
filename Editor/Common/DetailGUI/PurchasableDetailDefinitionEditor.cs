using System;
using System.Linq;
using UnityEngine;
using UnityEngine.GameFoundation.CatalogManagement;

namespace UnityEditor.GameFoundation
{
    [CustomEditor(typeof(EditorPurchasableDetailDefinition))]
    internal class PurchasableDetailDefinitionEditor : BaseDetailDefinitionEditor
    {
        private readonly string[] specialInventories = { InventoryCatalog.k_MainInventoryDefinitionId, InventoryCatalog.k_WalletInventoryDefinitionId };
        
        private SerializedProperty m_Payout_SerializedProperty;
        private SerializedProperty m_Prices_SerializedProperty;
        
        private EditorPurchasableDetailDefinition m_TargetDefinition;
        public void OnEnable()
        {
            // NOTE: this is a workaround to avoid a problem with Unity asset importer
            // - sometimes targets[0] is null when it shouldn't be
            // - the first two conditions are just a precaution
            if (targets.IsNullOrEmpty())
            {
                return;
            }

            m_TargetDefinition = targets[0] as EditorPurchasableDetailDefinition;

            if (m_TargetDefinition == null)
            {
                return;
            }
            
            m_Payout_SerializedProperty = serializedObject.FindProperty("m_Payout");
            m_Prices_SerializedProperty = serializedObject.FindProperty("m_Prices");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            using (var changeCheck = new EditorGUI.ChangeCheckScope())
            {
                // Available Items and inventory names + "<None>" for the source and default inventory selections
                string[] availableInventories = GameFoundationDatabaseSettings.database.inventoryCatalog.GetCollectionDefinitions().Select(definition => definition.id).Where(id => GameFoundationDatabaseSettings.database.inventoryCatalog.GetDefaultCollectionDefinition(id) != null).Prepend("<None>").Concat(specialInventories).ToArray();
                string[] availableItems = GameFoundationDatabaseSettings.database.inventoryCatalog.GetItemDefinitions().Select(definition => definition.id).ToArray();

                // For selecting the default inventories if an override isn't specified, can't be <None>
                var availableInventoriesWithoutNone = specialInventories.Concat(GameFoundationDatabaseSettings.database.inventoryCatalog.GetCollectionDefinitions().Select(definition => definition.id).Where(id => GameFoundationDatabaseSettings.database.inventoryCatalog.GetDefaultCollectionDefinition(id) != null)).ToArray();
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Default Source Inventory");
                    InventoryDropdownDefaults(serializedObject,"m_DefaultSourceInventoryId", availableInventoriesWithoutNone);
                }

                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Default Destination Inventory");
                    InventoryDropdownDefaults(serializedObject, "m_DefaultDestinationInventoryId", availableInventoriesWithoutNone);
                }
                
                GUILayout.Label("Payout");
                SerializedProperty outputItems = m_Payout_SerializedProperty.FindPropertyRelative("m_OutputItems");
                // See if outputItems exist, otherwise the item doesn't have any rewards
                if (outputItems.arraySize > 0)
                {
                    for (int i = 0; i < outputItems.arraySize; i++)
                    {
                        using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
                        {
                            SerializedProperty outputItem = outputItems.GetArrayElementAtIndex(i);
                            // Payout Object
                            using (new GUILayout.VerticalScope())
                            {
                                // Draw Payout Destination Override Field
                                using (new GUILayout.HorizontalScope())
                                {
                                    GUILayout.Label("Destination Inventory Override");
                                    GUILayout.FlexibleSpace();
                                    InventoryDropdown(outputItem, "m_DestinationInventoryId", availableInventories);

                                    // Delete Payout Item, put here for functionality and aesthetic
                                    if (GUILayout.Button("", GameFoundationEditorStyles.deleteButtonStyle))
                                    {
                                        outputItems.DeleteArrayElementAtIndex(i);
                                        break;
                                    }
                                }
                                // Draw Inventory Item Field
                                using (new GUILayout.HorizontalScope())
                                {
                                    GUILayout.Label("Inventory Item");
                                    GUILayout.FlexibleSpace();
                                    ItemDropdown(outputItem,"m_DefinitionId",availableItems);
                                }
                                // Draw Quantity Field
                                using (new GUILayout.HorizontalScope())
                                {
                                    GUILayout.Label("Quantity");
                                    GUILayout.FlexibleSpace();
                                    SerializedProperty quantity = outputItem.FindPropertyRelative("m_Quantity");
                                    quantity.intValue = EditorGUILayout.DelayedIntField(quantity.intValue);
                                }
                            }
                        }
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("No payouts specified!", GameFoundationEditorStyles.centeredGrayLabel);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    // Add New Payout
                    if (GUILayout.Button("+", GUILayout.Width(24)))
                    {
                        // Create the Payout and set its default values. The editor will copy the previous serializable object if this is not done
                        outputItems.InsertArrayElementAtIndex(outputItems.arraySize - 1 < 0 ? 0 : outputItems.arraySize - 1);
                        SerializedProperty createdOutputItem = outputItems.GetArrayElementAtIndex(outputItems.arraySize - 1);
                        createdOutputItem.FindPropertyRelative("m_DefinitionId").stringValue = availableItems[0];
                        createdOutputItem.FindPropertyRelative("m_Quantity").intValue = 0;
                        createdOutputItem.FindPropertyRelative("m_DestinationInventoryId").stringValue = String.Empty;
                    }
                }
                
                GUILayout.Label("Prices");
                using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
                {
                    // Are there Price Points? Or is this object Free
                    if (m_Prices_SerializedProperty.arraySize > 0)
                    {
                        for (int i = 0; i < m_Prices_SerializedProperty.arraySize; i++)
                        {
                            SerializedProperty price = m_Prices_SerializedProperty.GetArrayElementAtIndex(i);
                            using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
                            {
                                // Setup Price Point Id
                                SerializedProperty priceId = price.FindPropertyRelative("m_Name");
                                using (new GUILayout.HorizontalScope())
                                {
                                    GUILayout.Label("Price Point");
                                    GUILayout.FlexibleSpace();
                                    priceId.stringValue = EditorGUILayout.DelayedTextField(priceId.stringValue);

                                    // Delete a Price Point (Object)
                                    if (GUILayout.Button("", GameFoundationEditorStyles.deleteButtonStyle))
                                    {
                                        m_Prices_SerializedProperty.DeleteArrayElementAtIndex(i);
                                        break;
                                    }
                                }

                                // Setup Input Items for a Price Point
                                GUILayout.Label("Item Requirements");
                                SerializedProperty inputItems = price.FindPropertyRelative("m_InputItems");
                                if (inputItems.arraySize > 0)
                                {
                                    for (int j = 0; j < inputItems.arraySize; j++)
                                    {
                                        SerializedProperty inputItem = inputItems.GetArrayElementAtIndex(j);
                                        using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
                                        {
                                            // Draw Source Inventory Override Field
                                            using (new GUILayout.HorizontalScope())
                                            {
                                                GUILayout.Label("Source Inventory Override");
                                                GUILayout.FlexibleSpace();
                                                InventoryDropdown(inputItem,"m_SourceInventoryId", availableInventories);

                                                // Delete Input Item Requirement
                                                if (GUILayout.Button("", GameFoundationEditorStyles.deleteButtonStyle))
                                                {
                                                    inputItems.DeleteArrayElementAtIndex(j);
                                                    break;
                                                }
                                            }
                                            // Draw Inventory Item requirement field
                                            using (new GUILayout.HorizontalScope())
                                            {
                                                GUILayout.Label("Inventory Item");
                                                GUILayout.FlexibleSpace();
                                                ItemDropdown(inputItem,"m_DefinitionId", availableItems);
                                            }
                                            // Draw Quantity requirement field
                                            using (new GUILayout.HorizontalScope())
                                            {
                                                GUILayout.Label("Quantity");
                                                GUILayout.FlexibleSpace();
                                                SerializedProperty quantity = inputItem.FindPropertyRelative("m_Price");
                                                quantity.doubleValue =
                                                    EditorGUILayout.DelayedIntField((int) quantity.doubleValue);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // Price Point has no item requirement, in other words...
                                    EditorGUILayout.LabelField("Free!", GameFoundationEditorStyles.centeredGrayLabel);
                                }
                                
                                using (new GUILayout.HorizontalScope())
                                {
                                    // Add New Input Item to Price Point
                                    GUILayout.FlexibleSpace();
                                    if (GUILayout.Button("+", GUILayout.Width(24)))
                                    {
                                        // When Creating new input Item Set all fields to default to prevent copying
                                        inputItems.InsertArrayElementAtIndex(inputItems.arraySize - 1 < 0 ? 0 : inputItems.arraySize - 1);
                                        SerializedProperty createdInputItem = inputItems.GetArrayElementAtIndex(inputItems.arraySize - 1);
                                        createdInputItem.FindPropertyRelative("m_DefinitionId").stringValue = availableItems[0];
                                        createdInputItem.FindPropertyRelative("m_Price").doubleValue = 0;
                                        createdInputItem.FindPropertyRelative("m_SourceInventoryId").stringValue = String.Empty;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // No Price points!
                        EditorGUILayout.LabelField("No Prices available!", GameFoundationEditorStyles.centeredGrayLabel);
                    }
                    
                    using (new GUILayout.HorizontalScope())
                    {
                        // Add New Price Point
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("+", GUILayout.Width(24)))
                        {
                            // It's important to clear out the prices list here to ensure it doesn't get copied from the last price point object
                            m_Prices_SerializedProperty.InsertArrayElementAtIndex(m_Prices_SerializedProperty.arraySize - 1 < 0 ? 0 : m_Prices_SerializedProperty.arraySize - 1);
                            SerializedProperty createdPrice = m_Prices_SerializedProperty.GetArrayElementAtIndex(m_Prices_SerializedProperty.arraySize - 1);
                            m_Prices_SerializedProperty.GetArrayElementAtIndex(m_Prices_SerializedProperty.arraySize - 1).FindPropertyRelative("m_Name").stringValue = "default";
                            m_Prices_SerializedProperty.GetArrayElementAtIndex(m_Prices_SerializedProperty.arraySize - 1).FindPropertyRelative("m_InputItems").ClearArray();
                        }
                    }
                }
                
                if (changeCheck.changed)
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        // For all source, destination drop down boxes
        private static void InventoryDropdown(SerializedProperty serializedProperty, string propertyName, string[] inventoryArray)
        {
            SerializedProperty inventoryId = serializedProperty.FindPropertyRelative(propertyName);
            int itemIdIndex = Array.IndexOf(inventoryArray, inventoryId.stringValue);
            int previousItemIdIndex = itemIdIndex < 0 ? 0 : itemIdIndex;
            inventoryId.stringValue = inventoryArray[EditorGUILayout.Popup(previousItemIdIndex, inventoryArray, GUILayout.Width(187))];
            inventoryId.stringValue = inventoryId.stringValue == "<None>" ? String.Empty : inventoryId.stringValue;
        } 
        
        // For all item selection drop down boxes
        private static void ItemDropdown(SerializedProperty serializedProperty, string propertyName, string[] inventoryArray)
        {
            SerializedProperty inventoryId = serializedProperty.FindPropertyRelative(propertyName);
            int itemIdIndex = Array.IndexOf(inventoryArray, inventoryId.stringValue);
            int previousItemIdIndex = itemIdIndex < 0 ? 0 : itemIdIndex;
            inventoryId.stringValue = inventoryArray[EditorGUILayout.Popup(previousItemIdIndex, inventoryArray)];
        }
        
        // For all default inventory dropdown boxes.
        private static void InventoryDropdownDefaults(SerializedObject serializedObject, string propertyName, string[] inventoryArray)
        {
            SerializedProperty inventoryId = serializedObject.FindProperty(propertyName);
            int itemIdIndex = Array.IndexOf(inventoryArray, inventoryId.stringValue);
            int previousItemIdIndex = itemIdIndex < 0 ? 0 : itemIdIndex;
            inventoryId.stringValue = inventoryArray[EditorGUILayout.Popup(previousItemIdIndex, inventoryArray)];
        }
    }
}
