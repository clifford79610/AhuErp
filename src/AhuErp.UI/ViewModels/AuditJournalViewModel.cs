using System;
using System.Collections.ObjectModel;
using AhuErp.Core.Models;
using AhuErp.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AhuErp.UI.ViewModels
{
    /// <summary>
    /// Глобальный журнал аудита для администратора. Фильтрация по дате,
    /// типу действия и сущности; кнопка проверки целостности hash-цепочки.
    /// </summary>
    public partial class AuditJournalViewModel : ViewModelBase
    {
        private readonly IAuditService _audit;

        public ObservableCollection<AuditLog> Entries { get; } = new ObservableCollection<AuditLog>();

        public AuditActionType[] ActionTypes { get; } =
            (AuditActionType[])Enum.GetValues(typeof(AuditActionType));

        [ObservableProperty]
        private DateTime? from = DateTime.UtcNow.Date.AddDays(-30);

        [ObservableProperty]
        private DateTime? to = DateTime.UtcNow.Date.AddDays(1);

        [ObservableProperty]
        private AuditActionType? actionType;

        [ObservableProperty]
        private string entityType;

        [ObservableProperty]
        private string integrityStatus;

        [ObservableProperty]
        private string errorMessage;

        public AuditJournalViewModel(IAuditService audit)
        {
            _audit = audit ?? throw new ArgumentNullException(nameof(audit));
            Reload();
        }

        [RelayCommand]
        private void Reload()
        {
            ErrorMessage = null;
            Entries.Clear();
            try
            {
                var filter = new AuditQueryFilter
                {
                    From = From,
                    To = To,
                    ActionType = ActionType,
                    EntityType = string.IsNullOrWhiteSpace(EntityType) ? null : EntityType.Trim(),
                    Top = 1000
                };
                foreach (var entry in _audit.Query(filter)) Entries.Add(entry);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        [RelayCommand]
        private void VerifyIntegrity()
        {
            try
            {
                var corrupted = _audit.VerifyChain();
                IntegrityStatus = corrupted == null
                    ? "Цепочка целостности журнала аудита корректна."
                    : $"Нарушение в записи #{corrupted.Id} ({corrupted.Timestamp:dd.MM.yyyy HH:mm}).";
            }
            catch (Exception ex)
            {
                IntegrityStatus = $"Ошибка проверки: {ex.Message}";
            }
        }
    }
}
