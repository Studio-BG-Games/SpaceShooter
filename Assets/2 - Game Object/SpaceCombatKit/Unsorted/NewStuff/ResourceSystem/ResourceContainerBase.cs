using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ResourceContainerBase : MonoBehaviour
{
    public virtual ResourceType ResourceType { get { return null; } }

    public virtual float CapacityFloat
    {
        get { return 0; }
    }

    public virtual float CurrentAmountFloat
    {
        get { return 0; }
    }

    public virtual int CapacityInteger
    {
        get { return 0; }
    }

    public virtual int CurrentAmountInteger
    {
        get { return 0; }
    }

    public virtual bool IsFull { get { return false; } }

    public virtual bool IsEmpty { get { return false; } }

    public virtual float ChangeRate { get { return 0; } }

    [Header("Events")]

    public UnityEvent onEmpty;
    public UnityEvent onFilled;

    public virtual void AddRemove(float amount) { }

    public virtual bool CanAddRemove(float amount) { return false; }

    public virtual void AddRemove(int amount) { }

    public virtual bool CanAddRemove(int amount) { return false; }

    public virtual void Fill() { }

    public virtual void Empty() { }

    protected virtual void OnFilled()
    {
        onFilled.Invoke();
    }

    protected virtual void OnEmpty()
    {
        onEmpty.Invoke();
    }

    public virtual bool HasAmount(float amount)
    {
        return false;
    }

    public virtual bool HasAmount(int amount)
    {
        return false;
    }
}
