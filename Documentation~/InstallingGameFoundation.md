# Installing the Game Foundation package

1. In the Unity Editor, open the Package Manager window 
    (menu: **Window** → **Package Manager**).
    
    ![Open the Package Manager](images/image6.png)

2. In the **Package Manager** window, click **Advanced** and make sure that **Show preview packages** is enabled.
    
    ![Open the Package Manager](images/image7.png)

3. In the list of packages on the left, find **Game Foundation** and select it.
    
    ![Open the Package Manager](images/image10.png)

4. In the upper-right (bottom-right on some versions of Unity), click on the **Install** button.                                                                 
    
    ![Open the Package Manager](images/image12.png)      ![Open the Package Manager](images/image-install-br.png) 

5. After installation, the Game Foundation menu items and editor windows are available in your Unity project in the **Window** menu.        
    
    ![Open the Package Manager](images/image23.png)

## Quick start

Before Game Foundation can be used during runtime, it has to be initialized in your code. 
The following is an example of initializing Game Foundation with the Awake method of a MonoBehaviour.

```Csharp
using UnityEngine;
using UnityEngine.GameFoundation;

public class MyGameManager : MonoBehaviour
{
    void Awake()
    {
        // this data layer will not save any data, it is usually used for examples or tests
        MemoryDataLayer dataLayer = new MemoryDataLayer();

        // initialize Game Foundation for runtime access
        GameFoundation.Initialize(dataLayer);
    }
}
```

When you initialize Game Foundation, all managers, including InventoryManager, WalletManager, StatManager and TransactionManager will be initialized and ready to use.

* **Tip:** To verify that Game Foundation is working and installed correctly, please check the IsInitialized property on any Game Foundation manager.

```Csharp
using UnityEngine;
using UnityEngine.GameFoundation;

public class MyGameManager : MonoBehaviour
{
    void Awake()
    {
        // this data layer will not save any data, it is usually used for examples or tests
        MemoryDataLayer dataLayer = new MemoryDataLayer();

        // initialize Game Foundation for runtime access
        GameFoundation.Initialize(dataLayer);

        // verify that the manager is initialized
        if (InventoryManager.IsInitialized)
        {
            Debug.Log("Game Foundation is installed and ready!");
        }
        else
        {
            Debug.LogError("Error:  Game Foundation was unable to initialize.  Please check online help or docs for more information.");
        }
    }
}
```

After implementing the above code, when you press Play you will see that Game Foundation has been successfully installed and ready to build your game!

![Display Name and Id](images/image32.png)

Now you can head over to one of our Tutorials for more information:

01. [Creating an Inventory Item Definition](Tutorials/01-CreatingAnItemDefinition.md)
02. [Playing with items at runtime](Tutorials/02-PlayingWithRuntimeItem.md)
03. [Creating a Currency](Tutorials/03-CreatingCurrency.md)
04. [Playing with currencies at runtime](Tutorials/04-PlayingWithRuntimeCurrency.md)
05. [The Debugger window](Tutorials/05-Debugger.md)
06. [Adding static properties with details](Tutorials/06-AddStaticPropertiesWithDetails.md)
07. [Adding mutable properties with Stats](Tutorials/07-AddMutablePropertiesWithStats.md)
08. [Playing with stats at runtime](Tutorials/08-PlayingWithStats.md)
09. [Creating a Virtual Transaction](Tutorials/09-CreatingAVirtualTransaction.md)
10. [Playing with virtual transaction at runtime](Tutorials/10-PlayingWithRuntimeVirtualTransaction.md)
11. [Using IAP Transactions](Tutorials/11-PlayingWithIAPTransaction.md)
12. [Filtering transactions with Stores](Tutorials/12-FilterTransactionWithStore.md)

Also, please visit [Known Issues](KnownIssues.md) if you need further assistance.
