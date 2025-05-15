using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class PortalSpawner : MonoBehaviour
{
    public GameObject prefabPortal;
    private bool isSpawnedPortal;
    public ARRaycastManager raycastManager;
    private List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();

    [Header("Plane Visibility")]
    public ARPlaneManager planeManager;
    //public UnityEngine.UI.Button  togglePlaneButton;

    //private bool planesEnabled = true;
    public GameObject crossHair;

    public float minSeperationDistance = 1f;
    public Camera arCamera;

    public LayerMask arPlaneLayerMask;

    void Start()
    {
        isSpawnedPortal = false;
        //togglePlaneButton.onClick.AddListener(TogglePlaneDetection);
    }

    void Update()
    {
        //if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
        //{
        //    return;
        //}

        if (Input.touchCount == 1)
        {
            HandleSingleTouch();
        }
    }

    bool isFarFromPlayer(Vector3 pos)
    {
        return Vector3.Distance(arCamera.transform.position, pos) > minSeperationDistance;
    }



    void HandleSingleTouch()
    {
        Touch touch = Input.GetTouch(0); // Condition
        Vector3 screenCentre = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Ray ray = arCamera.ScreenPointToRay(screenCentre);
        RaycastHit hit;
        //Pose hitPose = raycastHits[0].pose;
        //Quaternion rotation = Quaternion.LookRotation(-arCamera.transform.forward, Vector3.up);


        //if (Input.touchCount > 0 && !isSpawnedPortal && Physics.Raycast(ray, out hit, Mathf.Infinity, arPlaneLayerMask) && isFarFromPlayer(hit.point))
        //{
        //    isSpawnedPortal = true;

        //    //Vector3 directionToCamera = arCamera.transform.position - hit.point;
        //    //directionToCamera.y = 0; // Keep portal level with the ground
        //    //Quaternion rotationToCamera = Quaternion.LookRotation(-directionToCamera.normalized, Vector3.up);
        //    GameObject portal = Instantiate(prefabPortal, hit.point,Quaternion.identity);
        //    //Debug.Log(portal.name);
        //    FindObjectOfType<PatternManager>().SetPortal(portal);

        //}
        if (Input.touchCount > 0 && !isSpawnedPortal && Physics.Raycast(ray, out hit, Mathf.Infinity, arPlaneLayerMask) && isFarFromPlayer(hit.point))
        {
            isSpawnedPortal = true;
            DisableARFeatures();
            crossHair.gameObject.SetActive(false);

            // Calculate rotation to face the camera
            Vector3 directionToCamera = arCamera.transform.position - hit.point;
            directionToCamera.y = 0;  // Keep portal level with the ground
            Quaternion rotationToCamera = Quaternion.LookRotation(-directionToCamera.normalized, Vector3.up);

            // Spawn the portal facing the camera
            GameObject portal = Instantiate(prefabPortal, hit.point, rotationToCamera);

            // Assign portal to PatternManager
            FindObjectOfType<PatternManager>().SetPortal(portal);
        }

    }
    void DisableARFeatures()
    {
        if (planeManager)
        {
            planeManager.enabled = false;
            foreach (var plane in planeManager.trackables)
            {
                plane.gameObject.SetActive(false);
            }
        }

        if (raycastManager)
            raycastManager.enabled = false;
    }

    //public void TogglePlaneDetection()
    //{
    //    planesEnabled = !planesEnabled;
    //    planeManager.enabled = planesEnabled;

    //    foreach (var plane in planeManager.trackables)
    //    {
    //        plane.gameObject.SetActive(planesEnabled);
    //    }
    //}
}