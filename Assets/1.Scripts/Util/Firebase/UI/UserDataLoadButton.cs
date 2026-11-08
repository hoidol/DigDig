using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

// 서버(Firebase RTDB)의 유저 데이터로 로컬 데이터를 덮어씀. 성공하면 UI 갱신을 위해 현재 씬을 다시 로드
public class UserDataLoadButton : ButtonUI
{
    public override void OnClickedBtn()
    {
        LoadAsync().Forget();
    }

    async UniTaskVoid LoadAsync()
    {
        BlockCanvas.Instance.OpenCanvas(TranslateManager.GetText("Loading"));
        button.interactable = false;
        bool success = await FirebaseManager.Instance.firebaseDB.LoadUserDataAsync();
        if (this == null)
            return;
        button.interactable = true;
        BlockCanvas.Instance.CloseCanvas();
        if (!success)
        {
            ToastCanvas.Toast(TranslateManager.GetText("Load_Failed"));
            return;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
