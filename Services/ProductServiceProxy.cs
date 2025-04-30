using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Models;

namespace Amazon.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetAllProductsAsync();
        Task<Product> GetProductByIdAsync(int id);
        Task<List<Product>> GetProductsByCategoryAsync(string category);
        Task AddProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(int id);
        List<Product> Products { get; }
    }

    internal class ProductServiceProxy : IProductService
    {
        // Static property for easy access
        public static ProductServiceProxy Current { get; } = new ProductServiceProxy();

        // Implement the Products property from the interface
        public List<Product> Products { get; private set; }

        public ProductServiceProxy()
        {
            // Initialize with some sample data
            Products = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "Kindle Paperwhite",
                    Description = "E-reader with high-resolution display",
                    Price = 139.99m,
                    Category = "Electronics",
                    ImageUrl = "/images/kindle.jpg",
                    StockQuantity = 50,
                    CreatedAt = DateTime.Now
                },
                new Product
                {
                    Id = 2,
                    Name = "Echo Dot",
                    Description = "Smart speaker with Alexa",
                    Price = 49.99m,
                    Category = "Electronics",
                    ImageUrl = "/images/echo-dot.jpg",
                    StockQuantity = 100,
                    CreatedAt = DateTime.Now
                }
            };
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            await Task.Delay(100);
            return Products.ToList();
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            await Task.Delay(50);
            return Products.FirstOrDefault(p => p.Id == id);
        }

        public async Task<List<Product>> GetProductsByCategoryAsync(string category)
        {
            await Task.Delay(100);
            return Products.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public async Task AddProductAsync(Product product)
        {
            await Task.Delay(50);
            // Ensure the product has a unique ID
            product.Id = Products.Any() ? Products.Max(p => p.Id) + 1 : 1;
            product.CreatedAt = DateTime.Now;
            Products.Add(product);
        }

        public async Task UpdateProductAsync(Product product)
        {
            await Task.Delay(50);
            var existingProduct = Products.FirstOrDefault(p => p.Id == product.Id);
            if (existingProduct != null)
            {
                // Remove the existing product
                Products.Remove(existingProduct);
                // Add the updated product
                Products.Add(product);
            }
        }

        public async Task DeleteProductAsync(int id)
        {
            await Task.Delay(50);
            var productToRemove = Products.FirstOrDefault(p => p.Id == id);
            if (productToRemove != null)
            {
                Products.Remove(productToRemove);
            }
        }
    }
}