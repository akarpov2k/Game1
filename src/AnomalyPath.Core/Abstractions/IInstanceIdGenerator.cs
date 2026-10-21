#nullable enable
namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Источник id экземпляров предметов: детерминированный, подменяется в тестах.</summary>
    public interface IInstanceIdGenerator
    {
        /// <summary>Возвращает следующий id экземпляра.</summary>
        /// <returns>Уникальный id.</returns>
        string Next();
    }
}
