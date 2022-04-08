using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PlayerLook : MonoBehaviour
{
    [Header("References")]
    [SerializeField] WallRun wallRun;

    [SerializeField] private float sensX = 100f;
    [SerializeField] private float sensY = 100f;

    [SerializeField] Transform cam = null;
    [SerializeField] Transform orientation = null;

    float mouseX;
    float mouseY;

    float multiplier = 0.01f;

    float xRotation;
    float yRotation;
    float tilt = 0;
    PhotonView PV;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        PV = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (!PV.IsMine) return;
        mouseX = Input.GetAxisRaw("Mouse X");
        mouseY = Input.GetAxisRaw("Mouse Y");
        tilt = Mathf.Lerp(tilt, 0, 2 * Time.deltaTime);
        yRotation += mouseX * sensX * multiplier;
        xRotation -= mouseY * sensY * multiplier;
        float xtRotation = xRotation + tilt;
        xtRotation = Mathf.Clamp(xtRotation, -90f, 90f);
        cam.transform.rotation = Quaternion.Euler(xtRotation, yRotation, wallRun.tilt);
        orientation.transform.rotation = Quaternion.Euler(0, yRotation, 0);
        // if (Input.GetKeyDown(KeyCode.Escape))
        // {
        //     Cursor.lockState = CursorLockMode.None;
        //     Cursor.visible = true;
        // }
        // if (Input.GetKeyDown(KeyCode.Mouse0))
        // {
        //     Cursor.lockState = CursorLockMode.Locked;
        //     Cursor.visible = false;
        // }
    }

    public void Recoil(float ti)
    {
        StartCoroutine(Recoiladd(ti));
    }

    IEnumerator Recoiladd(float ti)
    {
        int len = 10;
        for (int i = 0; i < len; i++)
        {
            tilt -= ti/len;
            yield return new WaitForEndOfFrame();
        }
    }
    
}