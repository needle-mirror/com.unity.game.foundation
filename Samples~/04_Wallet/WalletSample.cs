using System.Text;
using UnityEngine.GameFoundation.DefaultLayers;
using UnityEngine.UI;

namespace UnityEngine.GameFoundation.Sample
{
    /// <summary>
    /// This class manages the scene for showcasing the wallet and store sample.
    /// </summary>
    public class WalletSample : MonoBehaviour
    {
        private bool m_WrongDatabase;

        /// <summary>
        /// Flag for whether the Wallet has changes in it that have not yet been updated in the UI.
        /// </summary>
        private bool m_WalletChanged;

        /// <summary>
        /// We will want to hold onto reference to currency for easy use later.
        /// </summary>
        private Currency m_CoinDefinition;

        /// <summary>
        /// Used to reduce times mainText.text is accessed.
        /// </summary>
        private readonly StringBuilder m_DisplayText = new StringBuilder();

        /// <summary>
        /// Reference to the panel to display when the wrong database is in use.
        /// </summary>
        public GameObject wrongDatabasePanel;

        /// <summary>
        /// We will need a reference to the main text box in the scene so we can easily modify it.
        /// </summary>
        public Text mainText;

        /// <summary>
        /// Reference to the specific lose coin button to enable/disable when the action is not possible.
        /// </summary>
        public Button loseButton;

        /// <summary>
        /// Standard starting point for Unity scripts.
        /// </summary>
        private void Start()
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

            m_CoinDefinition = GameFoundation.catalogs.currencyCatalog.FindItem("coin");

            // Here we bind a listener that will set a walletChanged flag to callbacks on the Wallet Manager.
            // These callbacks will automatically be invoked anytime a currency balance is changed.
            // This prevents us from having to manually invoke RefreshUI every time we perform one of these actions.
            WalletManager.balanceChanged += OnCoinBalanceChanged;

            // We'll initialize our WalletManager's coin balance with 50 coins.
            // This will set the balance to 50 no matter what it's current balance is.
            WalletManager.SetBalance(m_CoinDefinition, 50);

            RefreshUI();
        }

        /// <summary>
        /// Standard Update method for Unity scripts.
        /// </summary>
        private void Update()
        {
            // This flag will be set to true when the balance of a currency has changed in the WalletManager
            if (m_WalletChanged)
            {
                RefreshUI();
                m_WalletChanged = false;
            }
        }

        /// <summary>
        /// Standard cleanup point for Unity scripts.
        /// </summary>
        private void OnDestroy()
        {
            if (WalletManager.IsInitialized)
                WalletManager.balanceChanged -= OnCoinBalanceChanged;
        }

        /// <summary>
        /// This method adds 50 coins to the wallet.
        /// </summary>
        public void FindBagOfCoins()
        {
            WalletManager.AddBalance(m_CoinDefinition, 50);
        }

        /// <summary>
        /// This method deducts 10 coins from the wallet.
        /// </summary>
        public void Drop10Coins()
        {
            WalletManager.RemoveBalance(m_CoinDefinition, 10);
        }

        /// <summary>
        /// This will fill out the main text box with information about the wallet.
        /// </summary>
        private void RefreshUI()
        {
            m_DisplayText.Clear();
            m_DisplayText.Append("<b><i>Wallet:</i></b>");
            m_DisplayText.AppendLine();
            
            var coinBalance = WalletManager.GetBalance(m_CoinDefinition);
            m_DisplayText.Append($"Currency - {m_CoinDefinition.displayName}: {coinBalance.ToString()}");

            mainText.text = m_DisplayText.ToString();

            RefreshLoseCoinsButton();
        }

        /// <summary>
        /// Enables/Disables the add/lose coins buttons.
        /// The addButton will always be interactable,
        /// but we can only lose coins if we have enough to lose.
        /// </summary>
        private void RefreshLoseCoinsButton()
        {
            var coinBalance = WalletManager.GetBalance(m_CoinDefinition);
            loseButton.interactable = coinBalance >= 10;
        }

        /// <summary>
        /// This will be called every time a currency balance is changed.
        /// </summary>
        /// <param name="args">
        /// Data related to the <see cref="WalletManager.balanceChanged"/> event.
        /// </param>
        private void OnCoinBalanceChanged(BalanceChangedEventArgs args)
        {
            if (args.currency.key != m_CoinDefinition.key)
                return;

            m_WalletChanged = true;
        }
    }
}
