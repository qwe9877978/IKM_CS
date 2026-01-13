using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKMC
{
    /// <summary>
    /// Репозиторий для выполнения операций с таблицей product в базе данных PostgreSQL.
    /// </summary>
    /// <remarks>
    /// Обеспечивает CRUD-операции для управления продуктами.
    /// Строка подключения берется из App.config.
    /// </remarks>
    public class ProductRepository
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["PostgreSQLConnection"].ConnectionString;

        /// <summary>
        /// Получает все записи из таблицы product.
        /// </summary>
        /// <returns>Список объектов Product, представляющих все продукты в базе данных.</returns>
        /// <exception cref="NpgsqlException">Возникает при ошибках подключения к базе данных.</exception>
        public List<Product> GetAll()
        {
            var products = new List<Product>();
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT id_product, price FROM product", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            IdProduct = reader.GetInt32(0),
                            Price = reader.GetDouble(1)
                        });
                    }
                }
            }
            return products;
        }

        /// <summary>
        /// Добавляет новую запись в таблицу product.
        /// </summary>
        /// <param name="product">Объект Product с данными для вставки.</param>
        /// <exception cref="NpgsqlException">Возникает при ошибках подключения или нарушении ограничений базы данных.</exception>
        public void Add(Product product)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    @"INSERT INTO product (id_product, price) 
                      VALUES (@id, @price)", conn))
                {
                    cmd.Parameters.AddWithValue("id", product.IdProduct);
                    cmd.Parameters.AddWithValue("price", product.Price);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Обновляет существующую запись в таблице product.
        /// </summary>
        /// <param name="product">Объект Product с обновленными данными.</param>
        /// <exception cref="NpgsqlException">Возникает при ошибках подключения к базе данных.</exception>

        public void Update(Product product)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    @"UPDATE product 
                      SET price = @price 
                      WHERE id_product = @id", conn))
                {
                    cmd.Parameters.AddWithValue("price", product.Price);
                    cmd.Parameters.AddWithValue("id", product.IdProduct);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Удаляет запись из таблицы product по ID.
        /// </summary>
        /// <param name="idProduct">ID продукта для удаления.</param>
        /// <exception cref="NpgsqlException">Возникает при ошибках подключения или нарушении внешних ключей (если продукт используется в магазинах).</exception>

        public void Delete(int idProduct)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    "DELETE FROM product WHERE id_product = @id", conn))
                {
                    cmd.Parameters.AddWithValue("id", idProduct);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}