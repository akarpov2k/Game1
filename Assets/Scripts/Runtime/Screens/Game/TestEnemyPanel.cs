#nullable enable
using System;
using AnomalyPath.Core.Combat;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Templates;
using AnomalyPath.Runtime.Ui.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AnomalyPath.Runtime.Screens.Game
{
    /// <summary>
    /// ВРЕМЕННАЯ тестовая обвязка основного экрана: тестовый противник и кнопка «Убить».
    /// Нужна, пока боевого экрана нет: кнопка прогоняет настоящий бой до победы и показывает его
    /// последствия — опыт, лут и события квестов. Удаляется вместе с пунктом ROADMAP
    /// «Временная тестовая обвязка» (блок I).
    /// </summary>
    public sealed class TestEnemyPanel
    {
        /// <summary>Защитный предел шагов тестового боя: дальше считаем, что убить не удалось.</summary>
        private const int MaxKillActions = 256;

        private readonly IUiHost _host;
        private readonly GameObject _root;
        private readonly Text _enemyName;
        private readonly Button _kill;
        private readonly Action _afterKill;
        private ActorTemplate? _enemy;
        private ItemTemplate? _weapon;
        private int _weaponSerial;

        /// <summary>Создаёт блок тестового противника, скрытый до первой игры.</summary>
        /// <param name="host">Хост интерфейса: сессия, сервисы и переходы.</param>
        /// <param name="parent">Родитель, обычно панель основного экрана.</param>
        /// <param name="afterKill">Что выполнить после завершённого тестового боя.</param>
        /// <exception cref="ArgumentNullException">Хост или обработчик не заданы.</exception>
        public TestEnemyPanel(IUiHost host, Transform parent, Action afterKill)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _afterKill = afterKill ?? throw new ArgumentNullException(nameof(afterKill));

            _root = new GameObject("TestEnemy", typeof(RectTransform), typeof(Image));
            _root.transform.SetParent(parent, false);
            var rect = (RectTransform)_root.transform;
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-PlayMetrics.SidePadding, -PlayMetrics.TestEnemyTop);
            rect.sizeDelta = new Vector2(PlayMetrics.TestEnemyWidth, PlayMetrics.TestEnemyHeight);
            var background = _root.GetComponent<Image>();
            background.color = PlayPalette.ActionPanel;
            background.raycastTarget = false;

            var innerWidth = PlayMetrics.TestEnemyWidth - (2f * PlayMetrics.TestEnemyPadding);
            UiWidgets.Label(
                host,
                _root.transform,
                "TestEnemyLabel",
                UiText.Get(host.Texts, UiTextKey.Game_Test_Enemy_Label),
                PlayMetrics.TestEnemyLabelFontSize,
                TextAnchor.MiddleLeft,
                UiFactory.MutedTextColor,
                PlayMetrics.TestEnemyPadding,
                PlayMetrics.TestEnemyPadding,
                innerWidth,
                PlayMetrics.TestEnemyLabelHeight);

            _enemyName = UiWidgets.Label(
                host,
                _root.transform,
                "TestEnemyName",
                string.Empty,
                PlayMetrics.TestEnemyNameFontSize,
                TextAnchor.MiddleLeft,
                Color.white,
                PlayMetrics.TestEnemyPadding,
                PlayMetrics.TestEnemyPadding + PlayMetrics.TestEnemyLabelHeight,
                innerWidth,
                PlayMetrics.TestEnemyNameHeight);

            _kill = host.Ui.CreateButton(
                _root.transform,
                UiText.Get(host.Texts, UiTextKey.Game_Test_Enemy_Kill),
                Kill);
            _kill.gameObject.name = "TestEnemyKill";
            UiWidgets.Place(
                (RectTransform)_kill.transform,
                PlayMetrics.TestEnemyPadding,
                PlayMetrics.TestEnemyPadding + PlayMetrics.TestEnemyLabelHeight + PlayMetrics.TestEnemyNameHeight,
                innerWidth,
                PlayMetrics.TestEnemyButtonHeight);

            _root.SetActive(false);
        }

        /// <summary>Обновляет блок по текущей сессии; без сессии или данных блок скрыт.</summary>
        public void Refresh()
        {
            _enemy = FindEnemy();
            _weapon = FindWeapon();
            var session = _host.Session;
            if (session == null || _enemy == null || _weapon == null)
            {
                Hide();
                return;
            }

            _enemyName.text = _host.Texts.Get(_enemy.NameKey);
            _kill.interactable = !session.InCombat;
            _root.SetActive(true);
        }

        /// <summary>Скрывает блок: например, в главном меню и вне игры.</summary>
        public void Hide() => _root.SetActive(false);

        private void Kill()
        {
            var session = _host.Session;
            var enemy = _enemy;
            var weaponTemplate = _weapon;
            if (session == null || enemy == null || weaponTemplate == null)
            {
                return;
            }

            if (session.InCombat)
            {
                _host.Sessions.EndCombat(session);
            }

            var rules = _host.Rules.Require();
            var playerActor = _host.Actors.Get(rules.PlayerActorId);
            if (!_host.Sessions.StartCombat(session, playerActor, enemy, DistanceZone.Mid))
            {
                _host.Toast(UiText.Get(_host.Texts, UiTextKey.Game_Test_Kill_Failed));
                return;
            }

            ResolveFight(session, weaponTemplate);
        }

        /// <summary>
        /// Проводит бой до смерти противника. Противник в тестовой обвязке не атакует:
        /// кнопка проверяет итог боя, а не убивает игрока и не отбирает инвентарь.
        /// </summary>
        private void ResolveFight(AnomalyPath.Core.Session.GameSession session, ItemTemplate weaponTemplate)
        {
            var weapon = CreateWeapon(weaponTemplate);
            var guard = 0;
            while (session.Combat != null && !session.Combat.IsFinished && guard < MaxKillActions)
            {
                guard++;
                var combat = session.Combat;
                if (!ReferenceEquals(combat.Current, combat.Player))
                {
                    _host.Sessions.EndTurn(session);
                    continue;
                }

                if (_host.Sessions.TryAttack(session, weapon, out _))
                {
                    continue;
                }

                if (weapon.MagazineRounds <= 0)
                {
                    weapon = CreateWeapon(weaponTemplate);
                    continue;
                }

                _host.Sessions.EndTurn(session);
            }

            var outcome = _host.Sessions.ApplyCombatOutcome(session);
            if (outcome.Status != CombatOutcomeStatus.Victory)
            {
                _host.Sessions.EndCombat(session);
                _host.Toast(UiText.Get(_host.Texts, UiTextKey.Game_Test_Kill_Failed));
                _afterKill();
                return;
            }

            _afterKill();
            _host.Toast(UiText.Format(_host.Texts, UiTextKey.Game_Test_Victory, outcome.XpGained));
            _host.ShowLoot(outcome.Loot);
        }

        private ItemInstance CreateWeapon(ItemTemplate template)
        {
            _weaponSerial++;
            var weapon = template.Weapon!;
            return new ItemInstance(
                "test_weapon_" + _weaponSerial,
                template.Id,
                1,
                null,
                null,
                null,
                weapon.Magazine);
        }

        /// <summary>Первый по id противник с таблицей лута: тестовый бой даёт лут.</summary>
        private ActorTemplate? FindEnemy()
        {
            ActorTemplate? found = null;
            foreach (var actor in _host.Actors.All)
            {
                if (actor.Type != ActorType.Enemy || actor.LootTableId == null)
                {
                    continue;
                }

                if (found == null || string.CompareOrdinal(actor.Id, found.Id) < 0)
                {
                    found = actor;
                }
            }

            return found;
        }

        /// <summary>Первое по id оружие с магазином: тестовому выстрелу нужны патроны.</summary>
        private ItemTemplate? FindWeapon()
        {
            ItemTemplate? found = null;
            foreach (var item in _host.Items.All)
            {
                if (item.Weapon == null || item.Weapon.Magazine <= 0)
                {
                    continue;
                }

                if (found == null || string.CompareOrdinal(item.Id, found.Id) < 0)
                {
                    found = item;
                }
            }

            return found;
        }
    }
}
