using UnityEngine;

public class KingSlimeBody : MonoBehaviour
{
    KingSlimePart[] kingslimeParts;
    void Awake()
    {
        kingslimeParts = GetComponentsInChildren<KingSlimePart>();
    }
    void Start()
    {
        UpdateCharacter();
    }
    public void UpdateCharacter()
    {
        foreach (var part in kingslimeParts)
        {
            part.UpdateCharacter();
        }
    }
}