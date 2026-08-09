using UnityEngine;

public class CharacterBody : MonoBehaviour
{
    CharacterPart[] characterParts;
    void Awake()
    {
        characterParts = GetComponentsInChildren<CharacterPart>();
    }
    void Start()
    {
        UpdateCharacter();
    }
    public void UpdateCharacter()
    {
        foreach (var part in characterParts)
        {
            part.UpdateCharacter();
        }
    }
}