using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKMC
{
    /// <summary>
    /// Представляет продукт в системе управления базой данных.
    /// </summary>
    /// <remarks>
    /// Содержит информацию о цене продукта.
    /// Используется для операций в таблице product.
    /// </remarks>
    public class Product
    {
        /// <summary>
        /// Уникальный идентификатор продукта.
        /// </summary>
        public int IdProduct { get; set; }

        /// <summary>
        /// Цена продукта в рублях.
        /// </summary>
        /// <remarks>
        /// Значение должно быть положительным числом.
        /// </remarks>
        public double Price { get; set; }
    }
}