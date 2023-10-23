using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Object
{
    private readonly Transform _parent;
    private readonly T _prefab;
    private readonly Queue<T> _returnedObjects = new();
    private readonly HashSet<T> _usingObjects = new();

    public ObjectPool(T prefab, int initCount = 0, Transform parent = null)
    {
        _prefab = prefab;
        _parent = parent;
        for (int i = 0; i < initCount; ++i)
        {
            _returnedObjects.Enqueue(Object.Instantiate(_prefab, _parent));
        }
    }

    public T Get()
    {
        T obj;
        if(_returnedObjects.Count == 0)
            obj = Object.Instantiate(_prefab, _parent);
        else
            obj = _returnedObjects.Dequeue();
        _usingObjects.Add(obj);

        if(obj is GameObject go) go.SetActive(true);
        else if(obj is Component comp) comp.gameObject.SetActive(true);

        return obj;
    }

    public void Return(T obj)
    {
        if (!_usingObjects.Contains(obj)) return;
        _usingObjects.Remove(obj);
        _returnedObjects.Enqueue(obj);

        if (obj is GameObject go) go.SetActive(false);
        else if (obj is Component comp) comp.gameObject.SetActive(false);
    }
}
