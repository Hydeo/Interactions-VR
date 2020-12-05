using System.Collections;
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
    List<GameObject> currentCollisions;
    [SerializeField]
    GameObject collisionVolume;
    private RaycastHit handRaycastHit;

    /* BUFFERS*/
    private Transform currentSelectedTarget;
    //Tracked Controller devices (position, velocity, ...)
    private Transform particulesInstance;

    public String ALLOW_MANIPULATION_TAG = "ALLOW_MANIPULATION";
    public Color reachableObjectOutlineColor;
    public Color targetedReachableObjectOutlineColor;
    // Start is called before the first frame update
    void Start()
    {
        xr_rig = GameObject.Find("VR Rig").GetComponent<XRRig>();
        Init();
    }

    public void Yolo()
    {
        Debug.Log("yolooooo");
    }

    private void OnTriggerEnter(Collider other)
    {
        /*if (other.gameObject.tag == ALLOW_MANIPULATION_TAG)
        {
            currentCollisions.Add(other.gameObject);

            Outline objectOutline = other.gameObject.GetComponent<Outline>();
            if (objectOutline)
            {   
                objectOutline.OutlineColor = reachableObjectOutlineColor;
                objectOutline.enabled = true;
            }
        }*/
    }

    private void OnTriggerExit(Collider other)
    {
        /*if (other.gameObject.tag == ALLOW_MANIPULATION_TAG)
        {
            currentCollisions.Remove(other.gameObject);

            Outline objectOutline = other.gameObject.GetComponent<Outline>();
            if (objectOutline)
            {
                objectOutline.enabled = false;
            }
        }*/
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

    private Vector3 GetDevicePosition(InputDevice d)
    {
        d.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position);
        return position;
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

    private bool TargetUnfocused()
    {
        return
            (particulesInstance != null && particulesInstance.gameObject != null) //If we have particules showing
            && (!IsGripActivated(handDevice) && currentSelectedTarget != null) // The player is not gripping and we had a target
            || (IsGripActivated(handDevice) && currentSelectedTarget != null && (handRaycastHit.transform == null || (handRaycastHit.transform != null && handRaycastHit.transform.tag != ALLOW_MANIPULATION_TAG)));//If we grip, had a target but don't aim at anytarget anymore
    }

    private Transform LookForTarget()
    {

        return null;
    }
    // Update is called once per frame
    void Update()
    {

        if (handDevice == null)
        {
            Init();
        }

        //What is hand pointing to?
        Physics.Raycast(handTransform.transform.position, handTransform.forward, out handRaycastHit, collisionVolume.GetComponent<MeshCollider>().bounds.size.x * collisionVolume.transform.localScale.x, player_complement_mask);

        /* Spell code */
        switch (current_spell)
        {
            /* --------------------------------- */
            /* Similar to HL:A's Grabbity Gloves */
            /* --------------------------------- */
            case Spell.ATTRACT:
                {

                    //If we aim at a valid target
                    if (handRaycastHit.transform != null)
                    {
                        UpdateLineRendererPosition(transform.InverseTransformPoint(this.transform.position), transform.InverseTransformPoint(handRaycastHit.transform.position));
                        handLineRenderer.enabled = true;
                    }
                    else if (handLineRenderer.enabled)
                    {
                        handLineRenderer.enabled = false;
                    }

                    //If we trigger aiming at a valid target
                    if (currentSelectedTarget == null && IsGripActivated(handDevice) && handRaycastHit.transform != null && handRaycastHit.transform.tag == ALLOW_MANIPULATION_TAG)
                    {
                        currentSelectedTarget = handRaycastHit.transform;
                        //Create particle system
                        particulesInstance = Instantiate(grabbityTracker, currentSelectedTarget.position, Quaternion.Euler(0.0f, 0.0f, 0.0f));
                        particulesInstance.SetParent(currentSelectedTarget);

                    }
                    else if (TargetUnfocused())
                    {
                        Destroy(particulesInstance.gameObject);
                        particulesInstance = currentSelectedTarget = null;
                    }

                    //Check for flick of hand
                    if (currentSelectedTarget != null && Vector3.Dot(GetDeviceVelocity(handDevice), (xr_rig.cameraGameObject.transform.forward - xr_rig.cameraGameObject.transform.up)) < -attractionSpellSensitivity)
                    {
                        //Calculate velocity and apply it to the target
                        Vector3 calculated_velocity = ComplementarCalculations.CalculateParabola(currentSelectedTarget.transform.position, handTransform.position);
                        currentSelectedTarget.GetComponent<Rigidbody>().velocity = calculated_velocity;
                        //Destroy particle system
                        Destroy(particulesInstance.gameObject);
                        particulesInstance = currentSelectedTarget = null;
                    }
                    break;
                }
            default:
                break;
        }
    }


}
