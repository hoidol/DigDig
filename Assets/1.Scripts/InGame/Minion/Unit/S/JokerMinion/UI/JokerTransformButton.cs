using UnityEngine;

// 조커 변신 버튼: 선택된 타일의 유닛이 조커일 때만 보임. 누르면 JokerSelectCanvas
// 이벤트 구독은 Awake/OnDestroy (꺼져 있어도 선택 이벤트를 받아야 함)
public class JokerTransformButton : ButtonUI
{
    JokerMinion joker;

    public override void Awake()
    {
        base.Awake();
        GameEventBus.Subscribe<SetTargetEvent>(OnSetTarget);
        GameEventBus.Subscribe<ReleaseTargetEvent>(OnReleaseTarget);
        SetJoker(null);
    }

    void OnDestroy()
    {
        GameEventBus.Unsubscribe<SetTargetEvent>(OnSetTarget);
        GameEventBus.Unsubscribe<ReleaseTargetEvent>(OnReleaseTarget);
    }

    void OnSetTarget(SetTargetEvent e) => SetJoker(e.unit as JokerMinion);

    void OnReleaseTarget(ReleaseTargetEvent e)
    {
        if (ReferenceEquals(e.unit, joker))
            SetJoker(null);
    }

    void SetJoker(JokerMinion joker)
    {
        this.joker = joker;
        gameObject.SetActive(joker != null);
    }

    public override void OnClickedBtn()
    {
        if (joker == null)
            return;
        JokerSelectCanvas.Instance.Open(joker);
    }
}
