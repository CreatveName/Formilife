using System;
using UnityEngine;

public class DrinkEffect : MonoBehaviour
{
    [SerializeField] private float thirstRestore = 25f;
    public bool cracked = false;
    public bool needsCrack = false;
    // Droplets spawned inside chambers manage their own respawning, so they
    // should not affect Formicary's separate global water count when drunk.
    public bool countsTowardFormicaryWater = true;
    private Formicary spawner;

    public void Drink(GameObject consumer)
    {
        AntNeeds needs = consumer.GetComponent<AntNeeds>();
        if(!needsCrack || cracked){

            if (needs != null)
            {
                needs.RestoreThirst(thirstRestore);
                Debug.Log("drinking!");
                if (countsTowardFormicaryWater)
                    Formicary.currNumWater--;
            }
            Destroy(gameObject);
        }
        else
        {
            return;
        }
    }
}