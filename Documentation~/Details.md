# Game Foundation Details

## Adding static data with details

Creating [catalog items] is a mandatory step to building your static data.
But you may need to give more details about your [catalog items], other than a name, an id and some tags.

The details give you the possibility to add sets of information, the same way you can add `components` to a `gameobject`.

### Creating Details

> Details can be added to any [catalog items] ([item definition][currencies], [virtual transactions], [IAP transactions], [stores]).

Adding details to an [item definition] is done in the __Inventory Window__.
Open this window (__Window → Game Foundation → Inventory__)

The _Detail Definitions_ section contains the details of the [item definition].

> Details are blocks of static data you can add to a definition in order to enrich it.

A couple of details are available in Game Foundation.
You can see the list of details if you click on the `Add Detail` button.

The available details are:

- Analytics Detail: used for Analytics
- [Assets Detail]: to links assets with your items.
- [Json Detail] (obsolete): to add personalized static data fields.



[catalog items]: Catalog.md#catalog-items

[item definition]: CatalogItems/InventoryItemDefinition.md

[currencies]: CatalogItems/Currency.md

[virtual transactions]: CatalogItems/VirtualTransaction.md

[iap transactions]: CatalogItems/IAPTransaction.md

[stores]: CatalogItems/Store.md

[assets detail]: Details/AssetsDetail.md

[json detail]: Details/JsonDetail.md
