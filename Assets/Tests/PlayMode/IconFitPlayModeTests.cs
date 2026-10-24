#nullable enable
using AnomalyPath.Runtime.Ui.Core;
using NUnit.Framework;
using UnityEngine;

namespace AnomalyPath.Tests.PlayMode
{
    /// <summary>Подбор иконки под слот: поворот и вписывание по длинной стороне.</summary>
    public sealed class IconFitPlayModeTests
    {
        /// <summary>Вытянутый слот получает повёрнутую иконку, квадратный — без поворота.</summary>
        /// <param name="slotWidth">Ширина слота.</param>
        /// <param name="slotHeight">Высота слота.</param>
        /// <param name="spriteWidth">Ширина спрайта.</param>
        /// <param name="spriteHeight">Высота спрайта.</param>
        /// <param name="rotated">Ожидаемый поворот.</param>
        /// <param name="expectedWidth">Ожидаемая видимая ширина.</param>
        /// <param name="expectedHeight">Ожидаемая видимая высота.</param>
        [TestCase(176f, 264f, 512f, 142f, true, 71f, 256f)]
        [TestCase(264f, 176f, 142f, 512f, true, 256f, 71f)]
        [TestCase(176f, 264f, 512f, 512f, false, 168f, 168f)]
        [TestCase(264f, 176f, 512f, 142f, false, 256f, 71f)]
        [TestCase(176f, 264f, 142f, 512f, false, 71f, 256f)]
        public void Compute_FitsLongAxis(
            float slotWidth,
            float slotHeight,
            float spriteWidth,
            float spriteHeight,
            bool rotated,
            float expectedWidth,
            float expectedHeight)
        {
            var fit = IconFit.Compute(slotWidth, slotHeight, spriteWidth, spriteHeight, 4f);

            Assert.AreEqual(rotated, fit.Rotated, "поворот не совпал с ориентацией слота");
            var visualWidth = fit.Rotated ? fit.Size.y : fit.Size.x;
            var visualHeight = fit.Rotated ? fit.Size.x : fit.Size.y;
            Assert.AreEqual(expectedWidth, visualWidth, 0.5f, "видимая ширина");
            Assert.AreEqual(expectedHeight, visualHeight, 0.5f, "видимая высота");
        }

        /// <summary>Нулевой слот не вписывает ничего.</summary>
        [Test]
        public void Compute_ZeroSlot_ReturnsEmpty()
        {
            var fit = IconFit.Compute(0f, 0f, 512f, 142f, 4f);

            Assert.IsFalse(fit.Rotated);
            Assert.AreEqual(Vector2.zero, fit.Size);
        }
    }
}
