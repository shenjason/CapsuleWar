using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "FPS/New ItemLibrary")]
public class ItemLibrary : ScriptableObject
{
    [SerializeField] public GameObject[] Library;
}
