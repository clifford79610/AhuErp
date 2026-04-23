using Microsoft.Win32;

namespace AhuErp.UI.Infrastructure
{
    /// <summary>
    /// Обёртка над <see cref="SaveFileDialog"/>.
    /// </summary>
    public sealed class FileDialogService : IFileDialogService
    {
        public string PromptSaveFile(string title, string filter, string defaultFileName)
        {
            var dialog = new SaveFileDialog
            {
                Title = title,
                Filter = filter,
                FileName = defaultFileName,
                OverwritePrompt = true,
                AddExtension = true
            };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }
    }
}
