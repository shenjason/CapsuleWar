using UnityEngine;
using TMPro;
using EZCameraShake;

public class DeathCam : MonoBehaviour
{
    public Transform Target;
    public TMP_Text DeathText;

    private void Start() {
        CameraShaker.Instance.ShakeOnce(5, 4, 0, 3);
    }
    private void LateUpdate() {
        transform.Rotate(Vector3.up * 45 * Time.deltaTime);
        if (Target != null)
        {
           transform.GetChild(0).LookAt(Target.position);
        }
        else
        {
            transform.GetChild(0).LookAt(transform);
        }
    }
    
}
