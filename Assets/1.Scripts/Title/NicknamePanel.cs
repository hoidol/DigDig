using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// (2026-11-27) 닉네임 입력 창 (Title 씬): 2~10자 · 금칙어 X · 중복 X(서버 NicknameRegistry 선점) → UserMyInfoManager.SetNickname
public class NicknamePanel : MonoBehaviour
{
    public TMP_InputField inputField;
    public Button confirmButton;
    public TMP_Text errorText; // 규칙 안내·오류 (선택)

    void Awake()
    {
        if (confirmButton != null)
            confirmButton.onClick.AddListener(Confirm);
        if (inputField != null)
        {
            inputField.characterLimit = UserMyInfoManager.NICKNAME_MAX;
            inputField.onValueChanged.AddListener(_ => ShowHint());
            inputField.onSubmit.AddListener(_ => Confirm());
        }
    }

    public void Open()
    {
        gameObject.SetActive(true);
        if (inputField != null)
            inputField.text = "";
        ShowHint();
    }

    void ShowHint()
    {
        if (errorText != null)
        {
            errorText.color = new Color(0.8f, 0.82f, 0.9f);
            errorText.text = string.Format(TranslateManager.GetText("Nickname_Hint"), UserMyInfoManager.NICKNAME_MIN, UserMyInfoManager.NICKNAME_MAX);
        }
    }

    bool checking;

    // 규칙 확인 → 서버 중복 확인(선점) → 저장. 확인하는 동안 버튼 잠금
    public void Confirm() => ConfirmAsync().Forget();

    public async UniTask ConfirmAsync()
    {
        if (checking)
            return;
        string raw = inputField != null ? inputField.text : "";
        string error = UserMyInfoManager.ValidateNickname(raw, out string nickname);
        if (error != null)
        {
            ShowError(error);
            return;
        }
        checking = true;
        if (confirmButton != null)
            confirmButton.interactable = false;
        NicknameRegistry.Result result;
        try
        {
            result = await NicknameRegistry.ClaimAsync(nickname, UserDataManager.Instance.userMyInfoManager.userMyInfoData.nickname);
        }
        finally
        {
            checking = false;
            if (this != null && confirmButton != null)
                confirmButton.interactable = true;
        }
        if (result != NicknameRegistry.Result.Ok)
        {
            ShowError(result == NicknameRegistry.Result.Taken ? "Nickname_Taken" : "Nickname_CheckFailed");
            return;
        }
        UserDataManager.Instance.userMyInfoManager.SetNickname(nickname);
        SoundManager.Play(SFXType.PurchaseComplete);
    }

    void ShowError(string key)
    {
        if (errorText != null)
        {
            errorText.color = new Color(1f, 0.45f, 0.45f);
            errorText.text = string.Format(TranslateManager.GetText(key), UserMyInfoManager.NICKNAME_MIN, UserMyInfoManager.NICKNAME_MAX);
        }
        SoundManager.Play(SFXType.Fail_NotEnough);
    }
}
