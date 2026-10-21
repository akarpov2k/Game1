#nullable enable
namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Проверка всех данных до инициализации игры: собирает ошибки, а не падает на первой.</summary>
    public interface IDataValidator
    {
        /// <summary>Проверяет весь источник данных.</summary>
        /// <returns>Отчёт со списком ошибок и предупреждений.</returns>
        Validation.ValidationReport Validate();
    }
}
