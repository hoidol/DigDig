using UnityEngine;
using TMPro;

public class BottomGameCanvas : MonoBehaviour
{
    public GameObject buttonObject;
    public AllySettingPanel allySettingPanel;
    public TileItemInfoPanel tileItemInfoPanel;
    public GameObject hideGameObject; // allySettingPanel이 보이는 동안 숨김. 비우면 하위의 "HideGame"을 찾음
    void Start()
    {
        if (hideGameObject == null)
            hideGameObject = FindChild("HideGame");

        GameEventBus.Subscribe<TileSelectedEvent>(OnTileSelectedEvent);
        GameEventBus.Subscribe<TileDeselectedEvent>(OnTileDeselectedEvent);
        buttonObject.SetActive(true);
        // 시작 시 allySettingPanel은 닫혀 있음(AllySettingPanel.Start가 끄지만 Start 순서가 보장되지 않아 직접 켬)
        if (hideGameObject != null)
            hideGameObject.SetActive(true);
    }

    GameObject FindChild(string childName)
    {
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
            if (t.name == childName)
                return t.gameObject;
        Debug.LogWarning($"[BottomGameCanvas] 하위에 {childName} 오브젝트가 없습니다.");
        return null;
    }

    // allySettingPanel이 보이면 HideGame 끄고, 닫히면 다시 켬
    void UpdateHideGame()
    {
        if (hideGameObject == null)
            return;
        hideGameObject.SetActive(!allySettingPanel.obj.activeSelf);
    }

    // 타일 선택 시 위 유닛 정보 + 타일(아이템) 정보. 빈 타일(유닛·아이템 없음)은 반응 없음
    void OnTileSelectedEvent(TileSelectedEvent e)
    {
        if (e.tile.tileObject == null && e.tile.tileItem == null)
        {
            OnTileDeselectedEvent(null);
            return;
        }

        buttonObject.SetActive(false);
        allySettingPanel.SetMinion(e.tile.tileObject as IAllyUnit);
        tileItemInfoPanel.SetTile(e.tile);
        UpdateHideGame();
    }

    void OnTileDeselectedEvent(TileDeselectedEvent e)
    {
        buttonObject.SetActive(true);
        allySettingPanel.SetMinion(null);
        tileItemInfoPanel.SetTile(null);
        UpdateHideGame();
    }

}
