using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#region GenericDictionary
/// <summary>
/// special container used to stock and send datas
/// </summary>
public class GenericDictionary
{
    Dictionary<string, dynamic> _dataBase = null;

    public dynamic Get(string key, dynamic defaultValue = default(dynamic))
    {
        if (_dataBase != null && _dataBase.ContainsKey(key))
        {
            return _dataBase[key];
        }
        return defaultValue;
    }

    public bool ContainsKey(string key)
    {
        return _dataBase == null ? false : _dataBase.ContainsKey(key);
    }

    public GenericDictionary Set(string key, dynamic value)
    {
        if (_dataBase == null)
        {
            _dataBase = new Dictionary<string, dynamic>();
        }
        _dataBase[key] = value;
        return this;
    }

    public bool Remove(string key)
    {
        return _dataBase.Remove(key);
    }
}
#endregion

public class MyEventSystem : MonoBehaviour
{
    #region singleton
    static MyEventSystem _instance;
    public static MyEventSystem instance { get { return _instance; } }
    MyEventSystem()
    {
        _instance = this;
    }
    #endregion

    #region variables
    private GenericDictionary _dataBase = new GenericDictionary();
    Dictionary<string, Dictionary<object, IList>> _dataCallbacks = new Dictionary<string, Dictionary<object, IList>>();
    Dictionary<string, Dictionary<object, IList>> _dynamicDataCallbacks = new Dictionary<string, Dictionary<object, IList>>();
    Dictionary<string, Dictionary<object, IList>> _eventCallbacks = new Dictionary<string, Dictionary<object, IList>>();
    #endregion

    #region Database Access and Manipulation
    public bool ContainsKey(string key)
    {
        return _dataBase.ContainsKey(key);
    }

    public dynamic Get(string key, dynamic defaultValue = default(dynamic), GenericDictionary options = null)
    {
        // First check for dynamic data callbacks
        if (_dynamicDataCallbacks.ContainsKey(key))
        {
            foreach (var pair in _dynamicDataCallbacks[key])
            {
                foreach (Func<GenericDictionary, dynamic> callback in pair.Value)
                {
                    dynamic result = callback(options);
                    if (result != null && !result.Equals(defaultValue))
                    {
                        return result;
                    }
                }
            }
        }

        // Return from the database
        if (_dataBase.ContainsKey(key))
        {
            return _dataBase.Get(key,defaultValue);
        }

        return defaultValue;
    }

    public dynamic Get(string key, GenericDictionary options)
    {
        return Get(key, default(dynamic), options);
    }

    public void Set(string key, dynamic value)
    {
        _dataBase.Set(key, value);
        // Trigger registered callbacks
        Dirty(key);
    }

    public void Dirty(string key)
    {
        if (_dataCallbacks.ContainsKey(key))
        {
            foreach (var pair in _dataCallbacks[key])
            {
                foreach (Action<dynamic> callback in pair.Value)
                {
                    callback(Get(key));
                }
            }
        }
    }

    public dynamic GetOrCreate(string key, dynamic defaultValue = default(dynamic))
    {
        if (!_dataBase.ContainsKey(key))
        {
            Set(key, defaultValue);
        }
        return _dataBase.Get(key,defaultValue);
    }

    #endregion

    #region Events, register and unregister

    public void Register(string key, object source, Action<dynamic> callback)
    {
        RegisterCallback(key, source, callback, _dataCallbacks);
    }

    public void RegisterDynamicData(string key, object source, Func<GenericDictionary, dynamic> callback)
    {
        RegisterCallback(key, source, callback, _dynamicDataCallbacks);
    }

    public void Unregister(object source)
    {
        UnregisterCallback(source, _dataCallbacks);
        UnregisterCallback(source, _eventCallbacks);
        UnregisterCallback(source, _dynamicDataCallbacks);
    }

    public delegate void EventAction(string name, GenericDictionary data);

    public void FireEvent(string name, GenericDictionary data = null)
    {
        if (_eventCallbacks.ContainsKey(name))
        {
            foreach (var pair in _eventCallbacks[name])
            {
                foreach (EventAction callback in pair.Value)
                {
                    callback(name, data);
                }
            }
        }
    }

    public void RegisterToEvent(string name, object source, EventAction callback)
    {
        RegisterCallback(name, source, callback, _eventCallbacks);
    }

    #endregion

    #region utils

    //register callback in dictionary 
    void RegisterCallback(string name, object source, dynamic callback, Dictionary<string, Dictionary<object, IList>> dictionary)
    {
        if (!dictionary.ContainsKey(name))
        {
            dictionary[name] = new Dictionary<object, IList>();
        }
        if (!dictionary[name].ContainsKey(source))
        {
            dictionary[name][source] = new List<dynamic>();
        }
        dictionary[name][source].Add(callback);
    }

    //unregister callback from dictionary
    void UnregisterCallback(object source, Dictionary<string, Dictionary<object, IList>> dictionary)
    {
        foreach (var pair in dictionary)
        {
            if (pair.Value.ContainsKey(source))
            {
                pair.Value.Remove(source);
            }
        }
    }

    #endregion
}
