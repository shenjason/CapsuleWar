using UnityEngine;
using Photon.Pun;
using EZCameraShake;

public class Grapple : Item
{
    public Transform CameraHolder;
    public Transform FirePoint;
    public LayerMask hitMask;
    [HideInInspector] public Vector3 hookPoint = Vector3.zero;
    public GameObject Player;
    public LineRenderer Line;
    bool Using = false;
    bool hitPlayer = false;
    Transform otherPlayer = null;
    private SpringJoint sj;
    private PhotonView PV;
    private Vector3 shootdir;

    private bool isHookingForPlayer = false;

    private void Awake() {
        PV = GetComponent<PhotonView>();
    }
    
    public override void Use()
    {
        SetRefs();
        Using = true;
    }

    private void LateUpdate() {
        if (!PV.IsMine)
        {
            if (isHookingForPlayer)
            {
                Line.positionCount = 2;
                Line.SetPosition(0, hookPoint);
                Line.SetPosition(1, FirePoint.position);
            }
            else
            {
                Line.positionCount = 0;
            }
            return;
        }

        if (Using)
        {
            StartGrapple();
        }
        else
        {
            StopGrapple();
        }
        Using = false;
        DrawLine();
        if (hitPlayer && Vector3.Distance(otherPlayer.position, Player.transform.position) < 2f)
        {
            CameraShaker.Instance.ShakeOnce(4, 4, 0, 2);
            StopGrapple();
            otherPlayer.GetComponent<IDamagable>().TakeDamage(10, Player.GetComponent<PhotonView>().ViewID);
            otherPlayer.GetComponent<IDamagable>().TakeKnockBack(30, shootdir);
            Player.GetComponent<PlayerMovement>().GunHitAni.SetTrigger("Hit");
            Player.GetComponent<Rigidbody>().velocity = shootdir * -30 + Vector3.up * 30;
        }
    }
    void StartGrapple()
    {
        Using = true;
        
        if (sj) return;
        hitPlayer = false;
        RaycastHit hit;
        if (Physics.Raycast(CameraHolder.position, CameraHolder.forward, out hit, 100, hitMask))
        {
            hookPoint = hit.point;
            sj = Player.AddComponent<SpringJoint>();
            sj.autoConfigureConnectedAnchor = false;
            sj.connectedAnchor = hookPoint;

            float dis = Vector3.Distance(Player.transform.position, hookPoint);

            sj.maxDistance = dis * 0.8f;
            sj.minDistance = dis * 0.25f;
            if (hit.transform.GetComponent<IDamagable>() != null)
            {
                if (hit.transform == Player) return;
                sj.spring = 9f;
                sj.damper = 0;
                sj.massScale = 4.5f;
                hitPlayer = true;
                otherPlayer = hit.transform;
                shootdir = CameraHolder.forward;
            }
            else
            {
                sj.spring = 4.5f;
                sj.damper = 7f;
                sj.massScale = 4.5f;
            }
            PV.RPC("RPC_SnycHook", RpcTarget.Others, hookPoint);

        }
    }

    private void OnDisable() {
        Destroy(sj);
        Using = false;
    }
    void DrawLine()
    {
        if (!sj) return;
        Line.positionCount = 2;
        Line.SetPosition(0, hookPoint);
        Line.SetPosition(1, FirePoint.position);
        if (hitPlayer)
        {
            sj.maxDistance *= 0.1f * Time.deltaTime;
            sj.minDistance *= 0.1f * Time.deltaTime;
        }
        else
        {
             sj.maxDistance *= 0.5f * Time.deltaTime;
            sj.minDistance *= 0.5f * Time.deltaTime;
        }
       
    }
    void StopGrapple()
    {
        hitPlayer = false;
        Line.positionCount = 0;
        Destroy(sj);
        PV.RPC("RPC_SnycDisHook", RpcTarget.Others);
    }
    void SetRefs()
    {
        if (CameraHolder == null)
        {
            CameraHolder = transform.parent.transform.parent.GetComponentInChildren<Camera>().transform;
        }
        if (Player == null)
        {
            Player = GetComponentInParent<PlayerMovement>().gameObject;
        }
    }

    public bool Isgrappling()
    {
        return sj != null;
    }

    [PunRPC]

    void RPC_SnycHook(Vector3 p)
    {
        hookPoint = p;
        isHookingForPlayer = true;
    }

    [PunRPC]

    void RPC_SnycDisHook()
    {
        isHookingForPlayer = false;
    }
}
