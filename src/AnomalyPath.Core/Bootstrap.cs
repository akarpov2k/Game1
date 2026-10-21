#nullable enable
using AnomalyPath.Core.Abstractions;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Di;
using AnomalyPath.Core.Localization;
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

            var container = new ServiceContainer();
            container.RegisterSingleton<IItemRepository>(items);
            container.RegisterSingleton<IItemCatalogService>(new ItemCatalogService(items));
            return container;
        }
    }
}
