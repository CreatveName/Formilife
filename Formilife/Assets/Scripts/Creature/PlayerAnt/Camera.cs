using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class TopDownCamera : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Follow")]
    public float followSmoothing = 10f; // higher = snappier; locks onto the ant

    [Header("Zoom")]
    public float zoomSpeed = 5f;
    public float zoomSmoothing = 2f;
    public float minZoom = 5f;
    public float maxZoom = 40f;

    //Private

    private Camera _camera;
    private float _targetZoom;

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

        // Start fully zoomed out at max zoom.
        SetZoom(maxZoom);
    }

    public void SetZoom(float zoom)
    {
        if (!_camera) _camera = GetComponent<Camera>();
        zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
        _targetZoom = zoom;
        if (_camera.orthographic) _camera.orthographicSize = zoom;
        else _camera.fieldOfView = zoom;
    }

    void LateUpdate()
    {
        HandleZoom();
        ApplyTransform();
    }

    void HandleZoom()
    {
        float scrollInput = Mouse.current.scroll.ReadValue().y;

        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            _targetZoom -= scrollInput * zoomSpeed;
            _targetZoom = Mathf.Clamp(_targetZoom, minZoom, maxZoom);
        }
    }

    void ApplyTransform()
    {
        // Frame-rate-independent smoothing: t -> 1 as dt grows, so the camera
        // always converges on the ant regardless of how fast it moves.
        float followT = 1f - Mathf.Exp(-followSmoothing * Time.deltaTime);
        Vector3 target = new Vector3(player.position.x, player.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, target, followT);

        float zoomT = 1f - Mathf.Exp(-zoomSmoothing * Time.deltaTime);
        if (_camera.orthographic)
        {
            _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, _targetZoom, zoomT);
        }
        else
        {
            _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, _targetZoom, zoomT);
        }
    }
}
