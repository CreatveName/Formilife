using UnityEngine;

public class Stackable : MonoBehaviour
{

    public double stackCount = 1.0;
    public bool isMerging = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Stackable other = collision.GetComponent<Stackable>();
        if (other == null) return;
        if (other == this) return;

        if (isMerging) return;
        if (other.isMerging) return; //lock so that the right game object is destoryed/kept

        if (transform.parent != null) return;
        if (other.transform.parent != null) return;

        MergeWith(other);
        
    }

    private void MergeWith(Stackable other)
    {
        isMerging = true;
        other.isMerging = true;

        stackCount += other.stackCount; //add all of the stackable count into this pile
        Debug.Log("stack size: " + stackCount);

        other.stackCount -= stackCount; //This can be better but im lazy and this will always make it less than 0

        if (other.stackCount <= 0) //This basically means it will destroy the gameobject that you are adding if the stack count goes below 0, which we do in the above equation
        {
            Destroy(other.gameObject);
        }
        
        isMerging = false;
    }


}
