using System.Collections;
using UnityEngine;
using Photon.Pun;
using EZCameraShake;

public class SingleShot : Gun
{
    public GameObject ScopeImage;
    public Transform cam;
    public PlayerMovement PlayerMovement;
    public GameObject GunLinePrefab;
    public GameObject bulletImpactPrefab;
    public ParticleSystem bulletShell;
    [HideInInspector] public ParticleSystem MussleFlash;
    PhotonView PV;

    private Animator Gunani;
    protected float reload = Mathf.Infinity;
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
        if (reload > ((GunInfo)itemInfo).GunReloadSpeed && ammo > 0)
        {
            Shoot();    
            reload = 0;
            ammo --;
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
        if (ScopeImage != null)
        {
            ScopeImage.SetActive(false);
            ItemGameObject.transform.GetChild(0).gameObject.SetActive(true);
        }
        // Gunani.SetTrigger("Switch");
        IsAimming = false;
    }

    void OnDisable()
    {
        reset();
    }

    protected void Shoot()
    {
        if (PV == null) return;
        if (!IsAimming)
        {
            GunShootIcon.Play("Shoot", -1, 0);
        }      
        CameraShaker.Instance.ShakeOnce(((GunInfo)itemInfo).GunShake, 4f, 0, 0.5f);
        // Gunani.SetTrigger("Shoot");
        Gunani.Play("Shoot", -1, 0);
        float offSet = IsAimming ? ((GunInfo)itemInfo).HitAimOffset : ((GunInfo)itemInfo).HitOffset;
        offSet *= PlayerMovement.isSprinting ? 1.8f : PlayerMovement.isMoving? 1.4f : 1;
        Vector3 Offsetdir = new Vector3(Random.Range(-offSet, offSet), Random.Range(-offSet, offSet), Random.Range(-offSet, offSet));
        Vector3 hitDir = (cam.forward + Offsetdir).normalized;
        RaycastHit hit;
        if (Physics.Raycast(cam.position, hitDir, out hit, ((GunInfo)itemInfo).GunRange, HitMask))
        {
            
            if (hit.transform.GetComponent<IDamagable>() != null)
            {
                float Dam = 0;
                if (((GunInfo)itemInfo).IntDamage)
                    Dam = (float)Random.Range((int)((GunInfo)itemInfo).MinDam + 1, (int)((GunInfo)itemInfo).Damage + 1);
                else
                {
                    Dam = Random.Range(((GunInfo)itemInfo).MinDam, ((GunInfo)itemInfo).Damage);
                }
                if (hit.transform.GetComponent<PlayerMovement>() && (hit.point.y - hit.transform.position.y) > 0.5f)
                {
                    Dam *= ((GunInfo)itemInfo).CritMultiplyer;
                    if (((GunInfo)itemInfo).IntDamage)
                    {
                        Dam = Mathf.Round(Dam);
                    }
                    ShowCritHit();
                }
                else
                {
                    ShowHit();
                }
                IDamagable hitDam = hit.transform.GetComponent<IDamagable>();
                hitDam.TakeDamage(Dam, PlayerMovement.GetComponent<PhotonView>().ViewID);
                PlayerMovement.IndicateDam(Dam);
                hitDam.TakeKnockBack(((GunInfo)itemInfo).HitKnockBack, hitDir);
            }
            else
            {
                PV.RPC("RPC_SpawnBulletImpact", RpcTarget.All, hit.point, hit.normal);
            }
            DrawLine(hit.point);
        }
        else
        {
            DrawLine(cam.position + hitDir * ((GunInfo)itemInfo).GunRange);
        }
        PlayerMovement.GetComponent<PlayerLook>().Recoil(((GunInfo)itemInfo).Recoil);
    }

    void DrawLine(Vector3 Target)
    {
        PV.RPC("RPC_DrawLine", RpcTarget.All, Target);
    }
    public override void Aim()
    {
        SetRefs();
        PlayerMovement.DeactivateCrosshairs();
        if (ScopeImage != null)
        {
            StartCoroutine(StartScope());
        }
        PlayerMovement.BobbingOffsetX = ((GunInfo)itemInfo).AimOffset.x;
        PlayerMovement.BobbingOffsetY = ((GunInfo)itemInfo).AimOffset.y;

        IsAimming = true;
    }

    public override void StopAim()
    {
        SetRefs();
        PlayerMovement.SwitchCrosshair();
        if (ScopeImage != null)
        {
            ScopeImage.SetActive(false);
            ItemGameObject.transform.GetChild(0).gameObject.SetActive(true);
        }
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
    IEnumerator StartScope()
    {
        yield return new WaitForSeconds(0.3f);
        if (IsAimming)
        {
            ScopeImage.SetActive(true);
            ItemGameObject.transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    protected void SetRefs()
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
        GameObject tracer = Instantiate(GunLinePrefab, FirePoint.position, Quaternion.identity);
        tracer.GetComponent<TrailRenderer>().AddPosition(FirePoint.position);
        tracer.transform.position = Target;
        MussleFlash.Play();
        if (bulletShell == null) return;
        bulletShell.Play();
    }


    void reset()
    {
        if (gameObject.activeInHierarchy == false) return;
        Gunani.Rebind();
        Gunani.Update(0f);
    }


}
