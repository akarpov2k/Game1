#nullable enable
using System;
using AnomalyPath.Runtime.Screens;
using AnomalyPath.Runtime.Ui.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Screens.Menu
{
    /// <summary>Информационный экран: «Об игре», «Авторы» и другие тексты.</summary>
    public sealed class InfoScreen : ScreenBase
    {
        private readonly IUiHost _host;
        private readonly Text _title;
        private readonly Text _body;
        private Action _back = () => { };

        /// <summary>Создаёт панель сведений.</summary>
        /// <param name="host">Хост интерфейса: сервисы и переходы.</param>
        /// <param name="canvas">Родительский канвас.</param>
        public InfoScreen(IUiHost host, Transform canvas)
            : base(host, canvas, "Info")
        {
            _host = host;
            var column = host.Ui.CreateColumn(Content, 1200);
            _title = host.Ui.CreateTitle(column, string.Empty);
            _body = host.Ui.CreateText(column, string.Empty, 22, TextAnchor.UpperLeft);
            _body.GetComponent<LayoutElement>().preferredHeight = 500;
            host.Ui.CreateButton(column, UiText.Get(host.Texts, UiTextKey.Common_Back), () => _back());
        }

        /// <summary>Esc работает как «Назад»: например, из «Авторов» возвращает в меню.</summary>
        /// <returns><c>true</c>, если Esc обработан.</returns>
        public override bool HandleEscape()
        {
            _back();
            return true;
        }

        /// <summary>Показывает текст.</summary>
        /// <param name="title">Заголовок.</param>
        /// <param name="body">Текст.</param>
        /// <param name="back">Куда вернуться по кнопке «Назад».</param>
        public void Show(string title, string body, Action back)
        {
            _title.text = title;
            _body.text = body;
            _back = back;
            _host.Router.Show(Panel);
        }
    }
}
