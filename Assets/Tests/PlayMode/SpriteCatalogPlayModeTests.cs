#nullable enable
using System.Collections;
using AnomalyPath.Runtime;
using AnomalyPath.Runtime.Ui.Art;
using AnomalyPath.Runtime.Ui.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AnomalyPath.Tests.PlayMode
{
    /// <summary>Каталог спрайтов: поиск по id и поведение пустого каталога.</summary>
    public sealed class SpriteCatalogPlayModeTests
    {
        /// <summary>Спрайт находится по id, неизвестный id не находится.</summary>
        [Test]
        public void SpriteCatalog_TryGet_ReturnsSpriteById()
        {
            var texture = new Texture2D(4, 4);
            var sprite = Sprite.Create(texture, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0.5f));
            var catalog = ScriptableObject.CreateInstance<SpriteCatalog>();
            try
            {
                catalog.SetEntries(new[] { new SpriteCatalog.Entry("icons/test", sprite) });

                Assert.IsTrue(catalog.TryGet("icons/test", out var found), "спрайт должен находиться по id");
                Assert.AreSame(sprite, found, "должен вернуться тот же спрайт");
                Assert.IsFalse(catalog.TryGet("icons/missing", out _), "неизвестный id не должен находиться");
            }
            finally
            {
                Object.DestroyImmediate(catalog);
                Object.DestroyImmediate(sprite);
                Object.DestroyImmediate(texture);
            }
        }

        /// <summary>Пустой каталог всегда отвечает «нет».</summary>
        [Test]
        public void NullSpriteCatalog_AlwaysMisses()
        {
            Assert.IsFalse(NullSpriteCatalog.Instance.TryGet("icons/test", out var sprite));
            Assert.IsNull(sprite);
        }

        /// <summary>Валидатор возвращает только отсутствующие id и не повторяет их.</summary>
        [Test]
        public void FindMissing_ReturnsOnlyAbsentIds()
        {
            var texture = new Texture2D(4, 4);
            var sprite = Sprite.Create(texture, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0.5f));
            var catalog = ScriptableObject.CreateInstance<SpriteCatalog>();
            try
            {
                catalog.SetEntries(new[] { new SpriteCatalog.Entry("icons/present", sprite) });

                var missing = SpriteCatalogValidator.FindMissing(
                    new[] { "icons/present", "icons/absent", "icons/absent", string.Empty },
                    catalog);

                CollectionAssert.AreEqual(new[] { "icons/absent" }, missing);
            }
            finally
            {
                Object.DestroyImmediate(catalog);
                Object.DestroyImmediate(sprite);
                Object.DestroyImmediate(texture);
            }
        }

        /// <summary>Без каталога отсутствуют все уникальные id.</summary>
        [Test]
        public void FindMissing_NullCatalog_ReturnsEveryDistinctId()
        {
            var missing = SpriteCatalogValidator.FindMissing(
                new[] { "icons/a", "icons/b", "icons/a" },
                NullSpriteCatalog.Instance);

            CollectionAssert.AreEqual(new[] { "icons/a", "icons/b" }, missing);
        }

        /// <summary>При старте сцены Main у каждого предмета есть загруженная иконка.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator MainScene_AllItemIcons_AreLoaded()
        {
            yield return SceneManager.LoadSceneAsync("Main");
            yield return null;

            var host = Object.FindAnyObjectByType<GameScreen>();
            Assert.IsNotNull(host, "GameScreen не найден в сцене Main");
            var game = host!;

            foreach (var item in game.Items.All)
            {
                Assert.IsTrue(game.Sprites.TryGet(item.Icon, out var sprite), $"Нет иконки {item.Icon}");
                Assert.IsNotNull(sprite, $"Нет спрайта для {item.Icon}");
                Assert.IsNotNull(sprite!.texture, $"Текстура {item.Icon} не загружена");
            }
        }

        /// <summary>Иконка АК-74 подключена в данных и загружается из новой текстуры 512×512.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator MainScene_Ak74Icon_IsLinkedAndLoaded()
        {
            yield return SceneManager.LoadSceneAsync("Main");
            yield return null;

            var host = Object.FindAnyObjectByType<GameScreen>();
            Assert.IsNotNull(host, "GameScreen не найден в сцене Main");
            var game = host!;

            var item = game.Items.Get("wpn_ak74");
            Assert.AreEqual("icons/wpn_ak74", item.Icon, "В описании АК-74 должен быть id иконки");
            Assert.IsTrue(game.Sprites.TryGet(item.Icon, out var sprite), "Иконка АК-74 не найдена в каталоге");
            Assert.IsNotNull(sprite, "Спрайт АК-74 не загружен");
            Assert.AreEqual(512, sprite!.texture.width, "Должна использоваться новая текстура 512 px");
            Assert.AreEqual(142, sprite.texture.height, "Текстура должна быть обрезана по высоте модели");
            Assert.AreEqual(512f, sprite.rect.width, "Спрайт должен занимать всю текстуру");
            Assert.AreEqual(142f, sprite.rect.height, "Спрайт должен занимать всю текстуру");
        }
    }
}
