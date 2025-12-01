using System.Collections;
using UnityEngine;

namespace NamPhuThuy.SpineAdapter
{
    public static class SpineHelper
    {
        public static void ChangeSkin(Spine.Unity.SkeletonGraphic skeletonGraphic, string skinName)
        {
            if (skeletonGraphic == null || skeletonGraphic.Skeleton == null)
                return;
        
            var skeleton = skeletonGraphic.Skeleton;
        
            // Try to find the skin by name
            var newSkin = skeleton.Data.FindSkin(skinName);
            if (newSkin == null)
            {
                Debug.LogWarning($"Chest skin not found: {skinName}");
                return;
            }
        
            // Apply new skin
            skeleton.SetSkin(newSkin);
            skeleton.SetSlotsToSetupPose();
        
            // Optional: re\-apply current animation to refresh visuals
            var animState = skeletonGraphic.AnimationState;
            var current = animState.GetCurrent(0);
            if (current != null)
            {
                animState.SetAnimation(0, current.Animation.Name, current.Loop);
            }
        
            skeletonGraphic.Update(0);
            skeletonGraphic.LateUpdate();
        }
    }

}