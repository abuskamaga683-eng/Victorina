using System;
using System.Collections.Generic;
using UnityEngine;

namespace Victorina.Infrastructure
{
    public class ServiceContainer
    {
        private readonly Dictionary<Type, object> _services = new();

        public void Register<TInterface>(TInterface instance)
        {
            if (instance == null)
            {
                Debug.LogError($"[ServiceContainer] ОШИБКА: попытка зарегистрировать null для {typeof(TInterface).Name}");
                throw new ArgumentNullException(nameof(instance));
            }
            _services[typeof(TInterface)] = instance;
            Debug.Log($"[ServiceContainer] ✓ Зарегистрирован: {typeof(TInterface).Name}");
        }

        public TInterface Resolve<TInterface>()
        {
            if (_services.TryGetValue(typeof(TInterface), out var service))
            {
                Debug.Log($"[ServiceContainer] Resolve<{typeof(TInterface).Name}> ✓");
                return (TInterface)service;
            }
            Debug.LogError($"[ServiceContainer] ОШИБКА: сервис {typeof(TInterface).Name} не зарегистрирован!");
            throw new InvalidOperationException($"Сервис {typeof(TInterface).Name} не зарегистрирован.");
        }

        public bool IsRegistered<TInterface>() => _services.ContainsKey(typeof(TInterface));
    }
}
