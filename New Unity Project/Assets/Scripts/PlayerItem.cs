using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class PlayerItem : MonoBehaviour
{
    public MeshRenderer Skin;
    public TMP_Text Name;
    public GameObject[] LocalStuff;
    [HideInInspector] public Photon.Realtime.Player myPlayer = null;
    private int skinI = 0;

    public void SetPlayerInfo(Photon.Realtime.Player _player)
    {
        myPlayer = _player;
        Name.SetText(_player.NickName);
        LobbyManeger.Instance.UpdatePlayerInfo("Skin", skinI);
    }

    public void TurnOnArrows()
    {
        foreach (GameObject LS in LocalStuff)
        {
            LS.SetActive(true);
        }
    }
    public void ApplyLocalChanges(int i)
    {
        Skin.material = SkinsManeger.Instance.skins[i].mainskinmat;
    }

    public void OnClickRightArrow()
    {
        skinI ++;
        if (skinI > SkinsManeger.Instance.skins.Length - 1)
        {
            skinI = 0;
        }
        LobbyManeger.Instance.UpdatePlayerInfo("Skin", skinI);
        // ApplyLocalChanges(skinI);
    }
    public void OnClickLeftArrow()
    {
        skinI --;
        if (skinI < 0)
        {
            skinI = SkinsManeger.Instance.skins.Length - 1;
        }
        LobbyManeger.Instance.UpdatePlayerInfo("Skin", skinI);
        // ApplyLocalChanges(skinI);
    }
}
