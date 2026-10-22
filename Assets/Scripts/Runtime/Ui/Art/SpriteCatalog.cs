#nullable enable
using System;
using System.Collections.Generic;
using AnomalyPath.Runtime.Ui.Core;
using UnityEngine;

namespace AnomalyPath.Runtime.Ui.Art
{
    /// <summary>
    /// Каталог спрайтов: пары «id из данных → спрайт». Заполняется редакторным инструментом
    /// <c>Tools/AnomalyPath/Собрать каталог спрайтов</c>, ссылка на ассет лежит на <c>GameScreen</c>.
    /// </summary>
    [CreateAssetMenu(menuName = "AnomalyPath/Sprite Catalog", fileName = "SpriteCatalog")]
    public sealed class SpriteCatalog : ScriptableObject, ISpriteCatalog
    {
        /// <summary>Запись каталога: id и спрайт.</summary>
        [Serializable]
        public sealed class Entry
        {
            [SerializeField] private string id = string.Empty;
            [SerializeField] private Sprite? value;

            /// <summary>Создаёт пустую запись.</summary>
            public Entry()
            {
            }

            /// <summary>Создаёт запись каталога.</summary>
            /// <param name="id">Id из данных.</param>
            /// <param name="sprite">Спрайт.</param>
            public Entry(string id, Sprite sprite)
            {
                this.id = id;
                value = sprite;
            }

            /// <summary>Id из данных.</summary>
            public string Id => id;

            /// <summary>Спрайт или <c>null</c>.</summary>
            public Sprite? Value => value;
        }

        [SerializeField] private List<Entry> entries = new List<Entry>();

        private Dictionary<string, Sprite>? _map;

        /// <summary>Записи каталога.</summary>
        public IReadOnlyList<Entry> Entries => entries;

        /// <inheritdoc />
        public bool TryGet(string id, out Sprite? sprite)
        {
            sprite = null;
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            _map ??= BuildMap();
            return _map.TryGetValue(id, out sprite);
        }

        /// <summary>Заменяет все записи каталога.</summary>
        /// <param name="items">Новые записи; записи без спрайта пропускаются.</param>
        public void SetEntries(IEnumerable<Entry> items)
        {
            entries.Clear();
            foreach (var item in items)
            {
                if (!string.IsNullOrEmpty(item.Id) && item.Value != null)
                {
                    entries.Add(new Entry(item.Id, item.Value!));
                }
            }

            _map = null;
        }

        /// <summary>Очищает каталог.</summary>
        public void Clear()
        {
            entries.Clear();
            _map = null;
        }

        private Dictionary<string, Sprite> BuildMap()
        {
            var map = new Dictionary<string, Sprite>(StringComparer.Ordinal);
            foreach (var entry in entries)
            {
                if (!string.IsNullOrEmpty(entry.Id) && entry.Value != null)
                {
                    map[entry.Id] = entry.Value!;
                }
            }

            return map;
        }
    }
}
