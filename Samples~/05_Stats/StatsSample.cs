using UnityEngine.GameFoundation.DataAccessLayers;
using UnityEngine.UI;

namespace UnityEngine.GameFoundation.Sample
{
    /// <summary>
    /// This class manages the scene for showcasing the Stats system.
    /// </summary>
    public class StatsSample : MonoBehaviour
    {
        private bool m_WrongDatabase;

        /// <summary>
        /// Reference to the panel to display when the wrong database is in use.
        /// </summary>
        public GameObject wrongDatabasePanel;

        /// <summary>
        /// We will need a reference to the main text box in the scene so we can easily modify it.
        /// </summary>
        public Text mainText;

        /// <summary>
        /// References to the specific buy/sell buttons to enable/disable when either action is not possible.
        /// </summary>
        public Button takeDamageButton;
        public Button healButton;

        /// <summary>
        /// References for easy access.
        /// </summary>
        private Inventory m_Backpack;
        private InventoryItem m_Sword;
        private InventoryItem m_HealthPotion;

        /// <summary>
        /// Stats are associated with game items, so we will need one to keep track of the player's health.
        /// </summary>
        private GameItem m_PlayerStats;

        /// <summary>
        /// Standard starting point for Unity scripts.
        /// </summary>
        void Start()
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
            //   the data required for the various services (Inventory, Stats, ...).
            // - For this sample we don't need to persist any data so we use the MemoryDataLayer
            //   that will store GameFoundation's data only for the play session.
            GameFoundation.Initialize(new MemoryDataLayer());

            // Create a backpack inventory instance and keep the reference. Grab its sword reference as well.
            m_Backpack = InventoryManager.CreateInventory("backpack", "MainBackpack", "Backpack");
            m_Sword = m_Backpack.GetItem("sword");
            m_HealthPotion = m_Backpack.GetItem("healthPotion");

            // Setup our player stats instance.
            m_PlayerStats = new GameItem(CatalogManager.gameItemCatalog.GetGameItemDefinition("player"));

            // Here we bind our UI refresh method to callbacks on the inventory manager.
            // These callbacks will automatically be invoked anytime an inventory is added, or removed.
            // This prevents us from having to manually invoke RefreshUI every time we perform one of these actions.
            m_Backpack.onItemAdded += RefreshUI;
            m_Backpack.onItemRemoved += RefreshUI;
            m_Backpack.onItemQuantityChanged += RefreshUI;

            RefreshUI();
        }

        /// <summary>
        /// This will fill out the main text box with information about the main inventory.
        /// </summary>
        /// <param name="item">This parameter will not be used, but must exist so the signature is compatible with the inventory callbacks so we can bind it.</param>
        private void RefreshUI(InventoryItem item = null)
        {
            // Show the player's health
            mainText.text = "Health: " + m_PlayerStats.GetStatFloat("health") + "\n\n";

            // Loop through every type of item within the inventory and display its name and quantity.
            foreach (InventoryItem inventoryItem in m_Backpack.GetItems())
            {
                // All game items have an associated display name, this includes game items.
                string itemName = inventoryItem.displayName;

                // Every inventory item has an associated quantity. This represents how many units of this item there are within the inventory.
                int quantity = inventoryItem.quantity;

                mainText.text += "<b>" + itemName + "</b>: ";

                mainText.text += quantity + "\n";

                // For items with health restore, durability, or damage stats, we want to display their values here.
                if (StatManager.HasIntValue(inventoryItem, "healthRestore"))
                {
                    mainText.text += "- Health Restore: " + inventoryItem.GetStatInt("healthRestore") + "\n";
                }

                if (StatManager.HasFloatValue(inventoryItem, "damage"))
                {
                    mainText.text += "- Damage: " + inventoryItem.GetStatFloat("damage") + "\n";
                }

                if (StatManager.HasIntValue(inventoryItem, "durability") && inventoryItem.quantity > 0)
                {
                    mainText.text += "- Durability: " + inventoryItem.GetStatInt("durability") + "\n";
                }

                mainText.text += "\n";
            }

            RefreshDamageAndHealButtons();
        }

        /// <summary>
        /// Apply the sword's damage value to the player's health.
        /// </summary>
        public void TakeDamage()
        {
            // Query the player's current health, and damage value of the sword
            float health = m_PlayerStats.GetStatFloat("health");
            float damage = m_Sword.GetStatFloat("damage");

            if (m_Sword.quantity > 0 && health > damage)
            {
                // Apply the damage if possible and update the stat
                health -= damage;
                m_PlayerStats.SetStatFloat("health", health);

                // Lower the sword's durability, if it drops to 0, a single sword has been used.
                int durability = m_Sword.GetStatInt("durability");
                if (durability == 1)
                {
                    m_Sword.quantity -= 1;
                    m_Sword.SetStatInt("durability", 4);
                }
                else
                {
                    m_Sword.SetStatInt("durability", m_Sword.GetStatInt("durability") - 1);
                }

                RefreshUI();
            }
        }

        /// <summary>
        /// Increases the player's health by the health restore stat of a health potion, then removes it.
        /// This only happens if there is at least one health potion in the inventory, and if the player's health is not maxed out.
        /// </summary>
        public void Heal()
        {
            if (m_HealthPotion.quantity > 0)
            {
                if (m_PlayerStats.GetStatFloat("health") < 100)
                {
                    float health = Mathf.Min(m_HealthPotion.GetStatInt("healthRestore") + m_PlayerStats.GetStatFloat("health"), 100f);
                    m_PlayerStats.SetStatFloat("health", health);
                    m_HealthPotion.quantity -= 1;
                }
            }
        }

        /// <summary>
        /// This method will turn the heal/damage buttons on/off if the conditions for their functionality are met. 
        /// </summary>
        public void RefreshDamageAndHealButtons()
        {
            takeDamageButton.interactable = m_Sword.quantity > 0 && m_PlayerStats.GetStatFloat("health") > m_Sword.GetStatFloat("damage");
            healButton.interactable = m_HealthPotion.quantity > 0 && m_PlayerStats.GetStatFloat("health") < 100;
        }
    }
}
