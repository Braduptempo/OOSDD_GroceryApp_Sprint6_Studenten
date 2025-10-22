using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Models;
using Microsoft.Data.Sqlite;

namespace Grocery.Core.Data.Repositories
{
    public class ProductRepository : DatabaseConnection, IProductRepository
    {
        private readonly List<Product> products = [];
        public ProductRepository()
        {
            InitializeDatabase();
            SeedDefaultData();
            GetAll();
        }
        
        /// <summary>
        /// Initializes the <c>product</c> table in the SQLite database if it does not already exist.
        /// </summary>
        /// <remarks>
        /// This method executes a <c>CREATE TABLE IF NOT EXISTS</c> statement to define the database schema
        /// for the <c>product</c> table.  
        /// The table contains the following columns:
        /// <list type="bullet">
        /// <item><description><c>Id</c> – Primary key, auto-incremented integer.</description></item>
        /// <item><description><c>Name</c> – Product name (text, required).</description></item>
        /// <item><description><c>Stock</c> – Quantity available in stock.</description></item>
        /// <item><description><c>ExpirationDate</c> – Expiration date of the product.</description></item>
        /// <item><description><c>Price</c> – Product price per unit.</description></item>
        /// </list>
        /// </remarks>
        private void InitializeDatabase()
        {
            const string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS product(
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Stock Int Not NULL,
                    ExpirationDate Date NOT NULL,
                    Price Decimal Not NULL
                );
            ";

            CreateTable(createTableQuery);
        }

        /// <summary>
        /// Inserts a predefined set of sample products into the <c>product</c> table.
        /// </summary>
        /// <remarks>
        /// This method creates a list of default product entries and generates an <c>INSERT</c> SQL statement for each.  
        /// All insert operations are executed within a single transaction using
        /// <see cref="DatabaseConnection.InsertMultipleWithTransaction(System.Collections.Generic.List{string})"/> 
        /// to ensure atomicity and consistency.
        ///
        /// Existing records are not updated or duplicated if the same data already exists in the table.
        /// </remarks>
        private void SeedDefaultData()
        {
            var defaultItems = new List<(string name, int stock, string expirationDate, Decimal price)>
            {
                ("Melk", 300, "2025-09-25", 0.95m),
                ("Kaas", 100, "2025-09-30", 7.98m),
                ("Brood", 400, "2025-09-12", 2.19m),
                ("Cornflakes", 0, "2025-12-31", 1.48m)
            };

            var insertQueries = defaultItems.Select(item =>
                $@"INSERT INTO product(Name, Stock, ExpirationDate, Price)
                VALUES ('{item.name}', {item.stock}, '{item.expirationDate}', {item.price});"
            ).ToList();
            
            InsertMultipleWithTransaction(insertQueries);
        }
        
        
        /// <summary>
        /// Retrieves all product records from the database.
        /// </summary>
        /// <returns>
        /// A list of all <see cref="Product"/> objects currently stored in the <c>product</c> table.
        /// </returns>
        /// <remarks>
        /// This method opens a connection to the database, executes a <c>SELECT *</c> query,
        /// and reads the results using a <see cref="SqliteDataReader"/>.  
        /// Each record is converted into a <see cref="Product"/> instance and added to the in-memory list.  
        /// 
        /// The database connection is closed automatically after reading all records.
        /// </remarks>
        public List<Product> GetAll()
        {
            products.Clear();
            
            string query = "SELECT * FROM product";
            
            OpenConnection();

            using (SqliteCommand command = new SqliteCommand(query, Connection))
            {
                SqliteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string name = reader.GetString(1);
                    int stock = reader.GetInt32(2);
                    DateOnly expirationDate = DateOnly.FromDateTime(reader.GetDateTime(3));
                    Decimal price = reader.GetDecimal(4);
                    products.Add(new Product(id, name, stock, expirationDate, price));
                }
            }
            CloseConnection();
            return products;
        }

        public Product? Get(int id)
        {
            return products.FirstOrDefault(p => p.Id == id);
        }

        public Product Add(Product item)
        {
            const string insertQuery = @"
                INSERT INTO product(Name, Stock, ExpirationDate, Price)
                VALUES(@Name, @Stock, @ExpirationDate, @Price);";
            
            OpenConnection();

            using (var command = new SqliteCommand(insertQuery, Connection))
            {
                command.Parameters.AddWithValue("@Name", item.Name);
                command.Parameters.AddWithValue("@Stock", item.Stock);
                command.Parameters.AddWithValue("@Date", item.ShelfLife);
                command.Parameters.AddWithValue("@Price", item.Price);
                
                item.Id = Convert.ToInt32(command.ExecuteScalar());
            }
            
            CloseConnection();

            return item;
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
