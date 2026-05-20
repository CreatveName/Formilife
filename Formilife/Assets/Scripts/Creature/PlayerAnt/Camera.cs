using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class TopDownCamera : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Follow")]
    public float followSmoothing = 10f;

    // Zoom
    // Orthographic: FOV
    // Perspective: Height (distance from player)

    [Header("Fov")]
    public float initialFov = 20f;
    public float fovSpeed = 5f;
    public float fovSmoothing = 2f;
    public float minFov = 10f;
    public float maxFov = 40f;

    [Header("Height")]
    public float initialHeight = 30f;
    public float heightSpeed = 5f;
    public float heightSmoothing = 2f;
    public float minHeight = 10f;
    public float maxHeight = 80f;
    //Private

    private Camera _camera;
    private float _targetZoom;
    private float _zoomDirection = 0f;

    // Either orthographic or perspective (3d) camera

    private bool isOrthographic = true;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        if (!_camera)
        {
            Debug.LogError("TopDownCamera requires a Camera component.", this);
            enabled = false;
            return;
        }
        else if (!player)
        {
            Debug.LogError("TopDownCamera requires a reference to the player Transform.", this);
            enabled = false;
            return;
        }

        setCameraType(isOrthographic);

    }

    public void setCameraType(bool orthographic)
    {
        isOrthographic = orthographic;
        _camera.orthographic = orthographic;
        if (orthographic)
        {
            SetFov(initialFov);
        }
        else
        {
            SetHeight(initialHeight);
        }
    }

    // Handles Zoom for orthographic camera, ranges from (1, 179)
    public void SetFov(float zoom, bool allowBeyondMax = false)
    {
        if (!_camera) _camera = GetComponent<Camera>();
        float upper = allowBeyondMax ? Mathf.Max(maxFov, zoom) : maxFov;
        zoom = Mathf.Clamp(zoom, minFov, upper);
        _targetZoom = zoom;
        if (_camera.orthographic) _camera.orthographicSize = zoom;
        else _camera.fieldOfView = zoom;
    }

    // Handles Zoom for perspective camera, ranges from (0.1, infinity)
    public void SetHeight(float height, bool allowBeyondMax = false)
    {
        if (!_camera) _camera = GetComponent<Camera>();
        float upper = allowBeyondMax ? Mathf.Max(maxHeight, height) : maxHeight;
        height = Mathf.Clamp(height, minHeight, upper);
        _targetZoom = height;
        if (!_camera.orthographic)
        {
            transform.position = player.position - transform.forward * height;
        }
    }
void Update()
{
    if (Keyboard.current.cKey.wasPressedThisFrame)
    {
        setCameraType(!isOrthographic);
    }

    HandleZoomKeys();
}

void HandleZoomKeys()
{
    bool min = Keyboard.current.digit9Key.wasPressedThisFrame;
    bool max = Keyboard.current.digit0Key.wasPressedThisFrame;

    // Hold - to zoom in, hold = to zoom out
    if (Keyboard.current.minusKey.isPressed)       _zoomDirection = -1f;
    else if (Keyboard.current.equalsKey.isPressed) _zoomDirection =  1f;
    else                                           _zoomDirection =  0f;

    if (isOrthographic)
    {
        if (min) SetFov(minFov);
        if (max) SetFov(maxFov);
        _targetZoom += _zoomDirection * fovSpeed * Time.deltaTime;
        _targetZoom  = Mathf.Clamp(_targetZoom, minFov, maxFov);
    }
    else
    {
        if (min) SetHeight(minHeight);
        if (max) SetHeight(maxHeight);
        _targetZoom += _zoomDirection * heightSpeed * Time.deltaTime;
        _targetZoom  = Mathf.Clamp(_targetZoom, minHeight, maxHeight);
    }
}

    void LateUpdate()
    {
        if (isOrthographic) {
            HandleZoomOrthographic();
            ApplyTransformOrthographic();
        }
        else
        {
            HandleZoomPerspective();
            ApplyTransformPerspective();   
        }
    }

    void HandleZoomOrthographic()
    {
        float scrollInput = Mouse.current.scroll.ReadValue().y;

        if (Mathf.Abs(scrollInput) > 0.01f)
        {

            _targetZoom -= scrollInput * fovSpeed;
            _targetZoom = Mathf.Clamp(_targetZoom, minFov, maxFov);
        }
    }

    void ApplyTransformOrthographic()
    {
        float followT = 1f - Mathf.Exp(-followSmoothing * Time.deltaTime);
        Vector3 target = new Vector3(player.position.x, player.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, target, followT);

        float zoomT = 1f - Mathf.Exp(-fovSmoothing * Time.deltaTime);
        if (_camera.orthographic)
        {
            _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, _targetZoom, zoomT);
        }
        else
        {
            _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, _targetZoom, zoomT);
        }
    }

    //Perspective camera zoom is handled by changing the height of the camera above the player, 
    // which is done by moving the camera along its forward vector

    void HandleZoomPerspective()
    {
        float scrollInput = Mouse.current.scroll.ReadValue().y;

        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            _targetZoom -= scrollInput * heightSpeed;
            _targetZoom = Mathf.Clamp(_targetZoom, minHeight, maxHeight);
        }
    }

    void ApplyTransformPerspective()
    {
        float zoomT = 1f - Mathf.Exp(-heightSmoothing * Time.deltaTime);
        Vector3 desiredPosition = player.position - transform.forward * _targetZoom;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, zoomT);
    }

}


