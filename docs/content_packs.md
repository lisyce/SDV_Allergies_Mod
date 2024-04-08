# Content Packs

You can create a content pack for BarleyZP's Allergies in order to add custom allergens into the game and assign allergens to modded items.

# Getting Started

If you've never made a content pack before, please check out the [wiki](https://stardewvalleywiki.com/Modding:Content_packs) for an overview. Creating a content pack for this mod is quite similar to creating a pack using something like [Content Patcher](https://www.nexusmods.com/stardewvalley/mods/1915). Content packs for this mod include a `manifest.json` and a `content.json` file.

# Manifest

Your `manifest.json` should describe a content pack for `BarleyZP.BzpAllergies`. Here is an example for a content pack that we will build that adds a grape allergy to the game:

```json
{
  "Name": "[BZPA] Grape Allergy",
  "Author": "Your Name",
  "Version": "1.0.0",
  "Description": "Grape allergy content pack for BarleyZP's Allergies",
  "UniqueID": "Your Name.ModId",
  "MinimumApiVersion": "3.8.0",
  "UpdateKeys": [],
  "ContentPackFor": {
    "UniqueID": "BarleyZP.BzpAllergies"
  },
  "Dependencies": [
    {
      "UniqueID": "BarleyZP.BzpAllergies",
      "IsRequired": true
    }
  ]
}
```

Content packs for this mod are customarily prefixed with `[BZPA]` to denote that they are a content pack for BarleyZP's Allergies.

# Content

Your `content.json` is used to define custom allergens and assign objects to an existing allergen. Here is an example that adds a custom "grape" allergy and assigns grapes as the food containing that allergen:

```json
{
  "Format": "1.0.0",
  "CustomAllergens": {
    "grape": {
      "Name": "Grapes"
    }
  },
  "AllergenAssignments": {
    "grape": {
      "ObjectIds": [
        "398"
      ]
    }
  }
}
```

## Fields

### Format

The `"Format"` field describes the version of this content pack framework that will be used. This should be `"1.0.0"`.

### CustomAllergens

Here, you may define custom allergens outside of the six provided by the base mod. It is NOT recommended to edit the base six allergens, which have Ids `"egg"`, `"wheat"`, `"fish"`, `"shellfish"`, `"treenuts"`, and `"dairy"`.

Custom allergens are defined by a unique Id key, which is customarily in all lowercase and must be unique among ALL allergens in the game, including from other content packs. They also have a `"Name"`, which is their name displayed to the player.

### AllergenAssignments

This is where you may give in-game objects an allergen. The keys again denote the unique Ids of the allergen you want to assign objects to.

Use `"ObjectIds"` (A list of string unqualified object Ids) to give objects with those Ids the allergen. This allows you to assign your custom allergens to in-game objects or to give your modded items one of the base six allergens. Derived goods (jelly, wine, etc.) are handled by the mod code, so the above `content.json` file would ensure that grape wine or other processed goods also contain the allergen, even though only the grape Id was listed.

Use `"ContextTags"` (A list of string context tags) to give any objects with those context tags the allergen. For example this `content.json` describes a custom mayo allergy (not eggs, mayo specifically!):

```json
{
  "Format": "1.0.0",
  "CustomAllergens": {
    "mayo": {
      "Name": "Mayo"
    }
  },
  "AllergenAssignments": {
    "mayo": {
      "ContextTags": [
        "mayo_item"
      ]
    }
  }
}
```

This can save typing if there are lots of items that share a context tag that you would like to all have the same allergen.

### ExcludeFromFish

Use this to specify a set of item IDs that you want to exclude from the fish allergen even if they have category `-4` (see section below this one). The JSON example below adds a frog allergy (for some modded frog object that is categorized as fish internally). The frog object will NOT have the fish allergy, but it WILL have the custom frog allergy.

```json
{
  "Format": "1.0.0",
  "CustomAllergens": {
    "frog": {
      "Name": "Frogs"
    }
  },
  "AllergenAssignments": {
    "frog": {
      "ObjectIds": [
        "MyMod.Frog"
      ]
    }
  },
  "ExcludeFromFish": [
    "MyMod.Frog"
  ]
}
```

#### Automatically-added allergens

- Any object with the tags `egg_item`, `mayo_item`, or `large_egg_item` contains the egg allergen
- Any object with the tags `milk_item`, `large_milk_item`, `cow_milk_item`, or `goat_milk_item` contains the dairy allergen
- The base mod contains both fish and shellfish allergens, but the game code describes all of these items under the same fish category. Any custom modded objects under the fish category (`-4`) will be automatically marked as having the fish allergy as long as it was NOT registered under the shellfish allergen as well. Even if your modded shellfish is not edible as-is (like the vanilla crab pot fish), you should still register it under the shellfish allergen so that processed goods from that shellfish are also registered under the shellfish alleregen.
  - If you exclude an item from the fish category with `ExcludeFromFish` if you don't want it to have the fish allergen OR the shellfish allergen