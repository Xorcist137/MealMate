using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Windows.Navigation;

namespace MealMate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<MealPlan> MyMealPlans { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            MyMealPlans = new List<MealPlan>();
            List<string>  baseIng = ["salt", "pepper","water"];
            MainFrame.Content = new IngredientPage();
            PopulateCategories();
            //FindTopIngredients();
        }

        public void AddMealPlan(MealPlan mealPlan)
        {
            this.MyMealPlans.Add(mealPlan);
            
        }

        private void FindTopIngredients()
        {
            string filePath = @"..\..\..\Assets\RAW_recipes.csv";
            var ingredientCounts = new Dictionary<string, int>();

            using (var reader = new StreamReader(filePath))
            {
                // Read the header
                var header = reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = ParseCsvLine(line);

                    // Assuming column K (11th column) contains the ingredients
                    if (values.Count >= 11)
                    {
                        string cellValue = values[10];
                        if (!string.IsNullOrWhiteSpace(cellValue))
                        {
                            var ingredients = ParseIngredients(cellValue);
                            foreach (var ingredient in ingredients)
                            {
                                if (ingredientCounts.ContainsKey(ingredient))
                                {
                                    ingredientCounts[ingredient]++;
                                }
                                else
                                {
                                    ingredientCounts[ingredient] = 1;
                                }
                            }
                        }
                    }
                }
            }

            var topIngredients = ingredientCounts
                .OrderByDescending(kvp => kvp.Value)
                .Take(500)
                .ToList();

            foreach (var ingredient in topIngredients)
            {
                Trace.WriteLine($"{ingredient.Key}");
            }
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
                            
                        };

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

        private List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            var current = new List<char>();
            bool inQuotes = false;

            foreach (var c in line)
            {
                if (c == '\"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(new string(current.ToArray()));
                    current.Clear();
                }
                else
                {
                    current.Add(c);
                }
            }

            result.Add(new string(current.ToArray()));
            return result;
        }

        private List<string> ParseIngredients(string cellValue)
        {
            var ingredients = cellValue.Trim('[', ']').Split(',')
                .Select(ingredient => ingredient.Trim('\'', ' '))
                .ToList();
            return ingredients;
        }

        private void Navigate_MyMealPlans(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new MyMealPlans());
        }
        private void Navigate_Home(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new IngredientPage());
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}