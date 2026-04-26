using System;
using System.Collections.ObjectModel;
using AhuErp.Core.Models;
using AhuErp.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AhuErp.UI.ViewModels
{
    /// <summary>
    /// Управление номенклатурой дел и видами документов.
    /// Минимально-необходимый CRUD-интерфейс на двух вкладках.
    /// </summary>
    public partial class NomenclatureViewModel : ViewModelBase
    {
        private readonly INomenclatureService _service;

        public ObservableCollection<NomenclatureCase> Cases { get; }
            = new ObservableCollection<NomenclatureCase>();

        public ObservableCollection<DocumentTypeRef> Types { get; }
            = new ObservableCollection<DocumentTypeRef>();

        [ObservableProperty]
        private NomenclatureCase selectedCase;

        [ObservableProperty]
        private DocumentTypeRef selectedType;

        [ObservableProperty]
        private string newCaseIndex;

        [ObservableProperty]
        private string newCaseTitle;

        [ObservableProperty]
        private int newCaseRetention = 5;

        [ObservableProperty]
        private string newTypeName;

        [ObservableProperty]
        private string newTypeShortCode;

        [ObservableProperty]
        private string newTypeTemplate = "{Code}-{CaseIndex}/{Year}-{Sequence:00000}";

        [ObservableProperty]
        private string errorMessage;

        public NomenclatureViewModel(INomenclatureService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            Reload();
        }

        [RelayCommand]
        private void Reload()
        {
            ErrorMessage = null;
            Cases.Clear();
            foreach (var c in _service.ListCases(activeOnly: false)) Cases.Add(c);
            Types.Clear();
            foreach (var t in _service.ListTypes(activeOnly: false)) Types.Add(t);
        }

        [RelayCommand]
        private void AddCase()
        {
            ErrorMessage = null;
            try
            {
                _service.AddCase(new NomenclatureCase
                {
                    Index = NewCaseIndex,
                    Title = NewCaseTitle,
                    RetentionPeriodYears = NewCaseRetention,
                    Year = DateTime.Now.Year,
                    IsActive = true
                });
                NewCaseIndex = null; NewCaseTitle = null; NewCaseRetention = 5;
                Reload();
            }
            catch (Exception ex) { ErrorMessage = ex.Message; }
        }

        [RelayCommand]
        private void AddType()
        {
            ErrorMessage = null;
            try
            {
                _service.AddType(new DocumentTypeRef
                {
                    Name = NewTypeName,
                    ShortCode = NewTypeShortCode,
                    DefaultDirection = DocumentDirection.Internal,
                    DefaultRetentionYears = 5,
                    RegistrationNumberTemplate = NewTypeTemplate,
                    IsActive = true
                });
                NewTypeName = null; NewTypeShortCode = null;
                Reload();
            }
            catch (Exception ex) { ErrorMessage = ex.Message; }
        }
    }
}
