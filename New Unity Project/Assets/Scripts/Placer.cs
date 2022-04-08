using UnityEngine;
using TMPro;

public class Placer : MonoBehaviour
{
    public TMP_Text TextDisplay;
    public void UpdatePlacer(string place, string name, string Kills, Color tc)
    {
        TextDisplay.color = tc;
        TextDisplay.text = place + " " + name + "   " + Kills;
    }
}
