using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView), typeof(PhotonTransformViewClassic))]
public class GunProjectile : MonoBehaviourPun
{

    public GameObject ImpactParticle;
    PhotonView PV;
    public float Damage = 30;
    PlayerMovement PM;
    Rigidbody rb;

    private void Awake() {
        PV = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
    }

    public void Setup(Vector3 dirF, PlayerMovement PMpass)
    {
        rb.AddForce(dirF * 2, ForceMode.Impulse);
        rb.AddTorque(dirF * 10, ForceMode.Impulse);
        PM = PMpass;
    }

    private void Update() {
        if (!PV.IsMine) return;
        if (transform.position.y < -50)
        {
            PhotonNetwork.Destroy(PV);
        }
    }

    private void OnCollisionEnter(Collision other) {
        
        if (other.gameObject.GetComponent<PlayerMovement>())
        {
            other.gameObject.GetComponent<PlayerMovement>().TakeKnockBack(20, rb.velocity.normalized);
            other.gameObject.GetComponent<PlayerMovement>().TakeDamage(Damage, PM.photonView.ViewID);
            PM.IndicateDam(Damage);
            PM.GunHitAni.SetTrigger("Hit");
        }
        PV.RPC("RPC_SpawnImpactParticle", RpcTarget.All, other.GetContact(0).point);
    }


    [PunRPC]

    void RPC_SpawnImpactParticle(Vector3 pos)
    {
        Instantiate(ImpactParticle, pos, Quaternion.identity);
        if (!PV.IsMine) return;
        PhotonNetwork.Destroy(PV);
    }
}
