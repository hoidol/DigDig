using UnityEngine;

[RequireComponent(typeof(DroneMovement))]
public class Drone : Ally
{
    public DroneMovement movement;

    public override void Spawn(Vector2 pos, int lv)
    {
        base.Spawn(pos, lv);
        movement.Spawn(pos);
    }


}