#nullable enable
using UnityEngine;

namespace AnomalyPath.Runtime.Ui.Core
{
    /// <summary>Пустой каталог: спрайтов нет, UI рисует цветные заглушки.</summary>
    public sealed class NullSpriteCatalog : ISpriteCatalog
    {
        /// <summary>Единственный экземпляр: каталог без состояния.</summary>
        public static readonly NullSpriteCatalog Instance = new NullSpriteCatalog();

        private NullSpriteCatalog()
        {
        }

        /// <inheritdoc />
        public bool TryGet(string id, out Sprite? sprite)
        {
            sprite = null;
            return false;
        }
    }
}
