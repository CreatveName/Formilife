using System;
using UnityEngine;

public class DrinkEffect : MonoBehaviour
{
    [SerializeField] private float thirstRestore = 25f;
    public bool cracked = false;
    public bool needsCrack = false;
    private Formicary spawner;

    public void Drink(GameObject consumer)
    {
        AntNeeds needs = consumer.GetComponent<AntNeeds>();
        if(!needsCrack || cracked){

            if (needs != null)
            {
                needs.RestoreThirst(thirstRestore);
                Debug.Log("drinking!");
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