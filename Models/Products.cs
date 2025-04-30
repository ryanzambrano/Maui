using System;
using System.Collections.Generic;

namespace Amazon.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public string ImageUrl { get; set; }
        public int StockQuantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsAvailable => StockQuantity > 0;

        // Optional: Method to apply discount
        public decimal GetDiscountedPrice(decimal discountPercentage)
        {
            return Price * (1 - (discountPercentage / 100));
        }
    }
}