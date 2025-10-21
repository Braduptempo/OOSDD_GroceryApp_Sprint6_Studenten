using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Models;

namespace Grocery.Core.Data.Repositories
{
    public class ProductRepository : DatabaseConnection, IProductRepository
    {
        private readonly List<Product> products;
        public ProductRepository()
        {
            InitializeDatabase();
            SeedDefaultData();
            GetAll();
            // products = [
            //     new Product(1, "Melk", 300, new DateOnly(2025, 9, 25), 0.95m),
            //     new Product(2, "Kaas", 100, new DateOnly(2025, 9, 30), 7.98m),
            //     new Product(3, "Brood", 400, new DateOnly(2025, 9, 12), 2.19m),
            //     new Product(4, "Cornflakes", 0, new DateOnly(2025, 12, 31), 1.48m)];
        }

        private void InitializeDatabase()
        {
            const string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS product(
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Stock Int Not NULL,
                    ExpirationDate Date NOT NULL,
                    Price Double Not NULL
                );
            ";

            CreateTable(createTableQuery);
        }

        private void SeedDefaultData()
        {
            var defaultItems = new List<(string name, int stock, string expirationDate, double price)>
            {
                ("Melk", 300, "2025-9-25", 0.95),
                ("Kaas", 100, "2025-9-30", 7.98),
                ("Brood", 400, "2025-9-12", 2.19),
                ("Cornflakes", 0, "2025-12-31", 1.48)
            };

            var insertQueries = defaultItems.Select(item =>
                $@"INSERT INTO product(Name, Stock, ExpirationDate, Price)
                VALUES ('{item.name}', '{item.stock}', '{item.expirationDate}', '{item.price}');"
            ).ToList();
            
            InsertMultipleWithTransaction(insertQueries);
        }
        public List<Product> GetAll()
        {
            return products;
        }

        public Product? Get(int id)
        {
            return products.FirstOrDefault(p => p.Id == id);
        }

        public Product Add(Product item)
        {
            throw new NotImplementedException();
        }

        public Product? Delete(Product item)
        {
            throw new NotImplementedException();
        }

        public Product? Update(Product item)
        {
            Product? product = products.FirstOrDefault(p => p.Id == item.Id);
            if (product == null) return null;
            product.Id = item.Id;
            return product;
        }
    }
}
