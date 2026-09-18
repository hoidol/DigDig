using UnityEngine;
namespace Lobby
{
    public class Growth1SlimeEntryPanel : SlimeEntryPanel
    {
        public GameObject unownPanel;

        public override void UpdatePanel()
        {
            if(slimeData ==null)
            {
                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(true);

            unownPanel.SetActive(false);

            if(!userSlime.own)
            {                
                unownPanel.SetActive(true);
            }

        }
    }    
}
