using UnityEngine;
public interface IPicker
{
    Transform Transform
    {
        get;
    }

    void PickUp(IPickable pickable);
}
