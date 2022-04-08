using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject SlotIconPrefab;

    private List<GameObject> ItemsInUI = new List<GameObject>();

    public void UpdateUI(List<Item> _items, int _SlectedIndex)
    {
        if (ItemsInUI != null)
        {
            foreach (GameObject item in ItemsInUI)
            {
                Destroy(item);
            }
        }

        for (int i = 0; i < _items.Count; i++)
        {
            GameObject _item = Instantiate(SlotIconPrefab, gameObject.transform);
            ItemsInUI.Add(_item);
            _item.GetComponent<Image>().sprite = _items[i].itemInfo.itemImage;
            _item.GetComponentInChildren<TMP_Text>().text = _items[i].itemInfo.itemname;
            if (i == _SlectedIndex)
            {
                _item.transform.GetChild(0).gameObject.SetActive(true);
            }
            else
            {
                _item.transform.GetChild(0).gameObject.SetActive(false);
            }
        }   
        
    }
}
