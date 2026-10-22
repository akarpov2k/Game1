#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace AnomalyPath.Runtime.Ui.Core
{
    /// <summary>Переключает полноэкранные панели: активна ровно одна.</summary>
    public sealed class ScreenRouter
    {
        private readonly List<GameObject> _panels = new List<GameObject>();

        /// <summary>Текущая показанная панель или <c>null</c>.</summary>
        public GameObject? Current { get; private set; }

        /// <summary>Добавляет панель в набор экранов.</summary>
        /// <param name="panel">Панель экрана.</param>
        public void Add(GameObject panel) => _panels.Add(panel);

        /// <summary>Показывает одну панель и скрывает остальные.</summary>
        /// <param name="panel">Панель, которую нужно показать.</param>
        public void Show(GameObject panel)
        {
            foreach (var item in _panels)
            {
                item.SetActive(item == panel);
            }

            Current = panel;
        }

        /// <summary>Скрывает все панели.</summary>
        public void HideAll()
        {
            foreach (var item in _panels)
            {
                item.SetActive(false);
            }

            Current = null;
        }
    }
}
