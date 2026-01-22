using Liberty.Models;

namespace Liberty.ViewModels
{
    public class ProductGetVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public int Rating { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        
    }
}
