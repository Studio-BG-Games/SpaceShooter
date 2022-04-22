using UnityEngine;

public class GameHelper : MonoBehaviour
{
    public void SpawnParticle(ParticleSystem system, Transform point, bool isParent)
    {
        var newSystem = Instantiate(system, point.position, point.rotation);
        if(isParent) newSystem.transform.SetParent(point);
    }
}