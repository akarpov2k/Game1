using System;
using System.Collections.Generic;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Di
{
    /// <summary>
    /// Минимальный DI-контейнер: регистрация синглтонов и разрешение по интерфейсу.
    /// Зависимости передаются в конструктор, статические синглтоны запрещены.
    /// </summary>
    public sealed class ServiceContainer
    {
        private readonly Dictionary<Type, object> _singletons = new Dictionary<Type, object>();

        /// <summary>Регистрирует готовый экземпляр под типом интерфейса.</summary>
        /// <typeparam name="TService">Тип интерфейса.</typeparam>
        /// <param name="instance">Экземпляр реализации.</param>
        /// <returns>Этот же контейнер для цепочки вызовов.</returns>
        /// <exception cref="ArgumentNullException">Экземпляр не задан.</exception>
        /// <exception cref="InvalidOperationException">Тип уже зарегистрирован.</exception>
        public ServiceContainer RegisterSingleton<TService>(TService instance)
            where TService : class
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var type = typeof(TService);
            if (_singletons.ContainsKey(type))
            {
                throw new InvalidOperationException(CoreText.Format(CoreErrors.ServiceAlreadyRegistered, type.Name));
            }

            _singletons[type] = instance;
            return this;
        }

        /// <summary>Возвращает зарегистрированный сервис.</summary>
        /// <typeparam name="TService">Тип интерфейса.</typeparam>
        /// <returns>Экземпляр сервиса.</returns>
        /// <exception cref="InvalidOperationException">Сервис не зарегистрирован.</exception>
        public TService Resolve<TService>()
            where TService : class
        {
            var type = typeof(TService);
            if (_singletons.TryGetValue(type, out var instance))
            {
                return (TService)instance;
            }

            throw new InvalidOperationException(CoreText.Format(CoreErrors.ServiceNotRegistered, type.Name));
        }

        /// <summary>Проверяет, зарегистрирован ли сервис.</summary>
        /// <typeparam name="TService">Тип интерфейса.</typeparam>
        /// <returns><c>true</c>, если сервис зарегистрирован.</returns>
        public bool IsRegistered<TService>() => _singletons.ContainsKey(typeof(TService));
    }
}
