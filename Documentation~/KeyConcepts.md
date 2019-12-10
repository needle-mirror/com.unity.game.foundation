# Key concepts

## Definition

For many things in your game, you’re going to want to predefine some or all of their properties. You’ll likely also want to spawn multiple instances of the same thing with the same properties (e.g.: ‘apple1’, ‘apple2’, and ‘apple3’ are all instances which refer to the same ‘apple’ definition). A definition lets you set up those properties once in the Editor, and then easily spawn as many of those as you need at runtime.

Having definitions also allows us to have a single point of reference to look back to, rather than having to copy this data to each instance.

Definitions are created and modified only in the Unity Editor. Definitions are derived from ScriptableObject, and as such are intended to not be modified at runtime. When you want to start working with an instance of an item at runtime, you construct a new GameItem instance of it based on the definition. From there you could save the state of the item if you want to keep using it across sessions.

```
GameItemDefinition appleDefinition =
    GameItemCatalog.GetGameItemDefinition(“apple”);

appleInstance1 =
    new GameItem(definition: appleDefinition, id: “apple1”);
```

## Detail Definition

A Detail Definition adds more data to a Game Item Definition or Inventory Item Definition. What that data is used for is up to the systems that access the item and/or your own custom code.

For example, here is a detail that tells a system what icon asset to use:

```
IconDetailDefinition iconDetail =
    mySword.GetDetailDefinition<IconDetailDefinition>();

if (iconDetail != null)
{
    spriteRenderer.sprite = iconDetail.icon;
}
```

## Game Item

Any concept in your gameplay that you want Game Foundation to manage can be defined as a **Game Item Definition**. While Game Foundation provides built-in systems for concepts like Inventory Item (with more such built-in systems coming in the future), for any generic concepts that you use in your own custom gameplay logic, you can define them as Game Item Definitions which allows you to take advantage of Game Foundation capabilities. For example, you may have a Player Character in your custom gameplay logic, but you may want to use Game Foundation to manage stats associated with that player character. By creating a **Game Item Definition** for that player character, you’ll be able to start using Game Foundation to manage that aspect of gameplay for you.

When you want to define a concept for a built-in Game Foundation system, such as an **Inventory Item Definition**, it may still be a good idea to create that concept as **Game Item Definition** initially, and then extend that definition in other systems. This is especially helpful when you want multiple systems to reuse and extend the same definition in different ways. For example, a ‘health potion’ can be used in different ways by the Inventory System and in the Reward System (not yet available in the current release). By defining the concept as a Game Item Definition and then extending that definition in each system, **Detail Definitions** you add to the base **Game Item Definition** (e.g. stats, icon, and analytics) will be inherited by the derived definitions in each system. This will help you avoid recreating those Details repeatedly for every system.

**Game Items** (as opposed to **Game Item Definitions**) only exist at runtime. Game Items are generated from Game Item Definitions and keep track of any data which you want to persist across sessions, or even temporary data that only exists at runtime. The data in Game Items could change after they are created, whereas the data in Definitions is considered permanent once they go into a built game.

You’ll still use **GameObjects** for presenting everything (things like rendering, physics, and UI).

## Systems

Systems are hubs for managing data at runtime. Each system maintains collections of items and the system determines how those items are managed.

When you want to read or update data, you should go through the system to do it, rather than going directly to the Game Item or Details that has the data in it. The reason for this is that the system may have some rules that the Game Items and Details don’t know about.

An example of a system applying rules would be a backpack which is an inventory system. The backpack has a maximum capacity. If you were to increase the quantity of green gems in that backpack directly, you could easily exceed the capacity of the backpack and the backpack wouldn’t realize it. But if you instead tell the backpack to increase the quantity of green gems, then the backpack can tell you that would exceed the max capacity, and prevent the increase.
