using System.Collections.Generic;
using System.Text;
using UnityEngine.GameFoundation.DefaultLayers;
using UnityEngine.UI;

namespace UnityEngine.GameFoundation.Sample
{
    /// <summary>
    /// This class manages the scene for showcasing the Properties system.
    /// </summary>
    public class PropertiesSample : MonoBehaviour
    {
        private bool m_WrongDatabase;

        /// <summary>
        /// Flag to refresh the UI at the next frame.
        /// </summary>
        private bool m_MustRefreshUI;

        /// <summary>
        /// References for easy access.
        /// </summary>
        private InventoryItem m_Sword;
        private InventoryItem m_HealthPotion;

        /// <summary>
        /// Variable to track player health value.
        /// </summary>
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
            //   the data required for the various services (Inventory, Wallet, ...).
            // - For this sample we don't need to persist any data so we use the MemoryDataLayer
            //   that will store GameFoundation's data only for the play session.
            GameFoundation.Initialize(new MemoryDataLayer());

            // For this sample, we're focusing on swords and health, so let's remove all others from the Inventory.
            // Note: this is helpful since we have an initial allocation of 2 apples and 1 orange.
            InventoryManager.RemoveAllItems();

            // We will create the sword and health potion inventoryItems in the InventoryManager and
            // store their references to get us started.
            m_Sword = InventoryManager.CreateItem("sword");
            m_HealthPotion = InventoryManager.CreateItem("healthPotion");

            // Here we bind our UI refresh method to callbacks on the inventory manager.
            // These callbacks will automatically be invoked anytime an inventory item is added, or removed.
            // This allows us to refresh the UI as soon as the changes are applied.
            InventoryManager.itemAdded += OnInventoryItemChanged;
            InventoryManager.itemRemoved += OnInventoryItemChanged;

            // These events will automatically be invoked when sword's or potion's properties are changed.
            m_Sword.propertyChanged += OnItemPropertyChanged;
            m_HealthPotion.propertyChanged += OnItemPropertyChanged;

            RefreshUI();
        }

        /// <summary>
        /// Standard ending point for Unity scripts.
        /// </summary>
        private void OnDestroy()
        {
            // We must make sure to stop listening to events when this script will be destroyed.
            InventoryManager.itemAdded -= OnInventoryItemChanged;
            InventoryManager.itemRemoved -= OnInventoryItemChanged;

            if (m_Sword != null)
            {
                m_Sword.propertyChanged -= OnItemPropertyChanged;
            }

            if (m_HealthPotion != null)
            {
                m_HealthPotion.propertyChanged -= OnItemPropertyChanged;
            }
        }

        /// <summary>
        /// Standard Update method for Unity scripts.
        /// </summary>
        private void Update()
        {
            // This flag will be set to true when something has changed in the InventoryManager (either items were added or removed)
            if (m_MustRefreshUI)
            {
                RefreshUI();
                m_MustRefreshUI = false;
            }
        }

        /// <summary>
        /// Apply the sword's damage value to the player's health.
        /// </summary>
        public void TakeDamage()
        {
            // Get the damage value of the sword by checking the damage property attached to the item.
            // Even though we're pretty sure our sword has a damage property attached,
            // we use TryGetProperty to prevent crashes in case we made a typo or
            // if the catalogs are not set up as expected.
            float damage = 0;
            if (m_Sword.TryGetProperty("damage", out var damageProperty))
            {
                // We explicitly use AsFloat here to get the float stored in the property
                // but we would have the same result if we directly assigned the property to the float variable. 
                damage = damageProperty.AsFloat();
            }

            // Get the quantity of swords available by checking the quantity property attached to the item.
            // We could use HasProperty and GetProperty to check the property's existence then retrieve its value
            // but it is faster to use TryGetProperty since it does both in a single call.
            var quantity = 0;
            if (m_Sword.TryGetProperty("quantity", out var quantityProperty))
            {
                quantity = quantityProperty;
            }

            if (quantity <= 0 || damage <= 0 || m_PlayerHealth < damage)
            {
                return;
            }

            // Apply the damage to playerHealth
            m_PlayerHealth -= damage;

            // If the sword doesn't have a durability property, no action needs to be taken.
            // If it does have a durability property, we will lower the sword's durability,
            // if it drops to 0, a single sword has been used.
            if (m_Sword.TryGetProperty("durability", out var durabilityProperty))
            {
                var durability = durabilityProperty.AsInt();
                if (durability == 1)
                {
                    // If there is only one quantity of sword left, remove the sword item from the Inventory,
                    // otherwise, reduce the quantity in the property by one and reset the durability property.
                    if (quantity == 1)
                    {
                        InventoryManager.RemoveItem(m_Sword);

                        // Once we remove the m_Sword item from the InventoryManager, the reference is no longer useful.
                        m_Sword = null;
                    }
                    else
                    {
                        // We use AdjustProperty to apply the change to the current property's value.
                        m_Sword.AdjustProperty("quantity", -1);

                        m_Sword.SetProperty("durability", 3);
                    }
                }
                else
                {
                    m_Sword.AdjustProperty("durability", -1);
                }
            }

            RefreshUI();
        }

        /// <summary>
        /// Increases the player's health by the health restore property of a health potion, then removes it.
        /// This only happens if there is at least one health potion in the inventory, and if the player's health is not maxed out.
        /// </summary>
        public void Heal()
        {
            // Get the quantity of health potions available by checking the quantity property attached to the item.
            var quantity = 0;
            if (m_HealthPotion.TryGetProperty("quantity", out var quantityProperty))
            {
                quantity = quantityProperty;
            }

            if (quantity <= 0 || m_PlayerHealth >= 100)
            {
                return;
            }

            // We'll confirm that Health Potion has the healthRestore property on it before accessing to prevent exceptions
            // and only reduce the quantity of the health potion if it can in fact restore health to the player.
            if (m_HealthPotion.TryGetProperty("healthRestore", out var healthRestoreProperty))
            {
                // Mathf.Min expects a float value but "healthRestore" is an integer property.
                // This isn't a problem, we can cast it to a floating value by either doing
                // an explicit cast or using the AsFloat method.
                var health = Mathf.Min(healthRestoreProperty.AsFloat() + m_PlayerHealth, 100f);

                m_PlayerHealth = health;

                // If there is only one quantity of health potion left,
                // remove the health potion item from the Inventory;
                // otherwise, reduce the quantity in the property by one.
                if (quantity == 1)
                {
                    InventoryManager.RemoveItem(m_HealthPotion);

                    // Once we remove the m_HealthPotion item from the InventoryManager, the reference is no longer useful.
                    m_HealthPotion = null;
                }
                else
                {
                    m_HealthPotion.AdjustProperty("quantity", -1);
                }

                // Remember, we don't need to set the refresh UI flag here since it will
                // be set by OnInventoryItemChanged as soon as the changes are applied.
            }
        }

        /// <summary>
        /// This will fill out the main text box with information about the main inventory.
        /// </summary>
        private void RefreshUI()
        {
            m_DisplayText.Clear();

            // To save allocations we will reuse our m_InventoryItems list each time.
            // The GetItems method will clear the list passed in.
            InventoryManager.GetItems(m_InventoryItems);

            m_DisplayText.Append("<b><i>Inventory:</i></b>");
            m_DisplayText.AppendLine();
            m_DisplayText.AppendLine();

            // Loop through every type of item within the inventory and display its name and quantity.
            foreach (var inventoryItem in m_InventoryItems)
            {
                // All InventoryItems have an associated InventoryItemDefinition which contains a display name.
                var itemName = inventoryItem.definition.displayName;

                // We'll initialize our quantity value to 1,
                // that way if an item doesn't have a quantity property,
                // we still represent the 1 item in the InventoryManager.
                // Depending on your implementation, you may instead want to initialize to 0.
                var quantity = 1;
                if (inventoryItem.TryGetProperty("quantity", out var quantityProperty))
                {
                    quantity = quantityProperty;
                }

                m_DisplayText.Append($"<b>{itemName}</b>: {quantity}");
                m_DisplayText.AppendLine();

                // For items with health restore, durability, or damage properties, we want to display their values here.
                if (inventoryItem.TryGetProperty("healthRestore", out var healthRestoreProperty))
                {
                    m_DisplayText.Append($"- Health Restore: {healthRestoreProperty}");
                    m_DisplayText.AppendLine();
                }

                if (inventoryItem.TryGetProperty("damage", out var damageProperty))
                {
                    m_DisplayText.Append($"- Damage: {damageProperty}");
                    m_DisplayText.AppendLine();
                }

                if (inventoryItem.TryGetProperty("durability", out var durabilityProperty) && quantity > 0)
                {
                    m_DisplayText.Append($"- Durability: {durabilityProperty}");
                    m_DisplayText.AppendLine();
                }

                m_DisplayText.AppendLine();
            }

            // Show the player's health
            m_DisplayText.AppendLine();
            m_DisplayText.Append($"<b>Health:</b> {m_PlayerHealth}");


            mainText.text = m_DisplayText.ToString();

            RefreshDamageAndHealButtons();
        }

        /// <summary>
        /// This method will turn the heal/damage buttons on/off if the conditions for their functionality are met. 
        /// </summary>
        private void RefreshDamageAndHealButtons()
        {
            // When initializing variables that will store Properties of type float using var,
            // the initial value needs to specify that it is a float.
            var swordDamage = 0f;
            var swordQuantity = 0;

            // First we'll safely get our items' properties that we need to complete our condition checks.
            if (m_Sword != null)
            {
                if (m_Sword.TryGetProperty("damage", out var damageProperty))
                {
                    swordDamage = damageProperty;
                }

                if (m_Sword.TryGetProperty("quantity", out var swordQuantityProperty))
                {
                    swordQuantity = swordQuantityProperty;
                }
            }

            var potionQuantity = 0;
            if (m_HealthPotion != null && m_HealthPotion.TryGetProperty("quantity", out var potionQuantityProperty))
            {
                potionQuantity = potionQuantityProperty;
            }

            takeDamageButton.interactable = swordQuantity > 0 && swordDamage > 0 && m_PlayerHealth >= swordDamage;
            healButton.interactable = potionQuantity > 0 && m_PlayerHealth < 100;
        }

        /// <summary>
        /// Listener for changes in InventoryManager.
        /// Will get called whenever an item is added or removed.
        /// </summary>
        /// <param name="itemChanged">
        /// This parameter will not be used, but must exist to be bound to
        /// <see cref="InventoryManager.itemAdded"/> and <see cref="InventoryManager.itemRemoved"/>.
        /// </param>
        private void OnInventoryItemChanged(InventoryItem itemChanged)
        {
            m_MustRefreshUI = true;
        }

        /// <summary>
        /// Listener for changes on an item's properties.
        /// Will get called whenever Sword's properties or Potion's properties are changed.
        /// </summary>
        /// <param name="args">
        /// This parameter will not be used, but must exist to be bound to
        /// <see cref="InventoryItem.propertyChanged"/>.
        /// </param>
        void OnItemPropertyChanged(PropertyChangedEventArgs args)
        {
            m_MustRefreshUI = true;
        }
    }
}
