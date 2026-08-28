using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SelectMergeMiniMeCanvas : CanvasUI<SelectMergeMiniMeCanvas>
{
    public SelectMergeMiniMePanel[] selectMergeMiniMePanels;

    UniTaskCompletionSource<string> tcs;

    public async UniTask<string> OpenCanvas(MiniMe miniMe1, MiniMe miniMe2)
    {
        List<MiniMeMergeData> canMakeMiniMes = GetCanMakeMiniMes(miniMe1, miniMe2);
        if (canMakeMiniMes.Count == 0)
        {
            Debug.Log("조합할 수 있는 미니미 없음");
            return null;
        }

        base.OpenCanvas();

        tcs = new UniTaskCompletionSource<string>();
        for (int i = 0; i < selectMergeMiniMePanels.Length; i++)
        {
            if (i < canMakeMiniMes.Count)
                selectMergeMiniMePanels[i].Set(canMakeMiniMes[i], OnPanelClicked);
            else
                selectMergeMiniMePanels[i].Hide();
        }

        string pickedMiniMeKey = await tcs.Task;

        foreach (SelectMergeMiniMePanel panel in selectMergeMiniMePanels)
            panel.Hide();

        CloseCanvas();
        return pickedMiniMeKey;
    }

    void OnPanelClicked(string key)
    {
        tcs?.TrySetResult(key);
    }

    List<MiniMeMergeData> GetCanMakeMiniMes(MiniMe miniMe1, MiniMe miniMe2)
    {
        //총 3가지
        //1. miniMe1 + miniMe2 조합 전용 특수 2단계 미니미 (growth1MiniMeKeys에 둘 다 포함)
        //2. miniMe1의 일반 2단계 형태 (growth1MiniMeKeys가 miniMe1.key 하나뿐)
        //3. miniMe2의 일반 2단계 형태 (growth1MiniMeKeys가 miniMe2.key 하나뿐)

        List<MiniMeMergeData> canMakeMiniMes = new List<MiniMeMergeData>();
        MiniMeMergeData[] conditionDatas = MiniMeManager.Instance.miniMeMergeDatas;
        for (int i = 0; i < conditionDatas.Length; i++)
        {
            string[] requireKeys = conditionDatas[i].growth1MiniMeKeys;
            if (requireKeys.Length == 1)
            {
                if (requireKeys[0] == miniMe1.key || requireKeys[0] == miniMe2.key)
                    canMakeMiniMes.Add(conditionDatas[i]);
            }
            else if (requireKeys.Contains(miniMe1.key) && requireKeys.Contains(miniMe2.key))
            {
                canMakeMiniMes.Add(conditionDatas[i]);
            }
        }
        return canMakeMiniMes;
    }
}
