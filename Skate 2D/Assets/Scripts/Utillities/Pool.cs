using System;
using System.Collections.Generic;

/// <summary>
/// The pool class allows developers to create a list of objects which they can re-use/recycle
/// </summary>
/// <typeparam name="T"></typeparam>
public class Pool<T>
{
    private T[] objects;
    private int index;
    /// <summary>
    /// The number of objects the pool holds.
    /// </summary>
    public int length { get; }

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
    public object GetObjectType() {return objects[0].GetType();}

    /// <summary>
    /// Get a random object with a limit
    /// </summary>
    /// <param name="maxExclusive">The max index to choose from; EXCLUSIVE. </param>
    /// <returns>Returns a random object except the object previously returned</returns>
    public T GetRandomObject(int maxExclusive)
    {
        int indexPreviouslyUsed = index-1;
        if(indexPreviouslyUsed == -1){indexPreviouslyUsed = objects.Length -1;}

        Random rnd = new Random();
        int indexToReturn;
        do
        {
            indexToReturn = rnd.Next(0, maxExclusive);
        } while (indexPreviouslyUsed == indexToReturn);

        index = indexToReturn;
        T currentObject = objects[index];
        return currentObject;
    }
    
    public T[] GetObjects() {return objects;}

    public void RollBack()
    {
        index --;
        if(index < 0) {index = length -1;}
    }
}