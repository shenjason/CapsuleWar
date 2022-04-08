using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class Health : MonoBehaviour
{
    private Slider HealthBar;
    [SerializeField] private Slider Damageidecator;
    private float Targetvalue;

    void Awake()
    {
        HealthBar = GetComponent<Slider>();
        HealthBar.value = HealthBar.maxValue;
        Targetvalue = HealthBar.maxValue;
        Damageidecator.value = Targetvalue;
    }

    public void UpdateValue(float value)
    {
        HealthBar.value = value;
        Targetvalue = value;
    }

    void FixedUpdate()
    {
        Damageidecator.value = Mathf.Lerp(Damageidecator.value, Targetvalue, 8 * Time.deltaTime);
    }
}
