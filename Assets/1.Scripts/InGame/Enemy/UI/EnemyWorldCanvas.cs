using System.Collections.Generic;
using UnityEngine;

// 적 머리 위 UI(체력바)를 모아 두는 캔버스. 하위에 둔 EnemyHpUI 템플릿을 적마다 복사(풀링)
// 템플릿 2종: 일반 적 = hpUITemplate / 엘리트·보너스 엘리트·보스 = bigHpUITemplate(더 크고 체력 글씨 표시, 셋이 같은 크기)
// Enemy.Spawn → Attach, 체력 변화 → Refresh, 죽음(Dead 시작)·경로 끝·비활성화 → Release
public class EnemyWorldCanvas : MonoSingleton<EnemyWorldCanvas>
{
    public EnemyHpUI hpUITemplate;    // 비워 두면 하위의 첫 EnemyHpUI (프리팹 에셋을 넣어도 됨, 복사본은 항상 이 캔버스 하위)
    public EnemyHpUI bigHpUITemplate; // 엘리트·보너스 엘리트·보스용. 비워 두면 일반 템플릿 사용

    readonly Stack<EnemyHpUI> pool = new();
    readonly Stack<EnemyHpUI> bigPool = new();
    readonly Dictionary<Enemy, EnemyHpUI> active = new();
    readonly HashSet<EnemyHpUI> bigUIs = new();

    void Awake()
    {
        if (hpUITemplate == null)
            hpUITemplate = GetComponentInChildren<EnemyHpUI>(true);
        if (hpUITemplate == null)
            Debug.LogWarning("EnemyWorldCanvas 하위에 EnemyHpUI 템플릿 없음 → 적 체력바 표시 안 함");
        HideTemplate(hpUITemplate);
        HideTemplate(bigHpUITemplate);
    }

    // 씬에 둔 템플릿만 숨김 (프리팹 에셋은 건드리지 않음)
    static void HideTemplate(EnemyHpUI template)
    {
        if (template != null && template.gameObject.scene.IsValid())
            template.gameObject.SetActive(false);
    }

    public void Attach(Enemy enemy)
    {
        if (enemy == null)
            return;
        bool big = enemy.UseBigHpUI && bigHpUITemplate != null;
        EnemyHpUI template = big ? bigHpUITemplate : hpUITemplate;
        if (template == null)
            return;
        if (active.TryGetValue(enemy, out EnemyHpUI ui) && bigUIs.Contains(ui) != big)
            Release(enemy); // 같은 적인데 종류가 다른 체력바가 붙어 있음
        if (!active.TryGetValue(enemy, out ui))
        {
            Stack<EnemyHpUI> p = big ? bigPool : pool;
            // UI 요소라 항상 이 캔버스 하위에 생성 (템플릿이 프리팹 에셋이거나 다른 곳에 있어도 동일)
            ui = p.Count > 0 ? p.Pop() : Instantiate(template, transform, false);
            if (big)
                bigUIs.Add(ui);
            active[enemy] = ui;
        }
        ui.Bind(enemy);
    }

    public void Refresh(Enemy enemy)
    {
        if (enemy != null && active.TryGetValue(enemy, out EnemyHpUI ui))
            ui.Refresh();
    }

    public void Release(Enemy enemy)
    {
        if (ReferenceEquals(enemy, null) || !active.Remove(enemy, out EnemyHpUI ui))
            return;
        if (ui == null)
            return;
        ui.Unbind();
        (bigUIs.Contains(ui) ? bigPool : pool).Push(ui);
    }

    public bool HasHpUI(Enemy enemy) => enemy != null && active.ContainsKey(enemy);
    public bool HasBigHpUI(Enemy enemy) => enemy != null && active.TryGetValue(enemy, out EnemyHpUI ui) && bigUIs.Contains(ui);
}
