using UnityEngine;
using UnityEngine.UI;

namespace Lobby
{
    /// <summary>
    /// 슬라임 조합 프리셋 버튼. 누르면 그 프리셋 조합으로 장착(불러오기).
    /// 선택된 프리셋은 장착이 바뀔 때마다 자동 저장된다(UserSlimeManager.EquiptUserSlime).
    /// SlimeManageCanvas 하위에 두면 자동으로 잡힌다. presetIdx = 0~4 (0 = 1번, 기본).
    /// </summary>
    public class SlimePresetButton : ButtonUI
    {
        public int presetIdx;
        public Image bgImage;
        public Text numberText;

        public void UpdateButton()
        {
            if (numberText != null)
                numberText.text = (presetIdx + 1).ToString();

            if (bgImage != null)
            {
                bool selected = UserDataManager.Instance.userSlimeManager.SelectedPresetIdx == presetIdx;
                bgImage.color = selected ? ColorSetting.activeColor : Color.gray;
            }
        }

        public override void OnClickedBtn()
        {
            UserDataManager.Instance.userSlimeManager.LoadPreset(presetIdx);
            LobbyManager.Instance.GetLobbyCanvas(LobbyState.Slime).UpdateCanvas();
        }
    }
}
