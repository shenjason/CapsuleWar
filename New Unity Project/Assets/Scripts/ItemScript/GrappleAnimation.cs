using UnityEngine;

public class GrappleAnimation : MonoBehaviour
{
    public Grapple grapple;
    private Quaternion desqua;
    private float roaspeed = 5f;
    private void Update() {
        if (!grapple.Isgrappling())
        {
            desqua = transform.parent.rotation;
        }
        else
        {
            desqua = Quaternion.LookRotation(grapple.hookPoint - transform.position);
        }

        transform.rotation = Quaternion.Lerp(transform.rotation, desqua, Time.deltaTime * roaspeed);
    }
}
