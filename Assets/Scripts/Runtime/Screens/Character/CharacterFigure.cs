#nullable enable
using AnomalyPath.Core.Player;
using AnomalyPath.Runtime.Ui.Core;
using UnityEngine;

namespace AnomalyPath.Runtime.Screens.Character
{
    /// <summary>Фигура персонажа по центру фрейма снаряжения.</summary>
    public sealed class CharacterFigure
    {
        private readonly IUiHost _host;
        private readonly Transform _content;

        /// <summary>Создаёт фигуру.</summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="content">Корень содержимого экрана.</param>
        public CharacterFigure(IUiHost host, Transform content)
        {
            _host = host;
            _content = content;
        }

        /// <summary>Рисует фигуру персонажа.</summary>
        public void Build()
        {
            CharacterWidgets.Box(_host, _content, "Figure", 184f, 40f, 192f, 400f, CharacterPalette.FigureArea);
            CharacterWidgets.Box(_host, _content, "FigureHead", 243f, 82f, 70f, 70f, CharacterPalette.Figure);
            CharacterWidgets.Box(_host, _content, "FigureBody", 205f, 160f, 150f, 160f, CharacterPalette.Figure);
            CharacterWidgets.Box(_host, _content, "FigureLegs", 215f, 320f, 130f, 100f, CharacterPalette.Figure);
        }
    }
}
