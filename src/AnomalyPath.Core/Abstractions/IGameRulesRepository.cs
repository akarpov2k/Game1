#nullable enable
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Rules;

namespace AnomalyPath.Core.Abstractions
{
    /// <summary>Доступ к правилам прохождения, загруженным из данных.</summary>
    public interface IGameRulesRepository
    {
        /// <summary>Правила прохождения или <c>null</c>, если файл не загружен.</summary>
        GameRulesTemplate? Rules { get; }

        /// <summary>Возвращает правила прохождения.</summary>
        /// <returns>Правила прохождения.</returns>
        /// <exception cref="DataException">Правила не загружены.</exception>
        GameRulesTemplate Require();
    }
}
