using UnityEngine;

public class AntNeeds : MonoBehaviour
{
    [SerializeField] private AntDefinition definition;

    [Header("Current Needs")]
    [SerializeField] private float hunger;
    [SerializeField] private float thirst;
    [SerializeField] private float energy;

    private void Start()
    {
        if (definition == null)
        {
            Debug.LogError($"{name} is missing an AntDefinition on AntNeeds.", this);
            enabled = false;
            return;
        }

        hunger = definition.maxHunger;
        thirst = definition.maxThirst;
        energy = definition.maxEnergy;
    }

    private void Update()
    {
        if (definition == null) return;
        ApplyDecay();
        ClampValues();
    }

    private void ApplyDecay()
    {
        hunger -= definition.hungerDecayRate * Time.deltaTime;
        thirst -= definition.thirstDecayRate * Time.deltaTime;
        energy -= definition.energyDecayRate * Time.deltaTime;
    }

    private void ClampValues()
    {
        hunger = Mathf.Clamp(hunger, 0, definition.maxHunger);
        thirst = Mathf.Clamp(thirst, 0, definition.maxThirst);
        energy = Mathf.Clamp(energy, 0, definition.maxEnergy);
    }

    // =========================
    // PUBLIC API (FOR AI)
    // =========================

    public float GetHungerNormalized()
    {
        return hunger / definition.maxHunger;
    }

    public float GetThirstNormalized()
    {
        return thirst / definition.maxThirst;
    }

    public float GetEnergyNormalized()
    {
        return energy / definition.maxEnergy;
    }

    public bool IsHungry()
    {
        return hunger <= definition.lowHungerThreshold;
    }

    public bool IsThirsty()
    {
        return thirst <= definition.lowThirstThreshold;
    }

    public bool IsTired()
    {
        return energy <= definition.tiredThreshold;
    }

    // =========================
    // FOOD/STAT INTERACTION
    // =========================

    public void RestoreHunger(float amount)
    {
        hunger += amount;
        hunger = Mathf.Clamp(hunger, 0, definition.maxHunger);
    }

    public void RestoreThirst(float amount)
    {
        thirst += amount;
        thirst = Mathf.Clamp(thirst, 0, definition.maxThirst);
    }

    public void RestoreEnergy(float amount)
    {
        energy += amount;
        energy = Mathf.Clamp(energy, 0, definition.maxEnergy);
    }
}