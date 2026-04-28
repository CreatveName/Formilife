using UnityEngine;

public class Formicary : MonoBehaviour
{
    public Texture2D logTexture;
    public Texture2D leafTexture;
    public Renderer wallR;
    public GameObject[] sandGrainOptions;
    private GameObject chosenSandGrain;

    void Start()
    {
        //Transform formicaryWalls = transform.Find("Assets/3D Models/Formicary.fbx");
        ChangeTexture(wallR, logTexture);
        PickRandomSandGrain();

    }

    void PickRandomSandGrain()
    {
        if (sandGrainOptions.Length == 0)
        {
            Debug.Log("No sand grains in list");
            return;
        }
        int index = Random.Range(0,sandGrainOptions.Length);
        chosenSandGrain = sandGrainOptions[index];
        Instantiate(chosenSandGrain, transform.position, Quaternion.identity);
    }

    void ChangeTexture(Renderer part, Texture2D newTexture)
    {
        //Renderer myRenderer = part.GetComponent<Renderer>();
        wallR.material.mainTexture = newTexture;
    }
}
