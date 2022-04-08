using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AmmoIndicator : MonoBehaviour
{
    public PlayerMovement PM;

    public TMP_Text text;

    public void UpdateAmmoIndicotr() {
        if (PM.items.Count == 0)
        {
            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(1).gameObject.SetActive(false);
            return;
        }
        else if (PM.items[PM.item_index].GetComponent<Gun>() == null)
        {
            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(1).gameObject.SetActive(false);
            return;
        }
        transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(1).gameObject.SetActive(true);
        string MGS = ((GunInfo)PM.items[PM.item_index].itemInfo).MagSize.ToString();
        string CA = ((Gun)PM.items[PM.item_index]).ammo.ToString();

        text.text = CA + "/" + MGS;
    }
}
