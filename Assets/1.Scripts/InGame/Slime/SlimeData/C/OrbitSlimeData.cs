using UnityEngine;


[CreateAssetMenu(fileName = "Orbit", menuName = "SlimeData/C/Orbit")]
public class OrbitSlimeData : EnhanceSlimeData
{
    public int[] orbitCount = { 1, 2, 3, 4, 5 };

    public float orbitRotationSpeed = 120;
    public float radius = 2;

}