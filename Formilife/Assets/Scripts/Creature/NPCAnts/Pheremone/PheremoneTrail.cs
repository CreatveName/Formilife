using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PheromoneTrail : MonoBehaviour
{
    [Header("Logic")]
    [SerializeField] private float radius = 1.25f;

    [Header("Visuals")]
    [SerializeField] private ParticleSystem gasEmitterPrefab;
    [SerializeField] private float emitterSpacing = 0.9f;

    [SerializeField] private float minWidth = 0.35f;
    [SerializeField] private float maxWidth = 0.55f;
    [SerializeField] private float pulseSpeed = 2f;

    public List<Vector3> points = new List<Vector3>();

    private LineRenderer line;
    private readonly List<ParticleSystem> gasEmitters = new List<ParticleSystem>();

    private void Awake()
    {
        line = GetComponent<LineRenderer>();

        line.useWorldSpace = true;
        line.positionCount = 0;

        line.startWidth = minWidth;
        line.endWidth = minWidth;

        line.numCapVertices = 8;
        line.numCornerVertices = 8;

        line.sortingOrder = 10;

        Material mat = new Material(Shader.Find("Sprites/Default"));
        line.material = mat;

        Color faintRed = new Color(1f, 0.1f, 0.03f, 0.18f);
        line.startColor = faintRed;
        line.endColor = faintRed;
    }

    private void Update()
    {
        if (line == null)
            return;

        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        float width = Mathf.Lerp(minWidth, maxWidth, t);

        line.startWidth = width;
        line.endWidth = width;
    }

    public void AddPoint(Vector3 point)
    {
        point.z = -0.1f;

        points.Add(point);

        line.positionCount = points.Count;
        line.SetPosition(points.Count - 1, point);

        TryAddGasEmitter(point);
    }

    private void TryAddGasEmitter(Vector3 point)
    {
        if (gasEmitterPrefab == null)
            return;

        if (gasEmitters.Count == 0)
        {
            CreateGasEmitter(point);
            return;
        }

        Vector3 lastEmitterPos = gasEmitters[gasEmitters.Count - 1].transform.position;
        float distanceFromLastEmitter = Vector3.Distance(lastEmitterPos, point);

        if (distanceFromLastEmitter >= emitterSpacing)
        {
            CreateGasEmitter(point);
        }
    }

    private void CreateGasEmitter(Vector3 point)
    {
        ParticleSystem emitter = Instantiate(gasEmitterPrefab, point, Quaternion.identity, transform);
        emitter.transform.position = point;
        emitter.Play();

        gasEmitters.Add(emitter);
    }

    public void SetVisible(bool visible)
    {
        if (line != null)
            line.enabled = visible;

        foreach (ParticleSystem emitter in gasEmitters)
        {
            if (emitter == null)
                continue;

            ParticleSystemRenderer renderer = emitter.GetComponent<ParticleSystemRenderer>();

            if (renderer != null)
                renderer.enabled = visible;
        }
    }

    public bool ContainsPoint(Vector3 position)
    {
        foreach (Vector3 point in points)
        {
            if (Vector3.Distance(position, point) <= radius)
                return true;
        }

        return false;
    }

    public Vector3 GetRandomPoint()
    {
        if (points.Count == 0)
            return transform.position;

        return points[Random.Range(0, points.Count)];
    }
}