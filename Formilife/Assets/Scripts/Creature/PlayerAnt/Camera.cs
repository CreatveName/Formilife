using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class TopDownCamera : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Zoom")]
    public float zoomSpeed = 5f;
    public float zoomSmoothing = 2f;
    public float minZoom = 5f;
    public float maxZoom = 15f;

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

        if (_camera.orthographic)
        {
            _targetZoom = _camera.orthographicSize;
        }
        else
        {
            _targetZoom = _camera.fieldOfView;
        }
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
        Vector3 smoothed = Vector3.Lerp(
            transform.position,
            new Vector3(player.position.x, player.position.y, transform.position.z),
            Time.deltaTime * zoomSmoothing
        );
        transform.position = smoothed;

        if (_camera.orthographic)
        {
            _camera.orthographicSize = Mathf.Lerp(
                _camera.orthographicSize, 
                _targetZoom,
                Time.deltaTime * zoomSmoothing
            );
        }
        else
        {
            float newY = Mathf.Lerp(
                transform.position.y,
                _targetZoom,
                Time.deltaTime * zoomSmoothing
            );
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
}
