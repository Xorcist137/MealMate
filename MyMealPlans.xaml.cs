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
    /// Interaction logic for MyMealPlans.xaml
    /// </summary>
    public partial class MyMealPlans : Page
    {
        private static readonly HttpClient client = new HttpClient();
        private string currentSortOption = "Calories";
        private bool isAscending = true;
        private List<MealPlan> currentMealPlans;
        public MyMealPlans()
        {
            InitializeComponent();
            var mainWindow = Application.Current.MainWindow as MainWindow;
            currentMealPlans = new List<MealPlan>(mainWindow?.MyMealPlans ?? new List<MealPlan>());

            LoadMealPlans();
        }

        private async void LoadMealPlans()
        {
            MealPlansStackPanel.Children.Clear();

            foreach (var mealPlan in currentMealPlans)
            {
                var mealPlanPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(0, 0, 0, 20)
                };

                var headerPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 0, 0, 10)
                };

                var nameTextBlock = new TextBlock
                {
                    Text = mealPlan.Name,
                    FontSize = 24,
                    FontWeight = FontWeights.Bold,
                    FontFamily = new FontFamily("/Assets/Fonts/#Playfair Display"),
                    VerticalAlignment = VerticalAlignment.Center
                };

                var deleteButton = new Button
                {
                    Content = "❌",
                    FontSize = 18,
                    Width = 30,
                    Height = 30,
                    Margin = new Thickness(10, 0, 0, 0),
                    Background = Brushes.Transparent,
                    BorderBrush = Brushes.Transparent,
                    Foreground = Brushes.Red,
                    Cursor = Cursors.Hand,
                    Tag = mealPlan
                };
                deleteButton.Click += DeleteButton_Click;

                headerPanel.Children.Add(nameTextBlock);
                headerPanel.Children.Add(deleteButton);

                var recipesPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal
                };

                await AddRecipeGrid(recipesPanel, mealPlan.Breakfast, "Breakfast");
                await AddRecipeGrid(recipesPanel, mealPlan.Lunch, "Lunch");
                await AddRecipeGrid(recipesPanel, mealPlan.Dinner, "Dinner");

                mealPlanPanel.Children.Add(headerPanel);
                mealPlanPanel.Children.Add(recipesPanel);

                MealPlansStackPanel.Children.Add(mealPlanPanel);
            }
        }
        private string TruncateString(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return input.Length <= maxLength ? input : input.Substring(0, maxLength - 3) + "...";
        }
        private async Task AddRecipeGrid(StackPanel panel, Recipe recipe, string mealType)
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
            string truncatedName = TruncateString(recipe.Name, 35);
            var recipeName = new TextBlock
            {
                Text = $"{mealType}: {truncatedName}",
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

            panel.Children.Add(recipeGrid);
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

        private string FormatRecipeNameForUrl(string recipeName)
        {
            return recipeName.ToLower()
                .Replace(" ", "-")
                .Replace("  ", "-")
                .Replace("'", "")
                .Replace(",", "")
                .Replace(".", "");
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button viewButton && viewButton.Tag is Recipe recipe)
            {
                RecipeDetails recipeDetailsPage = new RecipeDetails(recipe);
                NavigationService.Navigate(recipeDetailsPage);
            }
        }

        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            if (sender is Image failedImage)
            {
                failedImage.Visibility = Visibility.Collapsed;
            }
        }


        private void SortOrderToggle_Checked(object sender, RoutedEventArgs e)
        {
            isAscending = true;
            SortOrderToggle.Content = "Ascending";
            SortMealPlans();
        }

        private void SortOrderToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            isAscending = false;
            SortOrderToggle.Content = "Descending";
            SortMealPlans();
        }

        private void SortMealPlans()
        {
            if (currentMealPlans == null || !currentMealPlans.Any())
            {
                // If there are no meal plans, just return without sorting
                return;
            }

            IEnumerable<MealPlan> sortedMealPlans = currentMealPlans;

            switch (currentSortOption)
            {
                case "Calories":
                    sortedMealPlans = isAscending
                        ? sortedMealPlans.OrderBy(mp => mp.TotalCalories)
                        : sortedMealPlans.OrderByDescending(mp => mp.TotalCalories);
                    break;
                case "Fat":
                    sortedMealPlans = isAscending
                        ? sortedMealPlans.OrderBy(mp => mp.TotalFat)
                        : sortedMealPlans.OrderByDescending(mp => mp.TotalFat);
                    break;
                case "Protein":
                    sortedMealPlans = isAscending
                        ? sortedMealPlans.OrderBy(mp => mp.TotalProtein)
                        : sortedMealPlans.OrderByDescending(mp => mp.TotalProtein);
                    break;
                case "Carbs":
                    sortedMealPlans = isAscending
                        ? sortedMealPlans.OrderBy(mp => mp.TotalCarbs)
                        : sortedMealPlans.OrderByDescending(mp => mp.TotalCarbs);
                    break;
                case "Ingredients":
                    sortedMealPlans = isAscending
                        ? sortedMealPlans.OrderBy(mp => mp.TotalIngredients)
                        : sortedMealPlans.OrderByDescending(mp => mp.TotalIngredients);
                    break;
            }

            currentMealPlans = sortedMealPlans.ToList();
            LoadMealPlans();
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button deleteButton && deleteButton.Tag is MealPlan mealPlan)
            {
                var result = MessageBox.Show($"Are you sure you want to delete the meal plan '{mealPlan.Name}'?",
                                             "Confirm Deletion",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    currentMealPlans.Remove(mealPlan);
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    mainWindow?.MyMealPlans.Remove(mealPlan);
                    LoadMealPlans();
                }
            }
        }
        private void SortOption_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.Tag is string sortOption)
            {
                currentSortOption = sortOption;
                SortMealPlans();
            }
        }
    }


}

