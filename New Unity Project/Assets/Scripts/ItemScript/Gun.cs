using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Gun : Item
{
    public LayerMask HitMask;
    public Transform FirePoint;
    public Animator GunHitIcon;
    public Animator GunShootIcon;
    public abstract override void Use();
    public abstract void Aim();
    public abstract void StopAim();
    public abstract void Reload();
    [HideInInspector] public int ammo;

    public void ShowHit()
    {
        GunHitIcon.SetTrigger("Hit");
    }

    public void ShowCritHit()
    {
         GunHitIcon.SetTrigger("HitCrit");
    }
}
