using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class TopDownCamera : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created 
    [Header("Player")]
    public Transform player;

    [Header("Zoom")]
    public float zoomSpeed = 5f;
    public float zoomSmoothing = 5f;
    public float minZoom = 5f;
    public float maxZoom = 15f;

    //Header("Deadzone")


    //Private

    private Camera _camera;
    private float _targetZoom;
    private Vector3 _targetPosition;
    private Vector3 _dragOrigin;
    private bool _isDragging;

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
        HandleDeadzoneFollow();
        HandleDragPan();
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

    void HandleDeadzoneFollow()
    {
        
    }

    void HandleDragPan()
    {
        
    }

    void ApplyTransform()
    {
        
    }
}
