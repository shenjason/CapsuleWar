using System.Collections;
using UnityEngine;
using Photon.Pun;
using EZCameraShake;

public class MultiShot : Gun
{
    public Transform cam;
    public PlayerMovement PlayerMovement;
    [SerializeField] GameObject bulletImpactPrefab;
    ParticleSystem MussleFlash;
    public GameObject GunLine;
    PhotonView PV;
    [Header("Settings")]
    public float MinBullets, MaxBullets;

    private Animator Gunani;
    float reload = Mathf.Infinity;
    bool IsAimming;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
        MussleFlash = FirePoint.GetComponentInChildren<ParticleSystem>();
        Gunani = ItemGameObject.GetComponent<Animator>();
        ammo = ((GunInfo)itemInfo).MagSize;
    }

    public override void Use()
    {
        SetRefs();
        float TotalDam = 0;
        if (reload > ((GunInfo)itemInfo).GunReloadSpeed && ammo > 0)
        {
            CameraShaker.Instance.ShakeOnce(((GunInfo)itemInfo).GunShake, 4f, 0, 0.5f);
            for (int i = 0; i < Random.Range(MinBullets, MaxBullets); i++)
            {
                TotalDam += Shoot();
            }  
            ammo --;
            reload = 0;
            if (TotalDam != 0) PlayerMovement.IndicateDam(TotalDam);
            PlayerMovement.GetComponent<PlayerLook>().Recoil(((GunInfo)itemInfo).Recoil);
        }
    }
    void Update()
    {
        if (!PV.IsMine) return;
        reload += Time.deltaTime;
    }
    void OnEnable()
    {
        
        Gunani.Play("Switch", -1, 0);
        if (!PV.IsMine) return;
        IsAimming = false;
    }

    private void OnDisable() {
        if (!PV.IsMine) return;
        reset();
    }
    float Shoot()
    {
        if (PV == null) return 0;
        if (!IsAimming)
        {
            GunShootIcon.Play("Shoot", -1, 0);
        }
        Gunani.Play("Shoot", -1, 0);
        PV.RPC("PlayMussle", RpcTarget.All);
        float offSet = IsAimming ? ((GunInfo)itemInfo).HitAimOffset : ((GunInfo)itemInfo).HitOffset;
        offSet *= PlayerMovement.isSprinting ? 1.8f : PlayerMovement.isMoving? 1.4f : 1;
        Vector3 Offsetdir = new Vector3(Random.Range(-offSet, offSet), Random.Range(-offSet, offSet), Random.Range(-offSet, offSet));
        Vector3 hitDir = (cam.forward + Offsetdir).normalized;
        RaycastHit hit;
        if (Physics.Raycast(cam.position, hitDir, out hit, ((GunInfo)itemInfo).GunRange, HitMask))
        {
            float Dam = 0;
            if (((GunInfo)itemInfo).IntDamage)
                Dam = (float)Random.Range((int)((GunInfo)itemInfo).MinDam + 1, (int)((GunInfo)itemInfo).Damage + 1);
            else
            {
                Dam = Random.Range(((GunInfo)itemInfo).MinDam, ((GunInfo)itemInfo).Damage);
            }
            IDamagable hitDam = hit.transform.GetComponent<IDamagable>();
            if (hitDam != null)
            {
                hitDam.TakeDamage(Dam, PlayerMovement.GetComponent<PhotonView>().ViewID);
                hitDam.TakeKnockBack(((GunInfo)itemInfo).HitKnockBack, hitDir);
                ShowHit();
            }
            else
            {
                PV.RPC("RPC_SpawnBulletImpact", RpcTarget.All, hit.point, hit.normal);
            }
            DrawLine(hit.point);
            
            if (hit.transform.GetComponent<IDamagable>() == null)
            {
                
                return 0;
            }
            else
            {
                
                return Dam;
            }
        }
        else
        {
            DrawLine(cam.position + hitDir * ((GunInfo)itemInfo).GunRange);
            
            return 0;
        }
        
    }


    void DrawLine(Vector3 Target)
    {
        PV.RPC("RPC_DrawLine", RpcTarget.All, Target);
    }
    public override void Aim()
    {
        SetRefs();
        PlayerMovement.DeactivateCrosshairs();
        PlayerMovement.BobbingOffsetX = ((GunInfo)itemInfo).AimOffset.x;
        PlayerMovement.BobbingOffsetY = ((GunInfo)itemInfo).AimOffset.y;
        IsAimming = true;
    }

    public override void StopAim()
    {
        SetRefs();
        PlayerMovement.SwitchCrosshair();
        PlayerMovement.BobbingOffsetX = 0;
        PlayerMovement.BobbingOffsetY = 0;
        IsAimming = false;
    }

    public override void Reload()
    {
        SetRefs();
        PlayerMovement.Aimming = false;
        PlayerMovement.LockAimmingAndSwitching = true;
        StopAim();
        Gunani.Play("Reloading", -1, 0);
        StartCoroutine(Reloading());
    }

    IEnumerator Reloading()
    {
        yield return new WaitForSeconds(((GunInfo)itemInfo).MagReloadSpeed);
        Gunani.Play("StopReload", -1, 0);
        PlayerMovement.LockAimmingAndSwitching = false;
        ammo = ((GunInfo)itemInfo).MagSize;
    }

    void SetRefs()
    {
        if (cam == null)
        {
            cam = transform.parent.transform.parent.GetComponentInChildren<Camera>().transform;
        }
        if (PlayerMovement == null)
        {
            PlayerMovement = GetComponentInParent<PlayerMovement>();
        }
        if (GunHitIcon == null)
        {
            GunHitIcon = PlayerMovement.GunHitAni;
        }
        if (GunShootIcon == null)
        {
            GunShootIcon = (PlayerMovement.CrossHair).transform.GetChild(0).GetComponent<Animator>();
        }
    }

    //PunRpcFunctions

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
        GameObject tracer = Instantiate(GunLine, FirePoint.position, Quaternion.identity);
        tracer.GetComponent<TrailRenderer>().Clear();
        tracer.GetComponent<TrailRenderer>().AddPosition(FirePoint.position);
        tracer.transform.position = Target;
    }

    [PunRPC]

    void PlayMussle()
    {
        MussleFlash.Play();
    }

    void reset()
    {
        if (gameObject.activeInHierarchy == false) return;
        Gunani.Rebind();
        Gunani.Update(0f);
    }
}
