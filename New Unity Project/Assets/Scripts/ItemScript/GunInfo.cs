using UnityEngine;
[CreateAssetMenu(menuName = "FPS/New Gun")]
public class GunInfo : ItemInfo
{
    public float MinDam = 2;
    public float Damage = 5;
    public float Recoil = 3;
    public float CritMultiplyer = 1.2f;
    public bool IntDamage = true;
    public float GunRange = 50;
    public float GunReloadSpeed = 0.5f;
    public int MagSize = 6;
    public float MagReloadSpeed = 2.75f;
    public float HitOffset = 0.05f;
    public float HitAimOffset = 0.02f;
    public float Weight = 0.98f;
    public float GunShake = 2.75f;
    public float HitKnockBack = 5;
    public float GunAimFov = -20f;
    public Vector3 AimOffset;
}
