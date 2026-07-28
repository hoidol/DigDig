using UnityEngine;
using TMPro;
//현재 얼마나 있는지
public class MemoryPiecePanel : MonoBehaviour 
{
    public TMP_Text memoryPieceText;
    void Start()
    {
        GameEventBus.Subscribe<ChangedMemoryPieceEvent>(OnChangedMemoryPieceEvent);
        UpdatePanel();
    }

    void OnChangedMemoryPieceEvent(ChangedMemoryPieceEvent e)
    {
        UpdatePanel();
    }
    void UpdatePanel()
    {
        memoryPieceText.text = UserManager.Instance.userData.memoryPieceCount.ToString();
    }
}
public class ChangedMemoryPieceEvent
{

    public int curValue; //현재량
    public int changeValue; //변화량
    public ChangedMemoryPieceEvent(int cV, int chV)
    {
        curValue = cV;
        changeValue = chV;
    }
}