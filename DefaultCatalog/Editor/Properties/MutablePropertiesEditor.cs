﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    class MutablePropertiesEditor : PropertiesEditor<InventoryItemDefinitionAsset>
    {
        public static readonly GUIContent mutablePropertiesLabel = new GUIContent(
            "Mutable Properties",
            "Store a set of variable data inside your inventory items you can read and edit at runtime.");

        static readonly GUIContent k_DefaultValueLabel = new GUIContent(
            "Default Value",
            "This property's default value. It is the value this property will have when an item is created from this definition.");

        static readonly GUIContent k_EmptyMutableCollectionLabel
            = new GUIContent("No mutable properties configured");

        protected override Dictionary<string, Property> GetAssetProperties()
            => m_Asset.properties;

        protected override GUIContent GetValueLabel()
            => k_DefaultValueLabel;

        protected override GUIContent GetEmptyCollectionLabel()
            => k_EmptyMutableCollectionLabel;
    }
}
