using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/PlaceableScriptableObject", order = 1)]
public class PlaceableScriptableObject : ScriptableObject
{
    public GameObject Prefab;
    public Sprite Sprite;
}
