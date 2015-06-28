using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// An immutable storage class that represents a binding of a single EventResponder to an event on any MonoBehaviour.
/// </summary>
[Serializable]
public class EventResponseWrapper
{
    /// <summary>
    /// A direct reference to the MonoBehaviour to which this EventResponder is bound.
    /// </summary>
    [Obsolete("Direct MonoBehaviour references are no longer allowed. They are not compatible with prefabs.")]
    [SerializeField]
    private MonoBehaviour targetComponent;

    /// <summary>
    /// A string value depicting the type of the monobehaviour that we are targeting.
    /// This is so that, rather than targeting an explicit monobehaviour instance, we retrieve
    /// the matching instance on our own game-object at runtime by its type name.
    /// </summary>
    [SerializeField]
    private string targetComponentTypeName;

    [SerializeField]
    private string eventName;
    /// <summary>
    /// The string-name of the Event on the target MonoBehaviour.
    /// </summary>
    public string EventName { get { return eventName; } }

    [SerializeField]
    private EventResponder responder;
    /// <summary>
    /// The EventResponder object that will be hooked into the specified event.
    /// </summary>
    public EventResponder Responder { get { return responder; } }

    /// <summary>
    /// Creates a new instance of EventResponseWrapper, tying the given EventResponder to the specified Event.
    /// </summary>
    /// <param name="targetComponent">The MonoBehaviour that contains the event to which the EventResponder will be bound.</param>
    /// <param name="eventName">The string-name of the Event on the target MonoBehaviour.</param>
    /// <param name="responder">The EventResponder object that will be hooked into the specified event.</param>
    public EventResponseWrapper(MonoBehaviour targetComponent, string eventName, EventResponder responder)
    {
        this.targetComponentTypeName = targetComponent.GetType().Name;
        this.eventName = eventName;
        this.responder = responder;
    }

    /// <summary>
    /// Returns the MonoBehaviour that contains the event to which we are bound.
    /// </summary>
    #pragma warning disable 618     //Disable deprecation warning, the usage is deliberate here
    public MonoBehaviour GetTargetComponent(GameObject targetObject)
    {
        //Backwards-compatibility: if this is still referencing a monobehaviour, port the type name over if we haven't already
        if (targetComponent != null && (targetComponentTypeName == null || targetComponentTypeName == ""))
        {
            targetComponentTypeName = targetComponent.GetType().Name;
        }

        return (MonoBehaviour)targetObject.GetComponent(targetComponentTypeName);
    }

    /// <summary>
    /// Clones this EventResponseWrapper, generating a new EventResponder instance as a copy of our existing one.
    /// </summary>
    /// <param name="targetObject">The gameObject containing the new target component for the cloned instance.</param>
    /// <param name="ownerID">The instance ID to set as the owner of the new EventResponder instance.</param>
    /// <returns></returns>
    public EventResponseWrapper Clone(GameObject targetObject, int ownerID)
    {
        EventResponder clonedResponder = (EventResponder)ScriptableObject.Instantiate(Responder);
        clonedResponder.Owner = ownerID;

        return new EventResponseWrapper(GetTargetComponent(targetObject), EventName, clonedResponder);
    }
}