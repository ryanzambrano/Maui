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
        Task<Product?> GetProductByIdAsync(int id);
        Task<List<Product>> GetProductsByCategoryAsync(string category);
        Task AddProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(int id);
        List<Product> Products { get; }
    }

    public class ProductServiceProxy : IProductService
    {
        private static readonly List<Product> _products = new()
        {
            new Product
            {
                Id = 1,
                Name = "Laptop",
                Description = "High-performance laptop",
                Price = 999.99m,
                StockQuantity = 10,
                Category = "Electronics",
                ImageUrl = "https://example.com/laptop.jpg"
            },
            new Product
            {
                Id = 2,
                Name = "Smartphone",
                Description = "Latest smartphone model",
                Price = 699.99m,
                StockQuantity = 15,
                Category = "Electronics",
                ImageUrl = "https://example.com/phone.jpg"
            },
            new Product
            {
                Id = 3,
                Name = "Headphones",
                Description = "Wireless noise-cancelling headphones",
                Price = 199.99m,
                StockQuantity = 20,
                Category = "Electronics",
                ImageUrl = "https://example.com/headphones.jpg"
            }
        };

        public IEnumerable<Product> GetAllProducts()
        {
            return _products;
        }

        public void AddProduct(Product product)
        {
            product.Id = _products.Count > 0 ? _products.Max(p => p.Id) + 1 : 1;
            _products.Add(product);
        }

        public void UpdateProduct(Product product)
        {
            var existingProduct = _products.FirstOrDefault(p => p.Id == product.Id);
            if (existingProduct != null)
            {
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.StockQuantity = product.StockQuantity;
                existingProduct.Category = product.Category;
                existingProduct.ImageUrl = product.ImageUrl;
            }
        }

        public void DeleteProduct(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                _products.Remove(product);
            }
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            await Task.Delay(100);
            return _products.ToList();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            await Task.Delay(50);
            return _products.FirstOrDefault(p => p.Id == id);
        }

        public async Task<List<Product>> GetProductsByCategoryAsync(string category)
        {
            await Task.Delay(100);
            return _products.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public async Task AddProductAsync(Product product)
        {
            await Task.Delay(50);
            product.Id = _products.Count > 0 ? _products.Max(p => p.Id) + 1 : 1;
            product.CreatedAt = DateTime.Now;
            _products.Add(product);
        }

        public async Task UpdateProductAsync(Product product)
        {
            await Task.Delay(50);
            var existingProduct = _products.FirstOrDefault(p => p.Id == product.Id);
            if (existingProduct != null)
            {
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.StockQuantity = product.StockQuantity;
                existingProduct.Category = product.Category;
                existingProduct.ImageUrl = product.ImageUrl;
            }
        }

        public async Task DeleteProductAsync(int id)
        {
            await Task.Delay(50);
            var productToRemove = _products.FirstOrDefault(p => p.Id == id);
            if (productToRemove != null)
            {
                _products.Remove(productToRemove);
            }
        }

        public List<Product> Products => _products;
    }
}