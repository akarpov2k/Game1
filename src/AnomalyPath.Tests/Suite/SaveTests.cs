#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using AnomalyPath.Core;
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Equipment;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Json;
using AnomalyPath.Core.Player;
using AnomalyPath.Core.Save;
using AnomalyPath.Core.Services;
using AnomalyPath.Core.Session;
using AnomalyPath.Core.Trade;
using AnomalyPath.Core.World;
using InventoryEquipment = AnomalyPath.Core.Inventory.Equipment;

namespace AnomalyPath.Tests
{
    /// <summary>Сохранения: JSON-писатель, round-trip мира и игрока, слоты и ошибки.</summary>
    public sealed partial class SaveTests : TestBase
    {
        private readonly string _dataRoot;

        /// <summary>Создаёт категорию тестов сохранений.</summary>
        /// <param name="dataRoot">Корень данных проекта.</param>
        public SaveTests(string dataRoot)
        {
            _dataRoot = dataRoot;
        }

        /// <inheritdoc />
        public override void Run()
        {
            JsonWriterTests();
            var services = Bootstrap.Build(_dataRoot);
            var generator = services.Resolve<IWorldGenerator>();
            var travel = services.Resolve<ITravelService>();
            var items = services.Resolve<IItemRepository>();
            SlotNameTests();
            RoundTripTests(generator, travel, items);
            PlayerTests(generator, items);
            StorageTests(generator, items);
        }
    }
}
