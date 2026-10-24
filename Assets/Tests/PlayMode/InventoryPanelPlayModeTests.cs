#nullable enable
using AnomalyPath.Runtime.Ui.Art;
using AnomalyPath.Runtime.Ui.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Tests.PlayMode
{
    /// <summary>Текстура окна инвентаря: плашка валюты, запасной цвет и общие настройки подписи.</summary>
    public sealed class InventoryPanelPlayModeTests
    {
        /// <summary>Плашка валюты после зеркалирования лежит справа сверху.</summary>
        [Test]
        public void InventoryPanelArt_ToAbsolute_PlacesCurrencyPlateAtTopRight()
        {
            var rect = InventoryPanelArt.ToAbsolute(InventoryPanelArt.CurrencyPlate, 768f, 800f, 100f, 0f);

            Assert.Greater(rect.center.x, 100f + (768f * 0.5f), "Плашка валюты должна быть в правой половине окна");
            Assert.Less(rect.center.y, 100f, "Плашка валюты должна быть в верхней части окна");
            Assert.Greater(rect.width, 0f, "Ширина плашки");
            Assert.Greater(rect.height, 0f, "Высота плашки");
        }

        /// <summary>Найденный спрайт применяется как текстура окна, запасной цвет не используется.</summary>
        [Test]
        public void InventoryPanelArt_ApplyBackground_WithSprite_AssignsTexture()
        {
            var texture = new Texture2D(4, 4);
            var sprite = Sprite.Create(texture, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0.5f));
            var catalog = ScriptableObject.CreateInstance<SpriteCatalog>();
            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            try
            {
                catalog.SetEntries(new[] { new SpriteCatalog.Entry(InventoryPanelArt.SpriteId, sprite) });
                var image = panel.GetComponent<Image>();
                image.preserveAspect = true;

                var applied = InventoryPanelArt.ApplyBackground(catalog, image, Color.red);

                Assert.IsTrue(applied, "Спрайт должен найтись по id");
                Assert.AreSame(sprite, image.sprite, "Спрайт окна не назначен");
                Assert.AreEqual(Color.white, image.color, "Текстура рисуется без затемнения");
                Assert.IsFalse(image.preserveAspect, "Окно заполняет рамку целиком");
            }
            finally
            {
                Object.DestroyImmediate(panel);
                Object.DestroyImmediate(catalog);
                Object.DestroyImmediate(sprite);
                Object.DestroyImmediate(texture);
            }
        }

        /// <summary>Без спрайта остаётся запасной цвет и окно не считается текстурным.</summary>
        [Test]
        public void InventoryPanelArt_ApplyBackground_WithoutSprite_UsesFallback()
        {
            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            try
            {
                var image = panel.GetComponent<Image>();
                var fallback = new Color(0.1f, 0.2f, 0.3f, 1f);

                var applied = InventoryPanelArt.ApplyBackground(NullSpriteCatalog.Instance, image, fallback);

                Assert.IsFalse(applied, "Пустой каталог не даёт спрайта");
                Assert.IsNull(image.sprite, "Спрайт не должен назначаться");
                Assert.AreEqual(fallback, image.color, "Должен остаться запасной цвет");
            }
            finally
            {
                Object.DestroyImmediate(panel);
            }
        }

        /// <summary>Подпись валюты центрируется и получает контур для читаемости на металле.</summary>
        [Test]
        public void InventoryPanelArt_DecorateMoney_CentersAndOutlines()
        {
            var money = new GameObject("Money", typeof(RectTransform), typeof(Text));
            try
            {
                var label = money.GetComponent<Text>();
                InventoryPanelArt.DecorateMoney(label);

                Assert.AreEqual(TextAnchor.MiddleCenter, label.alignment, "Валюта стоит по центру плашки");
                Assert.IsNotNull(label.GetComponent<Outline>(), "Без контура текст теряется на текстуре");
            }
            finally
            {
                Object.DestroyImmediate(money);
            }
        }
    }
}
