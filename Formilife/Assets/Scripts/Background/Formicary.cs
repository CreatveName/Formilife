using UnityEngine;

public class Formicary : MonoBehaviour
{
    public Texture2D logTexture;
    public Texture2D leafTexture;
    public Texture2D lightWoodTexture;
    public Texture2D darkWoodTexture;

    public Renderer wallR;
    public Renderer backgroundPlane;



    void Start()
    {
        //Transform formicaryWalls = transform.Find("Assets/3D Models/Formicary.fbx");
        ChangeTexture(wallR, logTexture);
        ChangeTexture(backgroundPlane, lightWoodTexture);
    }

    void ChangeTexture(Renderer part, Texture2D newTexture)
    {
        //Renderer myRenderer = part.GetComponent<Renderer>();
        wallR.material.mainTexture = newTexture;
    }
}
