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
    /// Interaction logic for MyMealPlans.xaml
    /// </summary>
    public partial class MyMealPlans : Page
    {
        public MyMealPlans()
        {
            InitializeComponent();
            var mainWindow = Application.Current.MainWindow as MainWindow;
            MealPlansStackPanel.Children.Clear();

            // Sort and add meal plans
            LoadMealPlans(mainWindow.MyMealPlans);
        }

        private void LoadMealPlans(List<MealPlan> mealPlans)
        {
            

            foreach (var mealPlan in mealPlans)
            {
                var mealPlanPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(0, 0, 0, 20)
                };

                var nameTextBlock = new TextBlock
                {
                    Text = mealPlan.Name,
                    FontSize = 24,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 10),
                    FontFamily = new FontFamily("/Assets/Fonts/#Playfair Display")
                };

                var recipesPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal
                };

                AddRecipeImage(recipesPanel, mealPlan.Breakfast);
                AddRecipeImage(recipesPanel, mealPlan.Lunch);
                AddRecipeImage(recipesPanel, mealPlan.Dinner);

                mealPlanPanel.Children.Add(nameTextBlock);

                mealPlanPanel.Children.Add(recipesPanel);

                MealPlansStackPanel.Children.Add(mealPlanPanel);
            }
        }

        private void AddRecipeImage(StackPanel panel, Recipe recipe)
        {
            var image = new Image
            {
                Width = 400,
                Height = 300,
                Stretch = Stretch.UniformToFill,
                Margin = new Thickness(0, 0, 10, 0)
            };

            var formattedName = FormatRecipeNameForUrl(recipe.Name);
            var imageUrl = $"https://www.food.com/recipe/{formattedName}-{recipe.Id}/as-image";
            var bitmap = new BitmapImage(new Uri(imageUrl, UriKind.Absolute));
            image.Source = bitmap;

            panel.Children.Add(image);
            var ViewButton = new TextBlock
            {
                Text = "View Recipe",
                FontSize = 18,
                FontStyle = FontStyles.Italic,
                FontWeight = FontWeights.Bold,
                Cursor = Cursors.Hand,
                Tag = recipe,
                Margin = new Thickness(-130, -5, 0, 0),
                Height = 20,
                Width = 110,
                VerticalAlignment = VerticalAlignment.Top,
                Foreground = new SolidColorBrush(Colors.OrangeRed),
                Background = new SolidColorBrush(Colors.White)
            };
            ViewButton.MouseLeftButtonDown += ViewButton_Click;
            panel.Children.Add(ViewButton);
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
        private void ViewButton_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock viewButton && viewButton.Tag is Recipe recipe)
            {
                // Create an instance of the RecipeDetails page
                RecipeDetails recipeDetailsPage = new RecipeDetails(recipe);

                // Navigate to the RecipeDetails page
                NavigationService.Navigate(recipeDetailsPage);
            }
        }
        private string currentSortOption = "Calories";
        private bool isAscending = true;

        private void SortOption_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton)
            {
                currentSortOption = radioButton.Tag.ToString();
                SortMealPlans();
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
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow?.MyMealPlans == null) return;

            IEnumerable<MealPlan> sortedMealPlans = mainWindow.MyMealPlans;

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

            MealPlansStackPanel.Children.Clear();
            LoadMealPlans(sortedMealPlans.ToList());
        }

        
    }
}
