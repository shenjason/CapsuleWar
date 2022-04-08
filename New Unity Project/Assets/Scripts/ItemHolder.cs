using UnityEngine;

public class ItemHolder : MonoBehaviour
{
    public static ItemHolder Instance;
    
    private void Start() {
        Instance = this;
    }
    
}
