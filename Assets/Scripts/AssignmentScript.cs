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
        Camera mainCamera = Camera.main;

        // First 3D point -Camera position
        Vector3 cameraPos = mainCamera.transform.position;

        // Second 3D point: Screen → world conversion
        Vector3 cameraSpacePos =
            new Vector3(screenPosition.x, screenPosition.y, mainCamera.nearClipPlane);

        Vector3 worldSpacePos = mainCamera.ScreenToWorldPoint(cameraSpacePos);

        // Create the ray
        Ray ray = new Ray(cameraPos, (worldSpacePos - cameraPos).normalized);

        // Use RaycastAll so that ARPlane can still be detected behind virtual objects
        RaycastHit[] hits = Physics.RaycastAll(ray);

        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.CompareTag("ARPlane"))
            {
                return (hit.collider.gameObject, hit.point);
            }
        }

        return (null, null);
    }


    // Task 2: Geometric Registration
    public void GeometricRegistration(GameObject arObject, Vector3 hitPoint, GameObject arPlane)
    {
        ARInteractable ar = arObject.GetComponent<ARInteractable>();

        // pv (local) bottom center of object
        Vector3 p_v = ar.AnchorPointOfObject;

        // nv (local) object normal
        Vector3 n_v = ar.NormalDirectionOfObject;

        // pr (world) hit point
        Vector3 p_r = hitPoint;

        // nr (world) plane normal
        Vector3 n_r = arPlane.transform.up;

        // Store for later use (rotation & scaling)
        ar.currentAnchorPointInWorld = p_r;
        ar.currentNormalDirectionInWorld = n_r;

        // Step 1: Rotate object so that n_v aligns with n_r
        Quaternion rotation = Quaternion.FromToRotation(n_v, n_r);
        arObject.transform.rotation = rotation;

        // Step 2: Translate object so pv aligns with pr
        Vector3 pv_world_after_rotation = arObject.transform.TransformPoint(p_v);
        Vector3 translation = p_r - pv_world_after_rotation;

        arObject.transform.position += translation;
    }


    // Task 3: Object Rotation and Scaling
    public void ObjectRotation(GameObject arObject, float deltaAngle)
    {
        ARInteractable ar = arObject.GetComponent<ARInteractable>();

        // Rotation axis = plane normal
        Vector3 axis = ar.currentNormalDirectionInWorld;

        // Rotation center = object bottom center
        Vector3 center = ar.currentAnchorPointInWorld;

        // Use RotateAround for numerical stability
        arObject.transform.RotateAround(center, axis, deltaAngle);
    }

    public void ObjectScaling(GameObject arObject, float scaleRate)
    {
        ARInteractable ar = arObject.GetComponent<ARInteractable>();

        Vector3 center = ar.currentAnchorPointInWorld;

        // Move object so anchor is origin
        arObject.transform.position -= center;

        // Apply scaling
        arObject.transform.localScale *= scaleRate;

        // Move back to anchor
        arObject.transform.position += center;
    }

}