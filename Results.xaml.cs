using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Results : Page
    {
        private List<Recipe> matchedRecipes;
        private Recipe currentBreakfast, currentLunch, currentDinner;
        private static readonly HttpClient client = new HttpClient();

        public Results(List<string> checkedIngredients)
        {
            InitializeComponent();
            ShowLoadingMessage(true);
            MainGrid.Cursor = Cursors.Wait;
            Task.Run(() =>
            {
                RecipeSearcher searcher = new RecipeSearcher("../../../Assets/RAW_recipes.csv");
                matchedRecipes = searcher.SearchRecipes(checkedIngredients);

                // Use Dispatcher to update UI on the main thread
                Dispatcher.Invoke(() =>
                {
                    DisplayRecipes();
                    ShowLoadingMessage(false);
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
        private string TruncateString(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return input.Length <= maxLength ? input : input.Substring(0, maxLength - 3) + "...";
        }

        private async Task DisplayRecipes()
        {
            var random = new Random();
            var mealTypes = new[] { Breakfast_Recipes, Lunch_Recipes, Dinner_Recipes };

            foreach (var recipe in matchedRecipes)
            {
                var recipeGrid = new Grid
                {
                    Margin = new Thickness(5, 0, 5, 10),
                    Width = 350,
                    Height = 175,
                    VerticalAlignment = VerticalAlignment.Bottom,
                };

                var image = new Image
                {
                    Stretch = Stretch.UniformToFill,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Cursor = Cursors.Hand,
                    Tag = recipe,
                };

                var formattedName = FormatRecipeNameForUrl(recipe.Name);
                var recipeUrl = $"https://www.food.com/recipe/{formattedName}-{recipe.Id}";

                try
                {
                    string imageUrl = await GetRecipeImageUrl(recipeUrl);
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        image.Source = new BitmapImage(new Uri(imageUrl));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading image for recipe {recipe.Name}: {ex.Message}");
                }

                image.ImageFailed += Image_ImageFailed;
                image.MouseLeftButtonDown += Recipe_Click;
                string truncatedName = TruncateString(recipe.Name, 35);
                var recipeName = new TextBlock
                {
                    Text = truncatedName,
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 150),
                    Foreground = new SolidColorBrush(Colors.SaddleBrown),
                    Background = new SolidColorBrush(Colors.White),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Bottom
                };
                var viewButton = new Button
                {
                    Content = "View Recipe",
                    FontSize = 14,
                    FontStyle = FontStyles.Italic,
                    FontWeight = FontWeights.Bold,
                    Cursor = Cursors.Hand,
                    Tag = recipe,
                    Height = 28,
                    Width = 110,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(0, 0, 0, 10),
                    Foreground = new SolidColorBrush(Colors.OrangeRed),
                    Background = new SolidColorBrush(Colors.White),
                    BorderBrush = new SolidColorBrush(Colors.OrangeRed),
                };
                viewButton.Click += ViewButton_Click;

                recipeGrid.Children.Add(image);
                recipeGrid.Children.Add(viewButton);
                recipeGrid.Children.Add(recipeName);
                // Add click event to the Grid as well
                recipeGrid.MouseLeftButtonDown += Recipe_Click;
                recipeGrid.Tag = recipe;

                // Randomly assign to a meal type
                mealTypes[random.Next(mealTypes.Length)].Children.Add(recipeGrid);
            }
            ShowLoadingMessage(false);
        }
        private void ShowLoadingMessage(bool show)
        {
            LoadingMessage.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }
        private async Task<string> GetRecipeImageUrl(string recipeUrl)
        {
            string html = await client.GetStringAsync(recipeUrl);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var imgNode = htmlDocument.DocumentNode.SelectSingleNode("//img[contains(@class, 'primary-image')]");
            if (imgNode == null)
            {
                imgNode = htmlDocument.DocumentNode.SelectSingleNode("//img[contains(@class, 'recipe-image')]");
            }
            if (imgNode == null)
            {
                imgNode = htmlDocument.DocumentNode.SelectSingleNode("//div[contains(@class, 'primary-image')]//img");
            }

            return imgNode?.GetAttributeValue("src", "");
        }
        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button viewButton && viewButton.Tag is Recipe recipe)
            {
                // Create an instance of the RecipeDetails page
                RecipeDetails recipeDetailsPage = new RecipeDetails(recipe);

                // Navigate to the RecipeDetails page
                NavigationService.Navigate(recipeDetailsPage);
            }
        }
        private void Recipe_Click(object sender, MouseButtonEventArgs e)
        {
            var clickedElement = sender as FrameworkElement;
            var recipe = clickedElement?.Tag as Recipe;

            if (recipe == null) return;

            var parentPanel = clickedElement.GetVisualParent<Panel>();

            if (parentPanel == Breakfast_Recipes)
            {
                BreakfastMeal.ImageSource = (clickedElement as Image)?.Source ?? (clickedElement.FindVisualChild<Image>()?.Source);
                currentBreakfast = recipe;
            }
            else if (parentPanel == Lunch_Recipes)
            {
                LunchMeal.ImageSource = (clickedElement as Image)?.Source ?? (clickedElement.FindVisualChild<Image>()?.Source);
                currentLunch = recipe;
            }
            else if (parentPanel == Dinner_Recipes)
            {
                DinnerMeal.ImageSource = (clickedElement as Image)?.Source ?? (clickedElement.FindVisualChild<Image>()?.Source);
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
