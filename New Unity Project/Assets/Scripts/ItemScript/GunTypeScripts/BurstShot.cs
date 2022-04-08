using System.Collections;
using UnityEngine;
using Photon.Pun;
public class BurstShot : SingleShot
{
    [Header("BurstShotSetting")]
    public int BurstShots = 5;
    public float BurstShootDelay = 0.05f;
    public override void Use()
    {
        SetRefs();
        if (reload > ((GunInfo)itemInfo).GunReloadSpeed && ammo > 0)
        {
            StartCoroutine(BurstFire());
            reload = 0;
            ammo --;
        } 
    }

    IEnumerator BurstFire()
    {
        for (int i = 0; i < BurstShots; i++)
        {
            Shoot(); 
            yield return new WaitForSeconds(BurstShootDelay);
        }
    }

    //PunRPC

    [PunRPC]
    void RPC_SpawnBulletImpact(Vector3 hitPos, Vector3 hitnormal)
    {
        Collider[] col = Physics.OverlapSphere(hitPos, 0.1f, HitMask);
        if (col.Length != 0)
        {
            GameObject bulletimpact = Instantiate(bulletImpactPrefab, hitPos + hitnormal * 0.001f, Quaternion.identity);
            bulletimpact.transform.LookAt(hitPos + hitnormal);
            bulletimpact.transform.SetParent(col[0].transform);
        }
    }
    [PunRPC]
    void RPC_DrawLine(Vector3 Target)
    {
        GameObject tracer = Instantiate(GunLinePrefab, FirePoint.position, Quaternion.identity);
        tracer.GetComponent<TrailRenderer>().AddPosition(FirePoint.position);
        tracer.transform.position = Target;
        MussleFlash.Play();
        if (bulletShell == null) return;
        bulletShell.Play();
    }
}
