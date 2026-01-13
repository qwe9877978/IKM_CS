using System.Text;
using Npgsql;
using System;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;

namespace IKMC
{
    /// <summary>
    /// Главное окно приложения управления базой данных IKMC.
    /// </summary>
    /// <remarks>
    /// Предоставляет многофункциональный пользовательский интерфейс для работы с четырьмя таблицами PostgreSQL:
    /// - Магазины (shop)
    /// - Продукты (product)
    /// - Заказы (orders)
    /// - Курьеры (courier)
    /// 
    /// Особенности реализации:
    /// 1. Асинхронная загрузка данных при старте для предотвращения блокировки UI
    /// 2. Динамическое создание индикатора загрузки и статусной панели
    /// 3. Восстановление данных при ошибках обновления
    /// 4. Подтверждение удаления критически важных записей
    /// 5. Централизованная обработка исключений с информативными сообщениями
    /// </remarks>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Репозитории для выполнения операций с таблицами базы данных.
        /// </summary>
        /// <remarks>
        /// Инициализируются при создании окна. Используют строку подключения из App.config.
        /// Каждый репозиторий соответствует определенной таблице в базе данных.
        /// </remarks>
        private readonly ShopRepository _shopRepo = new ShopRepository();
        private readonly ProductRepository _productRepo = new ProductRepository();
        private readonly OrderRepository _orderRepo = new OrderRepository();
        private readonly CourierRepository _courierRepo = new CourierRepository();

        /// <summary>
        /// Коллекции данных для привязки к элементам управления DataGrid.
        /// </summary>
        /// <remarks>
        /// Используются ObservableCollection для автоматического обновления UI при изменении данных.
        /// Сохраняются как поля класса для доступа к счетчикам записей в статусной панели.
        /// </remarks>
        private ObservableCollection<Shop> _shops;
        private ObservableCollection<Product> _products;
        private ObservableCollection<Order> _orders;
        private ObservableCollection<Courier> _couriers;


        /// <summary>
        /// Инициализирует главное окно приложения.
        /// </summary>
        /// <remarks>
        /// Выполняет следующие действия:
        /// 1. Инициализирует компоненты XAML через InitializeComponent()
        /// 2. Подписывается на событие Loaded для запуска загрузки данных
        /// </remarks>
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            //Unloaded += MainWindow_Unloaded;
        }

        /// <summary>
        /// Обработчик события загрузки окна.
        /// </summary>
        /// <param name="sender">Источник события (окно).</param>
        /// <param name="e">Данные события загрузки.</param>
        /// <remarks>
        /// Запускает асинхронную загрузку данных через Dispatcher.BeginInvoke для предотвращения блокировки UI-потока.
        /// Гарантирует, что интерфейс остается отзывчивым во время подключения к базе данных.
        /// </remarks>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Загружаем данные асинхронно, чтобы не блокировать UI
            Dispatcher.BeginInvoke(new Action(LoadAllData));
        }

        /// <summary>
        /// Загружает данные для всех таблиц базы данных.
        /// </summary>
        /// <remarks>
        /// Последовательно вызывает методы загрузки для каждой таблицы:
        /// 1. Показывает индикатор загрузки
        /// 2. Выполняет загрузку в следующем порядке: магазины, продукты, заказы, курьеры
        /// 3. Обновляет статусную панель с количеством записей
        /// 4. Скрывает индикатор загрузки при успешном выполнении
        /// 
        /// При ошибке:
        /// - Скрывает индикатор загрузки
        /// - Показывает детализированное сообщение об ошибке с рекомендациями по исправлению
        /// </remarks>
        /// <exception cref="System.Exception">Любое исключение при работе с базой данных будет перехвачено и отображено пользователю.</exception>
        private void LoadAllData()
        {
            try
            {
                // Показываем индикатор загрузки
                ShowLoadingIndicator(true);

                LoadShops();
                LoadProducts();
                LoadOrders();
                LoadCouriers();

                // Скрываем индикатор загрузки
                ShowLoadingIndicator(false);

                // Показываем статусную панель
                ShowStatusBar($"Данные успешно загружены. Всего: {_shops?.Count ?? 0} магазинов, {_products?.Count ?? 0} продуктов");
            }
            catch (System.Exception ex)
            {
                ShowLoadingIndicator(false);
                MessageBox.Show($"Ошибка загрузки данных:\n{ex.Message}\n\nПроверьте подключение к базе данных в App.config",
                    "Ошибка загрузки", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Отображает или скрывает индикатор загрузки.
        /// </summary>
        /// <param name="isVisible">true для отображения индикатора; false для скрытия.</param>
        /// <remarks>
        /// Динамически создает UI-элементы при первом вызове:
        /// - Полупрозрачный фон, блокирующий взаимодействие с интерфейсом
        /// - Центральное диалоговое окно с прогресс-баром
        /// - Текстовое сообщение "Загрузка данных..."
        /// 
        /// При повторных вызовах использует уже существующие элементы через FindName.
        /// Все элементы создаются программно без использования XAML.
        /// </remarks>
        private void ShowLoadingIndicator(bool isVisible)
        {
            // Добавляем Grid для индикатора загрузки поверх всего контента
            if (FindName("LoadingGrid") is Grid loadingGrid)
            {
                loadingGrid.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                var grid = new Grid
                {
                    Name = "LoadingGrid",
                    Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)),
                    Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed
                };

                var border = new Border
                {
                    Width = 150,
                    Height = 100,
                    Background = Brushes.White,
                    CornerRadius = new CornerRadius(10),
                    BorderThickness = new Thickness(1),
                    BorderBrush = Brushes.Gray,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                var stackPanel = new StackPanel { Margin = new Thickness(10) };
                stackPanel.Children.Add(new TextBlock { Text = "Загрузка данных...", FontSize = 14, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 10) });

                var progressBar = new ProgressBar
                {
                    IsIndeterminate = true,
                    Height = 20,
                    Width = 120
                };

                stackPanel.Children.Add(progressBar);
                border.Child = stackPanel;
                grid.Children.Add(border);

                var parentGrid = (Grid)Content;
                parentGrid.Children.Add(grid);
            }
        }

        /// <summary>
        /// Отображает статусную панель с информационным сообщением.
        /// </summary>
        /// <param name="message">Текст сообщения для отображения.</param>
        /// <remarks>
        /// Динамически создает StatusBar при первом вызове:
        /// - Добавляет новую строку в сетку основного контента
        /// - Размещает панель в нижней части окна
        /// - Регистрирует имя элемента для последующего доступа через FindName
        /// 
        /// При повторных вызовах обновляет содержимое существующей панели.
        /// Высота панели фиксирована (25 пикселей).
        /// </remarks>
        private void ShowStatusBar(string message)
        {
            if (FindName("StatusBar") is StatusBar statusBarElement)
            {
                ((StatusBarItem)statusBarElement.Items[0]).Content = message;
            }
            else
            {
                var statusBar = new StatusBar { Height = 25 };
                statusBar.Items.Add(new StatusBarItem { Content = message });

                var parentGrid = (Grid)Content;
                parentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(25) });
                Grid.SetRow(statusBar, parentGrid.RowDefinitions.Count - 1);
                parentGrid.Children.Add(statusBar);
                statusBar.Name = "StatusBar";
                RegisterName("StatusBar", statusBar);
            }
        }
        /// <summary>
        /// Загружает данные из таблицы shop и привязывает их к DataGrid.
        /// </summary>
        /// <remarks>
        /// Обрабатывает исключения при обращении к базе данных:
        /// - При ошибке показывает MessageBox с деталями проблемы
        /// - Не прерывает загрузку других таблиц
        /// - Сохраняет ссылку на коллекцию в поле _shops для использования в статусной панели
        /// </remarks>
        /// <exception cref="Npgsql.NpgsqlException">Возникает при ошибках подключения к PostgreSQL.</exception>
        private void LoadShops()
        {
            try
            {
                var shops = new ObservableCollection<Shop>(_shopRepo.GetAll());
                ShopsGrid.ItemsSource = shops;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки магазинов: {ex.Message}", "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Загружает данные из таблицы product и привязывает их к DataGrid.
        /// </summary>
        /// <remarks>
        /// Обрабатывает исключения при обращении к базе данных:
        /// - При ошибке показывает MessageBox с деталями проблемы
        /// - Не прерывает загрузку других таблиц
        /// - Сохраняет ссылку на коллекцию в поле _products для использования в статусной панели
        /// </remarks>
        /// <exception cref="Npgsql.NpgsqlException">Возникает при ошибках подключения к PostgreSQL.</exception>
        private void LoadProducts()
        {
            try
            {
                var products = new ObservableCollection<Product>(_productRepo.GetAll());
                ProductsGrid.ItemsSource = products;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки продуктов: {ex.Message}", "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Загружает данные из таблицы order и привязывает их к DataGrid.
        /// </summary>
        /// <remarks>
        /// Обрабатывает исключения при обращении к базе данных:
        /// - При ошибке показывает MessageBox с деталями проблемы
        /// - Не прерывает загрузку других таблиц
        /// - Сохраняет ссылку на коллекцию в поле _orders для использования в статусной панели
        /// </remarks>
        /// <exception cref="Npgsql.NpgsqlException">Возникает при ошибках подключения к PostgreSQL.</exception>
        private void LoadOrders()
        {
            try
            {
                var orders = new ObservableCollection<Order>(_orderRepo.GetAll());
                OrdersGrid.ItemsSource = orders;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}", "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Загружает данные из таблицы courier и привязывает их к DataGrid.
        /// </summary>
        /// <remarks>
        /// Обрабатывает исключения при обращении к базе данных:
        /// - При ошибке показывает MessageBox с деталями проблемы
        /// - Не прерывает загрузку других таблиц
        /// - Сохраняет ссылку на коллекцию в поле _couriers для использования в статусной панели
        /// </remarks>
        /// <exception cref="Npgsql.NpgsqlException">Возникает при ошибках подключения к PostgreSQL.</exception>
        private void LoadCouriers()
        {
            try
            {
                var couriers = new ObservableCollection<Courier>(_courierRepo.GetAll());
                CouriersGrid.ItemsSource = couriers;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки курьеров: {ex.Message}", "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        //Магазины

        /// <summary>
        /// Обрабатывает добавление нового магазина.
        /// </summary>
        /// <param name="sender">Кнопка "Добавить".</param>
        /// <param name="e">Данные события клика.</param>
        /// <remarks>
        /// Алгоритм работы:
        /// 1. Открывает диалоговое окно AddEditShopWindow
        /// 2. При подтверждении сохраняет данные через репозиторий
        /// 3. При успешном добавлении:
        ///    - Показывает сообщение об успехе
        ///    - Перезагружает данные таблицы
        /// 4. При ошибке показывает детализированное сообщение с причиной
        /// 
        /// Особенность: Не выполняет дополнительной валидации, так как она реализована в диалоговом окне.
        /// </remarks>
        /// <exception cref="Npgsql.NpgsqlException">Возникает при нарушении ограничений базы данных (дубликат ID, внешние ключи).</exception>
        private void AddShop_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditShopWindow();
            if (window.ShowDialog() == true)
            {
                try
                {
                    _shopRepo.Add(window.Shop);
                    MessageBox.Show("Магазин успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadShops();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления магазина: {ex.Message}", "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Обрабатывает редактирование существующего магазина.
        /// </summary>
        /// <param name="sender">Кнопка "Редактировать".</param>
        /// <param name="e">Данные события клика.</param>
        /// <remarks>
        /// Алгоритм работы:
        /// 1. Проверяет выбор записи в таблице
        /// 2. Создает копию оригинальных данных для восстановления при ошибке
        /// 3. Открывает диалоговое окно с предзаполненными данными
        /// 4. При успешном обновлении:
        ///    - Показывает сообщение об успехе
        ///    - Перезагружает данные
        /// 5. При ошибке:
        ///    - Показывает сообщение с деталями
        ///    - Восстанавливает оригинальные значения в UI
        /// 
        /// Критически важная особенность: Механизм отката изменений в UI при ошибках обновления.
        /// </remarks>
        private void EditShop_Click(object sender, RoutedEventArgs e)
        {
            if (ShopsGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите магазин для редактирования!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selected = (Shop)ShopsGrid.SelectedItem;
            var originalShop = new Shop
            {
                IdShop = selected.IdShop,
                RateShop = selected.RateShop,
                IdProduct = selected.IdProduct
            };

            var window = new AddEditShopWindow(originalShop);
            if (window.ShowDialog() == true)
            {
                try
                {
                    _shopRepo.Update(window.Shop);
                    MessageBox.Show("Магазин успешно обновлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadShops();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления магазина: {ex.Message}", "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
                    // Возврат к исходным значениям в случае ошибки
                    selected.RateShop = originalShop.RateShop;
                    selected.IdProduct = originalShop.IdProduct;
                }
            }
        }

        /// <summary>
        /// Обрабатывает удаление выбранного магазина.
        /// </summary>
        /// <param name="sender">Кнопка "Удалить".</param>
        /// <param name="e">Данные события клика.</param>
        /// <remarks>
        /// Алгоритм работы:
        /// 1. Проверяет выбор записи в таблице
        /// 2. Запрашивает подтверждение удаления через MessageBox
        /// 3. При подтверждении пытается удалить запись через репозиторий
        /// 4. При успехе:
        ///    - Показывает сообщение об успехе
        ///    - Перезагружает данные
        /// 5. При ошибке показывает сообщение с возможными причинами:
        ///    - Нарушение внешних ключей (на магазин ссылаются заказы)
        ///    - Проблемы с подключением к базе данных
        /// 
        /// Безопасность: Использует двухэтапное подтверждение для предотвращения случайного удаления.
        /// </remarks>
        /// <exception cref="Npgsql.PostgresException">Возникает при нарушении ограничений внешних ключей.</exception>
        private void DeleteShop_Click(object sender, RoutedEventArgs e)
        {
            if (ShopsGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите магазин для удаления!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selected = (Shop)ShopsGrid.SelectedItem;
            var result = MessageBox.Show($"Вы уверены, что хотите удалить магазин с ID {selected.IdShop}?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _shopRepo.Delete(selected.IdShop);
                    MessageBox.Show("Магазин успешно удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadShops();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления магазина: {ex.Message}\n\nВозможно, на этот магазин ссылаются другие записи.",
                        "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Продукты

        /// <summary>
        /// Обрабатывает добавление нового магазина.
        /// </summary>
        /// <param name="sender">Кнопка "Добавить".</param>
        /// <param name="e">Данные события клика.</param>
        /// <remarks>
        /// Алгоритм работы:
        /// 1. Открывает диалоговое окно AddEditProductWindow
        /// 2. При подтверждении сохраняет данные через репозиторий
        /// 3. При успешном добавлении:
        ///    - Показывает сообщение об успехе
        ///    - Перезагружает данные таблицы
        /// 4. При ошибке показывает детализированное сообщение с причиной
        /// 
        /// Особенность: Не выполняет дополнительной валидации, так как она реализована в диалоговом окне.
        /// </remarks>
        /// <exception cref="Npgsql.NpgsqlException">Возникает при нарушении ограничений базы данных (дубликат ID, внешние ключи).</exception>
        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditProductWindow();
            if (window.ShowDialog() == true)
            {
                try
                {
                    _productRepo.Add(window.Product);
                    MessageBox.Show("Продукт успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadProducts();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления продукта: {ex.Message}", "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Обрабатывает редактирование существующего товара.
        /// </summary>
        /// <param name="sender">Кнопка "Редактировать".</param>
        /// <param name="e">Данные события клика.</param>
        /// <remarks>
        /// Алгоритм работы:
        /// 1. Проверяет выбор записи в таблице
        /// 2. Создает копию оригинальных данных для восстановления при ошибке
        /// 3. Открывает диалоговое окно с предзаполненными данными
        /// 4. При успешном обновлении:
        ///    - Показывает сообщение об успехе
        ///    - Перезагружает данные
        /// 5. При ошибке:
        ///    - Показывает сообщение с деталями
        ///    - Восстанавливает оригинальные значения в UI
        /// 
        /// Критически важная особенность: Механизм отката изменений в UI при ошибках обновления.
        /// </remarks>
        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите продукт для редактирования!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selected = (Product)ProductsGrid.SelectedItem;
            var originalProduct = new Product
            {
                IdProduct = selected.IdProduct,
                Price = selected.Price
            };

            var window = new AddEditProductWindow(originalProduct);
            if (window.ShowDialog() == true)
            {
                try
                {
                    _productRepo.Update(window.Product);
                    MessageBox.Show("Продукт успешно обновлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadProducts();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления продукта: {ex.Message}", "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
                    selected.Price = originalProduct.Price;
                }
            }
        }

        /// <summary>
        /// Обрабатывает удаление выбранного товара.
        /// </summary>
        /// <param name="sender">Кнопка "Удалить".</param>
        /// <param name="e">Данные события клика.</param>
        /// <remarks>
        /// Алгоритм работы:
        /// 1. Проверяет выбор записи в таблице
        /// 2. Запрашивает подтверждение удаления через MessageBox
        /// 3. При подтверждении пытается удалить запись через репозиторий
        /// 4. При успехе:
        ///    - Показывает сообщение об успехе
        ///    - Перезагружает данные
        /// 5. При ошибке показывает сообщение с возможными причинами:
        ///    - Нарушение внешних ключей (на магазин ссылаются заказы)
        ///    - Проблемы с подключением к базе данных
        /// 
        /// Безопасность: Использует двухэтапное подтверждение для предотвращения случайного удаления.
        /// </remarks>
        /// <exception cref="Npgsql.PostgresException">Возникает при нарушении ограничений внешних ключей.</exception>
        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите продукт для удаления!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selected = (Product)ProductsGrid.SelectedItem;
            var result = MessageBox.Show($"Вы уверены, что хотите удалить продукт с ID {selected.IdProduct}?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _productRepo.Delete(selected.IdProduct);
                    MessageBox.Show("Продукт успешно удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadProducts();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления продукта: {ex.Message}\n\nВозможно, этот продукт используется в других таблицах.",
                        "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Заказы

        /// <summary>
        /// Обрабатывает добавление нового магазина.
        /// </summary>
        /// <param name="sender">Кнопка "Добавить".</param>
        /// <param name="e">Данные события клика.</param>
        /// <remarks>
        /// Алгоритм работы:
        /// 1. Открывает диалоговое окно AddEditOrderWindow
        /// 2. При подтверждении сохраняет данные через репозиторий
        /// 3. При успешном добавлении:
        ///    - Показывает сообщение об успехе
        ///    - Перезагружает данные таблицы
        /// 4. При ошибке показывает детализированное сообщение с причиной
        /// 
        /// Особенность: Не выполняет дополнительной валидации, так как она реализована в диалоговом окне.
        /// </remarks>
        /// <exception cref="Npgsql.NpgsqlException">Возникает при нарушении ограничений базы данных (дубликат ID, внешние ключи).</exception>
        private void AddOrder_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditOrderWindow();
            if (window.ShowDialog() == true)
            {
                try
                {
                    _orderRepo.Add(window.Order);
                    MessageBox.Show("Заказ успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadOrders();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления заказа: {ex.Message}", "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Обрабатывает редактирование существующего заказа.
        /// </summary>
        /// <param name="sender">Кнопка "Редактировать".</param>
        /// <param name="e">Данные события клика.</param>
        /// <remarks>
        /// Алгоритм работы:
        /// 1. Проверяет выбор записи в таблице
        /// 2. Создает копию оригинальных данных для восстановления при ошибке
        /// 3. Открывает диалоговое окно с предзаполненными данными
        /// 4. При успешном обновлении:
        ///    - Показывает сообщение об успехе
        ///    - Перезагружает данные
        /// 5. При ошибке:
        ///    - Показывает сообщение с деталями
        ///    - Восстанавливает оригинальные значения в UI
        /// 
        /// Критически важная особенность: Механизм отката изменений в UI при ошибках обновления.
        /// </remarks>
        private void EditOrder_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите заказ для редактирования!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selected = (Order)OrdersGrid.SelectedItem;
            var originalOrder = new Order
            {
                IdOrder = selected.IdOrder,
                IdClient = selected.IdClient,
                IdShop = selected.IdShop,
                SummOrder = selected.SummOrder,
                Status = selected.Status,
                DateCreateOrder = selected.DateCreateOrder,
                TimeCreate = selected.TimeCreate,
                IdCourier = selected.IdCourier
            };

            var window = new AddEditOrderWindow(originalOrder);
            if (window.ShowDialog() == true)
            {
                try
                {
                    _orderRepo.Update(window.Order);
                    MessageBox.Show("Заказ успешно обновлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadOrders();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления заказа: {ex.Message}", "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
                    // Возврат к исходным значениям
                    selected.IdClient = originalOrder.IdClient;
                    selected.IdShop = originalOrder.IdShop;
                    selected.SummOrder = originalOrder.SummOrder;
                    selected.Status = originalOrder.Status;
                    selected.DateCreateOrder = originalOrder.DateCreateOrder;
                    selected.TimeCreate = originalOrder.TimeCreate;
                    selected.IdCourier = originalOrder.IdCourier;
                }
            }
        }

        /// <summary>
        /// Обрабатывает удаление выбранного заказа.
        /// </summary>
        /// <param name="sender">Кнопка "Удалить".</param>
        /// <param name="e">Данные события клика.</param>
        /// <remarks>
        /// Алгоритм работы:
        /// 1. Проверяет выбор записи в таблице
        /// 2. Запрашивает подтверждение удаления через MessageBox
        /// 3. При подтверждении пытается удалить запись через репозиторий
        /// 4. При успехе:
        ///    - Показывает сообщение об успехе
        ///    - Перезагружает данные
        /// 5. При ошибке показывает сообщение с возможными причинами:
        ///    - Нарушение внешних ключей (на магазин ссылаются заказы)
        ///    - Проблемы с подключением к базе данных
        /// 
        /// Безопасность: Использует двухэтапное подтверждение для предотвращения случайного удаления.
        /// </remarks>
        /// <exception cref="Npgsql.PostgresException">Возникает при нарушении ограничений внешних ключей.</exception>
        private void DeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите заказ для удаления!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selected = (Order)OrdersGrid.SelectedItem;
            var result = MessageBox.Show($"Вы уверены, что хотите удалить заказ с ID {selected.IdOrder}?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _orderRepo.Delete(selected.IdOrder);
                    MessageBox.Show("Заказ успешно удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadOrders();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления заказа: {ex.Message}", "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Курьеры

        /// <summary>
        /// Обрабатывает добавление нового магазина.
        /// </summary>
        /// <param name="sender">Кнопка "Добавить".</param>
        /// <param name="e">Данные события клика.</param>
        /// <remarks>
        /// Алгоритм работы:
        /// 1. Открывает диалоговое окно AddEditCourierWindow
        /// 2. При подтверждении сохраняет данные через репозиторий
        /// 3. При успешном добавлении:
        ///    - Показывает сообщение об успехе
        ///    - Перезагружает данные таблицы
        /// 4. При ошибке показывает детализированное сообщение с причиной
        /// 
        /// Особенность: Не выполняет дополнительной валидации, так как она реализована в диалоговом окне.
        /// </remarks>
        /// <exception cref="Npgsql.NpgsqlException">Возникает при нарушении ограничений базы данных (дубликат ID, внешние ключи).</exception>
        private void AddCourier_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditCourierWindow();
            if (window.ShowDialog() == true)
            {
                try
                {
                    _courierRepo.Add(window.Courier);
                    MessageBox.Show("Курьер успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadCouriers();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления курьера: {ex.Message}", "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Обрабатывает редактирование существующего курьера.
        /// </summary>
        /// <param name="sender">Кнопка "Редактировать".</param>
        /// <param name="e">Данные события клика.</param>
        /// <remarks>
        /// Алгоритм работы:
        /// 1. Проверяет выбор записи в таблице
        /// 2. Создает копию оригинальных данных для восстановления при ошибке
        /// 3. Открывает диалоговое окно с предзаполненными данными
        /// 4. При успешном обновлении:
        ///    - Показывает сообщение об успехе
        ///    - Перезагружает данные
        /// 5. При ошибке:
        ///    - Показывает сообщение с деталями
        ///    - Восстанавливает оригинальные значения в UI
        /// 
        /// Критически важная особенность: Механизм отката изменений в UI при ошибках обновления.
        /// </remarks>
        private void EditCourier_Click(object sender, RoutedEventArgs e)
        {
            if (CouriersGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите курьера для редактирования!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selected = (Courier)CouriersGrid.SelectedItem;
            var originalCourier = new Courier
            {
                IdCourier = selected.IdCourier,
                RateCourier = selected.RateCourier
            };

            var window = new AddEditCourierWindow(originalCourier);
            if (window.ShowDialog() == true)
            {
                try
                {
                    _courierRepo.Update(window.Courier);
                    MessageBox.Show("Курьер успешно обновлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadCouriers();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления курьера: {ex.Message}", "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
                    selected.RateCourier = originalCourier.RateCourier;
                }
            }
        }

        /// <summary>
        /// Обрабатывает удаление выбранного курьера.
        /// </summary>
        /// <param name="sender">Кнопка "Удалить".</param>
        /// <param name="e">Данные события клика.</param>
        /// <remarks>
        /// Алгоритм работы:
        /// 1. Проверяет выбор записи в таблице
        /// 2. Запрашивает подтверждение удаления через MessageBox
        /// 3. При подтверждении пытается удалить запись через репозиторий
        /// 4. При успехе:
        ///    - Показывает сообщение об успехе
        ///    - Перезагружает данные
        /// 5. При ошибке показывает сообщение с возможными причинами:
        ///    - Нарушение внешних ключей (на магазин ссылаются заказы)
        ///    - Проблемы с подключением к базе данных
        /// 
        /// Безопасность: Использует двухэтапное подтверждение для предотвращения случайного удаления.
        /// </remarks>
        /// <exception cref="Npgsql.PostgresException">Возникает при нарушении ограничений внешних ключей.</exception>
        private void DeleteCourier_Click(object sender, RoutedEventArgs e)
        {
            if (CouriersGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите курьера для удаления!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selected = (Courier)CouriersGrid.SelectedItem;
            var result = MessageBox.Show($"Вы уверены, что хотите удалить курьера с ID {selected.IdCourier}?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _courierRepo.Delete(selected.IdCourier);
                    MessageBox.Show("Курьер успешно удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadCouriers();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления курьера: {ex.Message}\n\nВозможно, на этого курьера ссылаются другие записи.",
                        "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}