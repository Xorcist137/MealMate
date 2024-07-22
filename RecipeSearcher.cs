using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

namespace MealMate
{
    class RecipeSearcher
    {
        private List<Recipe> allRecipes;

        public RecipeSearcher(string csvFilePath)
        {
            allRecipes = LoadRecipesFromCSV(csvFilePath);
        }

        private List<Recipe> LoadRecipesFromCSV(string filePath)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null
            };

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, config))
            {
                var records = csv.GetRecords<dynamic>().ToList();
                var recipes = new List<Recipe>();

                foreach (var record in records)
                {
                    string nutritionString = ((string)record.nutrition).Trim('[', ']');
                    var nutritionValues = nutritionString.Split(',').Select(s => double.Parse(s.Trim())).ToList();

                    string ingredientsString = ((string)record.ingredients).Trim('[', ']');
                    List<string> ingredients = ingredientsString
                        .Split(',')
                        .Select(i => i.Trim().Trim('\'', '"')) // Remove quotes and trim whitespace
                        .Where(i => !string.IsNullOrWhiteSpace(i)) // Remove any empty ingredients
                        .ToList();

                    string stepsString = (string)record.steps;
                    string instructions = string.Join(Environment.NewLine,
                        stepsString.Trim('[', ']').Split(',').Select(s => s.Trim('\'', '"', ' ')));

                    var recipe = new Recipe
                    {
                        Name = record.name,
                        Id = record.id,
                        Ingredients = ingredients,
                        Instructions = instructions,
                        Calories = (int)Math.Round(nutritionValues[0]),
                        Fat = (int)Math.Round(nutritionValues[1]),
                        Carbs = (int)Math.Round(nutritionValues[6]),
                        Protein = (int)Math.Round(nutritionValues[4])
                    };

                    recipes.Add(recipe);
                }

                return recipes;
            }
        }

        public List<Recipe> SearchRecipes(List<string> checkedIngredients)
        {
            var normalizedCheckedIngredients = checkedIngredients
                .Select(i => i.Trim().ToLower())
                .Where(i => i.Length > 2)  // Ignore very short ingredients
                .ToList();

            var recipesWithMatchPercentage = allRecipes.Select(recipe =>
            {
                var normalizedRecipeIngredients = recipe.Ingredients
                    .Select(i => i.Trim().ToLower())
                    .ToList();

                int matchCount = normalizedCheckedIngredients.Count(checkedIngredient =>
                    normalizedRecipeIngredients.Any(recipeIngredient =>
                        recipeIngredient.Contains(checkedIngredient)));

                double matchPercentage = (double)matchCount / normalizedRecipeIngredients.Count;

                return new { Recipe = recipe, MatchPercentage = matchPercentage };
            });

            return recipesWithMatchPercentage
                .OrderByDescending(r => r.MatchPercentage)  // Sort by match percentage in descending order
                .Take(100)
                .Select(r =>
                {
                    r.Recipe.MatchPercentage = r.MatchPercentage;  // Store the match percentage in the Recipe object
                    return r.Recipe;
                })
                .ToList();
        }
    }
}
