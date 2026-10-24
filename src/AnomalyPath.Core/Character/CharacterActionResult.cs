#nullable enable
namespace AnomalyPath.Core.Character
{
    /// <summary>Итог действия с предметом на экране персонажа.</summary>
    public enum CharacterActionResult
    {
        /// <summary>Действие выполнено.</summary>
        Ok,

        /// <summary>Предмет не найден ни в инвентаре, ни в экипировке.</summary>
        ItemMissing,

        /// <summary>Предмет нельзя надеть в этот слот.</summary>
        WrongSlot,

        /// <summary>В инвентаре нет места для освобождаемого предмета.</summary>
        NoRoom,

        /// <summary>Предмет не является артефактом.</summary>
        NotArtifact,

        /// <summary>Нет свободного слота под артефакт.</summary>
        NoFreeArtifactSlot,

        /// <summary>Предмет — не оружие, разряжать нечего.</summary>
        NotAWeapon,

        /// <summary>Магазин пуст.</summary>
        NothingToUnload,

        /// <summary>Магазин уже полон, заряжать нечего.</summary>
        MagazineFull,

        /// <summary>Предмет — не патроны.</summary>
        NotAmmo,

        /// <summary>Патроны не подходят к оружию по калибру.</summary>
        AmmoCaliberMismatch,

        /// <summary>В инвентаре нет патронов подходящего калибра.</summary>
        NoAmmo,

        /// <summary>В данных нет патронов нужного калибра.</summary>
        AmmoTemplateMissing,
    }
}
