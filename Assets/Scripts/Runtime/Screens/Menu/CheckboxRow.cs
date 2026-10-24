#nullable enable
using AnomalyPath.Runtime.Ui.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Screens.Menu
{
    /// <summary>
    /// Строка настройки с квадратным чекбоксом из прототипа стартового экрана:
    /// подпись слева, чекбокс справа. Строка собирается кодом, потому что в префабе
    /// нет заготовки чекбокса; размеры и цвета берутся из <see cref="MenuMetrics"/> и
    /// <see cref="MenuPalette"/>.
    /// </summary>
    public sealed class CheckboxRow : MonoBehaviour
    {
        [Tooltip("Переключатель-чекбокс")]
        [SerializeField]
        private Toggle toggle = null!;

        [Tooltip("Подпись настройки")]
        [SerializeField]
        private Text label = null!;

        [Tooltip("Галочка внутри чекбокса")]
        [SerializeField]
        private GameObject checkmark = null!;

        /// <summary>Текущее значение настройки.</summary>
        public bool Value => toggle.isOn;

        /// <summary>Заполняет строку подписью и начальным значением.</summary>
        /// <param name="labelText">Готовая подпись из каталога текстов.</param>
        /// <param name="value">Начальное значение.</param>
        public void Bind(string labelText, bool value)
        {
            label.text = labelText;
            SetValue(value);
        }

        /// <summary>Меняет значение, не вызывая подписку на изменение.</summary>
        /// <param name="value">Новое значение.</param>
        public void SetValue(bool value)
        {
            toggle.SetIsOnWithoutNotify(value);
            Apply();
        }

        /// <summary>Создаёт строку с чекбоксом и возвращает её компонент.</summary>
        /// <param name="ui">Фабрика элементов интерфейса.</param>
        /// <param name="parent">Колонка настроек.</param>
        /// <param name="name">Имя строки в иерархии.</param>
        /// <param name="labelText">Готовая подпись из каталога текстов.</param>
        /// <param name="value">Начальное значение.</param>
        /// <returns>Созданная строка.</returns>
        public static CheckboxRow Create(
            UiFactory ui,
            Transform parent,
            string name,
            string labelText,
            bool value)
        {
            var row = new GameObject(
                name,
                typeof(RectTransform),
                typeof(HorizontalLayoutGroup),
                typeof(LayoutElement));
            row.transform.SetParent(parent, false);
            row.SetActive(false);

            var layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 8;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            var rowElement = row.GetComponent<LayoutElement>();
            rowElement.preferredHeight = MenuMetrics.CheckboxRowHeight;
            rowElement.flexibleHeight = 0;

            var label = ui.CreateText(row.transform, labelText, 20, TextAnchor.MiddleLeft);
            label.gameObject.name = "SettingLabel";
            var labelElement = label.GetComponent<LayoutElement>();
            labelElement.preferredHeight = MenuMetrics.CheckboxSize;
            labelElement.flexibleWidth = 1;

            var toggleObject = new GameObject(
                name + "Toggle",
                typeof(RectTransform),
                typeof(Image),
                typeof(Toggle),
                typeof(LayoutElement),
                typeof(Outline));
            toggleObject.transform.SetParent(row.transform, false);
            var box = toggleObject.GetComponent<Image>();
            box.color = MenuPalette.Field;
            var outline = toggleObject.GetComponent<Outline>();
            outline.effectColor = MenuPalette.GridLine;
            outline.effectDistance = new Vector2(
                MenuMetrics.CheckboxOutline,
                -MenuMetrics.CheckboxOutline);
            var toggle = toggleObject.GetComponent<Toggle>();
            toggle.targetGraphic = box;
            toggle.transition = Selectable.Transition.None;
            toggle.isOn = false;
            var toggleElement = toggleObject.GetComponent<LayoutElement>();
            toggleElement.preferredWidth = MenuMetrics.CheckboxSize;
            toggleElement.preferredHeight = MenuMetrics.CheckboxSize;
            toggleElement.flexibleWidth = 0;
            toggleElement.flexibleHeight = 0;

            var checkmark = new GameObject(name + "Checkmark", typeof(RectTransform));
            checkmark.transform.SetParent(toggleObject.transform, false);
            var checkRect = (RectTransform)checkmark.transform;
            checkRect.anchorMin = new Vector2(0.5f, 0.5f);
            checkRect.anchorMax = new Vector2(0.5f, 0.5f);
            checkRect.pivot = new Vector2(0.5f, 0.5f);
            checkRect.sizeDelta = new Vector2(MenuMetrics.CheckboxSize, MenuMetrics.CheckboxSize);
            CreateTick(
                ui,
                checkmark.transform,
                "CheckmarkShort",
                MenuMetrics.CheckboxShortTick,
                new Vector2(-6f, -4f),
                45f);
            CreateTick(
                ui,
                checkmark.transform,
                "CheckmarkLong",
                MenuMetrics.CheckboxLongTick,
                new Vector2(5f, 3f),
                -45f);

            var component = row.AddComponent<CheckboxRow>();
            component.toggle = toggle;
            component.label = label;
            component.checkmark = checkmark;
            component.Bind(labelText, value);
            row.SetActive(true);
            return component;
        }

        private static void CreateTick(
            UiFactory ui,
            Transform parent,
            string name,
            float length,
            Vector2 position,
            float angle)
        {
            var image = ui.CreateImage(name, parent, MenuPalette.ButtonSelected);
            image.raycastTarget = false;
            var rect = image.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(MenuMetrics.CheckboxTickWidth, length);
            rect.anchoredPosition = position;
            rect.localEulerAngles = new Vector3(0f, 0f, angle);
        }

        private void Awake()
        {
            toggle.onValueChanged.AddListener(OnValueChanged);
            Apply();
        }

        private void OnDestroy()
        {
            toggle.onValueChanged.RemoveListener(OnValueChanged);
        }

        private void OnValueChanged(bool value) => Apply();

        private void Apply()
        {
            if (toggle == null || checkmark == null)
            {
                return;
            }

            checkmark.SetActive(toggle.isOn);
        }
    }
}
