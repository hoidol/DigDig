using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveEnemyInfoPanel : MonoBehaviour
{
    public GameObject obj;
    public DefenceTypePanel defenceTypePanel;
    public TMP_Text waveText;
    // public TMP_Text defenceTypeDesc;
    // public Image thumImage;
    
    void Start()
    {
        obj.SetActive(false);  
        GameEventBus.Subscribe<WaveStartEvent>(OnWaveStartEvent);

    }
    const float SHOW_TIME = 5f;
    Coroutine showCo;

    void OnWaveStartEvent(WaveStartEvent e)
    {
        SetWaveInfo(e.waveIdx);
        if (showCo != null)
            StopCoroutine(showCo);
        showCo = StartCoroutine(ShowCo());
    }

    // obj를 SHOW_TIME초 동안 보여준 뒤 숨김 (도중에 다시 호출되면 처음부터 다시 셈)
    IEnumerator ShowCo()
    {
        obj.SetActive(true);
        yield return new WaitForSeconds(SHOW_TIME);
        obj.SetActive(false);
        showCo = null;
    }

    public void SetWaveInfo(int w)
    {        
        waveText.text = $"Wave {w} 적 정보";
        defenceTypePanel.SetDefenceType(GameManager.Instance.GetWave(w).waveDefenceType);    
    }
}
