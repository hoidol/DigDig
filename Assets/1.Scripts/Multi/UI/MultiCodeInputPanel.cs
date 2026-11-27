using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// (2026-11-27) 로비 [코드입력] 창 (BattleCanvas, 메뉴 Tools/Lobby/Add Multi Game Buttons): 7자리 숫자 → 입장 = 그 코드의 방으로
public class MultiCodeInputPanel : MonoBehaviour
{
    public TMP_InputField inputField;
    public Button enterButton;
    public Button closeButton;
    public TMP_Text errorText;

    void Awake()
    {
        if (enterButton != null)
            enterButton.onClick.AddListener(Enter);
        if (closeButton != null)
            closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        if (inputField != null)
        {
            inputField.characterLimit = MultiGame.CODE_LENGTH;
            inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
            inputField.onSubmit.AddListener(_ => Enter());
        }
    }

    public void Open()
    {
        gameObject.SetActive(true);
        if (inputField != null)
            inputField.text = "";
        if (errorText != null)
            errorText.text = "";
    }

    public void Enter()
    {
        string code = inputField != null ? inputField.text.Trim() : "";
        if (!MultiGame.IsValidCode(code))
        {
            if (errorText != null)
                errorText.text = string.Format(TranslateManager.GetText("Multi_CodeInvalid"), MultiGame.CODE_LENGTH);
            SoundManager.Play(SFXType.Fail_NotEnough);
            return;
        }
        gameObject.SetActive(false);
        MultiGameButton.ConfirmThen(() => MultiMatchManager.Instance.JoinByCode(code).Forget());
    }
}
