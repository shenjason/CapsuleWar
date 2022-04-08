using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Linq;  
using System.Collections.Generic;

public class LeaderBoard : MonoBehaviourPunCallbacks
{
    public Placer[] Placers;
    public static LeaderBoard Instance;
    public Dictionary<string, int> EGI = new Dictionary<string, int>();

    private void Awake() {
        Instance = this;
    }

    public void UpdatePlacing()
    {
        if (Placers[0] != null)
        {
            foreach (Placer p in Placers)
            {
                p.gameObject.SetActive(false);
            }
        }
        
        Player[] Players = PhotonNetwork.PlayerList;
        List<int> k = new List<int>();
        foreach (Player p in Players)
        {
            k.Add((int)p.CustomProperties["Kills"]);
        }
        var ranked = k.Select((score, index) => new {Player=index, Score=score}).OrderByDescending(pair => pair.Score).ToList();
        EGI.Clear();
        for (int i = 0; i < Players.Length; i++)
        {
            if (Placers[i] != null)
            {
                Placers[i].gameObject.SetActive(true);
                Color c = new Color();
                if (Players[ranked[i].Player] == PhotonNetwork.LocalPlayer)
                {
                    c = Color.yellow;
                }
                else
                {
                    c = Color.white;
                }
                Placers[i].UpdatePlacer((i + 1).ToString(), Players[ranked[i].Player].NickName, ranked[i].Score.ToString(), c);
            }  
            EGI.Add(Players[ranked[i].Player].NickName, i);
        }
    }

    
}
