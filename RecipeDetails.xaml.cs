using System;
using System.Collections.Generic;
using System.Linq;
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

namespace MealMate
{
    /// <summary>
    /// Interaction logic for RecipeDetails.xaml
    /// </summary>
    public partial class RecipeDetails : Page
    {
        public RecipeDetails(Recipe recipe)
        {
            InitializeComponent();
            LoadRecipeDetails(recipe);
        }

        private void LoadRecipeDetails(Recipe recipe)
        {
            RecipeTitle.Text = recipe.Name;

            // Load image
            var formattedName = FormatRecipeNameForUrl(recipe.Name);
            var imageUrl = $"https://www.food.com/recipe/{formattedName}-{recipe.Id}/as-image";
            RecipeImage.Source = new BitmapImage(new Uri(imageUrl, UriKind.Absolute));

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
