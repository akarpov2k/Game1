#nullable enable
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AnomalyPath.Runtime.Screens.Talents
{
    /// <summary>
    /// Наведение на талант с задержкой: подсказка появляется, только если курсор задержался
    /// на карточке дольше заданного времени. Позиция курсора берётся из событий указателя,
    /// без чтения старого Input API.
    /// </summary>
    public sealed class TalentHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
    {
        private float _delay = 1f;
        private float _elapsed;
        private bool _inside;
        private bool _shown;
        private Vector2 _position;
        private Camera? _camera;
        private Action<Vector2, Camera?>? _show;
        private Action<Vector2, Camera?>? _move;
        private Action? _hide;

        /// <summary>Настраивает задержку и обработчики показа, перемещения и скрытия подсказки.</summary>
        /// <param name="delay">Сколько секунд курсор должен оставаться на карточке.</param>
        /// <param name="show">Показать подсказку в точке курсора.</param>
        /// <param name="move">Переместить уже показанную подсказку.</param>
        /// <param name="hide">Скрыть подсказку.</param>
        public void Configure(
            float delay,
            Action<Vector2, Camera?> show,
            Action<Vector2, Camera?> move,
            Action hide)
        {
            _delay = Mathf.Max(0f, delay);
            _show = show;
            _move = move;
            _hide = hide;
        }

        /// <inheritdoc />
        public void OnPointerEnter(PointerEventData eventData)
        {
            _inside = true;
            _shown = false;
            _elapsed = 0f;
            Capture(eventData);
        }

        /// <inheritdoc />
        public void OnPointerMove(PointerEventData eventData)
        {
            Capture(eventData);
            if (_shown)
            {
                _move?.Invoke(_position, _camera);
            }
        }

        /// <inheritdoc />
        public void OnPointerExit(PointerEventData eventData)
        {
            _inside = false;
            _shown = false;
            _elapsed = 0f;
            _hide?.Invoke();
        }

        private void Update()
        {
            if (!_inside || _shown)
            {
                return;
            }

            _elapsed += Time.unscaledDeltaTime;
            if (_elapsed < _delay)
            {
                return;
            }

            _shown = true;
            _show?.Invoke(_position, _camera);
        }

        private void OnDisable()
        {
            _inside = false;
            _shown = false;
            _elapsed = 0f;
            _hide?.Invoke();
        }

        private void Capture(PointerEventData eventData)
        {
            _position = eventData.position;
            _camera = eventData.enterEventCamera;
        }
    }
}
