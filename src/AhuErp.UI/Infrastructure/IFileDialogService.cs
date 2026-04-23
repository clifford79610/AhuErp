namespace AhuErp.UI.Infrastructure
{
    /// <summary>
    /// Абстракция над <see cref="Microsoft.Win32.SaveFileDialog"/>, позволяющая
    /// изолировать ViewModel от прямого обращения к WPF-диалогам и при
    /// необходимости подменять диалог в автотестах.
    /// </summary>
    public interface IFileDialogService
    {
        /// <summary>
        /// Показывает диалог «Сохранить как…» и возвращает выбранный путь
        /// или <c>null</c>, если пользователь отменил операцию.
        /// </summary>
        string PromptSaveFile(string title, string filter, string defaultFileName);
    }
}
