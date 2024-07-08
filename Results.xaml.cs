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
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Results : Page
    {
        private List<Recipe> matchedRecipes;
        private Recipe currentBreakfast, currentLunch, currentDinner;

        public Results(List<string> checkedIngredients)
        {
            InitializeComponent();
            MainGrid.Cursor = Cursors.Wait;
            Task.Run(() =>
            {
                RecipeSearcher searcher = new RecipeSearcher("../../../Assets/RAW_recipes.csv");
                matchedRecipes = searcher.SearchRecipes(checkedIngredients);

                // Use Dispatcher to update UI on the main thread
                Dispatcher.Invoke(() =>
                {
                    DisplayRecipes();
                    // Reset cursor
                    MainGrid.Cursor = Cursors.Arrow;
                });
            });
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

        private void DisplayRecipes()
        {
            var random = new Random();
            var mealTypes = new[] { Breakfast_Recipes, Lunch_Recipes, Dinner_Recipes };

            foreach (var recipe in matchedRecipes)
            {
                var image = new Image
                {
                    Width = 350,
                    Height = 175,
                    Stretch = Stretch.UniformToFill,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Cursor = Cursors.Hand,
                    Tag = recipe,
                    Margin = new Thickness(5, 0, 0, 0),
                };

                var formattedName = FormatRecipeNameForUrl(recipe.Name);
                var imageUrl = $"https://www.food.com/recipe/{formattedName}-{recipe.Id}/as-image";

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imageUrl, UriKind.Absolute);
                bitmap.EndInit();

                image.Source = bitmap;
                image.ImageFailed += Image_ImageFailed;
                image.MouseLeftButtonDown += Recipe_Click;


                // Randomly assign to a meal type
                mealTypes[random.Next(mealTypes.Length)].Children.Add(image);
            }
        }

        private void Recipe_Click(object sender, MouseButtonEventArgs e)
        {
            var clickedImage = sender as Image;
            var recipe = clickedImage.Tag as Recipe;

            if (clickedImage.Parent == Breakfast_Recipes)
            {
                BreakfastMeal.ImageSource = clickedImage.Source;
                currentBreakfast = recipe;
            }
            else if (clickedImage.Parent == Lunch_Recipes)
            {
                LunchMeal.ImageSource = clickedImage.Source;
                currentLunch = recipe;
            }
            else if (clickedImage.Parent == Dinner_Recipes)
            {
                DinnerMeal.ImageSource = clickedImage.Source;
                currentDinner = recipe;
            }
        }
        private void SaveMealPlan_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MealPlanName.Text))
            {
                MessageBox.Show("Please enter a name for your meal plan.");
                return;
            }

            if (currentBreakfast == null || currentLunch == null || currentDinner == null)
            {
                MessageBox.Show("Please select a recipe for each meal.");
                return;
            }

            var mealPlan = new MealPlan
            {
                Name = MealPlanName.Text,
                Breakfast = currentBreakfast,
                Lunch = currentLunch,
                Dinner = currentDinner
            };
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?.AddMealPlan(mealPlan);

            MessageBox.Show($"Meal plan '{mealPlan.Name}' saved successfully!");
            currentBreakfast = null;
            currentLunch = null;
            currentDinner = null;
            BreakfastMeal.ImageSource = null;
            LunchMeal.ImageSource = null;
            DinnerMeal.ImageSource = null;
            MealPlanName.Text = string.Empty;
        }
        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            if (sender is Image failedImage)
            {
                failedImage.Visibility = Visibility.Collapsed;
            }
        }
    }
    
}
