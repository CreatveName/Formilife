using UnityEngine;

public class Formicary : MonoBehaviour
{
    public Texture2D logTexture;
    public Texture2D leafTexture;
    public Renderer wallR;



    void Start()
    {
        //Transform formicaryWalls = transform.Find("Assets/3D Models/Formicary.fbx");
        ChangeTexture(wallR, logTexture);
    }

    void ChangeTexture(Renderer part, Texture2D newTexture)
    {
        //Renderer myRenderer = part.GetComponent<Renderer>();
        wallR.material.mainTexture = newTexture;
    }
}
