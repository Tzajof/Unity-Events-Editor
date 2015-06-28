using UnityEngine;
using UnityEditor;

/// <summary>
/// A Custom Editor for the Event Responders Component: it simply draws
/// a button that will open up the Events Editor Window.
/// </summary>
[CustomEditor(typeof(EventRespondersComponent))]
public class EventRespondersComponentEditor : Editor
{
	/// <summary>
	/// Draws our GUI in the Inspector.
	/// </summary>
	public override void OnInspectorGUI()
	{
		if (GUILayout.Button("Edit Events..."))
		{
			EventsEditorWindow.ShowWindow((EventRespondersComponent)target);
		}
	}
}
