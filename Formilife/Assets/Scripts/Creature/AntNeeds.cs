using UnityEngine;
using UnityEngine.InputSystem;

public class AntNeeds : MonoBehaviour
{
    [SerializeField] private AntDefinition definition;

    [Header("Current Needs")]
    [SerializeField] private float hunger;
    [SerializeField] private float thirst;
    [SerializeField] private float health;

    [Header("Death")]
    [SerializeField] private bool respawnOnDeath = false;
    [SerializeField] private Transform respawnPoint;
    [SerializeField, Range(0f, 1f)] private float respawnStatFraction = 0.5f;

    [Header("Debug")]
    [SerializeField] private Key debugDamageKey = Key.K;
    [SerializeField] private float debugDamageAmount = 25f;

    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private bool isDead;

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
        health = definition.maxHealth;

        spawnPosition = respawnPoint != null ? respawnPoint.position : transform.position;
        spawnRotation = respawnPoint != null ? respawnPoint.rotation : transform.rotation;
    }

    private void Update()
    {
        if (definition == null || isDead) return;
        ApplyDecay();
        ApplySurvivalDamage();
        ClampValues();
        HandleDebugInput();
    }

    private void ApplySurvivalDamage()
    {
        if (hunger <= 0f) TakeDamage(definition.starvationDamageRate * Time.deltaTime);
        if (thirst <= 0f) TakeDamage(definition.dehydrationDamageRate * Time.deltaTime);
    }

    private void ApplyDecay()
    {
        hunger -= definition.hungerDecayRate * Time.deltaTime;
        thirst -= definition.thirstDecayRate * Time.deltaTime;
    }

    private void ClampValues()
    {
        hunger = Mathf.Clamp(hunger, 0, definition.maxHunger);
        thirst = Mathf.Clamp(thirst, 0, definition.maxThirst);
        health = Mathf.Clamp(health, 0, definition.maxHealth);
    }

    private void HandleDebugInput()
    {
        Keyboard kb = Keyboard.current;
        if (kb != null && kb[debugDamageKey].wasPressedThisFrame)
        {
            TakeDamage(debugDamageAmount);
        }
    }

    // =========================
    // PUBLIC API (FOR AI)
    // =========================

    public float GetHungerNormalized() => hunger / definition.maxHunger;
    public float GetThirstNormalized() => thirst / definition.maxThirst;
    public float GetHealthNormalized() => health / definition.maxHealth;

    public bool IsHungry() => hunger <= definition.lowHungerThreshold;
    public bool IsThirsty() => thirst <= definition.lowThirstThreshold;
    public bool IsDead => isDead;

    // =========================
    // FOOD/STAT INTERACTION
    // =========================

    public void RestoreHunger(float amount)
    {
        hunger = Mathf.Clamp(hunger + amount, 0, definition.maxHunger);
    }

    public void RestoreThirst(float amount)
    {
        thirst = Mathf.Clamp(thirst + amount, 0, definition.maxThirst);
    }

    public void Heal(float amount)
    {
        health = Mathf.Clamp(health + amount, 0, definition.maxHealth);
    }

    // =========================
    // HEALTH / DEATH
    // =========================

    public void TakeDamage(float amount)
    {
        if (isDead || amount <= 0f) return;

        health -= amount;
        if (health <= 0f)
        {
            health = 0f;
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        PlayerPickup pickup = GetComponent<PlayerPickup>();
        if (pickup != null && pickup.HeldItem != null) pickup.Drop();

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        SpawnCorpse();

        if (respawnOnDeath)
        {
            Respawn();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SpawnCorpse()
    {
        if (definition.corpsePrefab == null) return;
        Instantiate(definition.corpsePrefab, transform.position, transform.rotation);
    }

    private void Respawn()
    {
        transform.SetPositionAndRotation(spawnPosition, spawnRotation);

        health = definition.maxHealth * respawnStatFraction;
        hunger = definition.maxHunger * respawnStatFraction;
        thirst = definition.maxThirst * respawnStatFraction;

        isDead = false;
    }
}
