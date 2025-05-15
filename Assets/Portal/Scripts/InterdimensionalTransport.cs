using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class InterdimensionalTransport : MonoBehaviour
{
    public Material[] materials; // Array of materials
    private void Start()
    {
        foreach (var mat in materials)
        {
            mat.SetInt("_StencilTest", (int)CompareFunction.Equal);
        }
    }
    void OnTriggerStay(Collider other)
    {
        if (other.name != "Main Camera")
            return;

        // Check if the camera is outside or inside the portal
        if (transform.position.z > other.transform.position.z)
        {
            Debug.Log("Outisde Portal");
            // Outside of the portal (render the alternate world)
            foreach (var mat in materials)
            {
                mat.SetInt("_StencilTest", (int)CompareFunction.Equal);
            }
        }
        else
        {
            Debug.Log("Inside portal");
            // Inside the portal (render the real world)
            foreach (var mat in materials)
            {
                mat.SetInt("_StencilTest", (int)CompareFunction.NotEqual);
            }
        }
    }
    private void OnDestroy()
    {
        foreach (var mat in materials)
        {
            mat.SetInt("_StencilTest", (int)CompareFunction.NotEqual);
        }
    }
    private void Update()
    {
        
    }
}
