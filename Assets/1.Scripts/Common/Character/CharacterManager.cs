using UnityEngine;

public class CharacterManager : MonoSingleton<CharacterManager>
{
    CharacterData[] characterDatas;
    private void Awake() 
    {
        characterDatas = Resources.LoadAll<CharacterData>($"CharacterData");
    }
    
    public CharacterData GetCharacterData(CharacterName characterName)
    {
        for(int i = 0; i < characterDatas.Length; i++)
        {
            if(characterDatas[i].characterName == characterName)
            {
                return characterDatas[i];
            }
        }
        return null;
    } 
}
public enum CharacterName
{
    Lucky   
}