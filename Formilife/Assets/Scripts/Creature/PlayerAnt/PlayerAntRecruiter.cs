using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAntRecruiter : MonoBehaviour
{
    [Header("Recruiting")]
    [SerializeField] private float recruitRange = 1.5f;
    [SerializeField] private int maxRecruits = 3;
    [SerializeField] private LayerMask antLayer;

    [Header("Follow Behavior")]
    [SerializeField] private float followSpacing = 1.1f;

    [Header("Carry Help")]
    [SerializeField] private float speedBonusPerHelper = 0.35f;
    [SerializeField] private float maxSpeedMultiplier = 2f;

    private readonly List<AntNPC> recruitedAnts = new List<AntNPC>();

    private PlayerPickup pickup;
    private PlayerAntMovement movement;

    private void Awake()
    {
        pickup = GetComponent<PlayerPickup>();
        movement = GetComponent<PlayerAntMovement>();
    }

    private void OnEnable()
    {
        if (pickup != null)
        {
            pickup.OnPickedUpItem += HandlePickedUpItem;
            pickup.OnDroppedItem += HandleDroppedItem;
        }
    }

    private void OnDisable()
    {
        if (pickup != null)
        {
            pickup.OnPickedUpItem -= HandlePickedUpItem;
            pickup.OnDroppedItem -= HandleDroppedItem;
        }
    }

    private void Update()
    {
        if (!StartMenu.GameStarted)
            return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        CleanupMissingRecruits();

        if (keyboard.rKey.wasPressedThisFrame)
        {
            TryRecruitNearbyAnt();
        }

        if (keyboard.qKey.wasPressedThisFrame)
        {
            DismissAll();
        }
    }

    public int GetRecruitCount()
    {
        CleanupMissingRecruits();
        return recruitedAnts.Count;
    }

    private void TryRecruitNearbyAnt()
    {
        if (recruitedAnts.Count >= maxRecruits)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, recruitRange, antLayer);

        AntNPC closestAnt = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            AntNPC ant = hit.GetComponentInParent<AntNPC>();

            if (ant == null)
                continue;

            if (recruitedAnts.Contains(ant))
                continue;

            // Only workers and soldiers should be recruitable.
            if (!ant.CanBeRecruited())
                continue;

            float distance = Vector2.Distance(transform.position, ant.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestAnt = ant;
            }
        }

        if (closestAnt == null)
            return;

        int slotIndex = recruitedAnts.Count;
        closestAnt.RecruitToPlayer(transform, slotIndex, followSpacing);
        recruitedAnts.Add(closestAnt);

        RefreshCarryHelp();
    }

    private void HandlePickedUpItem(IPickupable item)
    {
        RefreshCarryHelp();
    }

    private void HandleDroppedItem()
    {
        foreach (AntNPC ant in recruitedAnts)
        {
            if (ant != null)
                ant.StopHelpingCarry();
        }

        if (movement != null)
            movement.SetExternalSpeedMultiplier(1f);
    }

    private void RefreshCarryHelp()
    {
        CleanupMissingRecruits();

        if (pickup == null || pickup.HeldItem == null)
        {
            HandleDroppedItem();
            return;
        }

        Transform carriedTransform = pickup.HeldItem.GameObject.transform;

        int helperCount = 0;

        for (int i = 0; i < recruitedAnts.Count; i++)
        {
            AntNPC ant = recruitedAnts[i];

            if (ant == null)
                continue;

            ant.HelpCarry(carriedTransform, i);
            helperCount++;
        }

        float multiplier = 1f + helperCount * speedBonusPerHelper;
        multiplier = Mathf.Min(multiplier, maxSpeedMultiplier);

        if (movement != null)
            movement.SetExternalSpeedMultiplier(multiplier);
    }

    private void DismissAll()
    {
        foreach (AntNPC ant in recruitedAnts)
        {
            if (ant != null)
                ant.DismissRecruit();
        }

        recruitedAnts.Clear();

        if (movement != null)
            movement.SetExternalSpeedMultiplier(1f);
    }

    private void CleanupMissingRecruits()
    {
        for (int i = recruitedAnts.Count - 1; i >= 0; i--)
        {
            if (recruitedAnts[i] == null)
                recruitedAnts.RemoveAt(i);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, recruitRange);
    }
}
