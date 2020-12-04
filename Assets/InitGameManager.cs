using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class InitGameManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Refresh Rate : " + XRDevice.refreshRate + ". TimeStep set to "+ 1 / XRDevice.refreshRate);
        Time.fixedDeltaTime = 1 / XRDevice.refreshRate;
    }

}
