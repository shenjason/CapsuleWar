using UnityEngine;

public class Dummy : MonoBehaviour, IDamagable
{
    public void TakeDamage(float Dam, int ID)
    {
        return;
    }
    public void TakeKnockBack(float dirf, Vector3 dirn)
    {
        GetComponent<Rigidbody>().AddForce(dirf * dirn, ForceMode.Impulse);
    }
}
