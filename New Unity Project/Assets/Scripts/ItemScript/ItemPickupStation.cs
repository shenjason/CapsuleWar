using Photon.Pun;
using UnityEngine;
using System.IO;

[RequireComponent(typeof(PhotonView))]
public class ItemPickupStation : MonoBehaviourPun
{

    public GameObject DeployEffect;
    public ItemInfo ItemToDeploy;
    public Transform itemHolder;
    public bool AtStartDeploy = true;
    public float TimetoDeploy = 10;
    private float t = 0;
    PhotonView PV;

    private void Start() {
        if (AtStartDeploy) t = Mathf.Infinity;
        PV = GetComponent<PhotonView>();
    }

    private void Update() {
        if (!PhotonNetwork.IsMasterClient) return;
        bool itemThere = CheckifItemIsThere();
        if (!itemThere)
        {
            t += Time.deltaTime;
            if (t > TimetoDeploy)
            {
                GameObject currentitem = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "ItemsPickup", ItemToDeploy.PickUpGameObject.name), itemHolder.position, Quaternion.identity);
                PV.RPC("RPC_SpawnParticle", RpcTarget.All);
                currentitem.transform.parent = itemHolder;
                t = 0;
            }
        } 
    }

    bool CheckifItemIsThere()
    {
        if (itemHolder.childCount > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    [PunRPC]

    void RPC_SpawnParticle()
    {
        Instantiate(DeployEffect, itemHolder.position, Quaternion.identity);
    }

}
