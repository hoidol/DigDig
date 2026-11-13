using UnityEngine;
using UnityEngine.UI;

namespace Lobby
{
    public class DrawMinionPanel : MinionPanel 
    {
        public GameObject lengendHeightObject;
        public Image gradeBgImage;
        
        public override void SetData(MinionData uMData)
        {
            base.SetData(uMData);
            int gradeIdx =(int)uMData.grade;
            int sIndex = (int)GradeType.S;
            lengendHeightObject.SetActive(gradeIdx >= sIndex);
            gradeBgImage.color = Grade.GetGradeColor(uMData.grade);
            
            
        }
    }    
}
