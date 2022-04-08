using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunSwayAndBob : MonoBehaviour
{
    [Header("SwaySett")]
    [SerializeField] private float smooth = 8;
    [SerializeField] private float swaymultiplyer = 4;
    public BobingOverride[] BobSettings;
    private float currentTX = 0;
    private float currentTY = 0;
    private float xpos = 0;
    private float ypos = 0;
    [HideInInspector] public Vector2 BobingOffset = Vector2.zero;
    private Vector3 smoothV;


    public void SetBob(int index, float currentSpeed)
    {
        BobingOverride bs = BobSettings[index];
        float BM = (currentSpeed == 0) ? 1 : currentSpeed;

        currentTX += bs.SpeedX / 10 * Time.deltaTime * BM;
        currentTY += bs.SpeedY / 10 * Time.deltaTime * BM;

        xpos = bs.BobX.Evaluate(currentTX) * bs.IntensityX;
        ypos = bs.BobY.Evaluate(currentTY) * bs.IntensityY;
    }

    private void FixedUpdate() {
        GunSway();
        Vector3 target = new Vector3(xpos + BobingOffset.x, ypos + BobingOffset.y, 0);
        Vector3 DesPos = Vector3.SmoothDamp(transform.localPosition, target, ref smoothV, 0.1f);
        transform.localPosition = DesPos;
    }
    private void GunSway()
    {
        float MouseX = Input.GetAxisRaw("Mouse X") * swaymultiplyer;
        float MouseY = Input.GetAxisRaw("Mouse Y") * swaymultiplyer;

        Quaternion rotX = Quaternion.AngleAxis(-MouseY, Vector3.right);
        Quaternion rotY = Quaternion.AngleAxis(-MouseX, Vector3.up);

        Quaternion tarrot = rotX * rotY;

        transform.localRotation = Quaternion.Slerp(transform.localRotation, tarrot, smooth * Time.fixedDeltaTime);
    }



    [System.Serializable]
    public struct BobingOverride
    {
        [Header("X Settings")]
        public float SpeedX;
        public float IntensityX;
        public AnimationCurve BobX;

        [Header("Y Settings")]
        public float SpeedY;
        public float IntensityY;
        public AnimationCurve BobY;

    }
}
