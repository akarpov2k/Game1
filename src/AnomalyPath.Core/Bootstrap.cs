#nullable enable
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Combat;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Di;
using AnomalyPath.Core.Equipment;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Repositories;
using AnomalyPath.Core.Services;
using AnomalyPath.Core.Validation;

namespace AnomalyPath.Core
{
    /// <summary>
    /// Сборка приложения: проверка данных, репозитории, сервисы, контейнер.
    /// UI и MonoBehaviour сюда не входят: всё это чистый C#.
    /// </summary>
    public static class Bootstrap
    {
        /// <summary>Каталог текстов по умолчанию, относительно корня данных.</summary>
        public const string DefaultTextCatalog = "Localization/errors.ru.json";

        /// <summary>Собирает контейнер сервисов из папки данных.</summary>
        /// <param name="dataRoot">Корень данных (в редакторе — <c>Assets/Data</c>).</param>
        /// <returns>Контейнер с зарегистрированными репозиториями и сервисами.</returns>
        /// <exception cref="ValidationException">Данные не прошли проверку.</exception>
        public static ServiceContainer Build(string dataRoot)
        {
            var result = Preflight.Run(dataRoot);
            if (!result.CanStart)
            {
                throw new ValidationException(result.Report);
            }

            return result.Services!;
        }

        /// <summary>Строит репозитории и сервисы из уже загруженных данных — без повторного чтения файлов.</summary>
        /// <param name="data">Результат загрузки.</param>
        /// <returns>Контейнер с зарегистрированными репозиториями и сервисами.</returns>
        /// <exception cref="DataException">Сущность не соответствует схеме.</exception>
        public static ServiceContainer BuildServices(LoadedData data)
        {
            var items = ItemRepository.FromLoadedData(data);
            var damageTypes = DamageTypeRepository.FromLoadedData(data);
            var formulas = FormulaRepository.FromLoadedData(data);
            var actors = ActorRepository.FromLoadedData(data);
            var combatSettings = LoadCombatSettings(data);
            var balance = LoadBalance(data);
            var instanceIds = new SequentialInstanceIdGenerator();
            var equipmentSettings = LoadEquipmentSettings(data);

            var inventory = new InventoryService(items, instanceIds);

            var container = new ServiceContainer();
            container.RegisterSingleton<IItemRepository>(items);
            container.RegisterSingleton<IItemCatalogService>(new ItemCatalogService(items));
            container.RegisterSingleton<IDamageTypeRepository>(damageTypes);
            container.RegisterSingleton<IFormulaRepository>(formulas);
            container.RegisterSingleton<IActorRepository>(actors);
            container.RegisterSingleton<IInstanceIdGenerator>(instanceIds);
            container.RegisterSingleton<IInventoryService>(inventory);
            container.RegisterSingleton<IEquipmentSettings>(equipmentSettings);
            var equipmentService = new EquipmentService(items, inventory);
            container.RegisterSingleton<IEquipmentService>(equipmentService);
            var wear = new ArmorWearService(items);
            container.RegisterSingleton<IArmorWearService>(wear);
            container.RegisterSingleton<ICombatRandom>(new SeededCombatRandom(1));
            container.RegisterSingleton<ICombatService>(new CombatService(items, damageTypes, equipmentService, wear, formulas, combatSettings, balance, new SeededCombatRandom(1), inventory));
            return container;
        }

        private static CombatSettingsTemplate LoadCombatSettings(LoadedData data)
        {
            var records = data.All("combat");
            return records.Count == 0 ? CombatSettingsTemplate.Default : CombatSettingsTemplate.FromJson(records[0], "combat");
        }

        private static BalanceTemplate LoadBalance(LoadedData data)
        {
            var records = data.All("balance");
            return records.Count == 0 ? BalanceTemplate.Empty : BalanceTemplate.FromJson(records[0]);
        }

        private static IEquipmentSettings LoadEquipmentSettings(LoadedData data)
        {
            var records = data.All("equipment");
            return records.Count == 0
                ? EquipmentSettings.Default
                : EquipmentSettings.FromJson(records[0], "equipment");
        }
    }
}
