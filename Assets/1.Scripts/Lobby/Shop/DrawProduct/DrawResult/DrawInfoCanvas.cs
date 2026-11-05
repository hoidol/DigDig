using System;
using System.Linq;
using UnityEngine;

// 뽑기 등급별 확률 표시. 열 때마다 패널을 풀에서 꺼내 재사용한다
public class DrawInfoCanvas : CanvasUI<DrawInfoCanvas>
{
   public DrawInfoPanel drawInfoPanelPrefab;
   public Transform content; // 패널을 붙일 부모 (비우면 이 캔버스)

   readonly PoolingSystem<DrawInfoPanel> drawInfoPanelPool = new();

   public void OpenCanvas(DrawSO drawSO, Action closeCallback = null)
   {
      base.OpenCanvas(closeCallback);

      drawInfoPanelPool.SetPrefab(drawInfoPanelPrefab);
      drawInfoPanelPool.ReturnAll();

      float total = drawSO.drawInfos.Sum(info => info.chance);
      foreach (DrawInfo drawInfo in drawSO.drawInfos)
      {
         DrawInfoPanel drawInfoPanel = drawInfoPanelPool.GetObject<DrawInfoPanel>(content != null ? content : transform);
         drawInfoPanel.transform.localScale = Vector3.one; // PoolingSystem은 월드 기준으로 붙여서 캔버스 스케일이 섞임
         drawInfoPanel.transform.SetAsLastSibling(); // 재사용한 패널도 drawInfos 순서대로
         float percent = total > 0 ? drawInfo.chance / total * 100f : 0f;
         drawInfoPanel.SetInfo(drawInfo.grade, percent.ToString("0.##"));
      }
   }
}
