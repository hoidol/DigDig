using System;
using UnityEngine;

namespace Lobby
{
    public class MiniMeInfoCanvas : CanvasUI<MiniMeInfoCanvas>
    {
        public MiniMePanel miniMePanel;

        public void OpenCanvas(MiniMeData miniMeData, Action closeCallback = null)
        {
            base.OpenCanvas(closeCallback);
            miniMePanel.SetData(miniMeData);
            OpenCanvas();
        }

        public void OpenCanvas()
        {
            UpdateCanvas();
        }

        public void UpdateCanvas()
        {
            
        }

        public void OnClickedLeft()
        {
            
        }

        public void OnClickedRight()
        {
            
        }
    }
}
