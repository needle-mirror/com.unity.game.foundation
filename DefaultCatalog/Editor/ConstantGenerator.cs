using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using Microsoft.CSharp;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    public static class ConstantGenerator
    {
        [MenuItem("Assets/Game Foundation/Generate Constants", validate = true)]
        static bool GenerateConstantsEditorMenu_Validate()
        {
            return Selection.activeObject as GameFoundationDatabase != null;
        }

        [MenuItem("Assets/Game Foundation/Generate Constants")]
        static void GenerateConstantsEditorMenu()
        {
            GenerateConstants(Selection.activeObject as GameFoundationDatabase);
        }

        public static void GenerateConstants(GameFoundationDatabase database)
        {
            var path = EditorUtility.SaveFilePanelInProject("Save constant file", database.name, "cs", null);
            if (string.IsNullOrWhiteSpace(path))
                return;

            var name = Path.GetFileNameWithoutExtension(path);
            name = CollectionEditorTools.ConvertNameToKey(name);
            name = string.Concat(((char)(name[0] + ('A' - 'a'))).ToString(), name.Substring(1));
            var compileUnit = new CodeCompileUnit();

            var @namespace = new CodeNamespace();
            compileUnit.Namespaces.Add(@namespace);

            var databaseClass = new CodeTypeDeclaration(name)
            {
                Attributes = MemberAttributes.Static
            };
            @namespace.Types.Add(databaseClass);

            var @string = new CodeTypeReference(typeof(string));

            var items = GenerateCatalogConstants(database.inventoryCatalog, "Items", @string);
            databaseClass.Members.Add(items);

            var currencies = GenerateCatalogConstants(database.currencyCatalog, "Currencies", @string);
            databaseClass.Members.Add(currencies);

            var transactions = GenerateCatalogConstants(database.transactionCatalog, "Transactions", @string);
            databaseClass.Members.Add(transactions);

            var stores = GenerateCatalogConstants(database.storeCatalog, "Stores", @string);
            databaseClass.Members.Add(stores);

            var gameParameters = GenerateCatalogConstants(database.gameParameterCatalog, "GameParameters", @string);
            databaseClass.Members.Add(gameParameters);

            using (var provider = new CSharpCodeProvider())
            using (var sw = new StreamWriter(path, false))
            {
                var opt = new CodeGeneratorOptions
                {
                    BlankLinesBetweenMembers = false,
                    ElseOnClosing = false,
                    VerbatimOrder = false,
                    BracingStyle = "C",
                    IndentString = "    "
                };
                provider.GenerateCodeFromCompileUnit(compileUnit, sw, opt);
            }

            AssetDatabase.Refresh();
        }

        /// <summary>
        ///     Create a constant container to store all item keys of the given
        ///     <paramref name="catalog" /> as well as their properties' keys.
        /// </summary>
        /// <param name="catalog">
        ///     A set of items to create constants for.
        /// </param>
        /// <param name="className">
        ///     The name to give to the constant class.
        /// </param>
        /// <param name="string">
        ///     The type reference to the <see cref="string" /> class.
        /// </param>
        /// <typeparam name="TCatalogItemAsset">
        ///     The type of item stored in the catalog.
        /// </typeparam>
        /// <returns>
        ///     Returns the created constant container.
        /// </returns>
        static CodeTypeDeclaration GenerateCatalogConstants<TCatalogItemAsset>(
            SingleCollectionCatalogAsset<TCatalogItemAsset> catalog,
            string className,
            CodeTypeReference @string)
            where TCatalogItemAsset : CatalogItemAsset
        {
            const string staticPropertiesClassName = "StaticProperties";
            const string mutablePropertiesClassName = "Properties";
            const string definitionKeyFieldName = "key";

            var items = catalog.GetItems();
            var itemsContainerClass = new CodeTypeDeclaration(className)
            {
                Attributes = MemberAttributes.Static
            };

            foreach (var item in items)
            {
                var itemClass = new CodeTypeDeclaration(item.key)
                {
                    Attributes = MemberAttributes.Static
                };
                itemsContainerClass.Members.Add(itemClass);

                var keyConstant = new CodeMemberField(@string, definitionKeyFieldName)
                {
                    Attributes = MemberAttributes.Const | MemberAttributes.Public,
                    InitExpression = new CodePrimitiveExpression(item.key)
                };
                itemClass.Members.Add(keyConstant);

                if (TryGeneratePropertiesConstants(
                    item.staticProperties, staticPropertiesClassName, @string, out var propertiesClass))
                {
                    itemClass.Members.Add(propertiesClass);
                }

                if (item is InventoryItemDefinitionAsset inventoryItem
                    && TryGeneratePropertiesConstants(
                        inventoryItem.properties, mutablePropertiesClassName, @string, out propertiesClass))
                {
                    itemClass.Members.Add(propertiesClass);
                }
            }

            return itemsContainerClass;
        }

        /// <summary>
        ///     Create a constant container to store all keys of the given <paramref name="properties" />.
        /// </summary>
        /// <param name="properties">
        ///     A set of properties to create constants for.
        /// </param>
        /// <param name="className">
        ///     The name to give to the constant class.
        /// </param>
        /// <param name="string">
        ///     The type reference to the <see cref="string" /> class.
        /// </param>
        /// <param name="constantContainer">
        ///     The created constant container.
        /// </param>
        /// <returns>
        ///     Return true if at least one constant have been generated;
        ///     return false otherwise.
        /// </returns>
        static bool TryGeneratePropertiesConstants(
            Dictionary<string, Property> properties,
            string className,
            CodeTypeReference @string,
            out CodeTypeDeclaration constantContainer)
        {
            if (properties.Count <= 0)
            {
                constantContainer = null;

                return false;
            }

            constantContainer = new CodeTypeDeclaration(className)
            {
                Attributes = MemberAttributes.Static
            };

            foreach (var propertyKey in properties.Keys)
            {
                var propertyConstant = new CodeMemberField(@string, propertyKey)
                {
                    Attributes = MemberAttributes.Const | MemberAttributes.Public,
                    InitExpression = new CodePrimitiveExpression(propertyKey)
                };
                constantContainer.Members.Add(propertyConstant);
            }

            return true;
        }
    }
}
