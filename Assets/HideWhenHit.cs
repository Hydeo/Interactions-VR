using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideWhenHit : MonoBehaviour
{
    public float hidingTime = 3f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Coliding");
        Hide();
        Invoke("Show", hidingTime);
    }

    private void Show()
    {
        GetComponent<MeshRenderer>().enabled = true;
    }

    private void Hide()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
