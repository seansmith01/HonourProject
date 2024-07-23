using UnityEngine;

public class RotateAround : MonoBehaviour
{
    [SerializeField] Transform centerTransform;

    [SerializeField] float rotationSpeed = 30f;

    [SerializeField] Vector3 rotationAxis = Vector3.up;


    void Update()
    {
        // Check if the centerTransform is assigned
        if (centerTransform != null)
        {
            Vector3 rotationCenter = centerTransform.position;

            transform.RotateAround(rotationCenter, rotationAxis, rotationSpeed * Time.deltaTime);
        }
        else
        {
            Debug.LogWarning("Center transform not assigned. Please assign a transform to centerTransform.");
        }
    }
}
