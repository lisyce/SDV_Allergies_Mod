using StardewValley.GameData.Objects;
using StardewModdingAPI;
using System.Text.RegularExpressions;
using StardewValley;

namespace BZP_Allergies
{
    internal class AllergenManager : Initializable
    {
        public static readonly string ALLERIC_REACTION_DEBUFF = string.Format("{0}_allergic_reaction", ModEntry.MOD_ID);
        public static readonly string LACTASE_PILLS_BUFF = string.Format("{0}_buff_2", ModEntry.MOD_ID);
        public static readonly string REACTION_EVENT = string.Format("{0}_had_allergic_reaction", ModEntry.MOD_ID);

        public static readonly string ALLERGY_RELIEF_ID = string.Format("{0}_AllergyMedicine", ModEntry.MOD_ID);
        public static readonly string LACTASE_PILLS_ID = string.Format("{0}_LactasePills", ModEntry.MOD_ID);

        public static readonly string REACTION_DIALOGUE_KEY = string.Format("{0}_farmer_allergic_reaction", ModEntry.MOD_ID);

        public static Dictionary<string, ISet<string>> ALLERGEN_OBJECTS;

        public static Dictionary<string, string> ALLERGEN_TO_DISPLAY_NAME;

        public static Dictionary<string, ISet<string>> ALLERGEN_CONTEXT_TAGS;

        public static Dictionary<string, string> ALLERGEN_CONTENT_PACK;

        public static ISet<string> EXCLUDE_FROM_FISH;

        public static void InitDefaultDicts()
        {
            ALLERGEN_OBJECTS = new()
            {
                { "egg", new HashSet<string>{
                    "194", "195", "201", "203", "211", "213", "220", "221", "223", "234", "240", "648",
                    "732"
                }},
                { "wheat", new HashSet<string>{
                    "198", "201", "202", "203", "206", "211", "214", "216", "220", "221", "222", "223",
                    "224", "234", "239", "241", "604", "608", "611", "618", "651", "731", "732", "246",
                    "262"
                }},
                { "fish", new HashSet<string>{
                    "198", "202", "204", "212", "213", "214", "219", "225", "226", "227", "228", "242",
                    "265", "445"
                }},
                { "shellfish", new HashSet<string>{
                    "203", "218", "227", "228", "727", "728", "729", "730", "732", "733", "715", "372",
                    "717", "718", "719", "720", "723", "716", "721", "722"
                }},
                { "treenuts", new HashSet<string>{
                    "239", "607", "408"
                }},
                { "dairy", new HashSet<string>{
                    "195", "197", "199", "201", "206", "215", "232", "233", "236", "240", "243", "605",
                    "608", "727", "730", "904", "424", "426"
                }}
            };

            ALLERGEN_TO_DISPLAY_NAME = new()
            {
                { "egg", "Eggs" },
                { "wheat", "Wheat" },
                { "fish", "Fish" },
                { "shellfish", "Shellfish" },
                { "treenuts", "Tree Nuts" },
                { "dairy", "Dairy" }
            };

            ALLERGEN_CONTEXT_TAGS = new()
            {
                { "egg", new HashSet<string>{ "egg_item", "mayo_item", "large_egg_item" } },
                { "dairy", new HashSet<string>{ "milk_item", "large_milk_item", "cow_milk_item", "goat_milk_item" } }
            };

            ALLERGEN_CONTENT_PACK = new();

            EXCLUDE_FROM_FISH = new();
        }

        public static string GetAllergenContextTag(string allergen)
        {
            return ModEntry.MOD_ID + "_allergen_" + allergen.ToLower();
        }

        public static string GetAllergenDisplayName(string allergen)
        {
            string result = ALLERGEN_TO_DISPLAY_NAME.GetValueOrDefault(allergen, "");
            if (result.Equals(""))
            {
                throw new Exception("No allergen found named " + allergen.ToString());
            }
            return result;
        }

        public static ISet<string> GetObjectsWithAllergen(string allergen, IAssetDataForDictionary<string, ObjectData> data)
        {
            // labeled items
            ISet<string> result = ALLERGEN_OBJECTS.GetValueOrDefault(allergen, new HashSet<string>());

            // fish special case
            if (allergen == "fish")
            {
                ISet<string> fishItems = GetFishItems(data);
                result.UnionWith(fishItems);
            }

            ISet<string> items = GetItemsWithContextTags(ALLERGEN_CONTEXT_TAGS.GetValueOrDefault(allergen, new HashSet<string>()), data);
            result.UnionWith(items);

            return result;
        }
        public static bool FarmerIsAllergic(string allergen)
        {
            return ModEntry.Config.Farmer.Allergies.GetValueOrDefault(allergen, false);
        }

        public static bool FarmerIsAllergic (StardewValley.Object @object)
        {
            // special case: preserves sheet item (smoked fish, roe, jam, etc.)
            StardewValley.Object? madeFromObject = TryGetMadeFromObject(@object);
            if (madeFromObject != null)
            {
                return FarmerIsAllergic(madeFromObject);
            }

            // check each of the allergens
            foreach (string a in ALLERGEN_TO_DISPLAY_NAME.Keys)
            {
                if (@object.HasContextTag(GetAllergenContextTag(a)) && FarmerIsAllergic(a))
                {
                    return true;
                }
            }

            return false;
        }

        public static StardewValley.Object? TryGetMadeFromObject(StardewValley.Object @object)
        {
            // get context tags
            ISet<string> tags = @object.GetContextTags();

            // find the "preserve_sheet_index_{id}" tag
            Regex rx = new(@"^preserve_sheet_index_\d+$");
            List<string> filteredTags = tags.Where(t => rx.IsMatch(t)).ToList();
            if (filteredTags.Count == 0)
            {
                return null;
            }
            string preserve_sheet_tag = filteredTags[0];
            if (preserve_sheet_tag != null)
            {
                // get the id of the object it was made from
                Match m = Regex.Match(preserve_sheet_tag, @"\d+");
                if (m.Success)
                {
                    string madeFromId = m.Value;
                    return ItemRegistry.Create(madeFromId) as StardewValley.Object;
                }
            }
            return null;
        }

        private static ISet<string> GetFishItems (IAssetDataForDictionary<string, ObjectData> data)
        {
            ISet<string> result = new HashSet<string>();
            ISet<string> shellfish = ALLERGEN_OBJECTS.GetValueOrDefault("shellfish", new HashSet<string>());

            foreach (var item in data.Data)
            {
                ObjectData v = item.Value;
                string id = v.QualifiedItemId;
                if (v.Category == StardewValley.Object.FishCategory && !shellfish.Contains(id) && !EXCLUDE_FROM_FISH.Contains(id))
                {
                    result.Add(item.Key);
                }
            }

            return result;
        }

        private static ISet<string> GetItemsWithContextTags (ISet<string> tags, IAssetDataForDictionary<string, ObjectData> data)
        {
            ISet<string> result = new HashSet<string>();

            foreach (var item in data.Data)
            {
                ObjectData v = item.Value;
                foreach (string tag in tags)
                {
                    if (v.ContextTags != null && v.ContextTags.Contains(tag))
                    {
                        result.Add(item.Key);
                    }
                }
            }

            return result;
        }
    }
}
