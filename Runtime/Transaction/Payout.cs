using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Represents a set of items to grant for a successful transaction.
    /// </summary>
    public class Payout
    {
        /// <summary>
        /// Simple callback delegate
        /// </summary>
        public delegate void PayoutEvent();

        private List<OutputItem> m_PayoutItems;

        /// <summary>
        /// Array of OutputItems to be paid out.
        /// </summary>
        public OutputItem[] GetPayoutItems()
        {
            if (m_PayoutItems == null)
                return null;

            OutputItem[] items = new OutputItem[m_PayoutItems.Count];
            m_PayoutItems.CopyTo(items, 0);
            return items;
        }

        /// <summary>
        /// Puts all payout items into the given list.
        /// </summary>
        /// <param name="items">The given list to populate.</param>
        public void GetPayoutItems(List<OutputItem> items)
        {
            if (items == null)
            {
                return;
            }

            items.Clear();

            if (m_PayoutItems == null)
            {
                return;
            }
            
            items.AddRange(m_PayoutItems);
        }

        private event PayoutEvent m_PayoutEvent;

        /// <summary>
        /// Constructor for setting up a payout with multiple outputs.
        /// </summary>
        /// <param name="outputItems">The outputs this payout will give.</param>
        /// <param name="payoutEvent">The callback to invoke when the payout finishes.</param>
        public Payout(List<OutputItem> outputItems, PayoutEvent payoutEvent = null)
        {  
            m_PayoutItems = new List<OutputItem>();
            foreach (OutputItem outputItem in outputItems)
            {
                m_PayoutItems.Add(new OutputItem(outputItem));
            }
            m_PayoutEvent = payoutEvent;
        }

        /// <summary>
        /// Constructor for setting up a payout with a single output.
        /// Many payouts will be simple 1 items so this constructor allows easy setup for that without need of an external list.
        /// </summary>
        /// <param name="outputItem">The item this payout will give.</param>
        /// <param name="payoutEvent">The callback to invoke when the payout finishes.</param>
        public Payout(OutputItem outputItem, PayoutEvent payoutEvent = null)
        {
            m_PayoutItems = new List<OutputItem>();
            m_PayoutItems.Add(new OutputItem(outputItem));
            m_PayoutEvent = payoutEvent;
        }

        /// <summary>
        /// Copy constructor for setting up a new payout based off of an existing one.
        /// </summary>
        /// <param name="other">The other payout to base this one off of.</param>
        public Payout(Payout other)
        {
            m_PayoutItems = new List<OutputItem>();
            foreach (OutputItem outputItem in other.m_PayoutItems)
            {
                m_PayoutItems.Add(new OutputItem(outputItem));
            }
            
            m_PayoutEvent = other.m_PayoutEvent;
        }

        /// <summary>
        /// Simply invokes the payout event.
        /// This ensures other classes will only be able to trigger the event and not be able to do things like clear it.
        /// </summary>
        public void InvokePayoutEvent()
        {
            m_PayoutEvent?.Invoke();
        }
    }
}
