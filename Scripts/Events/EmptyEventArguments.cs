using System;
using System.Collections.Generic;

/// <summary>
/// EmptyEventArguments represents arguments for an event which provides no information
/// at all.
/// This is introduced so you can accurately and easily represent that you provide no special
/// arguments to event listeners.
/// </summary>
public class EmptyEventArguments : GenericEventArguments
{
	/// <summary>
	/// Returns a mapping of all valid arguments and their names- none!
	/// </summary>
	/// <returns></returns>
	public override Dictionary<string, Type> GetValidArguments()
	{
		return new Dictionary<string, Type>();
	}
}
