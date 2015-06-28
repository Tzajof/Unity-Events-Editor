using UnityEngine;
using UnityEditor;

/// <summary>
/// A custom editor for any kind of event responder that will draw all of the default property editors
/// with the exception of the Script property- we don't want designers changing the script of an event 
/// responder!
/// (I don't wanna know what would happen if you did...)
/// </summary>
[CustomEditor(typeof(EventResponder), true)]
public class EventResponderEditor : Editor
{
    /// <summary>
    /// Draws the Inspector for this EventResponder. Basically draws the vanilla GUI without the Script property.
    /// </summary>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject, "m_Script");

        serializedObject.ApplyModifiedProperties();
    }
}
