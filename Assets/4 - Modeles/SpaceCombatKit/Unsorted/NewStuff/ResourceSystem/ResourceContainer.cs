using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ResourceContainer : ResourceContainerBase
{

    [Header("Settings")]

    [SerializeField]
    protected ResourceType resourceType;
    public override ResourceType ResourceType
    {
        get { return resourceType; }
    }

    [SerializeField]
    protected float capacity = 100;
    public override float CapacityFloat { get { return capacity; } }
    public override int CapacityInteger { get { return (int)capacity; } }

    [SerializeField]
    protected float changeRate = 25;
    public override float ChangeRate
    {
        get { return changeRate; }
    }

    [SerializeField]
    protected float startAmount = 100;

    protected float currentAmount = 0;
    public override float CurrentAmountFloat { get { return currentAmount; } }
    public override int CurrentAmountInteger { get { return (int)currentAmount; } }

    protected float lastChangeTime = 0;

    [Header ("Refill After Empty")]

    [SerializeField]
    protected float emptiedPause = 0;

    [SerializeField]
    protected bool fillToCapacityAfterEmptiedPause = true;

    [Header ("Empty After Filled")]

    [SerializeField]
    protected float filledPause = 0;

    [SerializeField]
    protected bool emptyAfterFilledPause = false;

    protected bool pausing = false;
   
    protected float pauseStartTime;
    protected float pauseTime;

    public override bool IsFull
    {
        get { return Mathf.Approximately(currentAmount, capacity); }
    }

    public override bool IsEmpty
    {
        get { return Mathf.Approximately(currentAmount, 0); }
    }


    protected virtual void Awake()
    {
        currentAmount = Mathf.Clamp(startAmount, 0, capacity);
    }

    public override void AddRemove (float amount)
    {
        float nextValue = currentAmount + amount;

        if (nextValue >= capacity && !Mathf.Approximately(currentAmount, capacity))
        {
            OnFilled();
        }

        if (nextValue <= 0 && !Mathf.Approximately(currentAmount, 0))
        {
            OnEmpty();
        }

        currentAmount = Mathf.Clamp(nextValue, 0, capacity);

    }

    public override void AddRemove(int amount)
    {
        AddRemove((float)amount);
    }

    public override bool CanAddRemove(float amount)
    {
        
        if (pausing) return false;
        if (amount > 0 && (capacity - currentAmount) < amount) return false;
        if (amount < 0 && (currentAmount + amount) < 0) return false;

        return true;
    }

    public override bool CanAddRemove(int amount)
    {
        return CanAddRemove((float)amount);
    }

    public override void Fill()
    {
        if (pausing) return;
        currentAmount = capacity;
        OnFilled();
    }

    public override void Empty()
    {
        if (pausing) return;
        currentAmount = 0;
        OnEmpty();
    }

    protected override void OnFilled()
    {
        base.OnFilled();

        if (filledPause > 0)
        {
            pausing = true;
            pauseStartTime = Time.time;
            pauseTime = filledPause;
        }
    }

    protected override void OnEmpty()
    {
        base.OnEmpty();

        if (emptiedPause > 0)
        {
            pausing = true;
            pauseStartTime = Time.time;
            pauseTime = emptiedPause;
        }
    }

    public override bool HasAmount(float amount)
    {
        return (currentAmount >= amount);
    }

    public override bool HasAmount(int amount)
    {
        return (currentAmount >= amount);
    }

    protected virtual void Update()
    {
        if (!pausing)
        {
            AddRemove(changeRate * Time.deltaTime);
        }
        else
        {
            // If filled/emptied pause is finished, implement settings
            if (Time.time - pauseStartTime >= pauseTime)
            {
                pausing = false;

                if (IsEmpty && fillToCapacityAfterEmptiedPause)
                {
                    Fill();
                }
                else if (IsFull && emptyAfterFilledPause)
                {
                    Empty();
                }
            }
        }
    }
}
