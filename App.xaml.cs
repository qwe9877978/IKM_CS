using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading;

namespace IKMC
{
    /// <summary>
    /// Класс точки входа WPF-приложения IKMC Database Manager.
    /// </summary>
    /// <remarks>
    /// Обеспечивает:
    /// - Глобальную обработку необработанных исключений
    /// - Проверку подключения к базе данных при запуске
    /// - Централизованное отображение критических ошибок
    /// Является основным обработчиком событий уровня приложения.
    /// </remarks>
    public partial class App : Application
    {
        /// <summary>
        /// Переопределенный метод запуска приложения.
        /// </summary>
        /// <param name="e">Аргументы события запуска приложения.</param>
        /// <remarks>
        /// Выполняет следующие действия при старте:
        /// 1. Регистрирует обработчики глобальных исключений
        /// 2. Проверяет подключение к базе данных PostgreSQL через тестовый запрос
        /// 3. При возникновении ошибки показывает диалоговое окно и завершает приложение
        /// </remarks>
        /// <exception cref="Exception">Любое исключение при проверке подключения к базе данных приведет к завершению приложения.</exception>
        protected override void OnStartup(StartupEventArgs e)
        {
            // Обработчик необработанных исключений
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            try
            {
                // Проверка подключения к базе данных при старте
                var testRepo = new ShopRepository();
                var testResult = testRepo.GetAll();

                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                ShowErrorDialog($"Критическая ошибка при запуске приложения:\n\n{ex.Message}\n\nПроверьте:\n1. Запущен ли сервер PostgreSQL\n2. Правильность строки подключения в App.config\n3. Существует ли база данных my_db", "Ошибка запуска");
                Shutdown();
            }
        }

        /// <summary>
        /// Обработчик необработанных исключений на уровне домена приложения.
        /// </summary>
        /// <param name="sender">Источник события (AppDomain).</param>
        /// <param name="e">Данные события, содержащие информацию об исключении.</param>
        /// <remarks>
        /// Перехватывает исключения, возникающие вне UI-потока (фоновые операции, таймеры и т.д.).
        /// Отображает ошибку через централизованный диалог и завершает приложение.
        /// </remarks>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            ShowErrorDialog($"Необработанное исключение в приложении:\n\n{ex?.Message}", "Критическая ошибка");
        }

        /// <summary>
        /// Обработчик необработанных исключений в диспетчере WPF (UI-поток).
        /// </summary>
        /// <param name="sender">Источник события (Dispatcher).</param>
        /// <param name="e">Данные события с информацией об исключении.</param>
        /// <remarks>
        /// Перехватывает исключения в пользовательском интерфейсе (события кнопок, привязки данных и т.д.).
        /// Помечает исключение как обработанное (e.Handled = true), чтобы предотвратить аварийное завершение.
        /// Отображает ошибку через централизованный диалог.
        /// </remarks>
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ShowErrorDialog($"Ошибка в графическом интерфейсе:\n\n{e.Exception.Message}", "Ошибка интерфейса");
            e.Handled = true;
        }

        /// <summary>
        /// Отображает модальное диалоговое окно с сообщением об ошибке.
        /// </summary>
        /// <param name="message">Текст сообщения для отображения.</param>
        /// <param name="title">Заголовок диалогового окна.</param>
        /// <remarks>
        /// Использует стандартный MessageBox с иконкой ошибки.
        /// Является централизованным методом для всех отображений ошибок в приложении.
        /// </remarks>
        private void ShowErrorDialog(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
