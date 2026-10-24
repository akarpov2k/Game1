#nullable enable
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Character;
using AnomalyPath.Core.Combat;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Death;
using AnomalyPath.Core.Di;
using AnomalyPath.Core.Equipment;
using AnomalyPath.Core.Inventory;
using AnomalyPath.Core.Localization;
using AnomalyPath.Core.Nicks;
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

        /// <summary>Каталог текстов игры на основном языке (русский).</summary>
        public const string DefaultGameTextCatalog = "Localization/strings.ru.json";

        /// <summary>Каталог текстов игры на втором языке: откат, если ключа нет в основном.</summary>
        public const string FallbackGameTextCatalog = "Localization/strings.en.json";

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
        /// <param name="texts">Тексты игры; <c>null</c> — пустой каталог, ключи возвращаются как есть.</param>
        /// <returns>Контейнер с зарегистрированными репозиториями и сервисами.</returns>
        /// <exception cref="DataException">Сущность не соответствует схеме.</exception>
        public static ServiceContainer BuildServices(LoadedData data, IGameTexts? texts = null)
        {
            var items = ItemRepository.FromLoadedData(data);
            var damageTypes = DamageTypeRepository.FromLoadedData(data);
            var formulas = FormulaRepository.FromLoadedData(data);
            var actors = ActorRepository.FromLoadedData(data);
            var locations = LocationRepository.FromLoadedData(data);
            var topology = TopologyRepository.FromLoadedData(data);
            var loadouts = LoadoutRepository.FromLoadedData(data);
            var combatSettings = LoadCombatSettings(data);
            var balance = LoadBalance(data);
            var instanceIds = new SequentialInstanceIdGenerator();
            var equipmentSettings = LoadEquipmentSettings(data);
            var rules = GameRulesRepository.FromLoadedData(data);
            var nicknames = LoadNicknames(data);

            var inventory = new InventoryService(items, instanceIds);
            var travel = new TravelService();
            var world = new WorldGenerator(locations, topology);

            var container = new ServiceContainer();
            container.RegisterSingleton<IItemRepository>(items);
            container.RegisterSingleton<IGameTexts>(texts ?? GameTexts.Empty);
            container.RegisterSingleton<IItemCatalogService>(new ItemCatalogService(items));
            container.RegisterSingleton<IDamageTypeRepository>(damageTypes);
            container.RegisterSingleton<IFormulaRepository>(formulas);
            container.RegisterSingleton<IActorRepository>(actors);
            container.RegisterSingleton<ILocationRepository>(locations);
            container.RegisterSingleton<ITopologyRepository>(topology);
            container.RegisterSingleton<ILoadoutRepository>(loadouts);
            container.RegisterSingleton<IWorldGenerator>(world);
            container.RegisterSingleton<ITravelService>(travel);
            container.RegisterSingleton<IInstanceIdGenerator>(instanceIds);
            container.RegisterSingleton<IInventoryService>(inventory);
            container.RegisterSingleton<IEquipmentSettings>(equipmentSettings);
            container.RegisterSingleton<IGameRulesRepository>(rules);
            container.RegisterSingleton<INicknameGenerator>(nicknames);
            var deathRandom = new SeededDeathRandom(1);
            container.RegisterSingleton<IDeathRandom>(deathRandom);
            container.RegisterSingleton<IDeathService>(new DeathService(items, deathRandom));
            var equipmentService = new EquipmentService(items, inventory);
            container.RegisterSingleton<IEquipmentService>(equipmentService);
            container.RegisterSingleton<ICharacterActions>(new CharacterActions(items, inventory, equipmentService));
            var wear = new ArmorWearService(items);
            container.RegisterSingleton<IArmorWearService>(wear);
            container.RegisterSingleton<ICombatRandom>(new SeededCombatRandom(1));
            var combatService = new CombatService(items, damageTypes, equipmentService, wear, formulas, combatSettings, balance, new SeededCombatRandom(1), inventory);
            container.RegisterSingleton<ICombatService>(combatService);
            var loadoutPreviews = new LoadoutPreviewService(
                rules,
                loadouts,
                items,
                inventory,
                equipmentService,
                equipmentSettings,
                instanceIds);
            container.RegisterSingleton<ILoadoutPreviewService>(loadoutPreviews);
            container.RegisterSingleton<INewGameService>(new NewGameService(
                rules,
                world,
                actors,
                loadoutPreviews,
                nicknames));
            return container;
        }

        private static CombatSettingsTemplate LoadCombatSettings(LoadedData data)
        {
            var records = data.All("combat");
            if (records.Count == 0)
            {
                throw new DataException("combat", CoreErrors.FileNotFound);
            }

            return CombatSettingsTemplate.FromJson(records[0], "combat");
        }

        private static BalanceTemplate LoadBalance(LoadedData data)
        {
            var records = data.All("balance");
            return records.Count == 0 ? BalanceTemplate.Empty : BalanceTemplate.FromJson(records[0]);
        }

        private static IEquipmentSettings LoadEquipmentSettings(LoadedData data)
        {
            var records = data.All("equipment");
            if (records.Count == 0)
            {
                throw new DataException("equipment", CoreErrors.FileNotFound);
            }

            return EquipmentSettings.FromJson(records[0], "equipment");
        }

        private static INicknameGenerator LoadNicknames(LoadedData data)
        {
            var records = data.All("nicks");
            if (records.Count == 0)
            {
                return new NicknameGenerator(System.Array.Empty<string>());
            }

            var template = NicknameCatalogTemplate.FromJson(records[0], "nicks");
            return new NicknameGenerator(template.Names);
        }
    }
}
