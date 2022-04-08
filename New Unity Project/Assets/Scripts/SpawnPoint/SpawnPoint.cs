using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public GameObject Spawngfx;
    void Awake()
    {
        Spawngfx.SetActive(false);
    }
}
