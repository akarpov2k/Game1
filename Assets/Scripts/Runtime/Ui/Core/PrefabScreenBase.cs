#nullable enable
using UnityEngine;

namespace AnomalyPath.Runtime.Ui.Core
{
    /// <summary>
    /// Экран, собранный в префабе: панель, задник, контент и подписи приходят из ассета,
    /// код только связывает данные и заполняет динамику. Панель неактивна до показа роутером.
    /// </summary>
    public abstract class PrefabScreenBase : MonoBehaviour, IScreen
    {
        [Tooltip("Задник экрана; пусто — фон не подменяется")]
        [SerializeField]
        private ScreenBackdrop? backdrop = null;

        [Tooltip("Корень содержимого поверх задника; пусто — корень панели")]
        [SerializeField]
        private RectTransform? content = null;

        /// <summary>Хост интерфейса: сервисы и переходы.</summary>
        protected IUiHost Host { get; private set; } = null!;

        /// <inheritdoc />
        public GameObject Panel => gameObject;

        /// <summary>Корень содержимого экрана.</summary>
        protected RectTransform Content => content != null ? content : (RectTransform)transform;

        /// <summary>Тема фона; переопределяется экранами с динамическим фоном.</summary>
        protected virtual ScreenTheme Theme => ScreenTheme.Default;

        /// <summary>Связывает префаб с хостом: задник, фоновые изображения и локализованные подписи.</summary>
        /// <param name="host">Хост интерфейса.</param>
        public void Bind(IUiHost host)
        {
            Host = host;
            if (backdrop != null)
            {
                backdrop.Initialize(host);
                backdrop.Apply(Theme);
            }

            foreach (var label in GetComponentsInChildren<LocalizedText>(true))
            {
                label.Apply(host.Texts);
            }

            OnBound();
        }

        /// <summary>Применяет текущую тему фона заново — для экранов с динамическим фоном.</summary>
        protected void ApplyTheme()
        {
            if (backdrop != null)
            {
                backdrop.Apply(Theme);
            }
        }

        /// <summary>Возврат по Esc. По умолчанию экран Esc не обрабатывает.</summary>
        /// <returns><c>true</c>, если Esc обработан.</returns>
        public virtual bool HandleEscape() => false;

        /// <summary>Точка расширения: экран доводит привязку после общей части.</summary>
        protected virtual void OnBound()
        {
        }
    }
}
