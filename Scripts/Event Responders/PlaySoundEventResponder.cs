using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Plays a sound on an event.
/// </summary>
[Serializable]
public class PlaySoundEventResponder : EventResponder
{
	[SerializeField]
	[Tooltip("The selected sound to play.")]
	private AudioClip soundToPlay;
	[SerializeField]
	[Tooltip("Sets the volume of the soundclip.")]
	private float volume = 100;

	/// <summary>
	/// Returns the name of the event responder
	/// </summary>
	public override string Name
	{
		get { return "Play A Sound"; } 
	}

	/// <summary>
	/// The action performed when the event is triggered.
	/// </summary>
	/// <param name="sender">The sender infomation</param>
	/// <param name="args">Receives a generic event</param>
	public override void HandleEvent(object sender, GenericEventArguments args)
	{
		AudioSource.PlayClipAtPoint(soundToPlay, GameObject.transform.position, volume);
	}

	/// <summary>
	/// Returns the type of event we can handle.
	/// </summary>
	/// <returns></returns>
	public override System.Type GetHandledType()
	{
		return typeof(GenericEventArguments);
	}
}
