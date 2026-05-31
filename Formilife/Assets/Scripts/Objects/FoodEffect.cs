using UnityEngine;

public class FoodEffect : MonoBehaviour
{
    [SerializeField] private float hungerRestore = 25f;
    [SerializeField] private float thirstRestore = 5f;
    [SerializeField] private float healthRestore = 25f;
    public bool cracked = false;
    public bool needsCrack = false;

    public void Consume(GameObject consumer)
    {
        AntNeeds needs = consumer.GetComponent<AntNeeds>();
        if(!needsCrack || cracked){

            if (needs != null)
            {
                needs.RestoreHunger(hungerRestore);
                needs.RestoreThirst(thirstRestore);
                needs.Heal(healthRestore);
            }
            Destroy(gameObject);
        }
        else
        {
            return;
        }
    }
}