using System.Collections;
using TMPro;
using UnityEngine;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class AssignmentScript : Singleton<AssignmentScript>
{
    // Task 1: Raycasting
    public (GameObject, Vector3?) RaycastDetectionForARPlane(Vector2 screenPosition)
    {

        //Get main camera
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return (null, null);
        }

        Vector3 cameraPos = mainCamera.transform.position;
        Vector3 cameraSpacePos = new Vector3(screenPosition.x, screenPosition.y, mainCamera.nearClipPlane);
        Vector3 worldSpacePos = mainCamera.ScreenToWorldPoint(cameraSpacePos);

        //Construct the ray from camera through that world point
        Vector3 rayDir = (worldSpacePos - cameraPos).normalized;
        Ray ray = new Ray(cameraPos, rayDir);
        RaycastHit[] hits = Physics.RaycastAll(ray);

        //Find the first hit whose object is an ARPlane
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider != null &&
                hit.collider.gameObject.CompareTag("ARPlane"))
            {
                return (hit.collider.gameObject, hit.point);
            }
        }

        //If no ARPlane is hit, return nulls
        return (null, null);
    }


    // Task 2: Geometric Registration
    public void GeometricRegistration(GameObject arObject, Vector3 hitPoint, GameObject arPlane)
    {
        if (arObject == null || arPlane == null)
            return;

        ARInteractable interact = arObject.GetComponent<ARInteractable>();
        if (interact == null)
            return;

        Vector3 p_v = interact.AnchorPointOfObject;
        Vector3 n_v = interact.NormalDirectionOfObject.normalized;
        Vector3 p_r = hitPoint;
        Vector3 n_r = arPlane.transform.up.normalized;

        //Store registration info for later use (dragging, rotation, scaling)
        interact.currentAnchorPointInWorld = p_r;
        interact.currentNormalDirectionInWorld = n_r;
        interact.currentARPlane = arPlane;

        //Compute rotation: align object normal to plane normal
        Quaternion rotation = Quaternion.FromToRotation(n_v, n_r);
        Vector3 position = p_r - rotation * p_v;

        arObject.transform.rotation = rotation;
        arObject.transform.position = position;
    }


    // Task 3: Object Rotation
    public void ObjectRotation(GameObject arObject, float deltaAngle)
    {
        if (arObject == null) return;

        ARInteractable interact = arObject.GetComponent<ARInteractable>();
        if (interact == null) return;

        Vector3 anchorWorld = interact.currentAnchorPointInWorld;
        Vector3 axisWorld = interact.currentNormalDirectionInWorld;

        arObject.transform.RotateAround(anchorWorld, axisWorld.normalized, deltaAngle);
    }


    // Task 3: Object Scaling
    public void ObjectScaling(GameObject arObject, float scaleRate)
    {
        if (arObject == null) return;

        ARInteractable interact = arObject.GetComponent<ARInteractable>();
        if (interact == null) return;

        Transform t = arObject.transform;

        Vector3 anchorWorld = interact.currentAnchorPointInWorld;
        Vector3 anchorLocal = t.InverseTransformPoint(anchorWorld);

        t.localScale *= scaleRate;

        Vector3 newAnchorWorld = t.TransformPoint(anchorLocal);

        Vector3 correction = anchorWorld - newAnchorWorld;
        t.position += correction;
    }
}