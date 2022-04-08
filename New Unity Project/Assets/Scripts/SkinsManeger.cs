using UnityEngine;

public class SkinsManeger : MonoBehaviour
{
    public static SkinsManeger Instance;
    public Skin[] skins;


    private void Awake() {
        Instance = this;
    }
    private void Start() {
        DontDestroyOnLoad(gameObject);
    }


}

[System.Serializable]
public struct Skin
{
    public string name;
    public Material mainskinmat;
}