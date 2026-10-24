#nullable enable
using AnomalyPath.Runtime.Ui.Core;
using UnityEngine;

namespace AnomalyPath.Runtime.Screens
{
    /// <summary>
    /// Базовый экран: панель, общий тематический задник и доступ к хосту.
    /// Наследники добавляют содержимое после базового конструктора — фон остаётся позади.
    /// Тему фона можно переопределить свойством <see cref="Theme"/>.
    /// </summary>
    public abstract class ScreenBase : IScreen
    {
        private readonly ScreenBackdrop _backdrop;

        /// <summary>Создаёт панель экрана и её фон.</summary>
        /// <param name="host">Хост интерфейса: сервисы и переходы.</param>
        /// <param name="canvas">Родительский канвас.</param>
        /// <param name="panelName">Имя объекта панели.</param>
        protected ScreenBase(IUiHost host, Transform canvas, string panelName)
        {
            Host = host;
            Panel = host.Ui.CreatePanel(panelName, canvas);
            _backdrop = ScreenBackdrop.Create(host, Panel.transform);
            ApplyTheme();
        }

        /// <summary>Панель экрана.</summary>
        public GameObject Panel { get; }

        /// <summary>Хост интерфейса.</summary>
        protected IUiHost Host { get; }

        /// <summary>Корень содержимого: панель, поверх задника.</summary>
        protected Transform Content => Panel.transform;

        /// <summary>
        /// Тема фона. По умолчанию общий тематический задник; переопределяется в дочерних экранах.
        /// </summary>
        protected virtual ScreenTheme Theme => ScreenTheme.Default;

        /// <summary>Применяет текущую тему заново — нужно экранам с динамическим фоном.</summary>
        protected void ApplyTheme() => _backdrop.Apply(Theme);

        /// <summary>
        /// Возврат по Esc. По умолчанию экран Esc не обрабатывает: включается только там,
        /// где есть однозначное «Назад». Переопределяется в дочерних экранах.
        /// </summary>
        /// <returns><c>true</c>, если Esc обработан.</returns>
        public virtual bool HandleEscape() => false;
    }
}
