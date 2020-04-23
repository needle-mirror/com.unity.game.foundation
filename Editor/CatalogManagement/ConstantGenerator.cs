using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using Microsoft.CSharp;
using UnityEngine.GameFoundation.CatalogManagement;

namespace UnityEditor.GameFoundation.CatalogManagement
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
        
        static public void GenerateConstants(GameFoundationDatabase database)
        {
            var path = EditorUtility.SaveFilePanelInProject("Save constant file", database.name, "cs", null);
            if (string.IsNullOrWhiteSpace(path)) return;

            var name = Path.GetFileNameWithoutExtension(path);
            name = CollectionEditorTools.ConvertNameToId(name);
            name = string.Concat(((char)(name[0] + ('A' - 'a'))).ToString(), name.Substring(1));
            var compileUnit = new CodeCompileUnit();

            var @namespace = new CodeNamespace();
            compileUnit.Namespaces.Add(@namespace);

            var databaseClass = new CodeTypeDeclaration(name);
            @namespace.Types.Add(databaseClass);

            // Ignored, as static class is just a C# thing.
            databaseClass.Attributes = MemberAttributes.Static;

            var items = GenerateCatalogConstants(database.inventoryCatalog, "Items");
            databaseClass.Members.Add(items);

            var currencies = GenerateCatalogConstants(database.currencyCatalog, "Currencies");
            databaseClass.Members.Add(currencies);

            var transactions = GenerateCatalogConstants(database.transactionCatalog, "Transactions");
            databaseClass.Members.Add(transactions);

            var stores = GenerateCatalogConstants(database.storeCatalog, "Stores");
            databaseClass.Members.Add(stores);

            var stats = GenerateStatConstants(database);
            databaseClass.Members.Add(stats);

            var provider = new CSharpCodeProvider();

            using (StreamWriter sw = new StreamWriter(path, false))
            {
                var tw = new IndentedTextWriter(sw, "    ");
                var opt = new CodeGeneratorOptions();
                opt.BlankLinesBetweenMembers = false;
                opt.ElseOnClosing = false;
                opt.VerbatimOrder = false;
                provider.GenerateCodeFromCompileUnit(compileUnit, tw, opt);
                tw.Close();
            }

            AssetDatabase.Refresh();
        }

        static CodeTypeDeclaration GenerateCatalogConstants<TCatalogItemAsset>(
            SingleCollectionCatalogAsset<TCatalogItemAsset> catalog,
            string className)
            where TCatalogItemAsset : CatalogItemAsset
        {
            var items = catalog.GetItems();

            var itemClass = new CodeTypeDeclaration(className);

            // Ignored, as static class is just a C# thing.
            itemClass.Attributes = MemberAttributes.Static;

            var @string = new CodeTypeReference(typeof(string));

            foreach (var item in items)
            {
                var constant = new CodeMemberField(@string, item.id);
                constant.Attributes = MemberAttributes.Const | MemberAttributes.Public;

                itemClass.Members.Add(constant);

                var expression = new CodePrimitiveExpression(item.id);
                constant.InitExpression = expression;
            }

            return itemClass;
        }

        static CodeTypeDeclaration GenerateStatConstants
            (GameFoundationDatabase database)
        {
            var catalog = database.statCatalog;

            var stats = catalog.m_StatDefinitions;

            var statClass = new CodeTypeDeclaration("Stats");

            // Ignored, as static class is just a C# thing.
            statClass.Attributes = MemberAttributes.Static;

            var @string = new CodeTypeReference(typeof(string));

            foreach (var stat in stats)
            {
                var constant = new CodeMemberField(@string, stat.id);
                constant.Attributes = MemberAttributes.Const | MemberAttributes.Public;

                statClass.Members.Add(constant);

                var expression = new CodePrimitiveExpression(stat.id);
                constant.InitExpression = expression;
            }

            return statClass;
        }
    }
}
