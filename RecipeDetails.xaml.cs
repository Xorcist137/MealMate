using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HtmlAgilityPack;
namespace MealMate
{
    /// <summary>
    /// Interaction logic for RecipeDetails.xaml
    /// </summary>
    public partial class RecipeDetails : Page
    {
        private static readonly HttpClient client = new HttpClient();
        public RecipeDetails(Recipe recipe)
        {
            InitializeComponent();
            LoadRecipeDetails(recipe);
        }

        private async void LoadRecipeDetails(Recipe recipe)
        {
            RecipeTitle.Text = recipe.Name;

            // Load image
            var formattedName = FormatRecipeNameForUrl(recipe.Name);
            var recipeUrl = $"https://www.food.com/recipe/{formattedName}-{recipe.Id}";
            await LoadImageFromWebsite(recipeUrl);
            // Load ingredients
            IngredientsItemsControl.ItemsSource = recipe.Ingredients;

            // Load instructions
            InstructionsTextBlock.Text = recipe.Instructions;

            // Load nutrition information
            CaloriesTextBlock.Text = $"Calories: {recipe.Calories}";
            FatTextBlock.Text = $"Fat: {recipe.Fat}g";
            CarbsTextBlock.Text = $"Carbs: {recipe.Carbs}g";
            ProteinTextBlock.Text = $"Protein: {recipe.Protein}g";
        }
        private async Task LoadImageFromWebsite(string url)
        {
            try
            {
                string html = await client.GetStringAsync(url);

                // Save HTML to a file for inspection
                System.IO.File.WriteAllText("recipe_page.html", html);

                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);

                // Try different selectors
                var imgNode = htmlDocument.DocumentNode.SelectSingleNode("//img[contains(@class, 'primary-image')]");
                if (imgNode == null)
                {
                    imgNode = htmlDocument.DocumentNode.SelectSingleNode("//img[contains(@class, 'recipe-image')]");
                }
                if (imgNode == null)
                {
                    imgNode = htmlDocument.DocumentNode.SelectSingleNode("//div[contains(@class, 'primary-image')]//img");
                }

                if (imgNode != null)
                {
                    string imageUrl = imgNode.GetAttributeValue("src", "");
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        RecipeImage.Source = new BitmapImage(new Uri(imageUrl));
                    }
                    else
                    {
                        MessageBox.Show("Image URL not found on the page.");
                    }
                }
                else
                {
                    MessageBox.Show("HTML content saved to recipe_page.html. Please check this file to inspect the HTML structure.");
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Error accessing the website: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}");
            }
        }

        private string FormatRecipeNameForUrl(string recipeName)
        {
            return recipeName.ToLower()
                .Replace(" ", "-")
                .Replace("  ", "-")
                .Replace("'", "")
                .Replace(",", "")
                .Replace(".", "");
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}
