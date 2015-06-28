using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// A super-simple component with some example events that can be hooked into via the Events Editor GUI.
/// </summary>
public class ExampleEvents : MonoBehaviour
{
    /// <summary>
    /// Called on Start().
    /// </summary>
    public event EventHandler<EmptyEventArguments> OnStart;
    /// <summary>
    /// Called every Update().
    /// </summary>
    public event EventHandler<EmptyEventArguments> OnUpdate;
    /// <summary>
    /// Called by OnDestroy().
    /// </summary>
    public event EventHandler<EmptyEventArguments> OnDestroyed;

    // Use this for initialization
    void Start()
    {
        if (OnStart != null) OnStart(this, new EmptyEventArguments());
    }

    // Update is called once per frame
    void Update()
    {
        if (OnUpdate != null) OnUpdate(this, new EmptyEventArguments());
    }

    void OnDestroy()
    {
        if (OnDestroyed != null) OnDestroyed(this, new EmptyEventArguments());
    }
}
