using UnityEngine; 

public interface IPickupable
{
    float Weight { get; }
    bool CanBePickedUp { get; }
    GameObject GameObject {get; }

    void OnPickup(UnityEngine.Transform holder);
    void OnDrop();
}