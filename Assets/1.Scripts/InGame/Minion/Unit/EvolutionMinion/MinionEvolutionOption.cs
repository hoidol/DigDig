using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 진화 갈래 선택 창(MinionEvolutionCanvas)의 갈래 칸 하나: 이름 + 설명. 누르면 그 갈래로 진화
public class MinionEvolutionOption : MonoBehaviour
{
    public Button button;
    public TMP_Text nameText;
    public TMP_Text descText;

    MinionEvolutionCanvas canvas;
    MinionEvolutionData branch;

    void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
    }

    void Start()
    {
        if (button != null)
            button.onClick.AddListener(() => canvas?.Select(branch));
    }

    public void Set(MinionEvolutionCanvas owner, MinionEvolutionData data)
    {
        canvas = owner;
        branch = data;
        if (nameText != null)
            nameText.text = data.Title;
        if (descText != null)
            descText.text = data.Desc;
    }
}
