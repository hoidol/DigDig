using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectWavePanel : MonoBehaviour
{
    public Image directionImage;
    public Image thumImage;
    public TMP_Text waveNameText;
    WaveType waveType;
    WaveWayDirection waveWayDirection;
    Action<WaveType, WaveWayDirection> onClicked;

    public void Set(WaveType waveType, WaveWayDirection waveWayDirection, Action<WaveType, WaveWayDirection> onClicked)
    {
        this.waveType = waveType;
        this.waveWayDirection = waveWayDirection;
        this.onClicked = onClicked;
        
        waveNameText.text = waveType.ToString();
        // directionImage 왼쪽이 45 오른쪽이면 -45 앞이면 0
        directionImage.rectTransform.localRotation = Quaternion.Euler(new Vector3(0,0,0));
        switch (waveWayDirection)
        {
            case WaveWayDirection.Left:
            directionImage.rectTransform.localRotation = Quaternion.Euler(new Vector3(0,0,45));
            break;
            case WaveWayDirection.Right:
            directionImage.rectTransform.localRotation = Quaternion.Euler(new Vector3(0,0,-45));
            break;
        }
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void OnClickedPanel()
    {
        onClicked?.Invoke(waveType, waveWayDirection);
    }
}
