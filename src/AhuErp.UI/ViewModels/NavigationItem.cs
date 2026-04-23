using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AhuErp.UI.ViewModels
{
    /// <summary>
    /// Описывает один пункт навигационного меню главного окна. Реализован как
    /// <see cref="ObservableObject"/>, т.к. видимость пункта зависит от роли
    /// текущего пользователя и может обновляться после перелогина.
    /// </summary>
    public partial class NavigationItem : ObservableObject
    {
        public string Title { get; }

        public string ModuleKey { get; }

        public ViewModelBase ViewModel { get; }

        [ObservableProperty]
        private bool isAllowed;

        public NavigationItem(string title, string moduleKey, ViewModelBase viewModel)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Название пункта меню не может быть пустым.", nameof(title));
            if (string.IsNullOrWhiteSpace(moduleKey))
                throw new ArgumentException("ModuleKey не может быть пустым.", nameof(moduleKey));
            Title = title;
            ModuleKey = moduleKey;
            ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        }
    }
}
