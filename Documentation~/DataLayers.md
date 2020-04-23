# Data Layers

The Data Layer is a component which lets the develop should where to save, load, synchronize, but also control the states of all the objects of Game Foundation (see the [architecture] page for more context).

In this current version, only transaction are data-layer-authoritative, the API only notifies the data layer of the changes operated inside Game Foundation so it can save those changes on files locally, or synchronize them in the Cloud.

By default, two types of Data Layer are provided by Game Foundation:

- [Persistence Data Layer]
- [Memory Data Layer]

There is a third one, provided as a sample project, connecting Game Foundation to [ChilliConnect].
Please check the samples provided with this package.

## Memory Data Layer

This data layer is perfect for two situations:

- quick tests (it's the one we use to test the features of Game Foundation)
- unopinionated serialization and storage

The __Memory Data Layer__ initializes with two parameters:

- The __database__ of all the static data.
- The player data.

The benefit of this data layer is that Game Foundation doesn't force any file structure and location for saving the states, and it leaves this up to the developer.  

Building a game is complex, and require a lot of systems.
Game Foundation can be one of them, but you'll probably have more to deal with.
If you want to save Game Foundation states along with the data of other systems, that is the data layer you need, because loading and saving is done through classes and struct instances, instead of file path.

## Persistence Data Layer

The persistence data layer is a turn-key solution to save and load states to and from files.

If Game Foundation is the only system you want to persist, or if you don't need Game Foundation states to be saved along with your other states in the same file, the __Persistence Data Layer__ is the implementation you need.

While turn-key, you still have the flexibility to choose:

- the path of the file to save to, and load from
- the serialization format of this target file (the Json formatter is provided in this package)

This flexibility is provided by two interfaces:

- `IDataPersistence`, which collects the data directly from the __Persistence Data Layer__
- `IDataSerializer`, that you can use inside the implementation of an `IDataPersistence` to serialize and deserialize the data.

You can implement your own persistence and serializer to fit with your architecture and backend.

## ChilliConnect

ChilliConnect is the default backend solution of Unity.
It provides static data definition, A/B testing, segmentation, as a service.

You've built a data layer to adapt Game Foundation to ChilliConnect so you can benefit from a strong Cloud mechanism feature along with intuitive liveOps features.

You can find the implementation of ChilliConnect Adapters for Game Foundation in the sample files attached to this package.










[architecture]: Architecture.md
[memory data layer]: #memory-data-layer
[persistence data layer]: #persistence-data-layer
[chilliconnect]: #chilliconnect
