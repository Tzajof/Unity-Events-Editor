using System;
using System.Collections.Generic;

/// <summary>
/// GenericEventArguments provide generic storage for any kind of arguments that one might
/// seek to provide through an event. It is designed to be extended in order to create specialisations
/// of arguments for each type of event we want to expose, whilst still being compatible with any kind
/// of event receiver that accepts System.EventArgs.
/// 
/// This class is incredibly simple, but in reality it will be key to ensuring that our events are nicely
/// cross-compatible and able to be manipulated through the Unity editor.
/// </summary>
public abstract class GenericEventArguments : EventArgs
{
    private Dictionary<string, object> storage = new Dictionary<string, object>();
    private Dictionary<string, Type> validArguments;

    /// <summary>
    /// Returns a mapping of all valid argument names and their types for this arguments object.
    /// </summary>
    /// <returns>A dictionary of argument names to types</returns>
    public abstract Dictionary<string, Type> GetValidArguments();

    /// <summary>
    /// Constructor for GenericEventArguments- initialises our mapping of valid arguments.
    /// </summary>
    public GenericEventArguments()
    {
        validArguments = GetValidArguments();
    }

    /// <summary>
    /// Sets the value of the argument with the given name.
    /// Ensures that the name is that of a valid argument and that the value is of the correct type.
    /// </summary>
    /// <param name="name">Name of the argument to set.</param>
    /// <param name="value">The value to set the argument to.</param>
    protected void SetArgument(string name, object value)
    {
        //Is this a valid argument?
        if (!validArguments.ContainsKey(name))
        {
            throw new ArgumentException("Cannot set argument of name '" + name + "': that name is not a valid argument");
        }
        //Does the value match the valid type?
        if (value != null && !validArguments[name].IsAssignableFrom(value.GetType()))
        {
            throw new ArgumentException("Cannot set argument of name '" + name + "' to value '" + value + "': required type is " + validArguments[name]);
        }

        //Add in or update the value
        if (!storage.ContainsKey(name))
        {
            storage.Add(name, value);
        }
        else
        {
            storage[name] = value;
        }
    }

    /// <summary>
    /// Gets the argument with the given name as the requested type.
    /// Will check to make sure that the argument name is valid, the type is correct and that the argument has a value set for it.
    /// </summary>
    /// <typeparam name="T">The type to retrieve the argument's value as.</typeparam>
    /// <param name="name">The name of the argument value to retrieve.</param>
    /// <returns></returns>
    public T GetArgument<T>(string name)
    {
        //Is this a valid argument name?
        if (!validArguments.ContainsKey(name))
        {
            throw new ArgumentException("Cannot get argument of name '" + name + "': that name is not a valid argument");
        }
        //Can that argument actually be cast to the requested type?
        if (!typeof(T).IsAssignableFrom(validArguments[name]))
        {
            throw new ArgumentException("Cannot get argument of name '" + name + "' as " + typeof(T) + ": its valid type is " + validArguments[name]);
        }

        //Has this argument been set?
        if (!storage.ContainsKey(name))
        {
            throw new ArgumentException("Cannot get argument of name '" + name + "': the argument has not had a value set");
        }

        //Ok, go ahead, cast it and return
        T value = (T)storage[name];
        return value;
    }
}
