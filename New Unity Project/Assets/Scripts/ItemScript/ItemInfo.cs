using UnityEngine;
[CreateAssetMenu(menuName = "FPS/New Item")]
public class ItemInfo : ScriptableObject
{
    public string itemname;
    public Sprite itemImage;
    public GameObject PickUpGameObject;
}
