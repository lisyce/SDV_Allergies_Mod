using BZP_Allergies.Apis;
using BZP_Allergies.Config;
using StardewModdingAPI;

namespace BZP_Allergies.ContentPackFramework
{
    internal class LoadContentPacks : Initializable
    {
        public static void LoadPacks(IEnumerable<IContentPack> packs, ModConfig config)
        {
            foreach (IContentPack contentPack in packs)
            {
                Monitor.Log($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version}", LogLevel.Info);
                if (!ProcessPack(contentPack, config))
                {
                    Monitor.Log($"Unable to read content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version}", LogLevel.Error);
                }
            }
        }

        private static bool ProcessPack(IContentPack pack, ModConfig config)
        {
            ContentModel? content = pack.ReadJsonFile<ContentModel>("content.json");
            if (content == null)
            {
                // show 'required file missing' error
                Monitor.Log("Pack is missing a content.json, or it is empty.", LogLevel.Error);
                return false;
            }

            // check format
            if (content.Format == null || !content.Format.Equals("1.0.0"))
            {
                Monitor.Log("Valid content format was not specified. Valid formats are: \"1.0.0\"", LogLevel.Error);
                return false;
            }

            // custom allergens
            foreach (var pair in content.CustomAllergens)
            {
                CustomAllergen allergen = pair.Value;

                if (allergen.Name == null)
                {
                    Monitor.Log("No Name was specified for allergen with Id " + pair.Key, LogLevel.Error);
                    return false;
                }

                if (!AllergenManager.ALLERGEN_OBJECTS.ContainsKey(pair.Key))
                {
                    AllergenManager.ALLERGEN_OBJECTS.Add(pair.Key, new HashSet<string>());
                }

                if (!AllergenManager.ALLERGEN_CONTEXT_TAGS.ContainsKey(pair.Key))
                {
                    AllergenManager.ALLERGEN_CONTEXT_TAGS.Add(pair.Key, new HashSet<string>());
                }

                if (!AllergenManager.ALLERGEN_TO_DISPLAY_NAME.ContainsKey(pair.Key))
                {
                    AllergenManager.ALLERGEN_TO_DISPLAY_NAME.Add(pair.Key, allergen.Name);
                }

                AllergenManager.ALLERGEN_CONTENT_PACK.Add(pair.Key, pack.Manifest.Name);

                if (!config.Farmer.Allergies.ContainsKey(pair.Key))
                {
                    config.Farmer.Allergies.Add(pair.Key, false);
                }
            }

            // allergen assignments
            foreach (var pair in content.AllergenAssignments)
            {
                AllergenAssignments allergenAssign = pair.Value;

                if (pair.Key == null)
                {
                    Monitor.Log("No AllergenId was specified for allergen assignment with Id " + pair.Key, LogLevel.Error);
                    return false;
                }

                // object Ids
                foreach (string id in allergenAssign.ObjectIds)
                {
                    AllergenManager.ALLERGEN_OBJECTS[pair.Key].Add(id);
                }

                // context tags
                foreach (string tag in allergenAssign.ContextTags)
                {
                    AllergenManager.ALLERGEN_CONTEXT_TAGS[pair.Key].Add(tag);
                }
            }

            // Exclude from Fish
            foreach (string exclude in content.ExcludeFromFish) {
                AllergenManager.EXCLUDE_FROM_FISH.Add(exclude);
            }

            return true;  // no errors :)
        }
    }
}
