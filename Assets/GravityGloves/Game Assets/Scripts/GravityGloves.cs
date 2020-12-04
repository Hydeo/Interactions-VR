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

    [Header("Links")]
    //Left Hand's child "ObjectAttachmentPoint"
    [SerializeField]
    Transform left_hand_triplet;
    //Right Hand's child "ObjectAttachmentPoint"
    [SerializeField]
    Transform right_hand_triplet;
    //LayerMask excluding player colliders from Raycasting
    [SerializeField]
    LayerMask player_complement_mask;
    //Particle System to use on targeted objects
    [SerializeField]
    Transform grabbity_tracker;

    /* SteamVR Input */

    //Grip button (I don't own a Valve Index, knuckles may work differently)
    [SerializeField]
    public InputDeviceCharacteristics inputLeftDeviceCharacteristics;
    public InputDeviceCharacteristics inputRightDeviceCharacteristics;

    //To show current target
    public LineRenderer glove_line_renderer;

    /*Current data*/
    public Spell current_spell;
    [Header("Parameters")]
    [Range(0.0f, 1.0f)]
    //How much hand movement is needed to trigger the object's leap?
    [SerializeField]
    float attraction_spell_sensitivity;

    //Attached SteamVR Player Component Components
    XRRig xr_rig;

    //Buffers
    private RaycastHit left_hand_raycast_hit, right_hand_raycast_hit;
    //private SteamVR_Input_Sources in_source_buffer;

    /* ATTRACTION BUFFERS*/
    private Transform attr_left_target, attr_right_target;

    //Tracked Controller devices (position, velocity, ...)
    private InputDevice leftDevice;
    private InputDevice rightDevice;
    private Transform particulesInstance;
    // Start is called before the first frame update
    void Start()
    {
        xr_rig = GameObject.Find("VR Rig").GetComponent<XRRig>();
        Init();
    }

    private void Init()
    {
        //Get Linerenderer exisiting in the current object
        glove_line_renderer = GetComponent<LineRenderer>();

        //Get left controller
        List<UnityEngine.XR.InputDevice> leftDevices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(inputLeftDeviceCharacteristics, leftDevices);
        if (leftDevices.Count == 1)
        {
            leftDevice = leftDevices[0];
        }
        else
        {
            Debug.Log(string.Format("More or less than one left controller device detected for GravityGloves"));
        }

        //Get right controller
        List<UnityEngine.XR.InputDevice> rightDevices = new List<UnityEngine.XR.InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(inputRightDeviceCharacteristics, rightDevices);
        if (rightDevices.Count == 1)
        {
            rightDevice = rightDevices[0];
        }
        else
        {
            Debug.Log(string.Format("More  or less than one right controller device detected for GravityGloves"));
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
        glove_line_renderer.SetPosition(1, startPosition);
        glove_line_renderer.SetPosition(0, endPosition);
    }

    // Update is called once per frame
    void Update()
    {

        if (rightDevice == null || leftDevice == null)
        {
            Init();
        }

        //What is each hand pointing to?
        Physics.Raycast(left_hand_triplet.transform.position, left_hand_triplet.forward, out left_hand_raycast_hit, 50, player_complement_mask);
        Physics.Raycast(right_hand_triplet.transform.position, right_hand_triplet.forward, out right_hand_raycast_hit, 50, player_complement_mask);
        Debug.DrawRay(right_hand_triplet.transform.position, right_hand_triplet.forward, Color.green);
        Debug.DrawRay(left_hand_triplet.transform.position, left_hand_triplet.forward, Color.green);

        /* Spell code */
        switch (current_spell)
        {
            /* --------------------------------- */
            /* Similar to HL:A's Grabbity Gloves */
            /* --------------------------------- */
            case Spell.ATTRACT:
                {
                    /*
                    //If grip is pressed, change attraction target
                    if (IsGripActivated(leftDevice) && left_hand_raycast_hit.transform != null && left_hand_raycast_hit.transform.tag == "ALLOW_MANIPULATION")
                    {
                        attr_left_target = left_hand_raycast_hit.transform;
                        //Create and start particle system
                        Transform tracker = Instantiate(grabbity_tracker, attr_left_target.position, Quaternion.Euler(0.0f, 0.0f, 0.0f));
                        tracker.SetParent(attr_left_target);

                    }


                    //Check for flick of left hand
                    if (attr_left_target != null && Vector3.Dot(GetDeviceVelocity(leftDevice), (Camera.main.transform.forward - Camera.main.transform.up)) < -attraction_spell_sensitivity)
                    {
                        //Calculate velocity and apply it to the target
                        Vector3 calculated_velocity = ComplementarCalculations.CalculateParabola(attr_left_target.transform.position, left_hand_triplet.position);
                        attr_left_target.GetComponent<Rigidbody>().velocity = calculated_velocity;
                        //Destroy particle system
                        Destroy(attr_left_target.GetChild(1).gameObject);
                        attr_left_target = null;
                    }
                    */

                    //If grip is pressed, change attraction target

                    if (right_hand_raycast_hit.transform != null)
                    {
                        UpdateLineRendererPosition(GetDevicePosition(rightDevice), transform.InverseTransformPoint(right_hand_raycast_hit.transform.position));
                        glove_line_renderer.enabled = true;
                    }
                    else
                    {
                        glove_line_renderer.enabled = false;
                    }

                    if (attr_right_target == null && IsGripActivated(rightDevice) && right_hand_raycast_hit.transform != null && right_hand_raycast_hit.transform.tag == "ALLOW_MANIPULATION")
                    {
                        attr_right_target = right_hand_raycast_hit.transform;
                        //Create particle system
                        particulesInstance = Instantiate(grabbity_tracker, attr_right_target.position, Quaternion.Euler(0.0f, 0.0f, 0.0f));
                        particulesInstance.SetParent(attr_right_target);

                    }
                    else if ((particulesInstance != null && particulesInstance.gameObject != null)
                        && (!IsGripActivated(rightDevice) && attr_right_target != null)
                        || (IsGripActivated(rightDevice) && attr_right_target != null && (right_hand_raycast_hit.transform == null || (right_hand_raycast_hit.transform != null && right_hand_raycast_hit.transform.tag != "ALLOW_MANIPULATION"))))
                    {
                        Destroy(particulesInstance.gameObject);
                        particulesInstance = attr_right_target = null;
                    }

                    //Check for flick of right hand
                    if (attr_right_target != null && Vector3.Dot(GetDeviceVelocity(rightDevice), (xr_rig.cameraGameObject.transform.forward - xr_rig.cameraGameObject.transform.up)) < -attraction_spell_sensitivity)
                    {
                        //Calculate velocity and apply it to the target
                        Vector3 calculated_velocity = ComplementarCalculations.CalculateParabola(attr_right_target.transform.position, right_hand_triplet.position);
                        attr_right_target.GetComponent<Rigidbody>().velocity = calculated_velocity;
                        //Destroy particle system
                        Destroy(particulesInstance.gameObject);
                        particulesInstance = attr_right_target = null;
                    }

                    break;
                }
            default:
                break;
        }
    }


}
