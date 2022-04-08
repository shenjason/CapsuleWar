using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using Hashtable = ExitGames.Client.Photon.Hashtable;
public class PlayerManeger : MonoBehaviourPunCallbacks
{
    PhotonView PV;
    GameObject controller;
    PlayerMovement controllerPM;
    public GameObject DeathViewCam;

    bool hasCrateCon = false;
    int LastS = 1;

    float Clock = 0;
    int s = 0;
    int m = 0;
    public int Kills { get; private set; }

    Camera c;

    [HideInInspector] public RoomManeger RM;
    public ItemLibrary itemLibrary;
    void Awake()
    {
        PV = GetComponent<PhotonView>();
        if (!PV.IsMine) return;
        c = gameObject.AddComponent<Camera>();
        if (PhotonNetwork.IsMasterClient)
        {
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                Hashtable hash = p.CustomProperties;
                hash.Add("Kills", 0);
                p.SetCustomProperties(hash);
            }
        }
    }
    public void StartGame()
    {
        PV.RPC("RPC_CreateCon", RpcTarget.All);
    }
    void CreateController()
    {
        Transform spawnpoint = SpawnManeger.Instance.GetSpawnPoint();
        controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"),  spawnpoint.position , spawnpoint.rotation, 0, new object[] { PV.ViewID }) as GameObject;
        controllerPM = controller.GetComponent<PlayerMovement>();
        hasCrateCon = true;
        Destroy(c);
        SceneLoader SL = FindObjectOfType<SceneLoader>();
        SL.LoadSceneWithAniFakeClose();
    }

    private void Update() {
        if (!hasCrateCon) return;
        UpdateClock();
        if (Input.GetKeyDown(KeyCode.L))
        {
            PhotonNetwork.LeaveRoom();
        }
    }

    void UpdateClock()
    {
        Clock -= Time.deltaTime;
        
        m = Mathf.FloorToInt(Clock/60);
        s = Mathf.FloorToInt(Clock) - m * 60;
        if (s == 0 && LastS != 0 && PhotonNetwork.IsMasterClient)
        {
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                Hashtable Hash = new Hashtable();
                Hash.Add("ClockUpdate", Clock);
                p.SetCustomProperties(Hash);
            }
        }
        if (LastS != s)
        {
            controllerPM.SetTimer(m, s);
            if (Clock <= 0 && PhotonNetwork.IsMasterClient && hasCrateCon)
            {
                EndGame();
                hasCrateCon = false;
            }
        }
        LastS = s;
    }
    public void SetClock(float Time)
    {
        Clock = Time;
    }

    public void Die(Transform Player, string name)
    {
        StartCoroutine(Die2(Player, name));
    }

    public IEnumerator Die2(Transform Player, string name)
    {
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Death"), controller.transform.position + Vector3.up, Quaternion.identity);
        PhotonNetwork.Destroy(controller);
        //Display in chat
        GameObject Cam = Instantiate(DeathViewCam, controller.transform.position, Quaternion.identity);
        Cam.GetComponent<DeathCam>().Target = Player;
        Cam.GetComponent<DeathCam>().DeathText.text = "Killed by " + name;
        yield return new WaitForSeconds(5);
        Destroy(Cam);
        CreateController();
    }   

    public GameObject FindItemPrefabInLibrary(string name)
    {
        GameObject[] ILibrary = itemLibrary.Library;
        for (int i = 0; i < ILibrary.Length; i++)
        {
            if (ILibrary[i].GetComponent<Item>().itemInfo.itemname == name)
            {
                return ILibrary[i];
            }
        }
        Debug.LogError("Didn't find Item with itemname of " + name);
        return ILibrary[0];
    }


    public void SendPublicMesage(string Mes)
    {
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            Hashtable Hash = new Hashtable();
            Hash.Add("Mes", Mes);
            Hash.Add("Kills", Kills);
            p.SetCustomProperties(Hash);
        }
    }
    public void AddKillCount()
    {
        PV.RPC("RPC_AddKillCount", RpcTarget.All);
    }

    void EndGame()
    {
        PV.RPC("RPC_LoadNextSceneAni", RpcTarget.All);
    }

    [PunRPC]

    void RPC_CreateCon()
    {
        Clock = (int)PhotonNetwork.LocalPlayer.CustomProperties["GameTime"];
        if (PV.IsMine)
        {
            CreateController();
        }
    }

    [PunRPC]

    void RPC_LoadNextSceneAni()
    {
        LeaderBoard.Instance.UpdatePlacing();
        SceneLoader.Instance.LoadSceneWithAniEndGame("End", LeaderBoard.Instance.EGI);
    }

    [PunRPC]

    void RPC_AddKillCount()
    {
        Kills ++;
        if (PV.IsMine)
        {
            Hashtable hash = new Hashtable();
            hash.Add("Kills", Kills);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }
}
