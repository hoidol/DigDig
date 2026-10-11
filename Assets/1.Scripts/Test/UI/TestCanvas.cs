using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestCanvas : MonoBehaviour
{
    const int MAX_SPEED = 10;

    [Header("Speed")]
    public Button speedButton;
    public Button speedDownButton;
    public TMP_Text speedText;

    [Header("Gold")]
    public Button goldButton;
    public int goldAmount = 1000;

    [Header("Play Time")]
    public TMP_Text playTimeText;

    int speed = 1;
    float playTime = 0;
    float lastTimeScale = 1;

    void Awake()
    {
        // 테스트 모드가 아니면 오브젝트 자체를 끈다 (씬에서 활성 상태로 둬야 Awake가 호출됨)
        gameObject.SetActive(BuildSetting.Instance != null && BuildSetting.Instance.isTestMode);
    }

    void Start()
    {
        speedButton.onClick.AddListener(OnClickedSpeed);
        speedDownButton.onClick.AddListener(OnClickedSpeedDown);
        goldButton.onClick.AddListener(OnClickedGold);
        SetSpeed(1);
    }

    void Update()
    {
        // 다른 UI가 timeScale 0 → 1로 복구하면 테스트 배속을 다시 적용
        if (lastTimeScale == 0 && Time.timeScale > 0)
            Time.timeScale = speed;
        lastTimeScale = Time.timeScale;

        // scaled deltaTime 사용 → timeScale 0이면 정지, 배속이면 빠르게 흐름
        playTime += Time.deltaTime;

        int minutes = (int)(playTime / 60);
        float seconds = playTime % 60;
        playTimeText.text = $"{minutes:00}:{seconds:00.0} (x{Time.timeScale})";
    }

    void SetSpeed(int value)
    {
        speed = value > MAX_SPEED ? 1 : Mathf.Max(1, value);
        Time.timeScale = speed;
        lastTimeScale = speed;
        speedText.text = $"X{speed}";
    }

    void OnClickedSpeed()
    {
        SetSpeed(speed + 1);
    }

    void OnClickedSpeedDown()
    {
        SetSpeed(speed - 1);
    }

    void OnClickedGold()
    {
        Character.Instance.AddCoin(goldAmount);
    }
}
