namespace AhuErp.Core.Services
{
    /// <summary>
    /// Сервис, инкапсулирующий хэширование и проверку пароля.
    /// Алгоритм и параметры остаются деталью реализации.
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        /// Возвращает строковое представление хэша пароля (соль внутри).
        /// </summary>
        string Hash(string password);

        /// <summary>
        /// Проверяет пароль, не раскрывая соль/итерации вызывающему коду.
        /// </summary>
        bool Verify(string password, string hash);
    }
}
