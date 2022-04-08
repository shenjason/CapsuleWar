using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView), typeof(Collider))]
public class PickUp : MonoBehaviour
{
    public bool isGun = true;
    public ItemInfo itemInfo;
    private PhotonView PV;

    private void Awake() {
        PV = GetComponent<PhotonView>();
    }
    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            if (!collider.GetComponent<Rigidbody>()) return;
            PlayerMovement PM = collider.GetComponent<PlayerMovement>();
            if (PM.items.Count >= 5) return;
            PM.GetComponent<PhotonView>().RPC("RPC_Get_Item", RpcTarget.All, itemInfo.itemname);
            PV.RPC("RPC_Destroy", RpcTarget.All);
        }
    }

    private void Update() {
        if (transform.position.y < -30)
        {
            PV.RPC("RPC_Destroy", RpcTarget.All);
        }
    }

    [PunRPC]

    void RPC_Destroy()
    {
        Destroy(gameObject);
    }
}
