using TMPro;
using UnityEngine;

public class ValueChangerUI : MonoBehaviour
{
    public TMP_Text NumberText;
    public int MinValue = 1;
    public int MaxValue = 15;
    public int StartingValue = 5;
    [HideInInspector] public int value = 0;
    private void Start() {
        value = StartingValue;
        NumberText.SetText(value.ToString());
    }

    public void OnClickRight()
    {
        value ++;
        if (value > MaxValue)
        {
            value = MaxValue;
            return;
        }
        NumberText.SetText(value.ToString());
    }
    public void OnClickLeft()
    {
        value --;
        if (value < MinValue)
        {
            value = MinValue;
            return;
        }
        NumberText.SetText(value.ToString());
    }
}
