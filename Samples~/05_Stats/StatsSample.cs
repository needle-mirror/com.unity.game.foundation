using System.Collections.Generic;
using System.Text;
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
        /// Flag for whether the Inventory has changes in it that have not yet been updated in the UI.
        /// </summary>
        private bool m_InventoryChanged;

        /// <summary>
        /// References for easy access.
        /// </summary>
        private InventoryItem m_Sword;
        private InventoryItem m_HealthPotion;

        // Variable to track player health value
        private float m_PlayerHealth = 100;

        /// <summary>
        /// Reference to a list of InventoryItems in the InventoryManager.
        /// </summary>
        private readonly List<InventoryItem> m_InventoryItems = new List<InventoryItem>();
        
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
        /// References to the specific buy/sell buttons to enable/disable when either action is not possible.
        /// </summary>
        public Button takeDamageButton;
        public Button healButton;

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
            //   the data required for the various services (Inventory, Stats, ...).
            // - For this sample we don't need to persist any data so we use the MemoryDataLayer
            //   that will store GameFoundation's data only for the play session.
            GameFoundation.Initialize(new MemoryDataLayer());

            // We will create the sword and health potion inventoryItems in the InventoryManager and
            // store their references to get us started.
            m_Sword = InventoryManager.CreateItem("sword");
            m_HealthPotion = InventoryManager.CreateItem("healthPotion");

            // Here we bind our UI refresh method to callbacks on the inventory manager.
            // These callbacks will automatically be invoked anytime an inventory is added, or removed.
            // This prevents us from having to manually invoke RefreshUI every time we perform one of these actions.
            InventoryManager.itemAdded += OnInventoryItemChanged;
            InventoryManager.itemRemoved += OnInventoryItemChanged;

            RefreshUI();
        }

        /// <summary>
        /// Standard Update method for Unity scripts.
        /// </summary>
        private void Update()
        {
            // This flag will be set to true when something has changed in the InventoryManager (either items were added or removed)
            if (m_InventoryChanged)
            {
                RefreshUI();
                m_InventoryChanged = false;
            }
        }

        /// <summary>
        /// Apply the sword's damage value to the player's health.
        /// </summary>
        public void TakeDamage()
        {
            // Get the damage value of the sword by checking the damage stat attached to the item.
            // Even though we're pretty sure our sword should have a damage stat attached, we'll
            // confirm with HasStat before accessing to prevent crashes.
            // When initializing variables that will store StatValues of type float using var, the initial value needs to specify that it is a float
            var damage = 0f;
            if (m_Sword.HasStat("damage"))
            {
                damage = m_Sword.GetStat("damage");
            }
            
            // Get the quantity of swords available by checking the quantity stat attached to the item.
            var quantity = 0;
            if (m_Sword.HasStat("quantity"))
            {
                quantity = m_Sword.GetStat("quantity");
            }

            if (quantity > 0 && damage > 0 && m_PlayerHealth >= damage )
            {
                // Apply the damage to playerHealth
                m_PlayerHealth -= damage;

                // If the sword doesn't have a durability stat, no action needs to be taken.
                // If it does have a durability stat, we will lower the sword's durability,
                // if it drops to 0, a single sword has been used.
                if (m_Sword.HasStat("durability"))
                {
                    var durability = m_Sword.GetStat("durability");
                    if (durability == 1)
                    {
                        // If there is only one quantity of sword left, remove the sword item from the Inventory,
                        // otherwise, reduce the quantity in the stat by one and reset the durability stat.
                        if (quantity == 1)
                        {
                            InventoryManager.RemoveItem(m_Sword);
                            // Once we remove the m_Sword item from the InventoryManager, the reference is no longer useful.
                            m_Sword = null;
                        }
                        else
                        {
                            m_Sword.SetStat("quantity", quantity - 1);
                            m_Sword.SetStat("durability", 3);
                        }
                    }
                    else
                    {
                        m_Sword.SetStat("durability", m_Sword.GetStat("durability") - 1);
                    }
                }

                m_InventoryChanged = true;
            }
        }

        /// <summary>
        /// Increases the player's health by the health restore stat of a health potion, then removes it.
        /// This only happens if there is at least one health potion in the inventory, and if the player's health is not maxed out.
        /// </summary>
        public void Heal()
        {
            // Get the quantity of health potions available by checking the quantity stat attached to the item.
            var quantity = 0;
            if (m_HealthPotion.HasStat("quantity"))
            {
                quantity = m_HealthPotion.GetStat("quantity");
            }

            if (quantity > 0)
            {
                if (m_PlayerHealth < 100)
                {
                    // We'll confirm that Health Potion has the healthRestore stat on it before accessing to prevent exceptions
                    // and only reduce the quantity of the health potion if it can in fact restore health to the player.
                    if (m_HealthPotion.HasStat("healthRestore"))
                    {
                        // We need to cast GetStat to an int in this case because healthRestore is a StatValue of type int, but Mathf.Min expects a float
                        float health = Mathf.Min((int)m_HealthPotion.GetStat("healthRestore") + m_PlayerHealth, 100f);
                        m_PlayerHealth = health;
                        
                        // If there is only one quantity of health potion left, remove the health potion item from the Inventory,
                        // otherwise, reduce the quantity in the stat by one.
                        if (quantity == 1)
                        {
                            InventoryManager.RemoveItem(m_HealthPotion);
                            // Once we remove the m_HealthPotion item from the InventoryManager, the reference is no longer useful.
                            m_HealthPotion = null;
                        }
                        else
                        {
                            m_HealthPotion.SetStat("quantity", quantity - 1);
                        }
                    }
                }

                m_InventoryChanged = true;
            }
        }

        /// <summary>
        /// This will fill out the main text box with information about the main inventory.
        /// </summary>
        private void RefreshUI()
        {
            m_DisplayText.Clear();
            // Show the player's health
            m_DisplayText.Append("Health: " + m_PlayerHealth);
            m_DisplayText.AppendLine();
            m_DisplayText.AppendLine();

            // To save allocations we will reuse our m_InventoryItems list each time. The GetItems method will clear the list passed in.
            InventoryManager.GetItems(m_InventoryItems);
            
            // Loop through every type of item within the inventory and display its name and quantity.
            foreach (var inventoryItem in m_InventoryItems)
            {
                // All InventoryItems have an associated InventoryItemDefinition which contains a display name.
                string itemName = inventoryItem.definition.displayName;

                // We'll initialize our quantity value to 1, that way if an item doesn't have a quantity stat, we still represent the 1 item
                // in the InventoryManager. Depending on your implementation, you may instead want to initialize to 0.
                var quantity = 1;
                if (inventoryItem.HasStat("quantity"))
                {
                    quantity = inventoryItem.GetStat("quantity");
                }

                m_DisplayText.Append("<b>" + itemName + "</b>: " + quantity);
                m_DisplayText.AppendLine();

                // For items with health restore, durability, or damage stats, we want to display their values here.
                if (inventoryItem.HasStat("healthRestore"))
                {
                    m_DisplayText.Append("- Health Restore: " + inventoryItem.GetStat("healthRestore"));
                    m_DisplayText.AppendLine();
                }

                if (inventoryItem.HasStat("damage"))
                {
                    m_DisplayText.Append("- Damage: " + inventoryItem.GetStat("damage"));
                    m_DisplayText.AppendLine();
                }

                if (inventoryItem.HasStat("durability") && quantity > 0)
                {
                    m_DisplayText.Append("- Durability: " + inventoryItem.GetStat("durability"));
                    m_DisplayText.AppendLine();
                }

                m_DisplayText.AppendLine();
            }

            mainText.text = m_DisplayText.ToString();

            RefreshDamageAndHealButtons();
        }

        /// <summary>
        /// This method will turn the heal/damage buttons on/off if the conditions for their functionality are met. 
        /// </summary>
        private void RefreshDamageAndHealButtons()
        {
            // First we'll safely get our items' stats that we need to complete our condition checks.
            var swordQuantity = 0;
            if (m_Sword != null && m_Sword.HasStat("quantity"))
            {
                swordQuantity = m_Sword.GetStat("quantity");
            }

            // When initializing variables that will store StatValues of type float using var, the initial value needs to specify that it is a float
            var swordDamage = 0f;
            if (m_Sword != null && m_Sword.HasStat("damage"))
            {
                swordDamage = m_Sword.GetStat("damage");
            }

            var potionQuantity = 0;
            if (m_HealthPotion != null && m_HealthPotion.HasStat("quantity"))
            {
                potionQuantity = m_HealthPotion.GetStat("quantity");
            }
            
            takeDamageButton.interactable = swordQuantity > 0 && swordDamage > 0 && m_PlayerHealth >= swordDamage;
            healButton.interactable = potionQuantity > 0 && m_PlayerHealth < 100;
        }

        /// <summary>
        /// Listener for changes in InventoryManager. Will get called whenever an item is added or removed.
        /// Because many items can get added or removed at a time, we will have the listener only set a flag
        /// that changes exist, and on our next update, we will check the flag to see whether changes to the UI
        /// need to be made.
        /// </summary>
        /// <param name="itemChanged">This parameter will not be used, but must exist so the signature is compatible with the inventory callbacks so we can bind it.</param>
        private void OnInventoryItemChanged(GameItem itemChanged)
        {
            m_InventoryChanged = true;
        }
    }
}
