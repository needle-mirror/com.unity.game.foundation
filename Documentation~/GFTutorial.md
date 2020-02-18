# Game Foundation tutorials

## Creating a Game Item Definition

* **Tip:** A Game Item Definition is any type of data that represents a gameplay concept you want Game Foundation to manage. While Game Foundation already provides built-in concepts like Inventory Item, for any generic concept in your own custom logic (such as a Player Character), defining them as Game Items allows you to take advantage of Game Foundation capabilities. Furthermore, when you want the same concepts to be shared by multiple systems, you can define them as Game Items and then use ‘Reference Definition’ to inherit that definition in other systems.

You can define a “coin” in the Game Item catalog and have your Store and Wallet systems refer to that Game Item Definition when they want to know what a coin is.

1. Open the **Game Item** window (menu: **Window → Game Foundation → Game Item**).
This is a tool for viewing and managing the contents of your Game Item catalog.
![Open the Game Item window](images/image24.png)

1. Click the **+** button to create a new Game Item and then enter a **Display Name** and an **Id**.  The display name can be modified later, but the ID will be permanent.
![Display Name and Id](images/image15.png)

    * **Tip:**  String IDs are the easiest way to access object instances managed by Game Foundation (the alternative approach is hash number). We recognize that using string ID may not be the best solution in some cases. We’ll provide an alternative solution in a future release.

1. Click the **Create** button.
When a Game Item is created and added to the catalog, the **Detail Definitions** configuration options become available for that Game Item. A Detail in Game Foundation is a little bit of data added to a Game Item to describe it further, and details are often pertinent to specific systems.
![Detail Definitions](images/image26.png)

## Using the Wallet with Soft Currency

### Creating a soft currency

* **Tip:** Soft Currency, also called regular currency or free currency, is a resource designed to be adequately accessible through normal gameplay, without having to make micro-transactions. We can keep track of these soft currencies with a virtual “wallet”.

1. Open the **Inventory** window (menu: **Window → Game Foundation → Inventory**).
This is a tool for viewing and managing the contents of your Inventory catalog.

    ![Inventory window](images/image9.png)

1. Click the **+** button to create a new inventory item and then enter a **Display Name** and an **Id**. The display name can be modified later, but the ID will be permanent.

    ![Display Name and Id](images/image25.png)

1. Click the **Create** button.
When an item is created and added to the catalog, the **Categories** and **Detail Definitions** configuration options become available for that item.

    ![Categories and Details Definitions](images/image1.png)

1. A currency item must contain a currency detail.
Click the **Add Details** button and then select **Currency Detail**.
The **Currency Detail** allow you to select the type of currency you want to use, but for this tutorial it can stay as the default value, **Soft**.

    ![Inventory Details](images/image18.png)

    Adding a Currency Detail also automatically adds an Analytics Detail.
    ![Currency Analytics Detail](images/image17.png)

1. Currency items can be added to the **Wallet** at runtime.
You can display currency items automatically in the player’s wallet at the start of the game by making the currency item a **Default Item** in the **Wallet** inventory.
Click on the **Inventories** tab and select **Wallet** from the list of inventory items (the **Main** and **Wallet** inventories are built-in to Game Foundation and can’t be deleted; you can create additional custom inventories as needed).

    ![Inventory: Wallet](images/image13.png)

1. Each inventory contains a list of **Default Items** which are automatically added to that inventory when it is first instantiated at runtime.
In your **Wallet**, look under **Other Available Items** to find the **Coin** item, and click the **Add To Default Items** button next to it. This moves **Coin** to the **Default Items** list.

    ![Wallet: moving Coin to Default Items and setting quantity](images/image3.png)

1. You can now set the quantity of the item to be added to the wallet when it is first created. In this case, the player will start with 100 coins in their wallet.

### Spending currency in-game
**Tip:** You can also use the **[Transaction System](#using-the-transaction-system)** to manage currency exchanges.

1. After creating your Coin and adding it to the Wallet in the previous tutorial sections, add the following script to a GameObject in a Scene:

    ```
    using UnityEngine;
    using UnityEngine.GameFoundation;

    public class WalletTest : MonoBehaviour
    {
        void Awake()
        {
            // this data layer will not save any data, it is usually used for examples or tests
            IDataAccessLayer dataLayer = new MemoryDataLayer(GameFoundationSerializableData.Empty);

            // you always need to call Initialize once per session
            GameFoundation.Initialize(dataLayer);
        }

        void Start()
        {
            // get the coin item by its ID
            InventoryItem coinItem = Wallet.GetItem("coin");

            Debug.LogFormat("coins in wallet at start: {0}", coinItem.quantity);

            coinItem.quantity -= 25;

            Debug.LogFormat("coins in wallet: {0}", coinItem.quantity);
        }
    }
    ```

1. Click **Play**. In the console, you’ll see the player’s wallet starting out with 100 coins in it. 25 coins are then removed, leaving them with 75 coins.

    ![Taking 25 player coins](images/image8.png)

## Creating and applying Stats

* **Tip:** In Game Foundation you can use **Stats** to track and manage any numeric values in your gameplay, such as the health of a character, damage point of a weapon, or how many times the player has beat the level. Extending the stat system will allow you to apply formulas to stats based on modifiers and player progression.

1. Open the **Stat** window (menu: **Window → Game Foundation → Stat**).
This is a tool for viewing and managing the stats you want to adjust and persist at runtime.
![Stat window](images/image22.png)

1. For this example, we’ll use a sword with a **Damage** stat. Create the **Damage** stat by clicking the **+** button to create a new stat item with the following values:
    * **Display Name:** Damage
    * **Id:** damage
    * **Value Type:** Int
![Damage stat values](images/image4.png)

1. Click **Create**.
    * **Tip:** the numeric types currently supported are Float (C# System.Single) and Int (C# System.Int32). Once you choose which numeric type a stat is, it cannot be changed later.
1. Following the steps from the [Creating a Game Item Definition](#creating-a-game-item-definition) tutorial, create a Sword GameItem.
![Creating a Sword Gameitem](images/image2.png)

1. Click **Add Details** to add **Stat Details** to the Sword.
![Sword Stat details](images/image20.png)

1. In the  Stat Details section, you’ll see a popup menu with your only stat, Damage, already selected.
Click **Add** to add the Damage stat to your Sword, and then set the **Default Value** as 10.
1. Using what you've learned from the tutorials so far, do the following:
    * Create a “Damage Increase” stat with a Value Type of Int.
    * Create a “Damage Buff Spell” Game Item.
    * Give the spell a “Damage Increase” stat with a default value of 1.
    * Create a “Sword” Inventory Item with the Reference Definition set to Sword, and add it as a default item in the Main inventory.
    * Create a “Damage Buff Spell” Inventory Item with the Reference Definition set to *Damage Buff Spell* and also add it to the Main inventory’s default items.

    ![Damage increase](images/image5.png)

    ![Damage Buff Spell](images/image11.png)

    ![Damage Buff Spell in Default Items](images/image21.png)


### Working with Stats at runtime
Now that we have a sword and a spell, let’s use those while playing the game. We’ll access the sword and spell in the inventory, and consume the spell in order to improve the damage stat of the sword:

    // let's use a consumable scroll spell to buff our weapon

    void Awake()
    {
        // this data layer will not save any data, it is usually used for examples or tests
        IDataAccessLayer dataLayer = new MemoryDataLayer(GameFoundationSerializableData.Empty);

        // you always need to call Initialize once per session
        GameFoundation.Initialize(dataLayer);
    }

    void SharpenSword()
    {
        // get the sword from inventory
        InventoryItem swordItem = Inventory.main.GetItem("sword");

        // get the scroll spell from inventory
        InventoryItem scrollItem = Inventory.main.GetItem("damageBuffSpell");

        // get the sword’s current damage
        int swordDamage = StatManager.GetIntValue(swordItem, "damage");

        // find out how much the scroll spell will increase the damage
        int damageIncrease = StatManager.GetIntValue(scrollItem, "damageIncrease");

        // increase this sword’s damage permanently
        StatManager.SetIntValue(swordItem, "damage", swordDamage + damageIncrease);

        // consume the scroll spell by removing it from the inventory
        Inventory.main.RemoveItem("damageBuffSpell");
    }


## Saving and loading the Game State

Now that you’re playing the game and your wallet values have changed, you’ll want to save your progress to make sure these changes persist when you return to the game.
When you initialize the inventory manager (as you did in the previous tutorial), you should also pass in a persistence provider. That will automatically load the state that was last saved. When you’re ready to save or load the game state, you can use methods like these:

    using UnityEngine;
    using UnityEngine.GameFoundation;

    public class SaveGame : MonoBehaviour
    {
        PersistenceDataLayer dataLayer;

        void Awake()
        {
            // choose what format you want to use
            JsonDataSerializer dataSerializer = new JsonDataSerializer();

            // choose where to store the data
            IDataPersistence localPersistence = new LocalPersistence("GFTutorial", dataSerializer);

            // create a data access layer for Game Foundation and keep a reference to it
            // to save your progression whenever you want
            dataLayer = new PersistenceDataLayer(localPersistence);

            // tell Game Foundation to initialize using the created data access layer
            GameFoundation.Initialize(dataLayer);
        }

        public void Save()
        {
            // save Game Foundation's data
            dataLayer.Save();
        }

        public void Load()
        {
            // to load a fresh set of data for Game Foundation you need to: unitialize it ...
            GameFoundation.Uninitialize();

            // ... and re-initialize it with the desired data access layer
            GameFoundation.Initialize(dataLayer);
        }
    }

## Asynchronous operations and promises

A `Promise` is a lightweight object to easily track the completion, success or failure, of an asynchronous operation and its resulting value, if any.

To create, manage and recycle them you need to use a `PromiseGenerator`.
When generating a `Promise`, it will give you two objects:
* a `Completer`: use it in your asynchronous process to complete or reject the `Promise`. 
* a `Deferred`: expose it to allow other systems, like your UI, to check your `Promise` state and wait for its completion as they please (polling, coroutine, ...).
Note that you can use it to `Release` a `Promise` to recycle it.

Here is the `Save` function of the `PersistenceDataLayer` as a use case on the process side:

        public Deferred Save()
        {
            // ... Save specific logic here ...

            // Creating the promise and its handles
            m_PromiseGenerator.GetPromiseHandles(out var deferred, out var completer);

            // Start your asynchronous process and provide the Completer to allow it to complete your process (either it is a success or a failure).
            persistence.Save(data, completer.Resolve, completer.Reject);

            // Return the handle to allow developpers to properly wait for completion.
            return deferred;
        }

And here is a code sample on the caller side using coroutine:

        IEnumerator SaveGame()
        {
            // The datalayer assignation is ommitted since it isn't the focus here.
            PersistenceDataLayer dataLayer = ...;

            // Call your operation and keep a handle on it.
            Deferred saveOperation = dataLayer.Save();

            // Wait for your operation's completion.
            yield return saveOperation.Wait();

            // Release the promise to recycle it.
            saveOperation.Release();

            // Do your post save logic here ...
        }

## Defining Item details

* **Tip:** A Detail Definition is a predefined bit of data associated with a Game Item Definition. Details can be things like images, audio clips, metadata, etc. - anything that can be used to further identify an in-game item.

After doing the previous tutorials, you should be more comfortable with creating Game Items.
Now let's create a new Game Item, "apple," and then assign some sprites to it. We'll use two sprites:

* A thumbnail, used when we want to show an apple in a UI list;
* An enlargement of the thumbnail, used when we want to show a full-screen description of the apple.

1. Create a Game Item Definition with the id "apple".

    ![Apple GameItem](images/image27.png)

1. On that new Game Item add a Sprite Assets Detail.

    ![Apple Sprite Asset Detail](images/image28.png)

1. In the Sprite Assets Detail, click the "+" button twice to add two sprite entries.

    ![Apple Sprite Rows](images/image29.png)

1. These should have default keys of "Sprite" and "Sprite (1)". Change the keys to "thumbnail" and "enlargement" instead. For both entries, select an appropriate Sprite using either the object picker or by dragging Sprites from the Project window.

    ![Apple Sprites configured](images/image30.png)

## Working with Detail Definitions at runtime

After following the steps in "Defining Item Details" above, the next thing you'd want to know is how to access that data in your game. Let's write some code to put the thumbnail sprite to use.

    public Sprite GetAppleThumbnail()
    {
        GameItemDefinition appleDef =
            CatalogManager.gameItemCatalog.GetGameItemDefinition("apple");

        SpriteAssetsDetailDefinition appleSpriteDefs =
            appleDef.GetDetailDefinition<SpriteAssetsDetailDefinition>();

        // if the key doesn't exist, you'll get an error
        if (!appleSpriteDefs.ContainsKey("thumbnail"))
        {
            Debug.Log("Apple definition is missing a thumbnail!");
            return null;
        }

        return appleSpriteDefs.GetAsset("thumbnail");
    }

## Creating a custom Detail Definition

If none of the existing Detail Definition types work for a given Game Item, you can create a custom Detail Definition that inherits from BaseDetailDefinition.

To do this, you will need to create two Detail Definition classes: one for seeing the detail in the editor and one for using it in the runtime.
Let's modify our sword's Detail Definition by adding a description detail with a custom detail class.


1. Create a script named DescriptionEditorDetailDefinition.cs in your Assets.
Note there are two required methods in this script:
    * The first returns a Display Name, which is what appears in the list of Details when you're editing your Game Items.
    * The second is for creating the runtime DescriptionDetailDefinition and is called automatically during GameFoundation initialization.

          using UnityEngine.GameFoundation.CatalogManagement;

          public class DescriptionEditorDetailDefinition : BaseDetailDefinition
          {
              public override string DisplayName()
              {
                  return "Description Detail";
              }

              public override UnityEngine.GameFoundation.BaseDetailDefinition CreateRuntimeDefinition()
              {
                  return new DescriptionDetailDefinition();
              }
          }

1. Now we'll add a string field to store a description for each Game Item, and pass it to the constructor for the runtime DescriptionDetailDefinition. This allows your new Detail Definition type to appear in the list of available detail types in the Game Item editor.

        using UnityEngine.GameFoundation.CatalogManagement;

        public class DescriptionEditorDetailDefinition : BaseDetailDefinition
        {
            public string description;

            public override string DisplayName()
            {
                return "Description Detail";
            }

            public override UnityEngine.GameFoundation.BaseDetailDefinition CreateRuntimeDefinition()
            {
                return new DescriptionDetailDefinition(description);
            }
        }

1. Next, we'll create the runtime DescriptionDetailDefinition. It needs a constructor that accepts and sets all of the necessary fields. In this case, that is the string description.
The runtime detail definition is what will be included in your sword's list of Detail Definitions during runtime.

         using UnityEngine.GameFoundation;
 
         public class DescriptionDetailDefinition : BaseDetailDefinition
         {
             public string description;

             public DescriptionDetailDefinition(string newDescription) 
             {
                 description = newDescription;
             }
         }

After the Description Detail is added to a Game Item, you'll see new "Script" field in the description detail for that item. This field appears in all scripted classes.

* **Tip:** You can hide the Script field with a custom editor. Create a DescriptionDetailDefinitionEditor.cs script that has the following code in it to remove the Script field from your description details:

        using UnityEditor;
        using UnityEditor.GameFoundation;

        [CustomEditor(typeof(DescriptionDetailDefinition))]
        public class DescriptionDetailDefinitionEditor : BaseDetailDefinitionEditor
        {
        }

This particular script gives you a very simple version of Description Detail, but using this method to customize your editors can be very useful when adding complex information to Game Items.

# Using the Transaction System

The **Transaction System** lets you convert one type of item into another: for example, game currency into power-ups.  
The inputs and outputs for these transactions can range from a single item to combinations of different items with varying quantities.  
You can predefine allowable transactions by attaching **Purchasable Details** to your game items. When a transaction is initiated, the **Transaction System** uses the **Purchasable Detail's** information to validate the selected input in the source inventory, and then grants the outputs to the destination inventory (or informs the player that they don't have enough items for the transaction).  

## Creating a Purchasable Detail

Transactions are defined within **Game Foundation** by attaching **Purchasable Details** to game items. **Purchasable Details** specify input/payment and output/payout requirements: players can pay with game items or currencies to receive different game items or to invoke callbacks.   
Here, we'll use TransactionDetails to create a bundle that a player can purchase.  

1. Create a new **Game Item Definition** (as you learned in the first tutorial, [Creating a Game Item Definition](#creating-a-game-item-definition)). Call it "Ultimate Bundle".
 
2. In the new **Game Item Definition** window, click the **Add Details** button and then select **Purchasable Detail**.
You can also select a game item from your **Inventory** window (menu: **Window → Game Foundation → Inventory**) and add the **Purchasable Detail** in the item window.

![Adding Detail](images/image_purchasableDetail1.png)

3. In the **Purchasable Detail**, specify the default inventories that will be used to make payments and receive items.

![Default Inventories](images/image_PurchasableDetail2.png)

4. Add **Payout Items**: these are the items that will be sold in the Ultimate Bundle. Specify the item id and quantity of each item (For example: 1 rocket power-up, 2 bomb power-ups, and 500 game coins).   
You also have the option to specify a target inventory for each item, but this is not required.

![Payouts](images/image_PurchasableDetail3.png)

5. Add a key to represent the price point of this bundle. **Store Items** allow you to specify multiple payment options.

6. Add **Payment Items** to any price points you define, including quantity and id.  
You also have the option to specify a source inventory for payment, but this is not required.

![Prices](images/image_PurchasableDetail4.png)

## Handling a Transaction

Now that you've defined a transaction, you can add it to your game.   
The TransactionManager class contains static helper methods to handle the logic for you. They can be invoked from anywhere and need just a bit of data to take care of your transactions.    

* [Handling a simple transaction](#handling-a-simple-transaction)
* [Handling a transaction with inventory overrides](#handling-a-transaction-with-inventory-overrides)
* [Handling a transaction with success and failure callbacks](#handling-a-transaction-with-success-and-failure-callbacks)

### Handling a simple transaction

1. Get the **PurchasableDetail** for the transaction:

			PurchasableDetailDefinition purchasableDetail = InventoryManager.catalog.GetItemDefinition("yourPurchasableGameItemId").GetDetailDefinition<PurchasableDetailDefinition>();
			
2. Query the **Price** and **Payout** from the **PurchasableDetail** and pass them to the transaction manager by invoking **HandleTransaction**.  
This causes the input items to be deducted from the input inventory and the output items to be added to the output inventory. If the input inventory does not have enough items for the transaction, no action is taken on the inventories. Here, no input/output inventories are specified so they will default to main.  
   
   2a. A receipt is generated which can be accessed via the success/fail callbacks that contains information about the transaction, including whether it succeeded or failed.

			TransactionManager.HandleTransaction(purchasableDetail.GetPrice(), purchasableDetail.payout);

### Handling a transaction with inventory overrides

1. Get the **PurchasableDetail** for the transaction:

			PurchasableDetailDefinition purchasableDetail = InventoryManager.catalog.GetItemDefinition("yourPurchasableGameItemId").GetDetailDefinition<PurchasableDetailDefinition>();
			
2. Get references for the input and output inventories you want to use for the transaction:

			Inventory inputInventory = InventoryManager.GetInventory("yourInputInventoryId");
			Inventory outputInventory = InventoryManager.GetInventory("yourOutputInventoryId");
			
3. Query the **Price** and **Payout** from the **PurchasableDetail** and pass them to the transaction manager. 

   3a. When you pass specific inventories to the transaction manager, these will be used instead of the inventories specified in the **Purchasable Detail**. 
   
   3b. You can also pass a single inventory, either input or output. The other inventory type will then default to main.

			TransactionManager.HandleTransaction(purchasableDetail.GetPrice(), purchasableDetail.payout, inputInventory, outputInventory);

### Handling a transaction with success and failure callbacks

1. Set up your callback methods as shown below.   
These methods take in a **TransactionReceipt**, which contains useful information about the transaction's details and whether it succeeded or failed.   
**HandleTransaction** generates the transaction receipt and passes it to the callbacks when they are invoked.
A useful way to use the callbacks is to have a receipt variable and to set it in the callbacks. Then, after invoking **HandleTransaction**, you can check **MyReceipt** for the transaction's information.

		public TransactionReceipt MyReceipt;

		public void SuccessCallback(TransactionReceipt receipt)
		{
			MyReceipt = receipt;
		    Debug.Log("Success!");
		}
		
		public void FailureCallback(TransactionReceipt receipt)
		{
			MyReceipt = receipt;
		    Debug.Log("Failure...");
		}
		
2. Get the **PurchasableDetail** for the transaction:

			PurchasableDetailDefinition purchasableDetail = InventoryManager.catalog.GetItemDefinition("yourPurchasableGameItemId").GetDetailDefinition<PurchasableDetailDefinition>();
			
3. Query the **Price** and **Payout** from the **PurchasableDetail** and pass them to the transaction manager along with your callback methods.   
When the callbacks are passed, a successful transaction invokes the **SuccessCallback**, and a failed transaction invokes the **FailureCallback**.

		TransactionReceipt receipt = TransactionManager.HandleTransaction(purchasableDetail, inputInventory, outputInventory, SuccessCallback, FailureCallback);

* **Tip:** You can access specific parameters in methods with many optional parameters by specifying the name of the parameter, followed by a colon and then the value you want to pass in. The example in step 3 above can be written with specified parameters as follows:

		TransactionReceipt receipt = TransactionManager.HandleTransaction(transactionDetail, onTransactionSuccess : SuccessCallback, onTransactionFail : FailureCallback);
