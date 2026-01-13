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
    /// Репозиторий для выполнения операций с таблицей courier в базе данных PostgreSQL.
    /// </summary>
    /// <remarks>
    /// Обеспечивает CRUD-операции для управления курьерами.
    /// Особенность: поле id_courier является автоинкрементным, поэтому при добавлении не указывается.
    /// Строка подключения берется из App.config.
    /// </remarks>
    public class CourierRepository
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["PostgreSQLConnection"].ConnectionString;

        /// <summary>
        /// Получает все записи из таблицы courier.
        /// </summary>
        /// <returns>Список объектов Courier, представляющих всех курьеров в базе данных.</returns>
        /// <exception cref="NpgsqlException">Возникает при ошибках подключения к базе данных.</exception>
        public List<Courier> GetAll()
        {
            var couriers = new List<Courier>();
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT rate_courier, id_courier FROM courier", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        couriers.Add(new Courier
                        {
                            RateCourier = reader.GetDouble(0),
                            IdCourier = reader.GetInt32(1)
                        });
                    }
                }
            }
            return couriers;
        }

        /// <summary>
        /// Добавляет новую запись в таблицу courier.
        /// </summary>
        /// <param name="courier">Объект Courier с данными для вставки (ID генерируется автоматически).</param>
        /// <exception cref="NpgsqlException">Возникает при ошибках подключения к базе данных.</exception>
        public void Add(Courier courier)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    @"INSERT INTO courier (rate_courier) 
                      VALUES (@rate)", conn))
                {
                    cmd.Parameters.AddWithValue("rate", courier.RateCourier);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Обновляет существующую запись в таблице courier.
        /// </summary>
        /// <param name="courier">Объект Courier с обновленными данными.</param>
        /// <exception cref="NpgsqlException">Возникает при ошибках подключения к базе данных.</exception>
        public void Update(Courier courier)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    @"UPDATE courier 
                      SET rate_courier = @rate 
                      WHERE id_courier = @id", conn))
                {
                    cmd.Parameters.AddWithValue("rate", courier.RateCourier);
                    cmd.Parameters.AddWithValue("id", courier.IdCourier);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Удаляет запись из таблицы courier по ID.
        /// </summary>
        /// <param name="idCourier">ID курьера для удаления.</param>
        /// <exception cref="NpgsqlException">Возникает при ошибках подключения или нарушении внешних ключей (если курьер имеет заказы).</exception>
        public void Delete(int idCourier)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    "DELETE FROM courier WHERE id_courier = @id", conn))
                {
                    cmd.Parameters.AddWithValue("id", idCourier);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}