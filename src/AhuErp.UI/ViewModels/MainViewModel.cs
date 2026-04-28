using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using AhuErp.Core.Services;
using AhuErp.UI.Converters;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AhuErp.UI.ViewModels
{
    /// <summary>
    /// Корневая ViewModel. Содержит список пунктов навигации, фильтрует их
    /// по роли текущего пользователя (<see cref="RolePolicy"/>) и управляет
    /// активной подстраницей <see cref="CurrentViewModel"/>.
    /// </summary>
    public partial class MainViewModel : ViewModelBase
    {
        private readonly IAuthService _auth;
        private readonly INotificationService _notifications;
        private readonly DispatcherTimer _notificationTimer;

        public ObservableCollection<NavigationItem> NavigationItems { get; }

        public ObservableCollection<Notification> Notifications { get; } = new ObservableCollection<Notification>();

        [ObservableProperty]
        private NavigationItem selectedNavigationItem;

        [ObservableProperty]
        private ViewModelBase currentViewModel;

        [ObservableProperty]
        private string currentUserDisplayName;

        [ObservableProperty]
        private string currentUserRoleDisplayName;

        [ObservableProperty]
        private int unreadNotificationCount;

        [ObservableProperty]
        private bool isNotificationsOpen;

        public MainViewModel(IAuthService auth,
                             INotificationService notifications,
                             DashboardViewModel dashboardVm,
                             OfficeViewModel officeVm,
                             RkkViewModel rkkVm,
                             ArchiveViewModel archiveVm,
                             ItServiceViewModel itServiceVm,
                             FleetViewModel fleetVm,
                             WarehouseViewModel warehouseVm,
                             MyTasksViewModel myTasksVm,
                             NomenclatureViewModel nomenclatureVm,
                             AuditJournalViewModel auditJournalVm,
                             JournalViewModel journalVm,
                             SearchViewModel searchVm)
        {
            _auth = auth ?? throw new ArgumentNullException(nameof(auth));
            _notifications = notifications ?? throw new ArgumentNullException(nameof(notifications));

            NavigationItems = new ObservableCollection<NavigationItem>
            {
                new NavigationItem("Дашборд",    RolePolicy.Dashboard, dashboardVm),
                new NavigationItem("РКК (документы)", RolePolicy.Office, rkkVm),
                new NavigationItem("Документационное обеспечение", RolePolicy.Office,    officeVm),
                new NavigationItem("Мои задачи",  RolePolicy.MyTasks,   myTasksVm),
                new NavigationItem("Архивный отдел", RolePolicy.Archive, archiveVm),
                new NavigationItem("Склад / ТМЦ", RolePolicy.Warehouse, warehouseVm),
                new NavigationItem("ИТО",        RolePolicy.ItService, itServiceVm),
                new NavigationItem("Транспорт",  RolePolicy.Fleet,     fleetVm),
                new NavigationItem("Номенклатура дел", RolePolicy.Nomenclature, nomenclatureVm),
                new NavigationItem("Журналы регистрации", RolePolicy.Journals, journalVm),
                new NavigationItem("Поиск", RolePolicy.Search, searchVm),
                new NavigationItem("Журнал аудита", RolePolicy.AuditJournal, auditJournalVm),
            };

            ApplyRolePolicy();

            // Выбираем первый доступный пункт.
            foreach (var item in NavigationItems)
            {
                if (item.IsAllowed)
                {
                    SelectedNavigationItem = item;
                    break;
                }
            }

            // Лента уведомлений: подписываемся на изменения и заводим
            // DispatcherTimer на 60 секунд для автоматического Refresh.
            _notifications.Changed += OnNotificationsChanged;
            _notificationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(60)
            };
            _notificationTimer.Tick += (s, e) => SafeRefreshNotifications();
            _notificationTimer.Start();
            SafeRefreshNotifications();
        }

        private void OnNotificationsChanged(object sender, EventArgs e)
        {
            UnreadNotificationCount = _notifications.UnreadCount;
            Notifications.Clear();
            foreach (var n in _notifications.ListCurrent()) Notifications.Add(n);
        }

        private void SafeRefreshNotifications()
        {
            // Не падаем если репозитории не готовы (например, без логина).
            try { _notifications.Refresh(); }
            catch { /* Лента уведомлений не должна валить main loop. */ }
        }

        // Хук [ObservableProperty] от CommunityToolkit.Mvvm: вызывается при
        // изменении IsNotificationsOpen. ToggleButton bell-icon биндится
        // напрямую к IsNotificationsOpen (Mode=TwoWay), поэтому отдельная
        // ToggleNotificationsCommand была мёртвым кодом — а пользователю
        // важно видеть свежий список в момент открытия попапа, не ждать
        // следующего тика DispatcherTimer (≤60 секунд).
        partial void OnIsNotificationsOpenChanged(bool value)
        {
            if (value) SafeRefreshNotifications();
        }

        [RelayCommand]
        private void MarkAllNotificationsRead()
        {
            _notifications.MarkAllRead();
        }

        partial void OnSelectedNavigationItemChanged(NavigationItem value)
        {
            CurrentViewModel = value?.ViewModel;
        }

        [RelayCommand]
        private void NavigateTo(NavigationItem item)
        {
            if (item != null && item.IsAllowed) SelectedNavigationItem = item;
        }

        [RelayCommand]
        private void Logout()
        {
            _auth.Logout();
            System.Windows.Application.Current.Shutdown();
        }

        private void ApplyRolePolicy()
        {
            var employee = _auth.CurrentEmployee;
            if (employee == null)
            {
                foreach (var item in NavigationItems) item.IsAllowed = false;
                CurrentUserDisplayName = null;
                CurrentUserRoleDisplayName = null;
                return;
            }

            foreach (var item in NavigationItems)
            {
                item.IsAllowed = RolePolicy.IsAllowed(employee.Role, item.ModuleKey);
            }

            CurrentUserDisplayName = employee.FullName;
            CurrentUserRoleDisplayName = EnumDisplayConverter.Translate(employee.Role);
        }
    }
}
