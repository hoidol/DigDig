using UnityEngine;

public class CharacterBody : MonoBehaviour
{
    CharacterPart[] characterParts;
    void Awake()
    {
        characterParts = GetComponentsInChildren<CharacterPart>();
    }
    public void UpdateCharacter()
    {
        foreach (var part in characterParts)
        {
            part.UpdateCharacter();
        }
    }
}