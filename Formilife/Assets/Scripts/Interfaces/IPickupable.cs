public interface IPickupable
{
    float Weight { get; }
    bool CanBePickedUp { get; }

    void OnPickup(UnityEngine.Transform holder);
    void OnDrop();
}