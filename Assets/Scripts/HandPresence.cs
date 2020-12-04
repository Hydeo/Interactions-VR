using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandPresence : MonoBehaviour
{
    public bool showController = false;
    public InputDeviceCharacteristics inputDeviceCharacteristics;
    public List<GameObject> controllersPrefabs;
    public GameObject handModelPrefab;

    private InputDevice targetDevice;
    private GameObject spawnedController;
    private GameObject spawnedHandModel;
    private Animator handAnimator;


    // Start is called before the first frame update
    void Start()
    {
        TryInit();
    }

    private void TryInit()
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(inputDeviceCharacteristics, devices);

        if (devices.Count > 0)
        {
            targetDevice = devices[0];
            Debug.Log(targetDevice.name);
            GameObject prefab = controllersPrefabs.Find(controller => controller.name == targetDevice.name);
            if (prefab != null)
            {
                spawnedController = Instantiate(prefab, transform);
            }
            else
            {
                Debug.LogError("Did not found controller prefab");
                spawnedController = Instantiate(controllersPrefabs[0], transform);
            }

            spawnedHandModel = Instantiate(handModelPrefab, transform);
            handAnimator = spawnedHandModel.GetComponent<Animator>();
        }
    }

    void UpdateHandAnimation()
    {
        if(targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
        {
            handAnimator.SetFloat("Trigger", triggerValue);
        }
        else
        {
            handAnimator.SetFloat("Trigger", 0);
        }

        if (targetDevice.TryGetFeatureValue(CommonUsages.grip, out float gripValue))
        {
            handAnimator.SetFloat("Grip", gripValue);
        }
        else
        {
            handAnimator.SetFloat("Grip", 0);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(targetDevice == null || spawnedHandModel == null || spawnedController == null)
        {
            TryInit();
        }
        else
        {
            
            if (showController)
            {
                spawnedHandModel.SetActive(false);
                spawnedController.SetActive(true);
            }
            else
            {
                spawnedHandModel.SetActive(true);
                spawnedController.SetActive(false);
                UpdateHandAnimation();
            }
        }
       

    }

    private void GetFeatureValues()
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(inputDeviceCharacteristics, devices);

        if (devices.Count == 1)
        {
            UnityEngine.XR.InputDevice device = devices[0];
            targetDevice.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 velocity);
            targetDevice.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 angularVelocity);

            Debug.Log(string.Format("Device name '{0}' with velocity :  '{1}' and angular '{2}'", device.name, velocity, angularVelocity));
        }
        
    }
}
