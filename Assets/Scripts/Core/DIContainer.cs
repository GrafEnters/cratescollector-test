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
        Type type = typeof(T);
        if (_services.ContainsKey(type)) {
            _services[type] = service;
        } else {
            _services.Add(type, service);
        }
    }

    public T Get<T>() where T : class {
        Type type = typeof(T);
        if (_services.TryGetValue(type, out object service)) {
            return service as T;
        }

        return null;
    }

    public bool Has<T>() where T : class {
        return _services.ContainsKey(typeof(T));
    }
}
