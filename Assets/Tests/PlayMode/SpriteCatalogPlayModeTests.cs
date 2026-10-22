#nullable enable
using AnomalyPath.Runtime.Ui.Art;
using AnomalyPath.Runtime.Ui.Core;
using NUnit.Framework;
using UnityEngine;

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
    }
}
