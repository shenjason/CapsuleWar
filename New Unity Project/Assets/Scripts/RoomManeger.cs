using UnityEngine;
using Photon.Pun;
using System.IO;
using System.Collections;
using UnityEngine.SceneManagement;
using Photon.Realtime;

[RequireComponent(typeof(PhotonView))]
public class RoomManeger : MonoBehaviourPunCallbacks
{
    public static RoomManeger Instance;
    private ItemLibrary itemLibrary;


    void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }
    
    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.buildIndex == 2)
        {
            PlayerManeger PM = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManeger"), Vector3.zero, Quaternion.identity).GetComponent<PlayerManeger>();
            PM.RM = this;
        }
        if (!PhotonNetwork.IsMasterClient) return;
        StartCoroutine(WaitForPlayers());
    }

    IEnumerator WaitForPlayers()
    {
        while (FindPMs().Length != PhotonNetwork.CurrentRoom.PlayerCount)
        {
            yield return null;
        }
        foreach (PlayerManeger pm in FindPMs())
        {
            pm.StartGame();
        }
    }
    public override void OnLeftRoom()
    {
        SceneLoader SL = SceneLoader.Instance;
        if (SL == null) return;
        SL.LoadSceneWithAni("Lobby");   
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Destroy(gameObject);
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        SceneLoader.Instance.LoadSceneWithAniError("MainMenu", cause.ToString());
    }

    PlayerManeger[] FindPMs()
    {
        return FindObjectsOfType<PlayerManeger>();
    }
    public void Destroy()
    {
        Destroy(gameObject);
    }
}
