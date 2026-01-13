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
    /// Репозиторий для выполнения операций с таблицей orders в базе данных PostgreSQL.
    /// </summary>
    /// <remarks>
    /// Обеспечивает CRUD-операции с проверкой внешних ключей для магазинов и курьеров.
    /// Строка подключения берется из App.config.
    /// Особенность: дата создания заказа хранится как DateOnly, время как количество секунд с начала дня.
    /// </remarks>

    public class OrderRepository
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["PostgreSQLConnection"].ConnectionString;

        /// <summary>
        /// Получает все записи из таблицы orders.
        /// </summary>
        /// <returns>Список объектов Order, представляющих все заказы в базе данных.</returns>
        /// <exception cref="NpgsqlException">Возникает при ошибках подключения к базе данных.</exception>
        public List<Order> GetAll()
        {
            var orders = new List<Order>();
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    @"SELECT id_client, id_shop, summ_order, status, 
                             date_create_order, time_create, id_order, id_courier 
                      FROM orders", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        orders.Add(new Order
                        {
                            IdClient = reader.GetInt32(0),
                            IdShop = reader.GetInt32(1),
                            SummOrder = reader.GetInt64(2),
                            Status = reader.GetString(3),
                            DateCreateOrder = reader.GetDateTime(4),
                            TimeCreate = reader.GetInt32(5),
                            IdOrder = reader.GetInt32(6),
                            IdCourier = reader.GetInt32(7)
                        });
                    }
                }
            }
            return orders;
        }

        /// <summary>
        /// Добавляет новую запись в таблицу orders.
        /// </summary>
        /// <param name="order">Объект Order с данными для вставки.</param>
        /// <exception cref="NpgsqlException">Возникает при ошибках подключения или нарушении внешних ключей.</exception>
        /// <exception cref="System.ArgumentException">Возникает при указании несуществующих ID магазина или курьера.</exception>
        public void Add(Order order)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    @"INSERT INTO orders (id_client, id_shop, summ_order, status, 
                                        date_create_order, time_create, id_courier) 
                      VALUES (@client, @shop, @summ, @status, @date, @time, @courier)", conn))
                {
                    cmd.Parameters.AddWithValue("client", order.IdClient);
                    cmd.Parameters.AddWithValue("shop", order.IdShop);
                    cmd.Parameters.AddWithValue("summ", order.SummOrder);
                    cmd.Parameters.AddWithValue("status", order.Status);
                    cmd.Parameters.AddWithValue("date", DateOnly.FromDateTime(order.DateCreateOrder));
                    cmd.Parameters.AddWithValue("time", order.TimeCreate);
                    cmd.Parameters.AddWithValue("courier", order.IdCourier);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Обновляет существующую запись в таблице orders.
        /// </summary>
        /// <param name="order">Объект Order с обновленными данными.</param>
        /// <exception cref="NpgsqlException">Возникает при ошибках подключения к базе данных.</exception>
        public void Update(Order order)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    @"UPDATE orders 
                      SET id_client = @client, id_shop = @shop, summ_order = @summ, 
                          status = @status, date_create_order = @date, time_create = @time, 
                          id_courier = @courier 
                      WHERE id_order = @id", conn))
                {
                    cmd.Parameters.AddWithValue("client", order.IdClient);
                    cmd.Parameters.AddWithValue("shop", order.IdShop);
                    cmd.Parameters.AddWithValue("summ", order.SummOrder);
                    cmd.Parameters.AddWithValue("status", order.Status);
                    cmd.Parameters.AddWithValue("date", DateOnly.FromDateTime(order.DateCreateOrder));
                    cmd.Parameters.AddWithValue("time", order.TimeCreate);
                    cmd.Parameters.AddWithValue("courier", order.IdCourier);
                    cmd.Parameters.AddWithValue("id", order.IdOrder);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Удаляет запись из таблицы orders по ID.
        /// </summary>
        /// <param name="idOrder">ID заказа для удаления.</param>
        /// <exception cref="NpgsqlException">Возникает при ошибках подключения к базе данных.</exception>
        public void Delete(int idOrder)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    "DELETE FROM orders WHERE id_order = @id", conn))
                {
                    cmd.Parameters.AddWithValue("id", idOrder);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Проверяет существование магазина с указанным ID в таблице shop.
        /// </summary>
        /// <param name="idShop">ID магазина для проверки.</param>
        /// <returns>true, если магазин существует; иначе false.</returns>
        /// <exception cref="NpgsqlException">Возникает при ошибках подключения к базе данных.</exception>
        public bool ShopExists(int idShop)
        {
            return EntityExists("shop", "id_shop", idShop);
        }

        /// <summary>
        /// Проверяет существование курьера с указанным ID в таблице courier.
        /// </summary>
        /// <param name="idCourier">ID курьера для проверки.</param>
        /// <returns>true, если курьер существует; иначе false.</returns>
        /// <exception cref="NpgsqlException">Возникает при ошибках подключения к базе данных.</exception>
        public bool CourierExists(int idCourier)
        {
            return EntityExists("courier", "id_courier", idCourier);
        }

        /// <summary>
        /// Проверяет существование сущности в указанной таблице.
        /// </summary>
        /// <param name="table">Имя таблицы для проверки.</param>
        /// <param name="column">Имя столбца с идентификатором.</param>
        /// <param name="id">Значение идентификатора для поиска.</param>
        /// <returns>true, если запись существует; иначе false.</returns>
        /// <exception cref="NpgsqlException">Возникает при ошибках подключения к базе данных.</exception>
        /// <remarks>Внутренний вспомогательный метод для проверки внешних ключей.</remarks>
        private bool EntityExists(string table, string column, int id)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    $"SELECT 1 FROM {table} WHERE {column} = @id LIMIT 1", conn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        return reader.HasRows;
                    }
                }
            }
        }
    }
}
