using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpAni : MonoBehaviour
{
    public float SpinSpeed = 100;
    public float BobingSpeed = 12f;
    public float BobingIntensity = 0.2f;
    private float targetY;
    private void Awake() {
        targetY = BobingIntensity;
    }
    private void Update() {
        transform.Rotate(new Vector3(0, SpinSpeed * Time.deltaTime, 0), Space.Self);
        UpdatetargetY();
        transform.localPosition = Vector3.Slerp(transform.localPosition, new Vector3(0, targetY + 1, 0), BobingSpeed * Time.deltaTime);
    }

    void UpdatetargetY()
    {
        if (Mathf.Abs((targetY + 1) - transform.localPosition.y) < 0.03f)
        {
            targetY *= -1;
        }
    }
}
