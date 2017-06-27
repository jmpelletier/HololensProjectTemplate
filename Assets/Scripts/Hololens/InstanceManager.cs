using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

public class InstanceManager<T>
{
    private static List<T> _instances = new List<T>();
    public static ReadOnlyCollection<T> all
    {
        get { return _instances.AsReadOnly(); }
    }

    public void Add(T instance)
    {
        _instances.Add(instance);
    }

    public InstanceManager() {}

    [Serializable]
    public class InitializationException : Exception {
        public InitializationException() {}
        public InitializationException(string message) : base(message) { }
        public InitializationException(string message, Exception innerException) : base(message, innerException) { }
    };
    
    public T main
    {
        get {
            if (_instances.Count > 0)
            {
                return _instances[0];
            }
            else throw new InitializationException("Trying to access main instance but no instance exists!");
        }
    }
}