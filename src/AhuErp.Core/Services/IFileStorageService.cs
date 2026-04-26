using System.IO;

namespace AhuErp.Core.Services
{
    /// <summary>
    /// Абстракция над файловым хранилищем вложений документов.
    /// Реализация по умолчанию (<see cref="FileSystemStorageService"/>) пишет
    /// файлы в локальную папку с подкаталогами вида
    /// <c>Documents/{Year}/{RegNumber}/v{Version}/</c>; в будущем сюда же
    /// можно подключить S3/MinIO без изменения вызывающего кода.
    /// </summary>
    public interface IFileStorageService
    {
        /// <summary>
        /// Сохранить поток в хранилище. Возвращает относительный ключ доступа
        /// (<see cref="Models.DocumentAttachment.StoragePath"/>).
        /// </summary>
        string Store(Stream content, string registrationNumber, int version, string fileName);

        /// <summary>Открыть поток для чтения по относительному ключу.</summary>
        Stream Open(string storagePath);

        /// <summary>Удалить файл из хранилища (для compensation в тестах/откатов).</summary>
        bool Delete(string storagePath);

        /// <summary>Существует ли файл по ключу.</summary>
        bool Exists(string storagePath);
    }
}
