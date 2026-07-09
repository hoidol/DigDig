using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public abstract class BlessingButton : ButtonUI
{
    [SerializeField] protected TMP_Text titleText;
    public abstract void UpdateButton();
}
