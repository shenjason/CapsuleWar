using UnityEngine;
using TMPro;

namespace UselessStuff
{
    public class DestroyforAni : MonoBehaviour
    {
        public void Destroy()
        {
            Destroy(gameObject);
        }

        public void SetTextV(string t)
        {
            GetComponent<TMP_Text>().text = t;
        }
    }
}
