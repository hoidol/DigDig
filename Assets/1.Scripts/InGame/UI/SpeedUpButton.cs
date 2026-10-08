using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpeedUpButton : ButtonUI
{
    public float[] speeds = {1,1.5f,2f};
    public TMP_Text speedText;
    int speedIndex = 0;
    void Start()
    {        
        SetSpeed(0);
    }

    void SetSpeed(int idx)
    {
        speedIndex = idx  % speeds.Length;

        Time.timeScale = speeds[speedIndex];
        speedText.text = $"X{speeds[speedIndex]}";
    }

    public override void OnClickedBtn()
    {        
        SetSpeed(speedIndex+1);
    }
    
}