using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using IEnumerator = System.Collections.IEnumerator;
using Photon.Realtime;
using TMPro;


public class EndGameManeger : MonoBehaviourPunCallbacks
{
    public TMP_Text[] Places;

    public static EndGameManeger Instance;
    public Dictionary<string, int> Info = new Dictionary<string, int>();
    private void Awake() {
        Instance = this;
    }
    private void Start() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        StartCoroutine(SetPlaces());
    }
    public void OnClickLeave()
    {
        PhotonNetwork.LeaveRoom();
        RoomManeger.Instance.Destroy();
        SceneLoader.Instance.LoadSceneWithAni("Lobby");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        SceneLoader.Instance.LoadSceneWithAniError("MainMenu", cause.ToString());
    }
    IEnumerator SetPlaces()
    {
        yield return new WaitForSeconds(0.2f);
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Places[Info[PhotonNetwork.PlayerList[i].NickName]].SetText(PhotonNetwork.PlayerList[i].NickName); 
        }
        
    }

}
