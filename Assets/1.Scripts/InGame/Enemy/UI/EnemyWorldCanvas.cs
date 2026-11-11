using System.Collections.Generic;
using UnityEngine;

// 적 머리 위 UI(체력바)를 모아 두는 캔버스. 하위에 둔 EnemyHpUI 하나를 템플릿으로 적마다 복사(풀링)
// Enemy.Spawn → Attach, 체력 변화 → Refresh, 죽음(Dead 시작)·경로 끝·비활성화 → Release
public class EnemyWorldCanvas : MonoSingleton<EnemyWorldCanvas>
{
    public EnemyHpUI hpUITemplate; // 비워 두면 하위의 첫 EnemyHpUI (프리팹 에셋을 넣어도 됨, 복사본은 항상 이 캔버스 하위)

    readonly Stack<EnemyHpUI> pool = new();
    readonly Dictionary<Enemy, EnemyHpUI> active = new();

    void Awake()
    {
        if (hpUITemplate == null)
            hpUITemplate = GetComponentInChildren<EnemyHpUI>(true);
        if (hpUITemplate != null && hpUITemplate.gameObject.scene.IsValid()) // 씬에 둔 템플릿만 숨김 (프리팹 에셋은 건드리지 않음)
            hpUITemplate.gameObject.SetActive(false);
        else
            Debug.LogWarning("EnemyWorldCanvas 하위에 EnemyHpUI 템플릿 없음 → 적 체력바 표시 안 함");
    }

    public void Attach(Enemy enemy)
    {
        if (enemy == null || hpUITemplate == null)
            return;
        if (!active.TryGetValue(enemy, out EnemyHpUI ui))
        {
            // UI 요소라 항상 이 캔버스 하위에 생성 (템플릿이 프리팹 에셋이거나 다른 곳에 있어도 동일)
            ui = pool.Count > 0 ? pool.Pop() : Instantiate(hpUITemplate, transform, false);
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
        if (enemy == null || !active.Remove(enemy, out EnemyHpUI ui))
            return;
        if (ui == null)
            return;
        ui.Unbind();
        pool.Push(ui);
    }

    public bool HasHpUI(Enemy enemy) => enemy != null && active.ContainsKey(enemy);
}
