using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCam : MonoBehaviour
{
    [SerializeField] private Transform target;

    private void Follow()
    {
        if(target)
        {
            Vector3 __targetPos = new Vector3(target.position.x, transform.position.y, target.position.z);
            Vector3 postion = Vector3.Lerp(transform.position, __targetPos, 0.125f);

            transform.position = postion;
        }
    }
    private void Update()
    {
        Follow();
    }
}
