using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using AhuErp.Core.Models;
using AhuErp.Core.Services;
using AhuErp.UI.Infrastructure;
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
        private readonly IReportService _reports;
        private readonly IFileDialogService _fileDialog;

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

        [ObservableProperty]
        private string statusMessage;

        public ArchiveViewModel(IDocumentRepository documents,
                                ArchiveService archiveService,
                                IReportService reports,
                                IFileDialogService fileDialog)
        {
            _documents = documents ?? throw new ArgumentNullException(nameof(documents));
            _archiveService = archiveService ?? throw new ArgumentNullException(nameof(archiveService));
            _reports = reports ?? throw new ArgumentNullException(nameof(reports));
            _fileDialog = fileDialog ?? throw new ArgumentNullException(nameof(fileDialog));
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

        [RelayCommand(CanExecute = nameof(HasSelection))]
        private void GenerateCertificate()
        {
            ErrorMessage = null;
            StatusMessage = null;
            if (SelectedRequest == null) return;

            var path = _fileDialog.PromptSaveFile(
                title: "Сохранить архивную справку (Word)",
                filter: "Word documents (*.docx)|*.docx",
                defaultFileName: $"archive-certificate-{SelectedRequest.Id}.docx");
            if (string.IsNullOrWhiteSpace(path)) return;

            try
            {
                _reports.GenerateArchiveCertificate(SelectedRequest.Id, path);
                StatusMessage = $"Справка сохранена: {path}";
            }
            catch (IOException ex)
            {
                ErrorMessage = $"Не удалось записать файл (возможно, он открыт в Word): {ex.Message}";
            }
            catch (UnauthorizedAccessException ex)
            {
                ErrorMessage = $"Нет прав для записи в указанный каталог: {ex.Message}";
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
