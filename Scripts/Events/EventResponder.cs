using System;
using System.Reflection;
using UnityEngine;

/// <summary>
/// EventResponder is a base class for all classes that represent a response
/// to any given event.
/// You must specify which kind of event arguments you are capable of processing,
/// for example to be compatible with any and all events you can simply process
/// GenericEventArguments.
/// If you want to listen to only a certain type of event, declare your event arguments
/// as specific to that event, e.g. ControlMapping.ActionTriggeredEventArguments for
/// ControlMapping events.
/// </summary>
/// <typeparam name="T">The type of event arguments this class is capable of processing.</typeparam>
[System.Serializable]
public abstract class EventResponder : ScriptableObject
{
	/// <summary>
	/// The GameObject to which this EventResponder is currently attached. Will be non-null at runtime, null at all other times.
	/// </summary>
	public GameObject GameObject { get; private set; }

	/// <summary>
	/// The name of this event responder.
	/// </summary>
	public abstract string Name { get; }

	/// <summary>
	/// The path that this responder should reside in for the event responder menu, e.g. "Camera/".
	/// </summary>
	public virtual string MenuPath { get { return ""; } }

    private int ownerID = -1;
    /// <summary>
    /// Returns the instance ID of this EventResponder's Owner. This is not persisted; it will reset every time the Unity runtime is re-initialised.
    /// This may also be set publically, but only if the value has not already been set.
    /// </summary>
    public int Owner 
    { 
        get
        {
            return ownerID;
        }
        set
        {
            if (Owner != -1)
            {
                throw new InvalidOperationException("You cannot claim ownership of an EventResponder that has already been claimed.");
            }

            ownerID = value;
        }
    }

    /// <summary>
    /// Returns true if this EventResponder currently is currently owned by any object.
    /// </summary>
    public bool HasOwner { get { return Owner != -1; } }

	/// <summary>
	/// Responds to any event to which we are hooked in.
	/// </summary>
	/// <param name="sender">The object from which the event originated.</param>
	/// <param name="args">The GenericEventArguments (or derivative thereof) raised with the event.</param>
	public abstract void HandleEvent(object sender, GenericEventArguments args);

    /// <summary>
    /// Returns the type of event arguments that this responder can handle.
    /// Return GenericEventArguments to indicate you can handle anything.
    /// </summary>
    /// <returns></returns>
    public abstract Type GetHandledType();

	/// <summary>
	/// Convenience function to return the event arguments cast to our desired arguments type.
	/// If the type is incompatible, it will throw an exception with a descriptive error message.
	/// </summary>
	/// <typeparam name="T">Type to cast to.</typeparam>
	/// <param name="args">Arguments object to cast.</param>
	/// <returns></returns>
	protected T CastArguments<T>(GenericEventArguments args) where T : GenericEventArguments
	{
		try
		{
			return (T)args;
		}
		catch (InvalidCastException)
		{
			throw new ArgumentException("Given arguments were not of the correct type. Desired: " + typeof(T).Name + " Given: " + args.GetType().Name);
		}
	}

	/// <summary>
	/// Attaches this event responder to the event of eventName on the given MonoBehaviour.
	/// This may only be called once- as responders may only be attached to one event at runtime.
	/// If this responder is already attached to something, will throw an exception.
	/// </summary>
	/// <param name="targetComponent">The MonoBehaviour containing the event to which we must attach.</param>
	/// <param name="eventName">The string name of the event to attach to.</param>
	public void AttachToEvent(MonoBehaviour targetComponent, string eventName)
	{
		//Make sure we're not already attached to anything.
		if (GameObject != null)
		{
			throw new InvalidOperationException("Attempted to attach an already-attached event responder to an event. This is not allowed!");
		}

		EventInfo eventInfo = targetComponent.GetType().GetEvent(eventName);

		Delegate eventDelegate = Delegate.CreateDelegate(eventInfo.EventHandlerType, this, GetType().GetMethod("HandleEvent"));

		eventInfo.AddEventHandler(targetComponent, eventDelegate);

		GameObject = targetComponent.gameObject;
	}
}
