using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// The Event Responders component acts as storage for any event
/// responders on an object. It is also used to expose the Edit
/// button for the Events Editor.
/// </summary>
public class EventRespondersComponent : MonoBehaviour
{
    [SerializeField]
    private List<EventResponseWrapper> responseList;
    /// <summary>
    /// Our list of all EventResponseWrappers for this object (will initialise if necessary)
    /// </summary>
    private List<EventResponseWrapper> ResponseList
    {
        get
        {
            if (responseList == null) responseList = new List<EventResponseWrapper>();
            return responseList;
        }
    }

    /// <summary>
    /// On scene startup, hooks up all EventResponders to their target events.
    /// </summary>
	void Awake()
	{
		ValidateResponders();

		foreach (EventResponseWrapper response in ResponseList)
		{
			response.Responder.AttachToEvent(response.GetTargetComponent(gameObject), response.EventName);
		}
	}

	/// <summary>
	/// Gets a list of all responders for a given event.
	/// </summary>
	/// <param name="eventInfo">The event to look up responders for.</param>
	public List<EventResponder> GetRespondersForEvent(MonoBehaviour targetComponent, string eventName)
	{
		List<EventResponder> responders = new List<EventResponder>();

        foreach (EventResponseWrapper responseWrapper in ResponseList)
        {
            if (responseWrapper.GetTargetComponent(gameObject) == targetComponent && responseWrapper.EventName == eventName)
            {
                responders.Add(responseWrapper.Responder);
            }
        }

        return responders;
	}

	/// <summary>
	/// Adds a responder for the given event.
	/// </summary>
	/// <param name="eventInfo">Event to add the responder for.</param>
	/// <param name="responder">The responder object to add.</param>
	public void AddEventResponder(MonoBehaviour targetComponent, string eventName, EventResponder responder)
	{
        ResponseList.Add(new EventResponseWrapper(targetComponent, eventName, responder));
	}

	/// <summary>
	/// Removes a responder matching the given criteria from storage.
	/// </summary>
	/// <param name="targetComponent">The component on which the event lives.</param>
	/// <param name="eventName">The name of the event for which the responder listens.</param>
	/// <param name="responder">The responder to remove.</param>
	public void RemoveEventResponder(MonoBehaviour targetComponent, string eventName, EventResponder responder)
	{
		ResponseList.RemoveAll(delegate(EventResponseWrapper response)
		{
            if (response.GetTargetComponent(gameObject) == targetComponent && response.EventName == eventName && response.Responder == responder)
			{
				ScriptableObject.DestroyImmediate(response.Responder);
				return true;
			}
			else
			{
				return false;
			}
		});
	}

	/// <summary>
	/// Checks every single responder we have to ensure that:
	///  - The target event still exists
	///  - The responder still exists
	/// For any responder that does not meet these, an error log will be produced and the responder
	/// itself will be deleted.
	/// </summary>
	public void ValidateResponders()
	{
		ResponseList.RemoveAll(delegate(EventResponseWrapper response)
		{
			//Make sure the responder itself exists
			if (response.Responder == null)
			{
				Debug.LogError("Response with non-existent responder found, deleting (on GameObject " + gameObject.name + ")", gameObject);
				return true;
			}

			//Make sure the component containing the event still exists
            if (response.GetTargetComponent(gameObject) == null)
			{
				Debug.LogError("Responder " + response.Responder.Name + " for event " + response.EventName
                    + " is targeting a component that doesn't exist, deleting (on GameObject " + gameObject.name + ")", gameObject);

				ScriptableObject.DestroyImmediate(response.Responder);
				return true;
			}

			//Make sure the component still supports the stored event
            if (response.GetTargetComponent(gameObject).GetType().GetEvent(response.EventName) == null)
			{
                Debug.LogError("Event " + response.EventName + " no longer exists on " + response.GetTargetComponent(gameObject).GetType().Name
                    + ", deleting responder " + response.Responder.Name + " (on GameObject " + gameObject.name + ")", gameObject);

				ScriptableObject.DestroyImmediate(response.Responder);
				return true;
			}

			//All good!
			return false;
		});
	}

    /// <summary>
    /// This is called on:
    ///  - initial scene load
    ///  - inspector values changed
    ///  - new script initialisation (e.g. adding the component or duplicating it)
    /// </summary>
    void OnValidate()
    {
        //Validate our responders so we don't end up operating on any invalid ones
        ValidateResponders();

        List<EventResponseWrapper> clonedResponses = new List<EventResponseWrapper>();

        //Loop through all our responses: find any responders that are referenced from multiple places
        foreach (EventResponseWrapper response in ResponseList)
        {
            //If the given responder has no owner yet, claim it for ourselves
            if (!response.Responder.HasOwner)
            {
                response.Responder.Owner = GetInstanceID();
            }
            //If this responder has been claimed by someone else, we have a duplicate reference and must clone our own
            else if (response.Responder.Owner != GetInstanceID())
            {
                clonedResponses.Add(response.Clone(gameObject, GetInstanceID()));

                Debug.Log("Cloned response of type " + response.Responder.GetType() + " for event " +
                            response.EventName + " from owner " + response.Responder.Owner, gameObject);
                EditorUtility.SetDirty(this);
            }
        }

        //Add all the new clones to our list
        ResponseList.AddRange(clonedResponses);

        //Now that all cloning has been performed, remove all responders not owned by us
        ResponseList.RemoveAll(delegate(EventResponseWrapper response)
        {
            return response.Responder.Owner != GetInstanceID();
        });
    }
}
