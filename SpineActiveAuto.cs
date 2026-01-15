using UnityEngine;
using Spine.Unity;
using PrimeTween;

namespace NamPhuThuy.SpineAdapter
{
    public class SpineActiveAuto : MonoBehaviour
    {
        public enum ActiveMethod
        {
            NONE = 0,
            AWAKE = 1,
            ON_ENABLE = 2,
            START = 3,
            MANUAL = 4
        }

        public enum ActiveMode
        {
            NONE = 0,
            PLAY_ANIMATION = 1,
            FADE_IN = 2,
            FADE_OUT = 3,
            APPEAR_THEN_IDLE = 4,
        }

        [Header("Settings")]
        [SerializeField] private ActiveMethod activeMethod = ActiveMethod.ON_ENABLE;
        [SerializeField] private ActiveMode activeMode = ActiveMode.PLAY_ANIMATION;
        [SerializeField] private float delay = 0f;

        [Header("Targets")]
        [SerializeField] private SkeletonGraphic skeletonGraphic;
        [SerializeField] private SkeletonAnimation skeletonAnimation;

        [Header("Animation Config")]
        [SerializeField] private string animationName = "animation";
        [SerializeField] private string idleAnimationName = "idle";
        [SerializeField] private bool loop = true;
        [SerializeField] private float timeScale = 1f;

        [Header("Fade Config")]
        [SerializeField] private float fadeDuration = 0.5f;

        private void Awake()
        {
            if (activeMethod == ActiveMethod.AWAKE) Execute();
        }

        private void OnEnable()
        {
            if (activeMethod == ActiveMethod.ON_ENABLE) Execute();
        }

        private void Start()
        {
            if (activeMethod == ActiveMethod.START) Execute();
        }

        public void Execute()
        {
            if (activeMode == ActiveMode.NONE) return;

            if (skeletonGraphic != null)
            {
                ProcessActive(skeletonGraphic);
            }
            else if (skeletonAnimation != null)
            {
                ProcessActive(skeletonAnimation);
            }
        }

        private void ProcessActive(object spineObj)
        {
            bool isUI = spineObj is SkeletonGraphic;
            
            switch (activeMode)
            {
                case ActiveMode.PLAY_ANIMATION:
                    if (isUI) SpineHelper.PlayAnimation((SkeletonGraphic)spineObj, animationName, loop, timeScale);
                    else SpineHelper.PlayAnimation((SkeletonAnimation)spineObj, animationName, loop, timeScale);
                    break;
                case ActiveMode.FADE_IN:
                    if (isUI) SpineHelper.Fade((SkeletonGraphic)spineObj, 1f, fadeDuration, delay);
                    else SpineHelper.Fade((SkeletonAnimation)spineObj, 1f, fadeDuration, delay);
                    break;
                case ActiveMode.FADE_OUT:
                    if (isUI) SpineHelper.Fade((SkeletonGraphic)spineObj, 0f, fadeDuration, delay);
                    else SpineHelper.Fade((SkeletonAnimation)spineObj, 0f, fadeDuration, delay);
                    break;
                case ActiveMode.APPEAR_THEN_IDLE:
                    if (isUI) SpineHelper.PlayAppearThenLoop((SkeletonGraphic)spineObj, animationName, idleAnimationName);
                    else SpineHelper.PlayAppearThenLoop((SkeletonAnimation)spineObj, animationName, idleAnimationName);
                    break;
            }
        }
    }
}