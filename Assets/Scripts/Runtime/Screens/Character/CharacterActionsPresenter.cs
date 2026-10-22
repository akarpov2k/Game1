#nullable enable
using System;
using AnomalyPath.Core.Character;
using AnomalyPath.Core.Equipment;
using AnomalyPath.Core.Player;
using AnomalyPath.Runtime.Ui.Core;

namespace AnomalyPath.Runtime.Screens.Character
{
    /// <summary>Выполняет действия с предметами и сообщает результат на экран.</summary>
    public sealed class CharacterActionsPresenter
    {
        private readonly IUiHost _host;
        private readonly Action<string, bool> _notify;
        private readonly Action _refresh;

        /// <summary>Создаёт исполнитель действий.</summary>
        /// <param name="host">Хост интерфейса.</param>
        /// <param name="notify">Куда отдать сообщение: текст и признак успеха.</param>
        /// <param name="refresh">Что вызвать после действия.</param>
        public CharacterActionsPresenter(IUiHost host, Action<string, bool> notify, Action refresh)
        {
            _host = host;
            _notify = notify;
            _refresh = refresh;
        }

        /// <summary>Надевает предмет из инвентаря в указанный слот.</summary>
        /// <param name="instanceId">Id экземпляра.</param>
        /// <param name="slot">Слот.</param>
        public void Equip(string instanceId, EquipmentSlot slot) =>
            Run(player => _host.Actions.Equip(player.Inventory, player.Equipment, instanceId, slot), "Надето");

        /// <summary>Надевает предмет в подходящий слот (или в столбец артефактов).</summary>
        /// <param name="instanceId">Id экземпляра.</param>
        public void EquipAuto(string instanceId)
        {
            var player = Player;
            if (player == null)
            {
                _notify("Нет активной игры", false);
                return;
            }

            var item = player.Inventory.Find(instanceId);
            if (item == null)
            {
                Finish(CharacterActionResult.ItemMissing, "Надето");
                return;
            }

            var template = CharacterWidgets.Template(_host, item);
            if (template != null && !string.IsNullOrWhiteSpace(template.ArtifactType))
            {
                EquipArtifact(instanceId);
                return;
            }

            var slot = _host.Actions.DefaultSlot(player.Equipment, item);
            if (slot == null)
            {
                Finish(CharacterActionResult.WrongSlot, "Надето");
                return;
            }

            Equip(instanceId, slot.Value);
        }

        /// <summary>Надевает артефакт из инвентаря.</summary>
        /// <param name="instanceId">Id экземпляра.</param>
        public void EquipArtifact(string instanceId) =>
            Run(player => _host.Actions.EquipArtifact(player.Inventory, player.Equipment, instanceId), "Артефакт надет");

        /// <summary>Снимает предмет из слота.</summary>
        /// <param name="slot">Слот.</param>
        public void Unequip(EquipmentSlot slot) =>
            Run(player => _host.Actions.Unequip(player.Inventory, player.Equipment, slot), "Снято");

        /// <summary>Снимает артефакт из слота.</summary>
        /// <param name="index">Индекс слота.</param>
        public void UnequipArtifact(int index) =>
            Run(player => _host.Actions.UnequipArtifact(player.Inventory, player.Equipment, index), "Снято");

        /// <summary>Разряжает оружие в инвентарь.</summary>
        /// <param name="instanceId">Id экземпляра.</param>
        public void Unload(string instanceId)
        {
            var player = Player;
            if (player == null)
            {
                _notify("Нет активной игры", false);
                return;
            }

            var result = _host.Actions.Unload(player.Inventory, player.Equipment, instanceId, out var rounds);
            Finish(result, "Разряжено: " + rounds);
        }

        /// <summary>Разбирает предмет (тестовый режим).</summary>
        /// <param name="instanceId">Id экземпляра.</param>
        public void Disassemble(string instanceId) =>
            Run(player => _host.Actions.Disassemble(player.Inventory, player.Equipment, instanceId),
                "Разобрано (тестовый режим)");

        /// <summary>Выбрасывает предмет.</summary>
        /// <param name="instanceId">Id экземпляра.</param>
        public void Discard(string instanceId) =>
            Run(player => _host.Actions.Discard(player.Inventory, player.Equipment, instanceId), "Выброшено");

        private PlayerState? Player => _host.Session?.State.Player;

        private void Run(Func<PlayerState, CharacterActionResult> action, string okText)
        {
            var player = Player;
            if (player == null)
            {
                _notify("Нет активной игры", false);
                return;
            }

            Finish(action(player), okText);
        }

        private void Finish(CharacterActionResult result, string okText)
        {
            var ok = result == CharacterActionResult.Ok;
            _notify(ok ? okText : MessageFor(result), ok);
            _refresh();
        }

        private static string MessageFor(CharacterActionResult result) => result switch
        {
            CharacterActionResult.Ok => "Готово",
            CharacterActionResult.ItemMissing => "Предмет не найден",
            CharacterActionResult.WrongSlot => "Сюда нельзя надеть",
            CharacterActionResult.NoRoom => "Нет места в инвентаре",
            CharacterActionResult.NotArtifact => "Это не артефакт",
            CharacterActionResult.NoFreeArtifactSlot => "Нет свободного слота артефакта",
            CharacterActionResult.NotAWeapon => "Это не оружие",
            CharacterActionResult.NothingToUnload => "Магазин пуст",
            CharacterActionResult.AmmoTemplateMissing => "Нет патронов этого калибра",
            _ => "Не получилось",
        };
    }
}
