using Spine;
using Spine.Unity;
using UnityEngine;

namespace CreepyTools
{
    public class AnimationController : MonoBehaviour
    {
        [SerializeField]
        private Spine.Animation[] animations;

        [SerializeField]
        private SkeletonAnimation skeletonAnimation;

        public int AnimationCount { get { return animations.Length; } }

        public string SkeletonAnimationName { get { return skeletonAnimation.skeletonDataAsset.name; } }

        private void Start()
        {
            InitSkeletonAnimation();
        }

        private Spine.Animation GetAnimation(int index)
        {
            if (animations.Length <= 0)
            {
                Debug.LogError("no animations available");
                return null;
            }

            if (index < 0 || index > animations.Length)
            {
                Debug.LogError("invalid index");
                return null;
            }


            return animations[index];
        }

        public void SetAnimation(int index)
        {
            Spine.Animation animation = GetAnimation(index);

            if (animation != null)
            {
                skeletonAnimation.state.SetAnimation(0, animation, true);
            }
        }

        public string GetAnimationName()
        {
            return skeletonAnimation.AnimationName;
        }

        public void SetCharacter(SkeletonAnimation skeletonAnimation)
        {
            if (skeletonAnimation == null)
            {
                Debug.LogError("assign skeleton animation");
                return;
            }
            this.skeletonAnimation = skeletonAnimation;
            InitSkeletonAnimation();
        }

        private void InitSkeletonAnimation()
        {
            if (skeletonAnimation == null)
            {
                Debug.LogError("assign skeleton animation");
                return;
            }
            animations = skeletonAnimation.skeletonDataAsset.GetSkeletonData(true).Animations.Items;
        }
    }
}

