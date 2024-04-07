﻿using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Inventories;
using System.ComponentModel;
using static BZP_Allergies.AllergenManager;

namespace BZP_Allergies.HarmonyPatches
{
    public class InventoryData
    {
        public InventoryData()
        {
            Stack = 0;
            @Item = null;
        }

        public int Stack {  get; set; }
        public Item? @Item { get; set; }
    }


    [HarmonyPatch(typeof(CraftingRecipe), "createItem")]
    internal class PatchCreateItem : Initializable
    {
        public static StardewValley.Object? craftedObj = null;

        [HarmonyPostfix]
        static void CreateItem_Postfix(ref Item __result)
        {
            try
            {
                if (__result is StardewValley.Object obj && obj.Edibility > -300)  // must be edible
                {
                    craftedObj = obj;
                } else
                {
                    craftedObj = null;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(CreateItem_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }
    }

    [HarmonyPatch(typeof(CraftingRecipe), "consumeIngredients")]
    internal class PatchConsumeIngredients : Initializable
    {

        [HarmonyPrefix]
        [HarmonyBefore(new string[]{ "spacechase0.SpaceCore" })]
        static void ConsumeIngredients_Prefix(out Dictionary<string, InventoryData>? __state, List<IInventory> additionalMaterials)
        {
            try
            {
                if (PatchCreateItem.craftedObj == null)
                {
                    __state = null;
                    return;
                }

                // item counts and qualified ids in the original inventory
                __state = CopyInventoryData(additionalMaterials);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(ConsumeIngredients_Prefix)}:\n{ex}", LogLevel.Error);
                __state = null;
            }
        }

        [HarmonyPostfix]
        static void ConsumeIngredients_Postfix(Dictionary<string, InventoryData>? __state, List<IInventory> additionalMaterials)
        {
            try
            {
                if (PatchCreateItem.craftedObj == null)
                {
                    return;
                }

                if (__state == null)
                {
                    throw new Exception(nameof(ConsumeIngredients_Prefix) + " did not set output state.");
                }

                // get the item counts after ingredients consumed
                Dictionary<string, InventoryData> afterConsume = CopyInventoryData(additionalMaterials);

                // subtract afterConsume amounts from __state (before consume) to get the consumed materials
                ISet<InventoryData> usedItems = new HashSet<InventoryData>();
                foreach (var pair in __state)
                {
                    __state[pair.Key].Stack -= afterConsume.GetValueOrDefault(pair.Key, new()).Stack;

                    if (__state[pair.Key].Stack > 0)
                    {
                        // we used some of this item
                        usedItems.Add(pair.Value);
                    }
                }

                // what allergens did we cook this with?
                foreach (InventoryData item in usedItems)
                {
                    if (item == null || item.Item == null) continue;
                    Monitor.Log(item.Item.DisplayName, LogLevel.Debug);
                    // what allergens does it have?
                    List<string> allergens = AllergenManager.GetAllergensInObject(item.Item as StardewValley.Object);
                    foreach (string allergen in allergens)
                    {
                        PatchCreateItem.craftedObj.GetContextTags().Add(GetMadeWithContextTag(item.Item.ItemId));
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(ConsumeIngredients_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        // includes farmer inventory (Game1.player.Items)
        private static Dictionary<string, InventoryData> CopyInventoryData (List<IInventory> inventories)
        {
            Dictionary<string, InventoryData> results = new();
            foreach (IInventory container in inventories)
            {
                foreach (Item i in container)
                {
                    if (i == null) continue;
                    if (!results.ContainsKey(i.QualifiedItemId))
                    {
                        results[i.QualifiedItemId] = new();
                    }
                    InventoryData data = results[i.QualifiedItemId];
                    data.Stack += i.Stack;
                    data.Item = i;
                }
            }

            foreach (Item i in Game1.player.Items)
            {
                if (i == null) continue;
                if (!results.ContainsKey(i.QualifiedItemId))
                {
                    results[i.QualifiedItemId] = new();
                }
                InventoryData data = results[i.QualifiedItemId];
                data.Stack += i.Stack;
                data.Item = i;
            }

            return results;
        }
    }

}