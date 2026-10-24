#nullable enable
using UnityEngine;

namespace AnomalyPath.Runtime.Ui.Core
{
    /// <summary>Экран интерфейса, которым управляет маршрутизатор: панель и обработка Esc.</summary>
    public interface IScreen
    {
        /// <summary>Корневая панель экрана.</summary>
        GameObject Panel { get; }

        /// <summary>Возврат по Esc.</summary>
        /// <returns><c>true</c>, если экран обработал клавишу.</returns>
        bool HandleEscape();
    }
}
