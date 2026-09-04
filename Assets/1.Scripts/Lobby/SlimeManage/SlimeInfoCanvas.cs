using System;
using UnityEngine;

namespace Lobby
{
    public class SlimeInfoCanvas : CanvasUI<SlimeInfoCanvas>
    {
        public SlimePanel slimePanel;

        public void OpenCanvas(SlimeData slimeData, Action closeCallback = null)
        {
            base.OpenCanvas(closeCallback);
            slimePanel.SetData(slimeData);
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
