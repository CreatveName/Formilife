using UnityEngine;

public class Formicary : MonoBehaviour
{
    public Texture2D logTexture;
    public Texture2D leafTexture;



    void Start()
    {
        Transform formicaryWalls = transform.Find("Assets/3D Models/Formicary.fbx");
        ChangeTexture(formicaryWalls, logTexture);
    }

    void ChangeTexture(Transform part, Texture2D newTexture)
    {
        Renderer myRenderer = part.GetComponent<Renderer>();
        myRenderer.material.mainTexture = newTexture;
    }
}
