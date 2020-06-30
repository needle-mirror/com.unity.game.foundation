using System;
using UnityEngine.GameFoundation.DefaultLayers;

namespace UnityEngine.GameFoundation.Sample
{
    public class StoreSample : MonoBehaviour
    {
        private Transform m_WaitAnimation;
        private bool m_WrongDatabase;
        
        /// <summary>
        /// Prefab to show wait animation when Transaction is in progress
        /// </summary>
        public GameObject waitAnimationPrefab;

        /// <summary>
        /// Reference to the panel to display when the wrong database is in use.
        /// </summary>
        public GameObject wrongDatabasePanel;

        /// <summary>
        /// Standard starting point for Unity scripts.
        /// </summary>
        void Awake()
        {
            // The database has been properly setup.
            m_WrongDatabase = !SamplesHelper.VerifyDatabase();
            if (m_WrongDatabase)
            {
                wrongDatabasePanel.SetActive(true);
                return;
            }

            // - Initialize must always be called before working with any game foundation code.
            // - GameFoundation requires an IDataAccessLayer object that will provide and persist
            //   the data required for the various services (Inventory, Wallet, ...).
            // - For this sample we don't need to persist any data so we use the MemoryDataLayer
            //   that will store GameFoundation's data only for the play session.
            GameFoundation.Initialize(new MemoryDataLayer());
        }
        
        private void OnEnable()
        {
            TransactionManager.transactionInitiated += OnTransactionInitiated;
            TransactionManager.transactionSucceeded += OnTransactionSucceeded;
            TransactionManager.transactionFailed += OnTransactionFailed;
        }
        
        private void OnDisable()
        {
            TransactionManager.transactionInitiated -= OnTransactionInitiated;
            TransactionManager.transactionSucceeded -= OnTransactionSucceeded;
            TransactionManager.transactionFailed -= OnTransactionFailed;
        }
        
        private void OnTransactionInitiated(BaseTransaction transaction)
        {
            if (m_WaitAnimation == null && waitAnimationPrefab != null)
            {
                m_WaitAnimation = Instantiate(waitAnimationPrefab).transform;
            }
        }

        /// <summary>
        /// Callback that gets triggered when any item in the store is successfully purchased. Triggers the
        /// user-specified onPurchaseSuccess callback.
        /// </summary>
        private void OnTransactionSucceeded(BaseTransaction transaction, TransactionResult result)
        {
            RemoveLoadingAnimation();
        }

        /// <summary>
        /// Callback that gets triggered when any item in the store is attempted and fails to be purchased. Triggers the
        /// user-specified onPurchaseFailure callback.
        /// </summary>
        private void OnTransactionFailed(BaseTransaction transaction, Exception exception)
        {
            RemoveLoadingAnimation();
        }

        private void RemoveLoadingAnimation()
        {
            if (m_WaitAnimation == null) return;
            Destroy(m_WaitAnimation.gameObject);
            m_WaitAnimation = null;
        }
    }
}
