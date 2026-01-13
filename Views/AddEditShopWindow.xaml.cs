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
    /// Окно для добавления новых и редактирования существующих магазинов.
    /// </summary>
    /// <remarks>
    /// Обеспечивает валидацию вводимых данных и проверку существования связанного продукта.
    /// Содержит три текстовых поля для ID магазина, рейтинга и ID продукта.
    /// </remarks>
    public partial class AddEditShopWindow : Window
    {
        /// <summary>
        /// Объект Shop, содержащий данные для сохранения.
        /// </summary>
        public Shop Shop { get; private set; }
        private readonly ShopRepository _repo = new ShopRepository();

        /// <summary>
        /// Инициализирует окно для добавления нового или редактирования существующего магазина.
        /// </summary>
        /// <param name="shop">Существующий магазин для редактирования (null для создания нового).</param>
        public AddEditShopWindow(Shop shop = null)
        {
            InitializeComponent();
            Shop = shop ?? new Shop();
            DataContext = Shop;
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки "Сохранить".
        /// </summary>
        /// <remarks>
        /// Выполняет комплексную валидацию:
        /// 1. Проверка формата и диапазона числовых значений
        /// 2. Проверка существования продукта с указанным ID
        /// При успешной валидации устанавливает DialogResult = true
        /// </remarks>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIdShop.Text) ||
                !int.TryParse(txtIdShop.Text, out int idShop) || idShop <= 0)
            {
                MessageBox.Show("ID магазина должен быть положительным целым числом!", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtRate.Text) ||
                !double.TryParse(txtRate.Text, out double rate) || rate < 0 || rate > 5)
            {
                MessageBox.Show("Рейтинг должен быть числом от 0 до 5!", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtIdProduct.Text) ||
                !int.TryParse(txtIdProduct.Text, out int idProduct) || idProduct <= 0)
            {
                MessageBox.Show("ID продукта должен быть положительным целым числом!", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!_repo.ProductExists(idProduct))
            {
                MessageBox.Show($"Продукт с ID {idProduct} не существует в базе данных!", "Ошибка внешнего ключа", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Shop.IdShop = idShop;
            Shop.RateShop = rate;
            Shop.IdProduct = idProduct;
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
