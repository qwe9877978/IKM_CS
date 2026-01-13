using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKMC
{
    /// <summary>
    /// Представляет заказ в системе управления базой данных.
    /// </summary>
    /// <remarks>
    /// Содержит полную информацию о заказе, включая клиента, магазин, сумму, статус и курьера.
    /// Используется для операций в таблице orders.
    /// </remarks>
    public class Order
    {
        /// <summary>
        /// Уникальный идентификатор заказа.
        /// </summary>
        public int IdOrder { get; set; }

        /// <summary>
        /// Идентификатор клиента, сделавшего заказ.
        /// </summary>
        public int IdClient { get; set; }

        /// <summary>
        /// Идентификатор магазина, принимающего заказ.
        /// </summary>
        public int IdShop { get; set; }

        /// <summary>
        /// Сумма заказа в копейках.
        /// </summary>
        public long SummOrder { get; set; }

        /// <summary>
        /// Текущий статус заказа.
        /// </summary>
        /// <remarks>
        /// Допустимые значения: "Создан", "В пути", "Доставлен".
        /// </remarks>
        public string Status { get; set; }

        /// <summary>
        /// Дата создания заказа.
        /// </summary>
        /// <remarks>
        /// Формат: ГГГГ-ММ-ДД.
        /// </remarks>
        public DateTime DateCreateOrder { get; set; }

        /// <summary>
        /// Время создания заказа в секундах с начала дня.
        /// </summary>
        /// <remarks>
        /// Диапазон: 0-86400 секунд.
        /// </remarks>
        public int TimeCreate { get; set; }

        /// <summary>
        /// Идентификатор курьера, доставляющего заказ.
        /// </summary>
        public int IdCourier { get; set; }
    }
}