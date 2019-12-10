using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ProjectAutomata;

namespace ModdersYLib
{
    public delegate void JCommandDelegate(JObject root, JObject obj, JToken command);
    public class JCommands
    {
        public static JCommands Instance => new JCommands();
        public readonly Dictionary<string, JCommandDelegate> Commands = new Dictionary<string, JCommandDelegate>();

        JCommands()
        {
            foreach(var info in this.GetType().GetMethods().Where(x => x.Name.StartsWith("y")))
                Commands.Add(info.Name, (JCommandDelegate)info.CreateDelegate(typeof(JCommandDelegate), this));
        }

        public void yReplaceRecipe(JObject root, JObject obj, JToken command)
        {
            var recipeName = obj["name"].ToString();
            var recipe = GameData.instance.GetAsset<Recipe>(recipeName);
            var targetRecipeName = command.Value<string>();
            var targetRecipe = GameData.instance.GetAsset<Recipe>(targetRecipeName);

            //GameData.instance.DeregisterObject(targetRecipe);

            //recipe.name = targetRecipeName;
            //GameData.instance.RegisterObjectNameOnly(recipe);
            targetRecipe.ingredients = recipe.ingredients;
            targetRecipe.result = recipe.result;
            targetRecipe.gameDays = recipe.gameDays;
            targetRecipe.SvgIcon = recipe.SvgIcon;
            targetRecipe.Title = recipe.Title;

            //GameData.instance.DeregisterObject(recipe);
            GameData.instance.DeRegisterAsset(recipe);

            UnityEngine.Debug.Log($"Copied recipe {recipeName} into {targetRecipeName}");
        }

        public void yProducedByBuilding(JObject root, JObject obj, JToken command)
        {
            //   "yProducedByBuilding": "LumberyardGatherer",

            string[] keys = command.Value<string>().Split(',');
            foreach(string k in keys)
            {
                var building = GameData.instance.GetAsset<Building>(k.Trim());
                var recipeName = obj.Value<string>("name");
                var recipe = GameData.instance.GetAsset<Recipe>(recipeName);

                if (building != null)
                {
                    var recipeUser = building.GetComponent<RecipeUser>();
                    var recipes = recipeUser.availableRecipes;
                    recipeUser.availableRecipes = recipes.Concat(new[] { recipe }).ToArray();

                    UnityEngine.Debug.Log($"Added recipe {recipe.name} to building {building.name}");
                }
                else
                    UnityEngine.Debug.Log($"Building not found: {k}");
            }           
        }

        public void yHookHarvesterProduct(JObject root, JObject obj, JToken command)
        {
            Patch.HarvesterProductHook hph = new Patch.HarvesterProductHook(command);
            //Patch.HarvesterProductHook hph = command.ToObject<Patch.HarvesterProductHook>();

            hph.ExtraProduct = GameData.instance.GetAsset<ProductDefinition>(obj.Value<string>("name"));
            Patch.HarvesterProductHookList.Add(hph);
            UnityEngine.Debug.Log($"Hooked harvester product: Will deliver {hph.Amount} {hph.ExtraProduct.name} every {hph.Frequency} {hph.HookedProductName}.");
        }


        void AppendToRecipeResult(JObject root, JObject obj, JToken token)
        {
            if(token is JArray ja) foreach (var sobj in ja) AppendToRecipeResult(root, obj, sobj);
            if(token is JObject jo)
            {
                var ftoken = jo.Properties().First();
                var recipe = GameData.instance.GetAsset<Recipe>(ftoken.Name);
                var productdef = GameData.instance.GetAsset<ProductDefinition>(obj.Value<string>("name"));
                int amount = ftoken.Value.Value<int>();
                recipe.result.Add(productdef, amount);
                UnityEngine.Debug.Log($"AppendToRecipeResult: Adding {amount} {productdef.name} to existing recipe {recipe.name}.");
            }
        }

        public void yAppendToRecipeResult(JObject root, JObject obj, JToken command)
        {
            AppendToRecipeResult(root, obj, command);
        }

        public void ySetProductPriceFormula(JObject root, JObject obj, JToken command)
        {
            var productdef = GameData.instance.GetAsset<ProductDefinition>(obj.Value<string>("name"));
            productdef.price = UnityEngine.ScriptableObject.CreateInstance<Formula>();
            productdef.price.formula = command.Value<string>();
            productdef.price.OnGameDataLoaded();
            UnityEngine.Debug.Log($"SetProductPriceFormula: Price formula for product {productdef.name} set to {productdef.price.formula}.");
        }

        public void yVisualRepresentation(JObject root, JObject obj, JToken command)
        {
            var goPath = command.Value<string>();
            //UnityEngine.Resources.Load
            var go = UnityEngine.Resources.Load<UnityEngine.GameObject>(goPath);

            if (go == null) UnityEngine.Debug.Log($"yVisualRepresentation: GameObject {goPath} was not found.");
            else
            {
                var productdef = GameData.instance.GetAsset<ProductDefinition>(obj.Value<string>("name"));
                productdef.visualRepresentation = go;
            } 
        }
    }
}
