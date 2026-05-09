using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 벙커 구조물 - IronNestAbility로 소환되는 방어 구조물
public class Bunker : MonoBehaviour, IHittable
{
    public Transform Transform => transform;
    public Action onDestroyed;

    float curHp;
    float maxHp;
    [SerializeField] bool isMounted;
    float mountedTimer;
    const float DISMOUNT_DELAY = 0.5f;
    InvincibleEffect invincibleEffect;
    StatusEffectHandler playerStatusHandler;
    GameObject[] playerDirectChildren;
    [SerializeField] Image hpBar;
    [SerializeField] TMP_Text hpText;

    public void Spawn(Vector2 pos)
    {
        transform.position = pos;
        maxHp = Player.Instance.statMgr.MaxHp * 0.4f;
        curHp = maxHp;
        isMounted = false;
        playerStatusHandler = Player.Instance.GetComponentInChildren<StatusEffectHandler>();

        // Player 직속 자식 GameObjects 캐싱
        var playerTr = Player.Instance.transform;

        playerDirectChildren = new GameObject[2];
        playerDirectChildren[0] = playerTr.Find("Canvas").gameObject;
        playerDirectChildren[1] = playerTr.Find("Direction").gameObject;

        Debug.Log("Bunker Spawn()");
    }
    void Update()
    {
        hpBar.fillAmount = maxHp / maxHp;
        hpText.text = $"{(int)curHp}/{(int)maxHp}";
    }

    void LateUpdate()
    {
        if (!isMounted) return;

        mountedTimer += Time.deltaTime;

        // 탑승 후 0.2초 지나야 퇴장 가능
        if (mountedTimer >= DISMOUNT_DELAY && Player.Instance.MoveDirection.magnitude > 0.1f)
        {
            Dismount();
            return;
        }

        // 탑승 중 위치 고정
        Player.Instance.rg.linearVelocity = Vector2.zero;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isMounted) return;
        if (other.CompareTag("Player"))
        {
            Debug.Log("Bunker OnTriggerEnter2D");
            Mount();
        }

    }

    void Mount()
    {
        // Debug.Log("Bunker Mount()");
        isMounted = true;
        mountedTimer = 0f;

        // 플레이어 직속 자식 비활성화
        foreach (var child in playerDirectChildren)
            child.SetActive(false);
        Player.Instance.rg.position = transform.position;
        Player.Instance.GetComponent<Collider2D>().enabled = false;
        // 무적 상태 적용
        if (playerStatusHandler != null)
        {
            invincibleEffect = new InvincibleEffect();
            playerStatusHandler.Apply(invincibleEffect);
        }
    }

    void Dismount()
    {
        Debug.Log("Bunker Dismount()");
        isMounted = false;
        Player.Instance.GetComponent<Collider2D>().enabled = true;
        // 플레이어 직속 자식 재활성화
        foreach (var child in playerDirectChildren)
            child.SetActive(true);

        // 무적 해제
        invincibleEffect?.Deactivate();
        invincibleEffect = null;
    }

    public void TakeDamage(DamageData damageData)
    {
        if (!CanHit())
            return;
        curHp -= damageData.damage;
        damageData.Applyed(transform.position);

        PlayerTakeDamageText.SetText(transform.position + Vector3.up * 2
        + (Vector3)UnityEngine.Random.insideUnitCircle, $"-{((int)damageData.damage).ToString()}");
        curHp -= damageData.damage;
        if (curHp <= 0)
            DestroyBunker();
    }

    public bool CanHit() => curHp > 0;

    void DestroyBunker()
    {
        if (isMounted) Dismount();
        onDestroyed?.Invoke();
        Destroy(gameObject);
    }
}
