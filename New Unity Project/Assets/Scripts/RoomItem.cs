using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RoomItem : MonoBehaviour
{
    public TMP_Text roomName;
    public LobbyManeger maneger;

    void Start()
    {
        maneger = FindObjectOfType<LobbyManeger>();
    }
    public void SetRoomName(string name)
    {
        roomName.text = name;
    }

    public void OnClickItem()
    {
        maneger.JoinRoom(roomName.text);
    }
}
