using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
[RequireComponent(typeof(ARAnchorManager))]
[RequireComponent(typeof(ARPlaneManager))]
public class ArrowNavigator : MonoBehaviour
{
    public GameObject arrowPrefab;

    private ARRaycastManager raycastManager;
    private ARAnchorManager  anchorManager;
    private ARPlaneManager   planeManager;
    private GameObject       arrowInstance;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        anchorManager  = GetComponent<ARAnchorManager>();
        planeManager   = GetComponent<ARPlaneManager>();
    }

    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Vector2 touchPos = Input.GetTouch(0).position;
            if (raycastManager.Raycast(touchPos, hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = hits[0].pose;
                // 1) Obtén el ARPlane detectado
                var planeId = hits[0].trackableId;
                ARPlane plane = planeManager.GetPlane(planeId);

                // 2) Adjunta un ARAnchor al plano en esa pose
                ARAnchor anchor = anchorManager.AttachAnchor(plane, hitPose);
                if (anchor == null)
                {
                    Debug.LogWarning("No se pudo crear el anchor.");
                    return;
                }

                // 3) Instancia o mueve la flecha al anchor
                if (arrowInstance == null)
                {
                    arrowInstance = Instantiate(arrowPrefab, anchor.transform);
                }
                else
                {
                    arrowInstance.transform.SetParent(anchor.transform);
                    arrowInstance.transform.localPosition = Vector3.zero;
                    arrowInstance.transform.localRotation = Quaternion.identity;
                    arrowInstance.SetActive(true);
                }
            }
        }

        // Rotación continua hacia la cámara
        if (arrowInstance != null && arrowInstance.activeSelf)
        {
            Vector3 camPos = Camera.main.transform.position;
            Vector3 dir    = arrowInstance.transform.position - camPos;
            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                arrowInstance.transform.rotation =
                    Quaternion.Slerp(arrowInstance.transform.rotation,
                                     targetRot, Time.deltaTime * 5f);
            }
        }
    }
}
