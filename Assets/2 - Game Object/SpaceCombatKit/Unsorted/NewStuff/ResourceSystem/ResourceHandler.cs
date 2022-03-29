using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ResourceHandler
{
    public ResourceContainerBase resourceContainer;

    public float unitResourceChange;

    public bool perSecond = false;

    public bool setEmptyWhenInsufficient = true;

    public bool Ready(int numResourceChanges = 1)
    {
        if (setEmptyWhenInsufficient && unitResourceChange < 0) 
        {
            if (!resourceContainer.HasAmount(numResourceChanges * Mathf.Abs(unitResourceChange) * (perSecond ? Time.deltaTime : 1))) resourceContainer.Empty();
        } 

        if (!resourceContainer.CanAddRemove(numResourceChanges * unitResourceChange * (perSecond ? Time.deltaTime : 1))) return false;

        return true;
    }

    public virtual void Implement(int numResourceChanges = 1) 
    {
        resourceContainer.AddRemove(numResourceChanges * unitResourceChange * (perSecond ? Time.deltaTime : 1));
    }
}
