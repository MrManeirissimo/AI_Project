using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControll : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] private Camera _camera;

    [Header("Values")]
    [SerializeField] private float _translationSpeed = 1;
    [SerializeField] private float _scorllSpeed = 1000;

    private Vector3 _mouseStartPos;

    private void Awake()
    {
        if (!_camera) _camera = FindObjectOfType<Camera>();
    }
    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            _mouseStartPos = Input.mousePosition;
        }
        else if(Input.GetMouseButton(0))
        {
            float __distX = _mouseStartPos.x - Input.mousePosition.x;
            float __distY = _mouseStartPos.y - Input.mousePosition.y;

            _camera.transform.Translate(new Vector3(__distX, __distY, 0) * _translationSpeed * Time.deltaTime);
        }

        _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize + _scorllSpeed * -Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime, 10, 1000);
    }
}
