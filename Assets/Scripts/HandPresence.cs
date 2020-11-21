using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandPresence : MonoBehaviour
{
    public InputDeviceCharacteristics inputDeviceCharacteristics;
    public List<GameObject> controllersPrefabs;
    private InputDevice targetDevice;
    private GameObject spawnedController;

    // Start is called before the first frame update
    void Start()
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(inputDeviceCharacteristics, devices);
        
        if (devices.Count > 0)
        {
            targetDevice = devices[0];
            Debug.Log(targetDevice.name);
            GameObject prefab = controllersPrefabs.Find(controller => controller.name == targetDevice.name);
            if(prefab != null)
            {
                spawnedController = Instantiate(prefab, transform);
            }
            else
            {
                Debug.LogError("Did not found controller prefab");
                spawnedController = Instantiate(controllersPrefabs[0], transform);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
