using DG.Tweening;
using UnityEngine;

// 선택된 타일 위치를 보여주는 표시 오브젝트. 입력은 받지 않고 선택 이벤트만 따라감 (선택 판정은 SlimeSpawner)
public class TilePointer : MonoSingleton<TilePointer>
{
    public GameObject obj; // 실제 표시 오브젝트 (자기 자신을 끄면 구독이 안 되므로 자식을 켜고 끔)

    void Start()
    {
        obj.SetActive(false);
        GameEventBus.Subscribe<TileSelectedEvent>(OnTileSelected);
        GameEventBus.Subscribe<TileDeselectedEvent>(_ => Hide());
        GameEventBus.Subscribe<DragStartEvent>(_ => Hide()); // 드래그 중엔 숨기고 놓은 뒤 다시 선택될 때 표시
    }

    void OnTileSelected(TileSelectedEvent e)
    {
        // 빈 타일은 표시 안 함 (슬라임·캐릭터가 있거나 아이템이 설치된 타일만)
        if (e.tile.tileObject == null && e.tile.tileItem == null)
        {
            Hide();
            return;
        }

        bool moved = !obj.activeSelf || transform.position != e.tile.transform.position;
        transform.position = e.tile.transform.position;
        obj.SetActive(true);
        if (!moved)
            return; // 같은 타일 갱신(아이템 설치 등)은 연출 없음

        obj.transform.DOKill();
        obj.transform.localScale = Vector3.one;
        obj.transform.DOPunchScale(Vector3.one * 0.15f, 0.2f, 1);
    }

    void Hide()
    {
        obj.transform.DOKill();
        obj.SetActive(false);
    }
}
