using UnityEngine;
using System;
public abstract class AttackIndicator : MonoBehaviour
{
    public abstract void PlayIndicator(Action end);
    public abstract void StopIndicator();
}
