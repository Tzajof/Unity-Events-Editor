using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// Class responsible for providing GUI functionality to the Unity Editor for slotting event
/// responders in and out of events raised by components.
/// </summary>
public class EventsEditorWindow : EditorWindow
{
    //References to the object/component we are currently working on
    private EventRespondersComponent currentTargetResponders;
    private GameObject currentTargetGameObject;

    //List of all components on the target object
    private SelectionList<MonoBehaviour> components = new SelectionList<MonoBehaviour>(delegate(MonoBehaviour input) { return input.GetType().Name; });

    //A list of all events currently exposed by the currently selected component
    private SelectionList<EventInfo> currentExposedEvents = new SelectionList<EventInfo>(delegate(EventInfo input) { return input.Name; });

    //Selection list of all responders for the current selected event
    private SelectionList<EventResponder> currentEventResponders = new SelectionList<EventResponder>(delegate(EventResponder input) { return input.Name; });

    //Selection list of all responder types that apply to the currently selected event
    private SelectionList<Type> currentApplicableResponderTypes = new SelectionList<Type>(delegate(Type input) 
	{
		EventResponder tempInstance = (EventResponder)ScriptableObject.CreateInstance(input);

		string name = tempInstance.MenuPath + tempInstance.Name;

		ScriptableObject.DestroyImmediate(tempInstance);

		return name;
	});

    //Handle to the editor for the currently edited event responder
    private Editor currentEventResponderEditor;

    private GUIStyle headingStyle;
    /// <summary>
    /// Returns our GUIStyle for headings, initialising it if necessary.
    /// </summary>
    private GUIStyle HeadingStyle
    {
        get
        {
            if (headingStyle == null)
            {
                headingStyle = new GUIStyle(GUI.skin.label);
                headingStyle.fontSize = 18;
                headingStyle.fixedHeight = 30;
            }

            return headingStyle;
        }
    }

    /// <summary>
    /// Creates (or shows an existing) instance of this window.
    /// </summary>
    public static void ShowWindow(EventRespondersComponent responders)
    {
        EventsEditorWindow window = (EventsEditorWindow)EditorWindow.GetWindow(typeof(EventsEditorWindow));

        window.SetTargetResponderObject(responders);
    }

    /// <summary>
    /// Gets a list of all types of EventResponders that could be used for the given event.
    /// </summary>
    /// <param name="eventName">The event to retrieve the types for.</param>
    /// <returns></returns>
    private List<Type> GetApplicableResponderTypesForEvent(EventInfo eventInfo)
    {
        List<Type> applicableTypes = new List<Type>();

        //Since all events are checked to be of type EventHandler<T>, [0] will always be the argument type
        Type eventArgumentsType = eventInfo.EventHandlerType.GetGenericArguments()[0];

        foreach (Type type in Assembly.GetAssembly(typeof(EventResponder)).GetTypes())
        {
            if (!type.IsAbstract && type.IsSubclassOf(typeof(EventResponder)))
            {
                EventResponder tempInstance = (EventResponder)ScriptableObject.CreateInstance(type);

                if (tempInstance.GetHandledType().IsAssignableFrom(eventArgumentsType))
                {
                    applicableTypes.Add(type);
                }

                ScriptableObject.DestroyImmediate(tempInstance);
            }
        }

        return applicableTypes;
    }

    /// <summary>
    /// Returns whether or not the given event is suitable for use in our Events system; i.e.
    /// whether or not it is compatible with System.EventHandler<GenericEventArguments>
    /// </summary>
    /// <param name="eventInfo">The event to check.</param>
    /// <returns>True if the given event can be used.</returns>
    private bool IsEventSuitable(EventInfo eventInfo)
    {
        if (eventInfo.EventHandlerType.IsGenericType)
        {
            Type[] genericTypes = eventInfo.EventHandlerType.GetGenericArguments();

            if (genericTypes.Length == 1 && typeof(GenericEventArguments).IsAssignableFrom(genericTypes[0]))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Sets the responder storage object to use as our editing target.
    /// <param name="responders">The responders object that stores all of our event responses.</param>
    /// </summary>
    private void SetTargetResponderObject(EventRespondersComponent responders)
    {
        //Only update our info when the selection has ACTUALLY changed.
        if (responders != currentTargetResponders)
        {
            currentTargetResponders = responders;
            currentTargetGameObject = responders.gameObject;

			//Run a validation pass on the responders before we go working on them
			responders.ValidateResponders();

            List<MonoBehaviour> availableComponents = new List<MonoBehaviour>();

            //Find components that expose suitable events
            foreach (MonoBehaviour component in currentTargetGameObject.GetComponents<MonoBehaviour>())
            {
                EventInfo[] availableEvents = component.GetType().GetEvents();

                //Only include events that conform to EventHandler<GenericEventArguments>
                foreach (EventInfo availableEvent in availableEvents)
                {
                    if (IsEventSuitable(availableEvent))
                    {
                        availableComponents.Add(component);
                        break;
                    }
                }
            }

            //Generate a list of event names for display in the GUI.
            components.SetContents(availableComponents);
            components.SetSelection(-1);

            //Update selection information
            RefreshCurrentExposedEvents();
        }
    }

    /// <summary>
    /// Refreshes the list of currently exposed events to reflect those of the currently selected component.
    /// </summary>
    private void RefreshCurrentExposedEvents()
    {
        //Only update if no selection
        if (components.SelectedObject != null)
        {
            EventInfo[] availableEvents = components.SelectedObject.GetType().GetEvents();

            List<EventInfo> exposedEvents = new List<EventInfo>();

            //Only include events that conform to EventHandler<GenericEventArguments>
            foreach (EventInfo availableEvent in availableEvents)
            {
                if (IsEventSuitable(availableEvent))
                {
                    exposedEvents.Add(availableEvent);
                }
            }

            currentExposedEvents.SetContents(exposedEvents);
        }
        else
        {
            currentExposedEvents.SetContents(new List<EventInfo>());
        }

        currentExposedEvents.SetSelection(-1);
        RefreshCurrentEventResponders();
        RefreshApplicableResponderTypes();
    }

    /// <summary>
    /// Refreshes the list event responders to reflect those applying to the currently selected event.
    /// </summary>
    private void RefreshCurrentEventResponders()
    {
        if (currentExposedEvents.SelectedObject != null)
        {
            currentEventResponders.SetContents(currentTargetResponders.GetRespondersForEvent(components.SelectedObject, currentExposedEvents.SelectedObject.Name));
        }
        else
        {
            currentEventResponders.SetContents(new List<EventResponder>());
        }

        currentEventResponders.SetSelection(-1);
        RefreshSelectedEventResponder();
    }

    /// <summary>
    /// Refreshes our list of responder types that apply to the currently selected event.
    /// </summary>
    private void RefreshApplicableResponderTypes()
    {
        if (currentExposedEvents.SelectedObject != null)
        {
            currentApplicableResponderTypes.SetContents(GetApplicableResponderTypesForEvent(currentExposedEvents.SelectedObject));
        }
        else
        {
            currentApplicableResponderTypes.SetContents(new List<Type>());
        }

        currentApplicableResponderTypes.SetSelection(0);
    }

    /// <summary>
    /// Refreshes the event responder editor to show the currently selected event responder.
    /// </summary>
    private void RefreshSelectedEventResponder()
    {
        if (currentEventResponders.SelectedObject != null)
        {
            if (currentEventResponderEditor != null)
            {
                currentEventResponderEditor.ResetTarget();
            }

            currentEventResponderEditor = Editor.CreateEditor(currentEventResponders.SelectedObject);
            currentEventResponderEditor.RequiresConstantRepaint();
        }
        else
        {
            currentEventResponderEditor = null;
        }
    }

    /// <summary>
    /// Draws the GUI for our window.
    /// </summary>
    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();

        //Only draw everything if we currently have any selected objects
        if (components.Objects.Count > 0)
        {
            DrawCurrentComponentList();

            GUILayout.Box("", GUILayout.ExpandHeight(true), GUILayout.Width(1));

            DrawCurrentExposedEventsList();

            GUILayout.Box("", GUILayout.ExpandHeight(true), GUILayout.Width(1));

            DrawCurrentEventResponders();

            GUILayout.Box("", GUILayout.ExpandHeight(true), GUILayout.Width(1));

            DrawEventResponderEditor();
        }
        else
        {
            EditorGUILayout.LabelField("No events are currently available. Please select an object that has events to edit.");
        }
        
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// Draws our selectable list of components to the screen.
    /// </summary>
    private void DrawCurrentComponentList()
    {
        EditorGUILayout.BeginScrollView(Vector2.zero, GUILayout.Width(175));

        if (DrawSelectionList(components))
        {
            RefreshCurrentExposedEvents();
        }

        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// Draws our selectable list of all events exposed by the currently selected component to the screen.
    /// </summary>
    private void DrawCurrentExposedEventsList()
    {
        EditorGUILayout.BeginScrollView(Vector2.zero, GUILayout.Width(150));

        if (DrawSelectionList(currentExposedEvents))
        {
            RefreshCurrentEventResponders();
            RefreshApplicableResponderTypes();
        }

        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// Draws our list of all event responders to the currently responded event to the screen.
    /// </summary>
    private void DrawCurrentEventResponders()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(150));

        EditorGUILayout.BeginScrollView(Vector2.zero, GUILayout.Width(150));

        if (DrawSelectionList(currentEventResponders))
        {
            RefreshSelectedEventResponder();
        }

        EditorGUILayout.EndScrollView();

        //Only draw the responder controls if there are any types available
        if (currentApplicableResponderTypes.Objects.Count > 0)
        {
            DrawAddRemoveResponderControls();
        }

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Draws the field controls to add and remove any responder from the currently selected event.
    /// </summary>
    private void DrawAddRemoveResponderControls()
    {
        int selection = EditorGUILayout.Popup(currentApplicableResponderTypes.SelectedIndex, currentApplicableResponderTypes.ObjectNames);
        if (selection != currentApplicableResponderTypes.SelectedIndex)
        {
            currentApplicableResponderTypes.SetSelection(selection);
        }

        EditorGUILayout.BeginHorizontal();

        //Add a new responder when clicked
        if (GUILayout.Button("+"))
        {
            EventResponder newResponder = (EventResponder)ScriptableObject.CreateInstance(currentApplicableResponderTypes.SelectedObject);
            currentTargetResponders.AddEventResponder(components.SelectedObject, currentExposedEvents.SelectedObject.Name, newResponder);
            RefreshCurrentEventResponders();

            EditorUtility.SetDirty(currentTargetResponders);
        }

        //Remove the currently selected responder when clicked
        if (GUILayout.Button("-") && currentEventResponders.SelectedObject != null)
        {
            currentTargetResponders.RemoveEventResponder(components.SelectedObject, currentExposedEvents.SelectedObject.Name, currentEventResponders.SelectedObject);
            RefreshCurrentEventResponders();

            EditorUtility.SetDirty(currentTargetResponders);
        }

        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// Draws the editor for our currently selected event responder to the screen.
    /// </summary>
    private void DrawEventResponderEditor()
    {
        //Safety check: it's possible we can lose our selection references while maintaining our editor reference
        if (currentEventResponders.SelectedObject == null)
        {
            currentEventResponderEditor = null;
        }

        if (currentEventResponderEditor != null)
        {
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField(currentEventResponders.SelectedObject.Name, HeadingStyle);

            EditorGUILayout.Space();

            currentEventResponderEditor.OnInspectorGUI();

            EditorGUILayout.EndVertical();
        }
    }

    /// <summary>
    /// Draws the given selection list to the screen and, if the selection in the GUI changes, updates
    /// the list's selection likewise.
    /// Returns a boolean value representing whether or not the selection changed.
    /// </summary>
    /// <typeparam name="T">Generic parameter matching the selection list</typeparam>
    /// <param name="list">The list to display</param>
    /// <returns>True if the selection changed; false otherwise.</returns>
    private bool DrawSelectionList<T>(SelectionList<T> list)
    {
        int selection = GUILayout.SelectionGrid(list.SelectedIndex, list.ObjectNames, 1);

        //Did the selection change?
        if (selection != list.SelectedIndex)
        {
            list.SetSelection(selection);
            return true;
        }
        else
        {
            return false;
        }
    }
}
