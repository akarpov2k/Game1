#nullable enable
using System.Collections;
using System.Reflection;
using AnomalyPath.Runtime;
using AnomalyPath.Runtime.Screens.Menu;
using AnomalyPath.Runtime.Ui.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace AnomalyPath.Tests.PlayMode
{
    /// <summary>Экран новой игры собран префабом: панель, задник и подписи приходят из ассета.</summary>
    public sealed class PrefabScreenPlayModeTests
    {
        /// <summary>Панель новой игры — экземпляр префаба с привязанным задником и локализованными подписями.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator NewGame_PrefabScreen_BindsBackdropAndLocalizedTexts()
        {
            yield return LoadMain();

            Click("Новая игра");
            yield return null;

            var panel = GameObject.Find("NewGame");
            Assert.IsNotNull(panel, "Панель новой игры не создана");
            Assert.IsNotNull(panel.GetComponent<NewGameScreen>(), "Панель собрана не из префаба экрана");

            var backdrop = panel.transform.Find("ScreenBackdrop");
            Assert.IsNotNull(backdrop, "У панели нет задника из префаба");
            Assert.IsNotNull(backdrop.GetComponent<ScreenBackdrop>(), "У задника нет компонента ScreenBackdrop");
            Assert.IsNotNull(backdrop.GetComponent<Image>().sprite, "Задник не применил изображение темы");

            var start = GameObject.Find("Начать");
            Assert.IsNotNull(start, "Нет кнопки «Начать игру»");
            var screen = Object.FindAnyObjectByType<GameScreen>();
            Assert.IsNotNull(screen, "GameScreen не найден в сцене Main");
            var label = start.GetComponentInChildren<Text>();
            Assert.IsNotNull(label, "У кнопки «Начать игру» нет подписи");
            Assert.AreEqual(
                UiText.Get(screen.Texts, UiTextKey.NewGame_Start),
                label.text,
                "Подпись кнопки не взята из каталога текстов");
        }

        /// <summary>Спрайт, заданный в инспекторе, перекрывает картинку темы.</summary>
        /// <returns>Корутина теста.</returns>
        [UnityTest]
        public IEnumerator ScreenBackdrop_InspectorSprite_OverridesThemeImage()
        {
            yield return LoadMain();

            var host = Object.FindAnyObjectByType<GameScreen>();
            Assert.IsNotNull(host, "GameScreen не найден в сцене Main");

            var go = new GameObject("BackdropTest", typeof(RectTransform), typeof(Image));
            var image = go.GetComponent<Image>();
            var backdrop = go.AddComponent<ScreenBackdrop>();
            backdrop.Initialize(host, image);

            var texture = new Texture2D(2, 2);
            var sprite = Sprite.Create(texture, new Rect(0f, 0f, 2f, 2f), new Vector2(0.5f, 0.5f));
            var field = typeof(ScreenBackdrop).GetField("sprite", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, "У ScreenBackdrop нет поля sprite");
            field.SetValue(backdrop, sprite);

            backdrop.Apply(ScreenTheme.Default);

            Assert.AreSame(sprite, image.sprite, "Спрайт из инспектора не применился");
            Object.Destroy(go);
            Object.Destroy(texture);
            yield return null;
        }

        private static IEnumerator LoadMain()
        {
            yield return SceneManager.LoadSceneAsync("Main");
            yield return null;
        }

        private static void Click(string name)
        {
            var go = GameObject.Find(name);
            Assert.IsNotNull(go, $"Кнопка «{name}» не найдена");
            var button = go.GetComponent<Button>();
            Assert.IsNotNull(button, $"У «{name}» нет Button");
            button.onClick.Invoke();
        }
    }
}
