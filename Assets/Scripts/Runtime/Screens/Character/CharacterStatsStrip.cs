#nullable enable
using System;
using AnomalyPath.Core.Player;
using AnomalyPath.Runtime.Ui.Core;
using UnityEngine;

namespace AnomalyPath.Runtime.Screens.Character
{
    /// <summary>
    /// Характеристики внутри фрейма снаряжения: четыре строки во всю ширину фрейма,
    /// без отдельного блока и зазора от экипировки.
    /// </summary>
    public sealed class CharacterStatsStrip
    {
        private const float RowStep = 26f;
        private const float FirstRow = 690f;
        private const float BarWidth = 528f;

        private readonly IUiHost _host;
        private readonly Transform _content;

        /// <summary>Создаёт блок характеристик.</summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="content">Корень содержимого экрана.</param>
        public CharacterStatsStrip(IUiHost host, Transform content)
        {
            _host = host;
            _content = content;
        }

        /// <summary>Рисует строки характеристик поверх фона фрейма снаряжения.</summary>
        /// <param name="player">Состояние игрока.</param>
        public void Build(PlayerState player)
        {
            var world = _host.Session?.State.World;
            var stamina = world?.Stamina ?? 0;
            var staminaMax = world?.StaminaMax ?? 1;
            var needsMax = player.Needs.Max;

            AddRow(0, "Health", UiText.Get(_host.Texts, UiTextKey.Character_Stat_Health), player.Health, MaxHealth(player), CharacterPalette.Health);
            AddRow(1, "Food", UiText.Get(_host.Texts, UiTextKey.Character_Stat_Food), player.Needs.Hunger, needsMax, CharacterPalette.Hunger);
            AddRow(2, "Water", UiText.Get(_host.Texts, UiTextKey.Character_Stat_Water), player.Needs.Thirst, needsMax, CharacterPalette.Thirst);
            AddRow(3, "Stamina", UiText.Get(_host.Texts, UiTextKey.Character_Stat_Stamina), stamina, staminaMax, CharacterPalette.Stamina);
        }

        private void AddRow(int index, string key, string caption, int value, int max, Color color)
        {
            var safeMax = max < 1 ? 1 : max;
            var clamped = value < 0 ? 0 : value > safeMax ? safeMax : value;
            var y = FirstRow + (index * RowStep);

            CharacterWidgets.Label(_host, _content, "Stat" + key + "Label", caption, 12, TextAnchor.MiddleLeft,
                UiFactory.MutedTextColor, 16f, y, 380f, 14f);
            CharacterWidgets.Label(_host, _content, "Stat" + key + "Value", clamped + "/" + safeMax,
                12, TextAnchor.MiddleRight, UiFactory.TextColor, 410f, y, 134f, 14f);
            CharacterWidgets.Box(_host, _content, "Stat" + key + "Bar", 16f, y + 15f, BarWidth, 8f,
                CharacterPalette.Bar);
            var fill = BarWidth * clamped / safeMax;
            if (fill > 0f)
            {
                CharacterWidgets.Box(_host, _content, "Stat" + key + "Fill", 16f, y + 15f, fill, 8f, color);
            }
        }

        private int MaxHealth(PlayerState player)
        {
            try
            {
                var rules = _host.Rules.Rules;
                var bonus = _host.Talents.GetModifiers(player).HealthMax;
                if (rules == null)
                {
                    return Math.Max(1, player.Health + bonus);
                }

                return (_host.Actors.Get(rules.PlayerActorId).Combat?.Health ?? Math.Max(1, player.Health)) + bonus;
            }
            catch (Exception)
            {
                return Math.Max(1, player.Health);
            }
        }
    }
}
