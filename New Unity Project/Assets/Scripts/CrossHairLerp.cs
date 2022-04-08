using UnityEngine;

public class CrossHairLerp : MonoBehaviour
{
    Vector2 TargetScale = Vector2.one;

    private void Awake() {
        LerpToScale(1);
    }
    public void LerpToScale(float Scale)
    {
        TargetScale = new Vector2(Scale, Scale);
    }

    private void Update() {
        GetComponent<RectTransform>().sizeDelta = Vector2.Lerp(GetComponent<RectTransform>().sizeDelta, TargetScale * 100, 10 * Time.deltaTime);
    }
}
