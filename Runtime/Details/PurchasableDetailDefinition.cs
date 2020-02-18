using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    public class PurchasableDetailDefinition : BaseDetailDefinition
    {
        /// <summary>
        /// The output for this transaction
        /// </summary>
        public Payout payout { get; }

        private List<Price> m_Prices;
        private List<Price> m_SearchedPrices = new List<Price>();
        private List<string> m_PriceKeys = new List<string>();
        
        /// <summary>
        /// Inventory to look into if InputItem doesn't specify an inventory to look into.
        /// </summary>
        public string defaultSourceInventoryId { get; }
        
        /// <summary>
        /// Inventory to deposit into if OutputItem doesn't specify an inventory to deposit into.
        /// </summary>
        public string defaultDestinationInventoryId { get; }
        
        /// <summary>
        /// Sets up a detail for a single price and single payout. Also has optional parameters for default source and destination overrides.
        /// </summary>
        /// <param name="price">The price to use.</param>
        /// <param name="payout">The payout to use.</param>
        /// <param name="defaultSourceInventoryId">Inventory to look in for inputs.</param>
        /// <param name="defaultDestinationInventoryId">Inventory to look in for outputs.</param>
        public PurchasableDetailDefinition(Price price, Payout payout, string defaultSourceInventoryId = "", string defaultDestinationInventoryId = "")
        {
            m_Prices = new List<Price>() {new Price(price)};
            this.payout = payout;
            this.defaultSourceInventoryId = defaultSourceInventoryId;
            this.defaultDestinationInventoryId = defaultDestinationInventoryId;
        }

        /// <summary>
        /// Sets up a detail for multiple price points and a single payout. Also has optional parameters for default source and destination overrides.
        /// </summary>
        /// <param name="prices">The prices to use for this detail.</param>
        /// <param name="payout">The payout to use.</param>
        /// <param name="defaultSourceInventoryId">Inventory to look in for inputs.</param>
        /// <param name="defaultDestinationInventoryId">Inventory to look in for outputs.</param>
        public PurchasableDetailDefinition(List<Price> prices, Payout payout, string defaultSourceInventoryId = "", string defaultDestinationInventoryId = "")
        {
            m_Prices = new List<Price>();
            foreach (Price price in prices)
            {
                m_Prices.Add(new Price(price));
            }
            
            this.payout = payout;
            this.defaultSourceInventoryId = defaultSourceInventoryId;
            this.defaultDestinationInventoryId = defaultDestinationInventoryId;
        }

        /// <summary>
        /// Retrieve the price with the given id on this detail.
        /// Ideally this method should only be used when the developer knows the detail only has one price with the given id.
        /// If there are multiple prices with that id, it will simply return the first one, which is hard to determine exactly what it will be with multiple.
        /// </summary>
        /// <param name="name">The id to search for</param>
        /// <returns>The price with the matching ID.</returns>
        public Price GetPrice(string name = "default")
        {
            foreach (Price pricePoint in m_Prices)
            {
                if (pricePoint.name.Equals(name))
                {
                    return pricePoint;
                }
            }

            return null;
        }

        /// <summary>
        /// This is for getting all prices with a given id.
        /// </summary>
        /// <param name="name">The id to search for</param>
        /// <returns>An array of every Price on this detail with the given id.</returns>
        public Price[] GetPrices(string name = "default")
        {
            m_SearchedPrices.Clear();
            foreach (Price pricePoint in m_Prices)
            {
                if (pricePoint.name.Equals(name))
                {
                    m_SearchedPrices.Add(pricePoint);
                }
            }

            return m_SearchedPrices.ToArray();
        }

        /// <summary>
        /// Checks all prices for keys and returns an array of all keys with no duplicates.
        /// </summary>
        /// <returns>An array of unique price keys.</returns>
        public string[] GetPriceKeys()
        {
            m_PriceKeys.Clear();
            foreach (Price pricePoint in m_Prices)
            {
                string name = pricePoint.name;
                if (!m_PriceKeys.Contains(name))
                {
                    m_PriceKeys.Add(name);
                }
            }

            return m_PriceKeys.ToArray();
        }
    }
}
