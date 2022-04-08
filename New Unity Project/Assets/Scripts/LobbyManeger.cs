using TMPro;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using IEnumerator = System.Collections.IEnumerator;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Collections.Generic;

[RequireComponent(typeof(PhotonView))]
public class LobbyManeger : MonoBehaviourPunCallbacks
{
    public static LobbyManeger Instance;
    public GameObject settings;
    public TMP_InputField roomInputField;
    public GameObject lobbyPanel, roomPanel, StartButton;
    public TMP_Text roomName;
    public RoomItem roomItemPrefab;
    List<RoomItem> roomItemsList = new List<RoomItem>();
    public List<PlayerItem> PlayerItems = new List<PlayerItem>();
    public PlayerItem PlayerItemPrefab;
    public Transform PlayerItemHolder;
    public Transform contentObject;


    //Player
    private Hashtable playerInfo = new Hashtable();

    void Start()
    {
        Instance = this;
        PhotonNetwork.JoinLobby();
        playerInfo.Add("Skin", 0);
        playerInfo.Add("Ready", false);
    }

    public void OnClickCreateNewRoom()
    {
        if (roomInputField.text.Length >= 1)
        {
            PhotonNetwork.CreateRoom(roomInputField.text, new Photon.Realtime.RoomOptions(){MaxPlayers = 4});
        }

    }

    public override void OnJoinedRoom()
    {
        SceneLoader.Instance.LoadSceneWithGam(lobbyPanel, roomPanel);
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerInfo);
        StartButton.SetActive(PhotonNetwork.IsMasterClient);
        settings.SetActive(PhotonNetwork.IsMasterClient);
        roomName.SetText(PhotonNetwork.CurrentRoom.Name);
        UpdatePlayerList();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateRoomList(roomList);
    }
    
    void UpdateRoomList(List<RoomInfo> list)
    {
        foreach(RoomItem item in roomItemsList)
        {
            Destroy(item.gameObject);
        }
        roomItemsList.Clear();

        foreach(RoomInfo room in list)
        {
            if (!room.RemovedFromList)
            {
                RoomItem newRoom = Instantiate(roomItemPrefab, contentObject);
                newRoom.SetRoomName(room.Name);
                roomItemsList.Add(newRoom);
            }
        }
    }


    public void JoinRoom(string name)
    {
        PhotonNetwork.JoinRoom(name);
    }

    public void OnClickLeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
    public override void OnLeftRoom()
    {
       SceneLoader.Instance.LoadSceneWithGam(roomPanel, lobbyPanel);
    }
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        SceneLoader.Instance.LoadSceneWithAniError("MainMenu", cause.ToString());
    }

    void UpdatePlayerList()
    {
        foreach(PlayerItem item in PlayerItems)
        {
            Destroy(item.gameObject);
        }
        PlayerItems.Clear();
        if (PhotonNetwork.CurrentRoom == null)
        {
            return;
        }
         
        foreach (KeyValuePair<int, Photon.Realtime.Player> player in PhotonNetwork.CurrentRoom.Players)
        {
            PlayerItem newPlayerItem = Instantiate(PlayerItemPrefab, PlayerItemHolder);
            newPlayerItem.SetPlayerInfo(player.Value);
            if (PhotonNetwork.LocalPlayer == player.Value)
            {
                newPlayerItem.TurnOnArrows();
            }
            PlayerItems.Add(newPlayerItem);
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        UpdatePlayerList();
    }
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        UpdatePlayerList();
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        if (StartButton == null) return;
        StartButton.SetActive(PhotonNetwork.IsMasterClient);
        settings.SetActive(PhotonNetwork.IsMasterClient);   
    }
    public void OnClickStartButton()
    {
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerInfo);
        if (PhotonNetwork.IsMasterClient)
        {
            int secs = settings.GetComponent<ValueChangerUI>().value * 60;
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                Hashtable hash = p.CustomProperties;
                hash.Add("GameTime", secs);
                p.SetCustomProperties(hash);
            }
        }
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        StartCoroutine(LoadS());
    }

    IEnumerator LoadS()
    {
        photonView.RPC("RPC_LoadNextSceneAni", RpcTarget.All);
        yield return new WaitForSeconds(0.5f);
        PhotonNetwork.LoadLevel(2);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        foreach (PlayerItem PlayerItem in PlayerItems)
        {
            if (PlayerItem.myPlayer == targetPlayer)
            {
                PlayerItem.ApplyLocalChanges((int)targetPlayer.CustomProperties["Skin"]);
                return;
            }
        }
    }

    public void UpdatePlayerInfo(string key, int Info)
    {
        playerInfo[key] = Info;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerInfo);
    }

    //PunRPC
    [PunRPC]

    void RPC_LoadNextSceneAni()
    {
        SceneLoader.Instance.LoadSceneWithAniFake();
    }

}
