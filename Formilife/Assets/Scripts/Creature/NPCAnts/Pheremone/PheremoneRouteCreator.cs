using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class PheromoneRouteCreator : MonoBehaviour
{
    [Header("Route Setup")]
    [SerializeField] private LayerMask zoneLayer;
    [SerializeField] private PheromoneRoute routePrefab;

    [Header("Input")]
    [SerializeField] private KeyCode pheromoneModeKey = KeyCode.LeftShift;
    [SerializeField] private GameObject pheromoneIndicatorUI;

    private PheromoneZone selectedStart;

    private void Update()
    {
        bool inPheromoneMode =
            Keyboard.current != null &&
            Keyboard.current.leftShiftKey.isPressed;

        if (pheromoneIndicatorUI != null)
            pheromoneIndicatorUI.SetActive(inPheromoneMode);

        if (!inPheromoneMode)
        {
            selectedStart = null;
            return;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            TrySelectZone();
        }
    }
    private void TrySelectZone()
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);

        Collider2D hit = Physics2D.OverlapPoint(mouseWorld, zoneLayer);

        if (hit == null)
            return;

        PheromoneZone zone = hit.GetComponent<PheromoneZone>();

        if (zone == null)
            return;

        if (selectedStart == null)
        {
            selectedStart = zone;
            Debug.Log("Selected start zone: " + zone.name);
            return;
        }

        if (zone == selectedStart)
        {
            Debug.Log("Cannot connect a zone to itself.");
            return;
        }

        CreateRoute(selectedStart, zone);
        selectedStart = null;
    }

    private void CreateRoute(PheromoneZone start, PheromoneZone target)
    {
        PheromoneRoute route = Instantiate(routePrefab);
        route.startZone = start;
        route.targetZone = target;

        if (PheromoneRouteManager.Instance != null)
            PheromoneRouteManager.Instance.SetRoute(route);

        Debug.Log($"Created route: {target.name} → {start.name}");
    }
}