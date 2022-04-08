using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform cam;

    void LateUpdate()
    {
        if (cam == null)
            cam = FindObjectOfType<Camera>().transform;

        if (cam == null)
            return;
        transform.LookAt(cam.position);
        transform.Rotate(Vector3.up * 180);
    }
}
