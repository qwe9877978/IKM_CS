using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKMC
{
    /// <summary>
    /// Представляет магазин в системе управления базой данных.
    /// </summary>
    /// <remarks>
    /// Содержит информацию о рейтинге магазина и связанном продукте.
    /// Используется для операций в таблице shop.
    /// </remarks>
    public class Shop
    {
        /// <summary>
        /// Уникальный идентификатор магазина.
        /// </summary>
        public int IdShop { get; set; }

        /// <summary>
        /// Рейтинг магазина по шкале от 0 до 5.
        /// </summary>
        public double RateShop { get; set; }

        /// <summary>
        /// Идентификатор связанного продукта.
        /// </summary>
        /// <remarks>
        /// Должен соответствовать существующему id_product в таблице product.
        /// </remarks>
        public int IdProduct { get; set; }
    }
}
