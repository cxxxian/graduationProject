using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class FrameLimit : MonoBehaviour
{

    void Awake()
    {
        Application.targetFrameRate = 60;//锁定最大帧率为60帧
        //Application.targetFrameRate = 30;//锁定最大帧率为30帧
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("XR Device: " + XRSettings.loadedDeviceName);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
