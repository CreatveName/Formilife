using UnityEngine;
using UnityEngine.InputSystem;

public class Formicary : MonoBehaviour
{
    public Texture2D logTexture;
    public Texture2D grayLogTexture;
    public Texture2D leafTexture;
    public Texture2D darkLeafTexture;
    public Texture2D testTexture;

    public static int currNumWater = 5;

    public Renderer wallR;
    public GameObject[] sandGrainOptions;
    public GameObject drinkablePrefab;
    private GameObject chosenSandGrain;

    [Header("Tiling Settings")]
    public Vector2 textureTiling = new Vector2(5f, 5f);

    [Header("Debug Settings")]
    public bool debugMode = true;

    private Texture2D[] textureList;
    private int currentTextureIndex = 0;
    private string[] textureNames;

    public int numWater = 5;
    public Vector2 spawnAreaMin; // bottom-left of spawn area
    public Vector2 spawnAreaMax; // top-right of spawn area


    void Start()
    {
        Debug.Log("we're starting");
        textureList = new Texture2D[] { testTexture, logTexture, grayLogTexture, leafTexture, darkLeafTexture };
        textureNames = new string[] { "testTexture", "logTexture", "grayLogTexture", "leafTexture", "darkLeafTexture" };


        if (debugMode)
            ChangeTexture(wallR, textureList[currentTextureIndex]);

        for(int i = 0; i < numWater; ++i)
            SpawnDroplet();
    }
    void Update()
    {
        if (!debugMode) return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.pKey.wasPressedThisFrame)
        {
            CycleToNextTexture();
        }
        if(currNumWater != numWater)
        {
            Debug.Log("missing water!");
            while(currNumWater != numWater)
            {
                SpawnDroplet();
                currNumWater++;
            }
        }
    }

    //debug for krill, will be removed when texture system is finalized. cycles through textures on wall for testing purposes
    void CycleToNextTexture()
    {
        currentTextureIndex = (currentTextureIndex + 1) % textureList.Length;
        Texture2D next = textureList[currentTextureIndex];

        if (next == null)
        {
            Debug.LogWarning($"[Formicary] Texture at index {currentTextureIndex} ({textureNames[currentTextureIndex]}) is null, skipping...");
            CycleToNextTexture();
            return;
        }

        ChangeTexture(wallR, next);
        Debug.Log($"[Formicary] Switched to texture {currentTextureIndex}: {textureNames[currentTextureIndex]}");
    }

    //sand grain system for future use, not currently implemented
    void PickRandomSandGrain()
    {
        if (sandGrainOptions.Length == 0)
        {
            Debug.LogWarning("[Formicary] No sand grains in list");
            return;
        }
        int index = Random.Range(0, sandGrainOptions.Length);
        chosenSandGrain = sandGrainOptions[index];
        Debug.Log($"[Formicary] Picked sand grain: {chosenSandGrain.name}");
    }

    public void SpawnDroplet()
    {
        Vector2 randomPos = new Vector2(
            Random.Range(spawnAreaMin.x, spawnAreaMax.x),
            Random.Range(spawnAreaMin.y, spawnAreaMax.y)
        );
        GameObject spawnedDrinkable = Instantiate(drinkablePrefab, randomPos, Quaternion.identity);
    }

    void ChangeTexture(Renderer part, Texture2D newTexture)
    {
        if (part == null)
        {
            Debug.LogError("[Formicary] Renderer is null!");
            return;
        }
        if (newTexture == null)
        {
            Debug.LogError("[Formicary] Texture is null!");
            return;
        }

        part.material.mainTexture = newTexture;
        part.material.mainTextureScale = textureTiling;

        if (debugMode)
            Debug.Log($"[Formicary] Applied texture '{newTexture.name}' with tiling {textureTiling}");
    }
}