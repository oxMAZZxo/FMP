using System.Collections.Generic;

public class Pool<T>
{
    private T[] objects;
    private int index;
    internal int length { get; }

    /// <summary>
    /// Instantiates a pool only using an amount, no objects are in the pool
    /// </summary>
    /// <param name="amount">The size of the pool</param>
    public Pool(int amount)
    {
        objects = new T[amount];
        length = amount;
    }

    /// <summary>
    /// Instantiates a pool
    /// </summary>
    /// <param name="amount">The size of the pool</param>
    /// <param name="newObjects">The objects that will go in the pool (as params) or array</param>
    public Pool(int amount, params T[] newObjects)
    {
        objects = new T[amount];
        length = amount;
        objects = newObjects;
    }

    /// <summary>
    /// Instantiates a pool
    /// </summary>
    /// <param name="newObjects">The lists of objects to go in a pool</param>
    public Pool(List<T> newObjects)
    {
        objects = newObjects.ToArray();
        length = newObjects.Count;
    }

    /// <returns>Returns the next available object in the pool</returns>
    public T GetObject()
    {
        if(index >= objects.Length) { index = 0; }
        T currentObject = objects[index];
        index++;
        return currentObject;
    }

    /// <returns>Returns the type of objects the pool is holding</returns>
    public string GetObjectType() {return objects[0].GetType().Name;}
}