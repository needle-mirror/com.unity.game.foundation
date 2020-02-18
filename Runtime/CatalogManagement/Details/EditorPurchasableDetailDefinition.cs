using System.Collections.Generic;

namespace UnityEngine.GameFoundation.CatalogManagement
{
    public class EditorPurchasableDetailDefinition : BaseDetailDefinition
    {
        public override string DisplayName()
        {
            return "Purchasable Detail";
        }
        
        [SerializeField]
        private string m_DefaultSourceInventoryId = InventoryCatalog.k_WalletInventoryDefinitionId;
        /// <summary>
        /// Inventory to look into if InputItem doesn't specify an inventory to look into.
        /// </summary>
        public string defaultSourceInventoryId { get => m_DefaultSourceInventoryId; }

        [SerializeField]
        private string m_DefaultDestinationInventoryId = InventoryCatalog.k_MainInventoryDefinitionId;
        /// <summary>
        /// Inventory to deposit into if OutputItem doesn't specify an inventory to deposit into.
        /// </summary>
        public string defaultDestinationInventoryId { get => m_DefaultDestinationInventoryId; }
        
        // These fields are serialized an populated in the editor via serialized objects. Ignore "unused variable warnings"        
#pragma warning disable 649
        [SerializeField] private EditorPayout m_Payout;
        /// <summary>
        /// The output for this transaction
        /// </summary>
        public EditorPayout payout { get => m_Payout; }
        
        [SerializeField] 
        private List<EditorPrice> m_Prices;
        /// <summary>
        /// The price points available for this transaction.
        /// </summary>
        public List<EditorPrice> prices { get => new List<EditorPrice>(m_Prices); }
#pragma warning restore 649
        
        /// <summary>
        /// Retrieve the underlying payment items in this transaction
        /// </summary>
        /// <param name="name">name of the price points id you want the input items for</param>
        /// <returns></returns>
        public List<EditorInputItem> GetInputs(string name)
        {
            foreach (EditorPrice pricePoint in prices)
            {
                if (pricePoint.name.Equals(name))
                {
                    return pricePoint.inputItems;
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieve the underlying payout items in this transaction
        /// </summary>
        /// <returns></returns>
        public List<EditorOutputItem> GetOutputs()
        {
            return payout.outputItems;
        }

        /// <summary>
        /// Creates a runtime PurchasableDetailDefinition based on this editor-time PurchasableDetailDefinition.
        /// </summary>
        /// <returns>Runtime version of PurchasableDetailDefinition.</returns>
        public override UnityEngine.GameFoundation.BaseDetailDefinition CreateRuntimeDefinition()
        {
            // 4 Pieces of information that make a Purchasable Detail
            string defaultSourceInventoryId = m_DefaultSourceInventoryId;
            string defaultDestinationInventoryId = m_DefaultDestinationInventoryId;

            // Setting up OutputItems
            List<OutputItem> outputItems = new List<OutputItem>();
            foreach (var outputItem in this.payout.outputItems)
            {
                outputItems.Add(new OutputItem(outputItem.definitionId, outputItem.quantity, outputItem.destinationInventoryId));
            }
            var payout = new Payout(outputItems);
            var prices = new List<Price>();
            // Setting up Prices
            foreach (var price in this.prices)
            {
                //Setting up Input Items within the price object
                List<InputItem> inputItems = new List<InputItem>();
                foreach (var inputItem in price.inputItems)
                {
                    inputItems.Add(new InputItem(inputItem.definitionId, inputItem.price, inputItem.sourceInventoryId));
                }
                prices.Add(new Price(inputItems, price.name));
            }
                        
            return new PurchasableDetailDefinition(prices, payout, defaultSourceInventoryId, defaultDestinationInventoryId);
        }
    }
}
