using UnityEngine;

public class FitBackgroundToCamera3D : MonoBehaviour
{
    public Camera mainCamera;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        FitQuadToCamera();
    }

    void FitQuadToCamera()
    {
        float height = 2f * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) * Mathf.Abs(transform.position.z - mainCamera.transform.position.z);
        float width = height * mainCamera.aspect;

        // Scale quad so it covers the entire camera view
        transform.localScale = new Vector3(width, height, 1);
    }
}
