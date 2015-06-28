using System.Collections.Generic;

/// <summary>
/// SelectionList is intended to be used with a SelectionGrid inside the Unity Editor GUI.
/// It will take a list of objects, along with a delegate to use in order to determine
/// what name to show for each object in the SelectionGrid.
/// It also allows you to track the currently selected object.
/// </summary>
/// <typeparam name="T">The type to store and display in our list</typeparam>
public class SelectionList<T>
{
    /// <summary>
    /// A delegate to return the string to display to the screen for any given object.
    /// </summary>
    /// <param name="objectToStringify">The object to get the name for.</param>
    /// <returns>The object's display string.</returns>
    public delegate string Stringifier(T objectToStringify);

    private List<T> objects = new List<T>();
    /// <summary>
    /// Gets the current list of objects that we are displaying.
    /// </summary>
    public List<T> Objects { get { return objects; } }

    private string[] objectNames;
    /// <summary>
    /// Publically accessible array of names for the objects we are displaying. The index of each name will match
    /// the index of the matching object in Objects.
    /// </summary>
    public string[] ObjectNames { get { return objectNames; } }

    
    private int selectedIndex = -1;
    /// <summary>
    /// Index of the currently selected object. -1 for none.
    /// </summary>
    public int SelectedIndex { get { return selectedIndex; } }

    /// <summary>
    /// Gets the currently selected object from our list. Null if nothing is selected.
    /// </summary>
    public T SelectedObject 
    { 
        get 
        { 
            return selectedIndex >= 0 && selectedIndex < objects.Count ? objects[selectedIndex] : default(T); 
        } 
    }

    //Our stringification delegate.
    private event Stringifier Stringify;

    /// <summary>
    /// Constructs a new instance of SelectionList for the appropriate type, using the given function delegate to determine what
    /// object names are printed to the screen.
    /// </summary>
    /// <param name="nameFunction">Delegate to return the display name of any object.</param>
    public SelectionList(Stringifier nameFunction)
    {
        Stringify = nameFunction;
        GenerateNames();
    }

    /// <summary>
    /// Sets the current contents of this list, and re-generates their names for the ObjectNames array.
    /// </summary>
    /// <param name="objects">New contents of our list.</param>
    public void SetContents(List<T> objects)
    {
        this.objects = objects;
        GenerateNames();
    }

    /// <summary>
    /// Sets the current selection index. Set to -1 to indicate that nothing is selected.
    /// </summary>
    /// <param name="index">Newly selected object index.</param>
    public void SetSelection(int index)
    {
        selectedIndex = index;
    }

	/// <summary>
	/// Convenience function to set the given object as the current selection.
	/// If the object doesn't exist in this list, this function will have no effect.
	/// </summary>
	/// <param name="desiredSelection">The object you would like to now be selected.</param>
	public void SetSelectedObject(T desiredSelection)
	{
		
		for (int i = 0; i < Objects.Count; i++)
		{
			if (EqualityComparer<T>.Default.Equals(Objects[i], desiredSelection))
			{
				SetSelection(i);
				break;
			}
		}
	}

    /// <summary>
    /// Re-generates the array to contain names for all our current objects.
    /// </summary>
    private void GenerateNames()
    {
        objectNames = new string[objects.Count];
        for (int i = 0; i < objects.Count; i++)
        {
            objectNames[i] = Stringify(objects[i]);
        }
    }
}