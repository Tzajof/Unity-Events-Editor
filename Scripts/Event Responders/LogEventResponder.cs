using System;
using UnityEngine;

/// <summary>
/// A simple event responder that will log a message to the console every time it is called.
/// </summary>
[Serializable]
public class LogEventResponder : EventResponder
{
	[SerializeField]
    [Tooltip("The message to output to the console when called.")]
	private string message;

    /// <summary>
    /// Returns the name of this Event Responder as to be displayed in the editor.
    /// </summary>
	public override string Name { get { return "Log A Message"; } }

    /// <summary>
    /// Handles the event to which we are responding. In this case, simply logs our
    /// stored message.
    /// </summary>
    /// <param name="sender">The sender from which the event originated.</param>
    /// <param name="args">Our arguments object. Ignored for this responder.</param>
	public override void HandleEvent(object sender, GenericEventArguments args)
	{
		Debug.Log(message);
	}

    /// <summary>
    /// Returns the type of arguments we can handle. In this case, anything.
    /// </summary>
    /// <returns>The type of GenericEventArguments.</returns>
	public override Type GetHandledType()
	{
		return typeof(GenericEventArguments);
	}
}
