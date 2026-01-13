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
    /// Окно для добавления новых и редактирования существующих курьеров.
    /// </summary>
    /// <remarks>
    /// Особенность: поле ID курьера скрыто, так как генерируется автоматически в базе данных.
    /// Содержит только поле для ввода рейтинга курьера.
    /// </remarks>
    public partial class AddEditCourierWindow : Window
    {
        /// <summary>
        /// Объект Courier, содержащий данные для сохранения.
        /// </summary>
        public Courier Courier { get; private set; }

        /// <summary>
        /// Инициализирует окно для добавления нового или редактирования существующего курьера.
        /// </summary>
        /// <param name="courier">Существующий курьер для редактирования (null для создания нового).</param>
        public AddEditCourierWindow(Courier courier = null)
        {
            InitializeComponent();
            Courier = courier ?? new Courier();
            DataContext = Courier;
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки "Сохранить".
        /// </summary>
        /// <remarks>
        /// Валидация включает проверку рейтинга в диапазоне от 0 до 5.
        /// При успешной валидации устанавливает DialogResult = true
        /// </remarks>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtRate.Text) ||
                !double.TryParse(txtRate.Text, out double rate) || rate < 0 || rate > 5)
            {
                MessageBox.Show("Рейтинг должен быть числом от 0 до 5!", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Courier.RateCourier = rate;
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