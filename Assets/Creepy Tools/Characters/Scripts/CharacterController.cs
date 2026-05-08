using Spine.Unity;
using UnityEngine;

namespace CreepyTools
{
    public class CharacterController : MonoBehaviour
    {
        [SerializeField]
        private SkeletonAnimation[] characters;

        [SerializeField]
        private AnimationController animationController;

        [SerializeField]
        private UIController uicontroller;

        private int currentCharacterIndex = 0;
        public int CharacterCount { get { return characters.Length; } }
        public int CurrentCharacterIndex { get { return currentCharacterIndex; } }

        public void SetNextCharacter()
        {
            characters[currentCharacterIndex].gameObject.SetActive(false);
            currentCharacterIndex++;

            if(currentCharacterIndex >= characters.Length)
            {
                currentCharacterIndex = 0;
            }

            characters[currentCharacterIndex].gameObject.SetActive(true);
            animationController.SetCharacter(characters[currentCharacterIndex]);
            uicontroller.Reset();
        }

        public void SetPreviousCharacter()
        {
            characters[currentCharacterIndex].gameObject.SetActive(false);
            currentCharacterIndex--;

            if (currentCharacterIndex < 0)
            {
                currentCharacterIndex = characters.Length - 1;
            }

            characters[currentCharacterIndex].gameObject.SetActive(true);
            animationController.SetCharacter(characters[currentCharacterIndex]);
            uicontroller.Reset();
        }
    }
}

