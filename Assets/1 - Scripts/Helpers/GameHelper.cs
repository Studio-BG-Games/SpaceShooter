using UnityEngine;

public class GameHelper : MonoBehaviour
{
    public void SpawnParticle(ParticleSystem system, Transform point, bool isParent)
    {
        var newSystem = Instantiate(system, point.position, point.rotation);
        if(isParent) newSystem.transform.SetParent(point);
    }

    public void MakeInpulseRigibody(Rigidbody rb, GameObject targetFrom, float force)
    {
        var diraction = (rb.transform.position - targetFrom.transform.position).normalized;
        rb.AddForce(diraction*force, ForceMode.Impulse);
    }
    
    public void MakeTorque(Rigidbody rb, GameObject targetFrom, float force)
    {
        var startRotate = rb.transform.eulerAngles;
        var diraction = (rb.transform.position - targetFrom.transform.position).normalized;
        rb.transform.LookAt(rb.transform.position+diraction);
        var finishRotate = rb.transform.eulerAngles;
        rb.transform.eulerAngles = startRotate;

        var rotateDir = (finishRotate - startRotate).normalized;
        rb.AddTorque(rotateDir*force, ForceMode.Impulse);
        
    }


    public void MakeInpulseForward(Rigidbody rb, float force, bool isInverted)
    {
        rb.AddForce(rb.transform.forward * force * (isInverted?-1:1), ForceMode.Impulse);
    }
}