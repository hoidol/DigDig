using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CreepyTools
{
    public class UIController : MonoBehaviour
    {
        [SerializeField]
        private AnimationController animationController;

        [SerializeField]
        private CharacterController characterController;

        [SerializeField]
        private int currentIndex = 0;

        [SerializeField]
        private Text characterText;

        [SerializeField]
        private Text charactersCountText;

        [SerializeField]
        private Text animationNameText;

        [SerializeField]
        private Text skeletonAnimationNameText;

        [SerializeField] 
        private Text animationCountText;


        private void Start()
        {
            if (animationController == null)
            {
                Debug.LogError("animation controller is missing");
                return;
            }

            StartCoroutine(PlayDefaultAnimation());
        }

        private IEnumerator PlayDefaultAnimation()
        {
            yield return new WaitForSeconds(0.1f);
            animationController.SetAnimation(0);
            UpdateTexts();
        }

        public void NextAnimation()
        {
            currentIndex++;
            if (currentIndex >= animationController.AnimationCount)
            {
                currentIndex = 0;
            }

            animationController.SetAnimation(currentIndex);
            UpdateTexts();
        }

        public void PreviousAnimation()
        {
            currentIndex--;
            if (currentIndex < 0)
            {
                currentIndex = animationController.AnimationCount - 1;
            }

            animationController.SetAnimation(currentIndex);
            UpdateTexts();
        }

        private void UpdateTexts()
        {
            animationNameText.text = animationController.GetAnimationName();
            skeletonAnimationNameText.text = animationController.SkeletonAnimationName;
            if(characterText != null)
            {
                characterText.text = animationController.SkeletonAnimationName;
            }

            animationCountText.text = (currentIndex + 1) + " / " + (animationController.AnimationCount);

            if (charactersCountText != null)
            {
                charactersCountText.text = (characterController.CurrentCharacterIndex + 1) + " / " + (characterController.CharacterCount);
            }
        }

        public void Reset()
        {
            currentIndex = 0;

            animationController.SetAnimation(currentIndex);
            UpdateTexts();
        }
    }
}
