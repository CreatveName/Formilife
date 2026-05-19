using UnityEngine;
using UnityEngine.InputSystem;

public class TopDownCamera : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Follow")]
    public float followSmoothing = 10f;
    public float returnDelay = 1.5f;
    public float returnSmoothing = 5f;
    [Header("Pan")]
    public float panSpeed = 0.15f;
    public float panSmoothing = 12f;

    [Header("Zoom")]
    public float zoomSpeed = 2f;
    public float zoomSmoothing = 2f;
    public float minZoom = 5f;
    public float maxZoom = 80f;

    // Private
    private Camera _camera;
    private float _targetFOV;

    // Pan state
    private Vector3 _panOffset = Vector3.zero;
    private Vector3 _targetPanOffset = Vector3.zero;
    private Vector2 _lastMousePosition;
    private bool _isPanning = false;
    private bool _hasPanned = false;

    private Vector3 _lastPlayerPosition;
    private float _playerStillTimer = 0f;
    private bool _returningToPlayer = false;

    private void Awake()
    {
        _camera = GetComponent<Camera>();

        if (!_camera)
        {
            Debug.LogError("TopDownCamera requires a Camera component.", this);
            enabled = false;
            return;
        }
        if (!player)
        {
            Debug.LogError("TopDownCamera requires a reference to the player Transform.", this);
            enabled = false;
            return;
        }

        _camera.orthographic = false;

        _targetFOV = maxZoom;
        _camera.fieldOfView = maxZoom;

        _lastPlayerPosition = player.position;
    }

    public void SetZoom(float zoom, bool allowBeyondMax = false)
    {
        if (!_camera) _camera = GetComponent<Camera>();
        float upper = allowBeyondMax ? Mathf.Max(maxZoom, zoom) : maxZoom;
        zoom = Mathf.Clamp(zoom, minZoom, upper);
        _targetFOV = zoom;
        _camera.fieldOfView = zoom;
    }

    void LateUpdate()
    {
        HandleZoom();
        HandlePan();
        TrackPlayerMovement();
        ApplyTransform();
    }

    void HandleZoom()
    {
        float scrollInput = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            _targetFOV -= scrollInput * zoomSpeed;
            _targetFOV = Mathf.Clamp(_targetFOV, minZoom, maxZoom);
        }
    }

    void HandlePan()
    {
        bool mouseHeld = Mouse.current.middleButton.isPressed
                      || Mouse.current.rightButton.isPressed;

        if (mouseHeld)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            if (!_isPanning)
            {
                _lastMousePosition = mousePos;
                _isPanning = true;
            }
            else
            {
                Vector2 delta = mousePos - _lastMousePosition;
                _lastMousePosition = mousePos;

                if (delta.sqrMagnitude > 0.01f)
                {
                    float fovScale = _camera.fieldOfView / maxZoom;

                    Vector3 right   = transform.right;
                    Vector3 up      = transform.up;

                    _targetPanOffset -= (right   * delta.x
                                      + up       * delta.y)
                                      * panSpeed * fovScale;

                    _hasPanned = true;
                    _returningToPlayer = false;
                    _playerStillTimer = 0f;
                }
            }
        }
        else
        {
            _isPanning = false;
        }
    }

    void TrackPlayerMovement()
    {
        float playerMoveDelta = Vector3.Distance(player.position, _lastPlayerPosition);

        if (playerMoveDelta > 0.01f)
        {
            _lastPlayerPosition = player.position;
            _playerStillTimer = 0f;

            if (_hasPanned)
            {
                _returningToPlayer = true;
            }
        }
        else if (_hasPanned)
        {
            _playerStillTimer += Time.deltaTime;

            if (_playerStillTimer >= returnDelay)
            {
                _returningToPlayer = true;
            }
        }

        if (_returningToPlayer)
        {
            float t = 1f - Mathf.Exp(-returnSmoothing * Time.deltaTime);
            _targetPanOffset = Vector3.Lerp(_targetPanOffset, Vector3.zero, t);

            if (_targetPanOffset.sqrMagnitude < 0.001f)
            {
                _targetPanOffset = Vector3.zero;
                _panOffset = Vector3.zero;
                _hasPanned = false;
                _returningToPlayer = false;
            }
        }
    }

    void ApplyTransform()
    {
        float panT = 1f - Mathf.Exp(-panSmoothing * Time.deltaTime);
        _panOffset = Vector3.Lerp(_panOffset, _targetPanOffset, panT);

        float followT = 1f - Mathf.Exp(-followSmoothing * Time.deltaTime);
        Vector3 playerTarget = new Vector3(player.position.x, player.position.y, transform.position.z);
        Vector3 smoothedFollow = Vector3.Lerp(transform.position, playerTarget, followT);
        transform.position = smoothedFollow + _panOffset;

        float zoomT = 1f - Mathf.Exp(-zoomSmoothing * Time.deltaTime);
        _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, _targetFOV, zoomT);
    }
}