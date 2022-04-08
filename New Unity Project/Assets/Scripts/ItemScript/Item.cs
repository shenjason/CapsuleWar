using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public abstract class Item : MonoBehaviourPun
{
    public ItemInfo itemInfo;
    public GameObject ItemGameObject;

    [HideInInspector] public PhotonView itemPV;
    public abstract void Use();
}
