using UnityEngine;

public class FoodEffect : MonoBehaviour
{
    [SerializeField] private float hungerRestore = 25f;

    public void Consume(GameObject consumer)
    {
        AntNeeds needs = consumer.GetComponent<AntNeeds>();

        if (needs != null)
        {
            needs.RestoreHunger(hungerRestore);
        }

        Destroy(gameObject);
    }
}