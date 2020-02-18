using System;
using System.Collections.Generic;
using System.Globalization;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Contains a set of data to describe how a transaction occurred.
    /// </summary>
    public class TransactionReceipt
    {
        private long m_TransactionId;
        private string m_TransactionDateTime;
        private bool m_TransactionResult;
        private string m_FailureReason;
        private string m_InputInventory;
        private string m_OutputInventory;
        private string[] m_InputItems;
        private string[] m_OutputItems;

        /// <summary>
        /// The unique id for this receipt
        /// </summary>
        public long transactionId
        {
            get => m_TransactionId;
        }

        /// <summary>
        /// The date time this transaction occured
        /// Format is CultureInfo.InvariantCulture
        /// </summary>
        public string transactionDateTime
        {
            get => m_TransactionDateTime;
        }

        /// <summary>
        /// Whether or not the transaction was a success
        /// </summary>
        public bool succeeded
        {
            get => m_TransactionResult;
        }

        /// <summary>
        /// Whether or not the transaction was a failure
        /// </summary>
        public bool failed
        {
            get => !m_TransactionResult;
        }

        /// <summary>
        /// The reason this transaction failed (if it failed.)
        /// </summary>
        public string failureReason
        {
            get => m_FailureReason;
        }

        /// <summary>
        /// The id of the input inventory used for the transaction.
        /// </summary>
        public string inputInventory
        {
            get => m_InputInventory;
        }

        /// <summary>
        /// The id of the output inventory used for the transaction
        /// </summary>
        public string outputInventory
        {
            get => m_OutputInventory;
        }

        /// <summary>
        /// The inputs used for this transaction
        /// </summary>
        public string[] inputItems
        {
            get => m_InputItems;
        }

        /// <summary>
        /// The outputs used for this transaction
        /// </summary>
        public string[] outputItems
        {
            get => m_OutputItems;
        }

        /// <summary>
        /// Basic constructor takes in values for all fields of this receipt.
        /// Because the receipt is readonly, these values become set in stone once the receipt is made.
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="transactionResult"></param>
        /// <param name="inputInventory"></param>
        /// <param name="outputInventory"></param>
        /// <param name="price"></param>
        /// <param name="payout"></param>
        public TransactionReceipt(long transactionId, bool transactionResult, string failureReason, string inputInventory, string outputInventory, Price price, Payout payout)
        {
            m_TransactionId = transactionId;
            m_TransactionDateTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            m_FailureReason = failureReason;
            m_TransactionResult = transactionResult;
            m_InputInventory = inputInventory;
            m_OutputInventory = outputInventory;

            if (price != null)
            {
                var paymentItems = price.GetInputItems();
                if (paymentItems != null)
                {
                    List<string> inputItemsList = new List<string>();
                    foreach (InputItem inputItem in paymentItems)
                    {
                        inputItemsList.Add(inputItem.ToString());
                    }
                    m_InputItems = inputItemsList.ToArray();
                }
            }

            if (payout != null)
            {
                var payoutItems = payout.GetPayoutItems();
                if (payoutItems != null)
                {
                    List<string> outputItemsList = new List<string>();
                    foreach (OutputItem outputItem in payoutItems)
                    {
                        outputItemsList.Add(outputItem.ToString());
                    }

                    m_OutputItems = outputItemsList.ToArray();
                }
            }
        }
    }
}
