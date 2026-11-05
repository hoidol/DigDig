using System;
using UnityEngine;
namespace Lobby
{
    public abstract class DrawResultCanvas<T> : CanvasUI<T>  where T : MonoBehaviour
{
    public virtual void OpenCanvas(string[] pickedKeys, Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        
    }
}
}
