using UnityEngine;
using UnityEngine.InputSystem;

public class PheromoneDrawer : MonoBehaviour
{
    [Header("Drawing")]
    [SerializeField] private PheromoneTrail trailPrefab;
    [SerializeField] private float pointSpacing = 0.15f;

    [Header("UI")]
    [SerializeField] private GameObject pheromoneIndicatorUI;
    [SerializeField] private GameObject pheromoneOverlay;

    private PheromoneTrail currentTrail;
    private Vector3 lastPoint;
    private bool isDrawing;

    private void Update()
    {
        bool inPheromoneMode =
            Keyboard.current != null &&
            Keyboard.current.leftShiftKey.isPressed;

        if (pheromoneIndicatorUI != null)
            pheromoneIndicatorUI.SetActive(inPheromoneMode);

        if (pheromoneOverlay != null)
            pheromoneOverlay.SetActive(inPheromoneMode);

        // Toggle finished trails
        if (PheromoneManager.Instance != null)
        {
            foreach (var trail in PheromoneManager.Instance.GetAllTrails())
            {
                if (trail != null)
                    trail.SetVisible(inPheromoneMode);
            }
        }

        // Toggle current trail
        if (currentTrail != null)
            currentTrail.SetVisible(inPheromoneMode);

        if (!inPheromoneMode)
        {
            if (isDrawing)
                EndTrail();

            return;
        }

        if (Mouse.current == null)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            BeginTrail();
        }

        if (Mouse.current.leftButton.isPressed && isDrawing)
        {
            ContinueTrail();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame && isDrawing)
        {
            EndTrail();
        }
    }

    private void BeginTrail()
    {
        Vector3 mouseWorld = GetMouseWorldPosition();

        currentTrail = Instantiate(trailPrefab);
        currentTrail.AddPoint(mouseWorld);
        lastPoint = mouseWorld;
        isDrawing = true;
    }

    private void ContinueTrail()
    {
        Vector3 mouseWorld = GetMouseWorldPosition();

        if (Vector2.Distance(lastPoint, mouseWorld) >= pointSpacing)
        {
            currentTrail.AddPoint(mouseWorld);
            lastPoint = mouseWorld;
        }
    }

    private void EndTrail()
    {
        if (currentTrail != null)
        {
            PheromoneManager.Instance.RegisterTrail(currentTrail);
        }
        else
        {
            Debug.LogWarning("EndTrail called but currentTrail was null.");
        }

        isDrawing = false;
        currentTrail = null;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();

        Vector3 mousePos = new Vector3(
            mouseScreen.x,
            mouseScreen.y,
            Mathf.Abs(Camera.main.transform.position.z)
        );

        Vector3 world = Camera.main.ScreenToWorldPoint(mousePos);
        world.z = 0f;

        return world;
    }
}