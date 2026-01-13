using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IKMC
{
    /// <summary>
    /// Окно для добавления новых и редактирования существующих продуктов.
    /// </summary>
    /// <remarks>
    /// Содержит два текстовых поля для ID продукта и его цены.
    /// Обеспечивает базовую валидацию вводимых данных.
    /// </remarks>
    public partial class AddEditProductWindow : Window
    {
        /// <summary>
        /// Объект Product, содержащий данные для сохранения.
        /// </summary>
        public Product Product { get; private set; }

        /// <summary>
        /// Инициализирует окно для добавления нового или редактирования существующего продукта.
        /// </summary>
        /// <param name="product">Существующий продукт для редактирования (null для создания нового).</param>
        public AddEditProductWindow(Product product = null)
        {
            InitializeComponent();
            Product = product ?? new Product();
            DataContext = Product;
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки "Сохранить".
        /// </summary>
        /// <remarks>
        /// Валидация включает:
        /// - Проверку положительного значения ID
        /// - Проверку положительной цены
        /// При успешной валидации устанавливает DialogResult = true
        /// </remarks>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIdProduct.Text) ||
                !int.TryParse(txtIdProduct.Text, out int idProduct) || idProduct <= 0)
            {
                MessageBox.Show("ID продукта должен быть положительным целым числом!", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPrice.Text) ||
                !double.TryParse(txtPrice.Text, out double price) || price <= 0)
            {
                MessageBox.Show("Цена должна быть положительным числом!", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Product.IdProduct = idProduct;
            Product.Price = price;
            DialogResult = true;
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки "Отмена".
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}