using UnityEngine;
using System;
public abstract class AreaIndicator : MonoBehaviour
{
    public virtual void PlayIndicator(float sec, Action end)
    {

    }
    public virtual void PlayIndicator(float size, float sec, Action end)
    {

    }
    public abstract void StopIndicator();
}
