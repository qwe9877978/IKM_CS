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
    /// Окно для добавления новых и редактирования существующих заказов.
    /// </summary>
    /// <remarks>
    /// Содержит расширенную валидацию всех полей, включая:
    /// - Проверку внешних ключей (магазин, курьер)
    /// - Валидацию формата даты и диапазона времени
    /// - Специальную обработку статуса через ComboBox
    /// - Время создания хранится в секундах с начала дня (0-86400)
    /// </remarks>
    public partial class AddEditOrderWindow : Window
    {
        /// <summary>
        /// Объект Order, содержащий данные для сохранения.
        /// </summary>
        public Order Order { get; private set; }
        private readonly OrderRepository _repo = new OrderRepository();

        /// <summary>
        /// Инициализирует окно для добавления нового или редактирования существующего заказа.
        /// </summary>
        /// <param name="order">Существующий заказ для редактирования (null для создания нового).</param>
        public AddEditOrderWindow(Order order = null)
        {
            InitializeComponent();
            Order = order ?? new Order { DateCreateOrder = DateTime.Today };
            DataContext = Order;

            // Устанавливаем выбранный статус в ComboBox
            if (!string.IsNullOrWhiteSpace(Order.Status))
            {
                foreach (ComboBoxItem item in cmbStatus.Items)
                {
                    if (item.Content.ToString() == Order.Status)
                    {
                        cmbStatus.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки "Сохранить".
        /// </summary>
        /// <remarks>
        /// Выполняет комплексную валидацию:
        /// 1. Проверка формата и диапазона всех числовых полей
        /// 2. Проверка существования связанных сущностей (магазин, курьер)
        /// 3. Валидация формата даты (ГГГГ-ММ-ДД)
        /// 4. Корректная обработка статуса из ComboBox
        /// При успешной валидации устанавливает DialogResult = true
        /// </remarks>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Валидация ID клиента
            if (string.IsNullOrWhiteSpace(txtIdClient.Text) ||
                !int.TryParse(txtIdClient.Text, out int idClient) || idClient <= 0)
            {
                MessageBox.Show("ID клиента должен быть положительным целым числом!", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Валидация ID магазина
            if (string.IsNullOrWhiteSpace(txtIdShop.Text) ||
                !int.TryParse(txtIdShop.Text, out int idShop) || idShop <= 0)
            {
                MessageBox.Show("ID магазина должен быть положительным целым числом!", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!_repo.ShopExists(idShop))
            {
                MessageBox.Show($"Магазин с ID {idShop} не существует в базе данных!", "Ошибка внешнего ключа", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Валидация суммы заказа
            if (string.IsNullOrWhiteSpace(txtSumm.Text) ||
                !long.TryParse(txtSumm.Text, out long summ) || summ <= 0)
            {
                MessageBox.Show("Сумма заказа должна быть положительным числом!", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка статуса через ComboBoxItem
            if (cmbStatus.SelectedItem is not ComboBoxItem selectedItem ||
                string.IsNullOrWhiteSpace(selectedItem.Content?.ToString()))
            {
                MessageBox.Show("Выберите корректный статус заказа из списка!", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Присваиваем строковое значение статуса
            Order.Status = selectedItem.Content.ToString();

            // Валидация даты
            if (string.IsNullOrWhiteSpace(txtDate.Text) ||
                !DateTime.TryParseExact(txtDate.Text, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime date))
            {
                MessageBox.Show("Дата должна быть в формате ГГГГ-ММ-ДД!", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Валидация времени
            if (string.IsNullOrWhiteSpace(txtTime.Text) ||
                !int.TryParse(txtTime.Text, out int time) || time < 0 || time > 86400)
            {
                MessageBox.Show("Время должно быть в диапазоне от 0 до 86400 секунд!", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Валидация ID курьера
            if (string.IsNullOrWhiteSpace(txtIdCourier.Text) ||
                !int.TryParse(txtIdCourier.Text, out int idCourier) || idCourier <= 0)
            {
                MessageBox.Show("ID курьера должен быть положительным целым числом!", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!_repo.CourierExists(idCourier))
            {
                MessageBox.Show($"Курьер с ID {idCourier} не существует в базе данных!", "Ошибка внешнего ключа", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Присваивание значений
            Order.IdClient = idClient;
            Order.IdShop = idShop;
            Order.SummOrder = summ;
            Order.DateCreateOrder = date;
            Order.TimeCreate = time;
            Order.IdCourier = idCourier;

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
