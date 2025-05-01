using System;
using System.Collections.Generic;

namespace Amazon.Models
{
    public class Product
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public decimal Price { get; set; }
        public required string Category { get; set; }
        public required string ImageUrl { get; set; }
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