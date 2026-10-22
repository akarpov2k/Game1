#nullable enable
using System;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.World;
using AnomalyPath.Runtime.Ui.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Screens.Game
{
    /// <summary>
    /// Часы основного экрана: круглый циферблат с половинами дня и ночи и стрелкой,
    /// которая идёт по текущей фазе суток: день — правая половина, ночь — левая.
    /// Рядом выводится численное время и номер дня.
    /// </summary>
    public sealed class PlayClock
    {
        private readonly GameObject _root;
        private readonly RectTransform _hand;
        private readonly Text _time;
        private readonly IGameTexts _texts;

        /// <summary>Создаёт часы в левом верхнем углу экрана.</summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="parent">Родитель, обычно панель основного экрана.</param>
        public PlayClock(IUiHost host, Transform parent)
        {
            _texts = host.Texts;
            _root = new GameObject("Clock", typeof(RectTransform));
            _root.transform.SetParent(parent, false);
            var rootRect = (RectTransform)_root.transform;
            rootRect.anchorMin = new Vector2(0f, 1f);
            rootRect.anchorMax = new Vector2(0f, 1f);
            rootRect.pivot = new Vector2(0f, 1f);
            rootRect.anchoredPosition = new Vector2(PlayMetrics.SidePadding, -PlayMetrics.ClockTop);
            rootRect.sizeDelta = new Vector2(
                PlayMetrics.ClockSize + PlayMetrics.ClockTimeGap + PlayMetrics.ClockTimeWidth,
                PlayMetrics.ClockSize);

            var face = host.Ui.CreateImage("ClockFace", _root.transform, Color.white);
            face.sprite = PlayArt.ClockFace();
            face.preserveAspect = true;
            face.raycastTarget = false;
            PlaceDial((RectTransform)face.transform);

            var hand = host.Ui.CreateImage("ClockHand", face.transform, Color.white);
            hand.sprite = PlayArt.ClockHand();
            hand.preserveAspect = true;
            hand.raycastTarget = false;
            _hand = hand.rectTransform;
            _hand.anchorMin = new Vector2(0.5f, 0.5f);
            _hand.anchorMax = new Vector2(0.5f, 0.5f);
            _hand.pivot = new Vector2(0.5f, 0.5f);
            _hand.anchoredPosition = Vector2.zero;
            _hand.sizeDelta = new Vector2(PlayMetrics.ClockSize, PlayMetrics.ClockSize);

            _time = host.Ui.CreateText(
                _root.transform,
                string.Empty,
                PlayMetrics.ClockTimeFontSize,
                TextAnchor.MiddleLeft);
            _time.raycastTarget = false;
            var timeRect = _time.rectTransform;
            timeRect.anchorMin = new Vector2(0f, 1f);
            timeRect.anchorMax = new Vector2(0f, 1f);
            timeRect.pivot = new Vector2(0f, 1f);
            timeRect.anchoredPosition = new Vector2(
                PlayMetrics.ClockSize + PlayMetrics.ClockTimeGap,
                0f);
            timeRect.sizeDelta = new Vector2(PlayMetrics.ClockTimeWidth, PlayMetrics.ClockTimeHeight);
        }

        /// <summary>Текущий угол стрелки в градусах: 0 — вверх, дальше по часовой стрелке.</summary>
        public float HandAngle => _hand.localEulerAngles.z;

        /// <summary>Показывает время и ставит стрелку по часам мира.</summary>
        /// <param name="clock">Часы мира.</param>
        public void Show(WorldClock clock)
        {
            if (clock == null)
            {
                throw new ArgumentNullException(nameof(clock));
            }

            _root.SetActive(true);
            _hand.localEulerAngles = new Vector3(0f, 0f, Angle(clock));
            var hour = Mathf.FloorToInt((float)clock.Hours);
            var minute = Mathf.FloorToInt((float)((clock.Hours - hour) * 60d));
            _time.text = UiText.Format(
                _texts,
                UiTextKey.Game_Clock,
                clock.Day,
                hour,
                minute);
        }

        /// <summary>Скрывает часы, пока игра не начата.</summary>
        public void Hide() => _root.SetActive(false);

        /// <summary>
        /// Угол стрелки для часов мира. День занимает правую половину циферблата,
        /// ночь — левую; внутри фазы стрелка идёт пропорционально времени.
        /// </summary>
        /// <param name="clock">Часы мира.</param>
        /// <returns>Угол в градусах для поворота стрелки.</returns>
        /// <exception cref="ArgumentNullException">Часы не переданы.</exception>
        public static float Angle(WorldClock clock)
        {
            if (clock == null)
            {
                throw new ArgumentNullException(nameof(clock));
            }

            if (clock.Phase == TimePhase.Day)
            {
                var length = clock.NightStartHour - clock.DayStartHour;
                var progress = length <= 0d ? 0d : (clock.Hours - clock.DayStartHour) / length;
                return Mathf.Repeat(0f - (180f * Mathf.Clamp01((float)progress)), 360f);
            }

            var nightLength = 24d - clock.NightStartHour + clock.DayStartHour;
            var elapsed = clock.Hours >= clock.NightStartHour
                ? clock.Hours - clock.NightStartHour
                : clock.Hours + 24d - clock.NightStartHour;
            var nightProgress = nightLength <= 0d ? 0d : elapsed / nightLength;
            return Mathf.Repeat(-180f - (180f * Mathf.Clamp01((float)nightProgress)), 360f);
        }

        private static void PlaceDial(RectTransform rect)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(PlayMetrics.ClockSize, PlayMetrics.ClockSize);
        }
    }
}
