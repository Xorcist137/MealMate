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
using System.IO;

namespace MealMate
{
    /// <summary>
    /// Interaction logic for IngredientPage.xaml
    /// </summary>
    public partial class IngredientPage : Page
    {
        public IngredientPage()
        {
            InitializeComponent();
            PopulateCategories();
        }

        private Dictionary<string, List<string>> ParseCategoriesCSV(string filePath)
        {
            var categories = new Dictionary<string, List<string>>();

            foreach (var line in File.ReadLines(filePath))
            {
                var parts = line.Split(',');
                if (parts.Length == 2)
                {
                    var ingredient = parts[0].Trim();
                    var category = parts[1].Trim();

                    if (!categories.ContainsKey(category))
                    {
                        categories[category] = new List<string>();
                    }
                    categories[category].Add(ingredient);
                }
            }

            return categories;
        }

        private void PopulateCategories()
        {
            var categories = ParseCategoriesCSV("../../../Assets/categories.csv");
            foreach (var category in categories)
            {
                HashSet<string> defaultCheckedIngredients = new HashSet<string> { "salt", "pepper", "water", };
                var contentBorderName = category.Key.Replace(" ", "") + "Content";
                var contentBorder = FindName(contentBorderName) as Border;
                if (contentBorder != null)
                {
                    var grid = new Grid();
                    for (int i = 0; i < 4; i++)
                    {
                        grid.ColumnDefinitions.Add(new ColumnDefinition());
                    }

                    var ingredientCount = category.Value.Count;
                    var itemsPerColumn = (int)Math.Ceiling(ingredientCount / 4.0);

                    // Create all necessary rows upfront
                    for (int i = 0; i < itemsPerColumn; i++)
                    {
                        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    }

                    for (int i = 0; i < ingredientCount; i++)
                    {
                        var checkBox = new CheckBox
                        {
                            Content = category.Value[i],
                            Margin = new Thickness(10, 5, 0, 5),
                            FontSize = 18,
                            FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Assets/Fonts/#Playfair Display"),
                            FontWeight = FontWeights.Bold,
                            IsChecked = false
                        };
                        if (defaultCheckedIngredients.Contains(category.Value[i].ToLower()))
                        {
                            checkBox.IsChecked = true;
                        }
                        int column = i / itemsPerColumn;
                        int row = i % itemsPerColumn;

                        Grid.SetColumn(checkBox, column);
                        Grid.SetRow(checkBox, row);
                        grid.Children.Add(checkBox);
                    }

                    var scrollViewer = new ScrollViewer
                    {
                        Content = grid,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                    };
                    contentBorder.Child = scrollViewer;
                }
            }
        }
        private void Category_Border_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border clickedBorder)
            {
                string contentName = clickedBorder.Tag + "Content";
                var contentBorder = this.FindName(contentName) as Border;

                if (contentBorder != null)
                {
                    contentBorder.Visibility = contentBorder.Visibility == Visibility.Collapsed
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }
            }
        }
        private void FindMealPlansButton_Click(object sender, MouseButtonEventArgs e)
        {
            List<string> checkedIngredients = GetCheckedIngredients();
            Results resultsPage = new Results(checkedIngredients);
            NavigationService.Navigate(resultsPage);
        }
        private IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
        private List<string> GetCheckedIngredients()
        {
            List<string> checkedIngredients = new List<string>();
            foreach (var contentBorder in FindVisualChildren<Border>(this).Where(b => b.Name.EndsWith("Content")))
            {
                if (contentBorder.Child is ScrollViewer scrollViewer && scrollViewer.Content is Grid grid)
                {
                    foreach (var checkBox in grid.Children.OfType<CheckBox>())
                    {
                        if (checkBox.IsChecked == true)
                        {
                            checkedIngredients.Add(checkBox.Content.ToString());
                        }
                    }
                }
            }
            return checkedIngredients;
        }

        // Helper method to find visual children

    }
}
