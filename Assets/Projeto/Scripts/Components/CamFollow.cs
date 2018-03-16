using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollow : MonoBehaviour
{
    // The target we are following
    [Header("References")]
    [SerializeField] private Transform target;

    [Header("Values")]
    // The distance in the x-z plane to the target
    [SerializeField] private float distance = 10.0f;
    [SerializeField] private float distanceMod = 1.0f;

    // the height we want the camera to be above the target
    [SerializeField] private float height = 5.0f;

    [Header("Optional values")]
    [SerializeField] private float rotationDamping;
    [SerializeField] private float heightDamping;

    public void SetCamera()
    {
        transform.position = target.position;
        transform.position -= target.rotation * Vector3.forward * distance * ((OffsetPercentage / distanceMod) + 1);
        transform.position += target.rotation * Vector3.up * height;
        transform.LookAt(target, target.up);
    }
    public float OffsetPercentage
    {
        get; set;
    }

    void LateUpdate()
    {
        // Early out if we don't have a target
        if (!target)
            return;

        // Calculate the current rotation angles
        var wantedRotationAngle = target.eulerAngles.y;
        var wantedHeight = target.position.y + height;

        var wantedSpot = target.position + (target.rotation * new Vector3(0, height, distance));

        var currentRotationAngle = transform.eulerAngles.y;
        var currentHeight = transform.position.y;

        //// Damp the rotation around the y-axis
        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);

        //// Damp the height
        currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);

        //// Convert the angle into a rotation
        var currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

        //// Updates distance
        float __followDistance = distance * ((OffsetPercentage / distanceMod) + 1);

        //// Set the position of the camera on the x-z plane to:
        //// distance meters behind the target
        transform.position = target.position;
        transform.position -= target.rotation * Vector3.forward * __followDistance;
        transform.position += target.rotation * Vector3.up * height;

        //// Set the height of the camera
        //transform.position = new Vector3(transform.position.x, wantedSpot.y, transform.position.z);

        // Always look at the target
        transform.LookAt(target, target.up);
    }
}