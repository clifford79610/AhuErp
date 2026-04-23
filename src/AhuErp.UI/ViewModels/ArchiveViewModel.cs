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
    /// ViewModel раздела «Архив» — CRUD для <see cref="ArchiveRequest"/>
    /// с учётом правил из <see cref="ArchiveService"/> (30-дневный дедлайн
    /// и требование обоих сканов для завершения).
    /// </summary>
    public partial class ArchiveViewModel : ViewModelBase
    {
        private readonly IDocumentRepository _documents;
        private readonly ArchiveService _archiveService;

        public ObservableCollection<ArchiveRequest> Requests { get; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
        [NotifyCanExecuteChangedFor(nameof(CompleteCommand))]
        private ArchiveRequest selectedRequest;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private string draftTitle;

        [ObservableProperty]
        private bool draftHasPassportScan;

        [ObservableProperty]
        private bool draftHasWorkBookScan;

        [ObservableProperty]
        private string errorMessage;

        public ArchiveViewModel(IDocumentRepository documents, ArchiveService archiveService)
        {
            _documents = documents ?? throw new ArgumentNullException(nameof(documents));
            _archiveService = archiveService ?? throw new ArgumentNullException(nameof(archiveService));
            Requests = new ObservableCollection<ArchiveRequest>();
            Reload();
        }

        partial void OnSelectedRequestChanged(ArchiveRequest value)
        {
            if (value == null)
            {
                DraftTitle = null;
                DraftHasPassportScan = false;
                DraftHasWorkBookScan = false;
                return;
            }
            DraftTitle = value.Title;
            DraftHasPassportScan = value.HasPassportScan;
            DraftHasWorkBookScan = value.HasWorkBookScan;
        }

        [RelayCommand]
        private void New()
        {
            SelectedRequest = null;
            DraftTitle = string.Empty;
            DraftHasPassportScan = false;
            DraftHasWorkBookScan = false;
            ErrorMessage = null;
        }

        [RelayCommand(CanExecute = nameof(CanSave))]
        private void Save()
        {
            ErrorMessage = null;
            try
            {
                if (SelectedRequest == null)
                {
                    var request = _archiveService.CreateRequest(DraftTitle, DateTime.Now);
                    request.HasPassportScan = DraftHasPassportScan;
                    request.HasWorkBookScan = DraftHasWorkBookScan;
                    _documents.Add(request);
                }
                else
                {
                    SelectedRequest.Title = DraftTitle;
                    SelectedRequest.HasPassportScan = DraftHasPassportScan;
                    SelectedRequest.HasWorkBookScan = DraftHasWorkBookScan;
                    _documents.Update(SelectedRequest);
                }
                Reload();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        [RelayCommand(CanExecute = nameof(HasSelection))]
        private void Delete()
        {
            if (SelectedRequest == null) return;
            _documents.Remove(SelectedRequest.Id);
            Reload();
            New();
        }

        [RelayCommand(CanExecute = nameof(HasSelection))]
        private void Complete()
        {
            ErrorMessage = null;
            try
            {
                _archiveService.CompleteRequest(SelectedRequest);
                _documents.Update(SelectedRequest);
                Reload();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        private bool CanSave() => !string.IsNullOrWhiteSpace(DraftTitle);
        private bool HasSelection() => SelectedRequest != null;

        private void Reload()
        {
            Requests.Clear();
            foreach (var r in _documents.ListArchiveRequests()
                                        .OrderByDescending(r => r.CreationDate))
            {
                Requests.Add(r);
            }
        }
    }
}
