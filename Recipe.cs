using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealMate
{
    public class Recipe
    {
        public string Name { get; set; }
        public List<string> Ingredients { get; set; }
        public string Instructions { get; set; }

        public string Id { get; set; }

        public int Calories { get; set; }
        public int Fat { get; set; }
        public int Carbs { get; set; }
        public int Protein { get; set; }
    }
}
