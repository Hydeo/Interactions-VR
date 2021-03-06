﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Linq;
using System;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

/** ************************************************* */
/** Gravity Gloves Basic Implementation by Frank01001 */
/** ************************************************* */

public class GravityGloves : MonoBehaviour
{
    //Enum used to select action (I named it spell because I initially intended this to be one of many actions)
    public enum Spell { NONE, ATTRACT };

    XRRig xr_rig;
    InputDevice handDevice;
    [SerializeField]
    InputDeviceCharacteristics handDeviceCharacteristics;

    [SerializeField]
    Transform handTransform;
    //LayerMask excluding player colliders from Raycasting
    [SerializeField]
    LayerMask player_complement_mask;
    //Particle System to use on targeted objects
    [SerializeField]
    Transform grabbityTracker;
    //To show current target
    [SerializeField]
    LineRenderer handLineRenderer;
    /*Current data*/
    [SerializeField]
    Spell current_spell;
    //How much hand movement is needed to trigger the object's leap?
    [Range(0.0f, 1.0f)]
    [SerializeField]
    float attractionSpellSensitivity;

    [SerializeField]
    List<GameObject> currentObjectsReachable;
    GameObject currentObjectFocused;
    GameObject currentObjectActive;


    [SerializeField]
    GameObject collisionVolume;

    /* BUFFERS*/
    //Tracked Controller devices (position, velocity, ...)
    private Transform particulesInstance;

    public String ALLOW_MANIPULATION_LAYER = "GrabInteractible";
    public Color reachableObjectOutlineColor;
    public Color focusObjectOutlineColor;
    public bool isGloveActivated = false;

    // Start is called before the first frame update
    void Start()
    {
        xr_rig = GameObject.Find("VR Rig").GetComponent<XRRig>();
        Init();
    }

    public void OnFocusTriggerEnter(Collider other)
    {
        if (IsOnInteractableLayer(other) && !isGloveActivated )
        {
            if(currentObjectFocused != null)
            {
                UpdateObjectOutlineIfManipulable(reachableObjectOutlineColor, currentObjectFocused.GetComponent<Collider>());
            }
            UpdateObjectOutlineIfManipulable(focusObjectOutlineColor, other);
            currentObjectFocused = other.gameObject;
        }
    }
    public void OnFocusTriggerExit(Collider other)
    {
        if (IsOnInteractableLayer(other) && !isGloveActivated)
        {
            UpdateObjectOutlineIfManipulable(reachableObjectOutlineColor, other);
            currentObjectFocused = null;
        }
    }

    public void OnReachableTriggerEnter(Collider other)
    {
        if (IsOnInteractableLayer(other))
        {
            UpdateObjectOutlineIfManipulable(reachableObjectOutlineColor, other);
            currentObjectsReachable.Add(other.gameObject);
        }
    }

    public void OnReachableTriggerExit(Collider other)
    {
        if (IsOnInteractableLayer(other))
        {
            Outline objectOutline = other.gameObject.GetComponent<Outline>();
            if (objectOutline == null)
            {
                objectOutline = other.gameObject.GetComponentInParent<Outline>();
            }
            if (objectOutline)
            {
                objectOutline.enabled = false;
            }
            currentObjectsReachable.Remove(other.gameObject);
        }
    }

    private bool IsOnInteractableLayer(Collider c)
    {
        return c.gameObject.layer == LayerMask.NameToLayer(ALLOW_MANIPULATION_LAYER);
    }
    private void UpdateObjectOutlineIfManipulable(Color c, Collider other)
    {

        Outline objectOutline = other.gameObject.GetComponent<Outline>();
        
        if(objectOutline == null)
        {
            objectOutline = other.gameObject.GetComponentInParent<Outline>();
        }
        
        if (objectOutline)
        {
            objectOutline.OutlineColor = c;
            objectOutline.enabled = true;
        }

    }

    private void Init()
    {
        //Get Linerenderer exisiting in the current object
        handLineRenderer = GetComponent<LineRenderer>();

        //Get controller
        List<InputDevice> handDevices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(handDeviceCharacteristics, handDevices);
        if (handDevices.Count == 1)
        {
            handDevice = handDevices[0];
        }
        else
        {
            Debug.Log(string.Format("More or less than one left controller device detected for GravityGloves"));
        }

    }

    private Vector3 GetDeviceVelocity(InputDevice d)
    {
        d.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 velocity);
        return velocity;
    }

    private bool IsGripActivated(InputDevice d)
    {
        d.TryGetFeatureValue(CommonUsages.gripButton, out bool gripValue);
        return gripValue;
    }

    private void UpdateLineRendererPosition(Vector3 startPosition, Vector3 endPosition)
    {
        handLineRenderer.SetPosition(1, startPosition);
        handLineRenderer.SetPosition(0, endPosition);
    }

    private void UpdateLineRenderer()
    {
        //If we aim at a valid target
        if (currentObjectActive != null)
        {
            UpdateLineRendererPosition(transform.InverseTransformPoint(this.transform.position), transform.InverseTransformPoint(currentObjectFocused.transform.position));
            handLineRenderer.enabled = true;
        }
        else if (handLineRenderer.enabled)
        {
            handLineRenderer.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (handDevice == null || !handDevice.isValid)
        {
            Init();
        }

        //What is hand pointing to?
        //Physics.Raycast(handTransform.transform.position, handTransform.forward, out handRaycastHit, collisionVolume.GetComponent<MeshCollider>().bounds.size.x * collisionVolume.transform.localScale.x, player_complement_mask);

        /* Spell code */
        switch (current_spell)
        {
            /* --------------------------------- */
            /* Similar to HL:A's Grabbity Gloves */
            /* --------------------------------- */
            case Spell.ATTRACT:
                {
                    
                    if(!IsGripActivated(handDevice) && isGloveActivated)
                    {
                        isGloveActivated = false;
                    }

                    UpdateLineRenderer();

                    //If we trigger aiming at a valid target
                    if (currentObjectFocused != null && IsGripActivated(handDevice) && !isGloveActivated)
                    {
                        //Create particle system
                        particulesInstance = Instantiate(grabbityTracker, currentObjectFocused.transform.position, Quaternion.Euler(0.0f, 0.0f, 0.0f));
                        particulesInstance.SetParent(currentObjectFocused.transform);
                        isGloveActivated = true;
                        currentObjectActive = currentObjectFocused;
                    }
                    else if (!isGloveActivated && particulesInstance != null)
                    {
                        Destroy(particulesInstance.gameObject);
                        particulesInstance = null;
                        currentObjectActive = null;
                    }

                    //Check for flick of hand
                    if (currentObjectActive != null && particulesInstance!= null && Vector3.Dot(GetDeviceVelocity(handDevice), (xr_rig.cameraGameObject.transform.forward - xr_rig.cameraGameObject.transform.up)) < -attractionSpellSensitivity)
                    {
                        //Calculate velocity and apply it to the target
                        Vector3 calculated_velocity = ComplementarCalculations.CalculateParabola(currentObjectActive.transform.position, handTransform.position);
                        currentObjectFocused.GetComponent<Rigidbody>().velocity = calculated_velocity;
                        //Destroy particle system
                        if (particulesInstance != null)
                        {
                            Destroy(particulesInstance.gameObject);
                            particulesInstance = null;
                            currentObjectActive = currentObjectFocused = null;
                        }
                    }
                    break;
                }
            default:
                break;
        }
    }


}
