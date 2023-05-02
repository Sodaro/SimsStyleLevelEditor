using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FramerateLimiter : MonoBehaviour
{
    [SerializeField, Min(0)] private int _targetFrameRate = 165;
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = _targetFrameRate;
    }
}
