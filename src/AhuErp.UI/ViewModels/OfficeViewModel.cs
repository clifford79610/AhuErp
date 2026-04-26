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
    /// ViewModel раздела «Отдел документационного обеспечения» — простая CRUD-оболочка над
    /// <see cref="IDocumentRepository"/> для входящих и внутренних документов.
    /// </summary>
    public partial class OfficeViewModel : ViewModelBase
    {
        private readonly IDocumentRepository _documents;

        public ObservableCollection<Document> Documents { get; }

        public DocumentType[] EditableTypes { get; } =
        {
            DocumentType.Incoming,
            DocumentType.Internal
        };

        public DocumentStatus[] Statuses { get; } =
            (DocumentStatus[])Enum.GetValues(typeof(DocumentStatus));

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
        private Document selectedDocument;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private string draftTitle;

        [ObservableProperty]
        private DocumentType draftType = DocumentType.Incoming;

        [ObservableProperty]
        private DocumentStatus draftStatus = DocumentStatus.New;

        [ObservableProperty]
        private DateTime draftDeadline = DateTime.Today.AddDays(14);

        [ObservableProperty]
        private string errorMessage;

        public OfficeViewModel(IDocumentRepository documents)
        {
            _documents = documents ?? throw new ArgumentNullException(nameof(documents));
            Documents = new ObservableCollection<Document>();
            Reload();
        }

        partial void OnSelectedDocumentChanged(Document value)
        {
            if (value == null) return;
            DraftTitle = value.Title;
            DraftType = value.Type;
            DraftStatus = value.Status;
            DraftDeadline = value.Deadline == default ? DateTime.Today.AddDays(14) : value.Deadline;
        }

        [RelayCommand]
        private void New()
        {
            SelectedDocument = null;
            DraftTitle = string.Empty;
            DraftType = DocumentType.Incoming;
            DraftStatus = DocumentStatus.New;
            DraftDeadline = DateTime.Today.AddDays(14);
            ErrorMessage = null;
        }

        [RelayCommand(CanExecute = nameof(CanSave))]
        private void Save()
        {
            ErrorMessage = null;
            try
            {
                if (SelectedDocument == null)
                {
                    var doc = new Document
                    {
                        Type = DraftType,
                        Title = DraftTitle,
                        CreationDate = DateTime.Now,
                        Deadline = DraftDeadline,
                        Status = DraftStatus
                    };
                    _documents.Add(doc);
                }
                else
                {
                    SelectedDocument.Title = DraftTitle;
                    SelectedDocument.Type = DraftType;
                    SelectedDocument.Status = DraftStatus;
                    SelectedDocument.Deadline = DraftDeadline;
                    _documents.Update(SelectedDocument);
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
            if (SelectedDocument == null) return;
            _documents.Remove(SelectedDocument.Id);
            Reload();
            New();
        }

        private bool CanSave() => !string.IsNullOrWhiteSpace(DraftTitle);

        private bool HasSelection() => SelectedDocument != null;

        private void Reload()
        {
            Documents.Clear();
            foreach (var doc in _documents.ListByType(DocumentType.Incoming)
                                          .Concat(_documents.ListByType(DocumentType.Internal))
                                          .OrderByDescending(d => d.CreationDate))
            {
                Documents.Add(doc);
            }
        }
    }
}
