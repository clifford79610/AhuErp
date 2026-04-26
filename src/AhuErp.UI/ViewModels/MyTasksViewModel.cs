using System;
using System.Collections.ObjectModel;
using System.Linq;
using AhuErp.Core.Models;
using AhuErp.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AhuErp.UI.ViewModels
{
    /// <summary>
    /// Раздел «Мои задачи / На контроле»: показывает поручения, в которых
    /// текущий пользователь является исполнителем, контролёром или автором.
    /// Поддерживает фильтр по роли и быстрое изменение статуса.
    /// </summary>
    public partial class MyTasksViewModel : ViewModelBase
    {
        private readonly ITaskService _tasks;
        private readonly IAuthService _auth;

        public MyTasksScope[] AvailableScopes { get; } =
            (MyTasksScope[])Enum.GetValues(typeof(MyTasksScope));

        public DocumentTaskStatus[] AvailableStatuses { get; } =
            (DocumentTaskStatus[])Enum.GetValues(typeof(DocumentTaskStatus));

        public ObservableCollection<DocumentTask> Tasks { get; }
            = new ObservableCollection<DocumentTask>();

        [ObservableProperty]
        private MyTasksScope selectedScope = MyTasksScope.AsExecutor;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(MarkInProgressCommand))]
        [NotifyCanExecuteChangedFor(nameof(MarkCompletedCommand))]
        private DocumentTask selectedTask;

        [ObservableProperty]
        private string reportText;

        [ObservableProperty]
        private string errorMessage;

        public MyTasksViewModel(ITaskService tasks, IAuthService auth)
        {
            _tasks = tasks ?? throw new ArgumentNullException(nameof(tasks));
            _auth = auth ?? throw new ArgumentNullException(nameof(auth));
            Reload();
        }

        partial void OnSelectedScopeChanged(MyTasksScope value) => Reload();

        [RelayCommand]
        private void Reload()
        {
            ErrorMessage = null;
            Tasks.Clear();
            var employee = _auth.CurrentEmployee;
            if (employee == null) return;
            var list = _tasks.ListMyTasks(employee.Id, SelectedScope)
                             .OrderBy(t => t.Status == DocumentTaskStatus.Completed ? 1 : 0)
                             .ThenBy(t => t.Deadline);
            foreach (var t in list) Tasks.Add(t);
        }

        [RelayCommand(CanExecute = nameof(HasSelection))]
        private void MarkInProgress() => ChangeStatus(DocumentTaskStatus.InProgress);

        [RelayCommand(CanExecute = nameof(HasSelection))]
        private void MarkCompleted() => ChangeStatus(DocumentTaskStatus.Completed);

        private void ChangeStatus(DocumentTaskStatus status)
        {
            if (SelectedTask == null) return;
            try
            {
                _tasks.UpdateStatus(SelectedTask.Id, status,
                    actorId: _auth.CurrentEmployee?.Id ?? 0,
                    reportText: ReportText);
                ReportText = null;
                Reload();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        private bool HasSelection() => SelectedTask != null;
    }
}
