using System;
using System.Collections.Generic;
using UnityEngine;

public class DIContainer : MonoBehaviour {
    private static DIContainer _instance;

    private readonly Dictionary<Type, object> _services = new();

    public static DIContainer Instance {
        get {
            if (_instance == null) {
                GameObject containerObject = new("DIContainer");
                _instance = containerObject.AddComponent<DIContainer>();
                DontDestroyOnLoad(containerObject);
            }

            return _instance;
        }
    }

    public void Register<T>(T service) where T : class {
        if (service == null) {
            Debug.LogError($"Attempted to register null service of type {typeof(T).Name}");
            return;
        }

        Type type = typeof(T);
        if (_services.ContainsKey(type)) {
            Debug.LogWarning($"Service of type {type.Name} is already registered. Overwriting.");
        }

        _services[type] = service;
    }

    public T Get<T>() where T : class {
        Type type = typeof(T);
        if (_services.TryGetValue(type, out object service)) {
            return service as T;
        }

        return null;
    }

    public T GetRequired<T>() where T : class {
        Type type = typeof(T);
        if (_services.TryGetValue(type, out object service)) {
            T result = service as T;
            if (result == null) {
                Debug.LogError($"Service of type {type.Name} is registered but cast failed");
                throw new InvalidCastException($"Failed to cast service to {type.Name}");
            }
            return result;
        }

        Debug.LogError($"Required service of type {type.Name} is not registered");
        throw new InvalidOperationException($"Service of type {type.Name} is not registered");
    }

    public bool TryGet<T>(out T service) where T : class {
        Type type = typeof(T);
        if (_services.TryGetValue(type, out object serviceObj)) {
            service = serviceObj as T;
            return service != null;
        }

        service = null;
        return false;
    }

    public bool Has<T>() where T : class {
        return _services.ContainsKey(typeof(T));
    }
}
