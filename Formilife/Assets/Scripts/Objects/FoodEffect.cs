using UnityEngine;

public class FoodEffect : MonoBehaviour
{
    [SerializeField] private float hungerRestore = 25f;
    [SerializeField] private float thirstRestore = 5f;
    [SerializeField] private float healthRestore = 25f;
    public bool cracked = false;
    public bool needsCrack = false;

    [Header("Audio")]
    [SerializeField] private AudioClip eatSound;
    [Range(0f, 1f)]
    [SerializeField] private float eatVolume = 1f;

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
            // Play through a temporary one-shot source since this object is
            // destroyed immediately and can't outlive its own AudioSource.
            if (eatSound != null)
                AudioSource.PlayClipAtPoint(eatSound, transform.position, eatVolume);
            Destroy(gameObject);
        }
        else
        {
            return;
        }
    }
}