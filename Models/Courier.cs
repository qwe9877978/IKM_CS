using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKMC
{
    /// <summary>
    /// Представляет курьера в системе управления базой данных.
    /// </summary>
    /// <remarks>
    /// Содержит информацию о рейтинге курьера.
    /// Используется для операций в таблице courier.
    /// </remarks>
    public class Courier
    {
        /// <summary>
        /// Уникальный идентификатор курьера.
        /// </summary>
        public int IdCourier { get; set; }

        /// <summary>
        /// Рейтинг курьера по шкале от 0 до 5.
        /// </summary>
        public double RateCourier { get; set; }
    }
}