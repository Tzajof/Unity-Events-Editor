using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// This Event Responder will spawn an object when activated at its game object's position.
/// </summary>
[Serializable]
public class SpawnObjectEventResponder : EventResponder
{
	[SerializeField]
	[Tooltip("The Prefab you would like to spawn")]
	private GameObject objectToSpawn;

	/// <summary>
	/// The user-friendly name of this event responder.
	/// </summary>
	public override string Name { get { return "Spawn A Prefab"; } }

	/// <summary>
	/// Performs the action of spawning the designated object.
	/// </summary>
	/// <param name="sender">Object from which the event originated. Ignored.</param>
	/// <param name="args">Event arguments object. Ignored.</param>
	public override void HandleEvent(object sender, GenericEventArguments args)
	{
		GameObject.Instantiate(objectToSpawn, GameObject.transform.position, Quaternion.identity);
	}

	/// <summary>
	/// Returns the type of events we can handle- anything.
	/// </summary>
	/// <returns></returns>
	public override Type GetHandledType()
	{
		return typeof(GenericEventArguments);
	}
}
