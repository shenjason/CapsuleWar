using System.Collections;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    public TMP_Text Errortext;
    public TMP_InputField usernameInput;
    public TMP_Text ButtonText;
    public static ConnectToServer Instance;

    private void Awake() {
        Instance = this;
    }
    public void OnClickConnect()
    {
        if (usernameInput.text.Length >= 1)
        {
            PhotonNetwork.NickName = usernameInput.text;
            ButtonText.SetText("Connecting...");
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    public void DisplayErrorMesage(string t)
    {
        StartCoroutine(DisplayError(t));
    }

    public override void OnConnectedToMaster()
    {
        SceneLoader.Instance.LoadSceneWithAni("Lobby");
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    IEnumerator DisplayError(string t)
    {
        Errortext.SetText(t);
        yield return new WaitForSeconds(5f);
        Errortext.SetText("");
    }
}
