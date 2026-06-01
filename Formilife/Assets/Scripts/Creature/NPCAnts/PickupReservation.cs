using UnityEngine;

public class PickupReservation : MonoBehaviour
{
    [SerializeField] private float reservationTimeout = 5f;

    private AntNPC reservedBy;
    private float reservedAt;

    public bool IsReserved => reservedBy != null;

    public bool IsReservedByAnother(AntNPC ant)
    {
        ClearIfExpired();
        return reservedBy != null && reservedBy != ant;
    }

    public bool TryReserve(AntNPC ant)
    {
        ClearIfExpired();

        if (ant == null) return false;
        if (reservedBy != null && reservedBy != ant) return false;

        reservedBy = ant;
        reservedAt = Time.time;
        return true;
    }

    public void Release(AntNPC ant)
    {
        if (reservedBy == ant)
            reservedBy = null;
    }

    public void ForceRelease()
    {
        reservedBy = null;
    }

    private void ClearIfExpired()
    {
        if (reservedBy != null && Time.time - reservedAt > reservationTimeout)
            reservedBy = null;
    }
}