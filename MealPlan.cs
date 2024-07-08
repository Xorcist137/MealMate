using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealMate
{
    public class MealPlan
    {
        public string Name { get; set; }
        public Recipe Breakfast { get; set; }
        public Recipe Lunch { get; set; }
        public Recipe Dinner { get; set; }

        public int TotalCalories => Breakfast.Calories + Lunch.Calories + Dinner.Calories;
        public int TotalFat => Breakfast.Fat + Lunch.Fat + Dinner.Fat;
        public int TotalProtein => Breakfast.Protein + Lunch.Protein + Dinner.Protein;
        public int TotalCarbs => Breakfast.Carbs + Lunch.Carbs + Dinner.Carbs;
        public int TotalIngredients => Breakfast.Ingredients.Count + Lunch.Ingredients.Count + Dinner.Ingredients.Count;
    }
}
