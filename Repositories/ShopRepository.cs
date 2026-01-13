using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace IKMC
{
        /// <summary>
    /// Репозиторий для выполнения операций с таблицей shop в базе данных PostgreSQL.
    /// </summary>
    /// <remarks>
    /// Обеспечивает CRUD-операции и проверку существования связанных сущностей.
    /// Строка подключения берется из App.config (ключ PostgreSQLConnection).
    /// </remarks>
    public class ShopRepository
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["PostgreSQLConnection"].ConnectionString;

        /// <summary>
        /// Получает все записи из таблицы shop.
        /// </summary>
        /// <returns>Список объектов Shop, представляющих все магазины в базе данных.</returns>
        /// <exception cref="NpgsqlException">Возникает при ошибках подключения к базе данных.</exception>
        public List<Shop> GetAll()
        {
            var shops = new List<Shop>();
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT id_shop, rate_shop, id_product FROM shop", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        shops.Add(new Shop
                        {
                            IdShop = reader.GetInt32(0),
                            RateShop = reader.GetDouble(1),
                            IdProduct = reader.GetInt32(2)
                        });
                    }
                }
            }
            return shops;
        }

        /// <summary>
        /// Добавляет новую запись в таблицу shop.
        /// </summary>
        /// <param name="shop">Объект Shop с данными для вставки.</param>
        /// <exception cref="NpgsqlException">Возникает при ошибках подключения или нарушении ограничений базы данных (например, дубликат ID).</exception>
        /// <exception cref="System.InvalidOperationException">Возникает при попытке добавить магазин с несуществующим продуктом.</exception>
        public void Add(Shop shop)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    @"INSERT INTO shop (id_shop, rate_shop, id_product) 
                      VALUES (@id, @rate, @productId)", conn))
                {
                    cmd.Parameters.AddWithValue("id", shop.IdShop);
                    cmd.Parameters.AddWithValue("rate", shop.RateShop);
                    cmd.Parameters.AddWithValue("productId", shop.IdProduct);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Обновляет существующую запись в таблице shop.
        /// </summary>
        /// <param name="shop">Объект Shop с обновленными данными.</param>
        /// <exception cref="NpgsqlException">Возникает при ошибках подключения к базе данных.</exception>
        public void Update(Shop shop)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    @"UPDATE shop 
                      SET rate_shop = @rate, id_product = @productId 
                      WHERE id_shop = @id", conn))
                {
                    cmd.Parameters.AddWithValue("rate", shop.RateShop);
                    cmd.Parameters.AddWithValue("productId", shop.IdProduct);
                    cmd.Parameters.AddWithValue("id", shop.IdShop);
                    cmd.ExecuteNonQuery();
                }
            }
        }


        /// <summary>
        /// Удаляет запись из таблицы shop по ID.
        /// </summary>
        /// <param name="idShop">ID магазина для удаления.</param>
        /// <exception cref="NpgsqlException">Возникает при ошибках подключения или нарушении внешних ключей (если на магазин ссылаются заказы).</exception>
        public void Delete(int idShop)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    "DELETE FROM shop WHERE id_shop = @id", conn))
                {
                    cmd.Parameters.AddWithValue("id", idShop);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Проверяет существование продукта с указанным ID в таблице product.
        /// </summary>
        /// <param name="idProduct">ID продукта для проверки.</param>
        /// <returns>true, если продукт существует; иначе false.</returns>
        /// <exception cref="NpgsqlException">Возникает при ошибках подключения к базе данных.</exception>
        public bool ProductExists(int idProduct)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    "SELECT 1 FROM product WHERE id_product = @id LIMIT 1", conn))
                {
                    cmd.Parameters.AddWithValue("id", idProduct);
                    using (var reader = cmd.ExecuteReader())
                    {
                        return reader.HasRows;
                    }
                }
            }
        }
    }
}
