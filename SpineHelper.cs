using System;
using System.Collections;
using Spine.Unity;
using Spine;
using UnityEngine;
using PrimeTween;

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
        
            // Optional: re-apply current animation to refresh visuals
            var animState = skeletonGraphic.AnimationState;
            var current = animState.GetCurrent(0);
            if (current != null)
            {
                animState.SetAnimation(0, current.Animation.Name, current.Loop);
            }
        
            skeletonGraphic.Update(0);
            skeletonGraphic.LateUpdate();
        }
        
        #region PLAY ANIMATION

        /// <summary>
        /// Plays an animation for UI Spine (SkeletonGraphic)
        /// </summary>
        public static void PlayAnimation(SkeletonGraphic skeletonGraphic, string animationName, bool loop = true, float timeScale = 1f)
        {
            if (skeletonGraphic == null || skeletonGraphic.AnimationState == null)
                return;

            // Optional: Check if animation exists to prevent errors
            if (skeletonGraphic.Skeleton.Data.FindAnimation(animationName) == null)
            {
                Debug.LogWarning($"[SpineHelper] Animation '{animationName}' not found on {skeletonGraphic.name}");
                return;
            }

            skeletonGraphic.timeScale = timeScale;
            skeletonGraphic.AnimationState.SetAnimation(0, animationName, loop);
        }

        /// <summary>
        /// Plays an animation for World Spine (SkeletonAnimation)
        /// </summary>
        public static void PlayAnimation(SkeletonAnimation skeletonAnimation, string animationName, bool loop = true, float timeScale = 1f)
        {
            if (skeletonAnimation == null || skeletonAnimation.AnimationState == null)
                return;

            // Optional: Check if animation exists to prevent errors
            if (skeletonAnimation.Skeleton.Data.FindAnimation(animationName) == null)
            {
                Debug.LogWarning($"[SpineHelper] Animation '{animationName}' not found on {skeletonAnimation.name}");
                return;
            }

            skeletonAnimation.timeScale = timeScale;
            skeletonAnimation.AnimationState.SetAnimation(0, animationName, loop);
        }

        #endregion

        #region CHAIN ANIMATIONS

        /// <summary>
        /// Play idle animation (no loop), then immediately play main animation (with loop)
        /// SkeletonGraphic version
        /// </summary>
        /// <param name="skeletonGraphic">The skeleton graphic</param>
        /// <param name="appearAnimName">Idle animation name (plays once)</param>
        /// <param name="idleAnimName">Main animation name (loops)</param>
        /// <param name="timeScale">Animation time scale</param>
        public static void PlayAppearThenLoop(SkeletonGraphic skeletonGraphic, string appearAnimName, string idleAnimName, float timeScale = 1f)
        {
            if (skeletonGraphic == null || skeletonGraphic.AnimationState == null)
                return;

            // Validate animations exist
            if (skeletonGraphic.Skeleton.Data.FindAnimation(appearAnimName) == null)
            {
                Debug.LogWarning($"[SpineHelper] Idle animation '{appearAnimName}' not found on {skeletonGraphic.name}");
                return;
            }

            if (skeletonGraphic.Skeleton.Data.FindAnimation(idleAnimName) == null)
            {
                Debug.LogWarning($"[SpineHelper] Main animation '{idleAnimName}' not found on {skeletonGraphic.name}");
                return;
            }

            skeletonGraphic.timeScale = timeScale;

            // Play idle animation (no loop)
            TrackEntry idleTrack = skeletonGraphic.AnimationState.SetAnimation(0, appearAnimName, false);

            // Queue main animation to play immediately after idle completes (with loop)
            skeletonGraphic.AnimationState.AddAnimation(0, idleAnimName, true, 0f);
        }

        /// <summary>
        /// Play idle animation (no loop), then immediately play main animation (with loop)
        /// SkeletonAnimation version
        /// </summary>
        public static void PlayAppearThenLoop(SkeletonAnimation skeletonAnimation, string appearAnimName, string idleAnimName, float timeScale = 1f)
        {
            if (skeletonAnimation == null || skeletonAnimation.AnimationState == null)
                return;

            // Validate animations exist
            if (skeletonAnimation.Skeleton.Data.FindAnimation(appearAnimName) == null)
            {
                Debug.LogWarning($"[SpineHelper] Idle animation '{appearAnimName}' not found on {skeletonAnimation.name}");
                return;
            }

            if (skeletonAnimation.Skeleton.Data.FindAnimation(idleAnimName) == null)
            {
                Debug.LogWarning($"[SpineHelper] Main animation '{idleAnimName}' not found on {skeletonAnimation.name}");
                return;
            }

            skeletonAnimation.timeScale = timeScale;

            // Play idle animation (no loop)
            TrackEntry idleTrack = skeletonAnimation.AnimationState.SetAnimation(0, appearAnimName, false);

            // Queue main animation to play immediately after idle completes (with loop)
            skeletonAnimation.AnimationState.AddAnimation(0, idleAnimName, true, 0f);
        }

        /// <summary>
        /// Play multiple animations in sequence (SkeletonGraphic)
        /// </summary>
        /// <param name="skeletonGraphic">The skeleton graphic</param>
        /// <param name="animations">Array of (animName, loop, delay) tuples</param>
        /// <param name="timeScale">Animation time scale</param>
        public static void PlayAnimationSequence(SkeletonGraphic skeletonGraphic, (string animName, bool loop, float delay)[] animations, float timeScale = 1f)
        {
            if (skeletonGraphic == null || skeletonGraphic.AnimationState == null || animations.Length == 0)
                return;

            skeletonGraphic.timeScale = timeScale;

            // Play first animation
            var firstAnim = animations[0];
            skeletonGraphic.AnimationState.SetAnimation(0, firstAnim.animName, firstAnim.loop);

            // Queue remaining animations
            for (int i = 1; i < animations.Length; i++)
            {
                var anim = animations[i];
                skeletonGraphic.AnimationState.AddAnimation(0, anim.animName, anim.loop, anim.delay);
            }
        }

        /// <summary>
        /// Play multiple animations in sequence (SkeletonAnimation)
        /// </summary>
        public static void PlayAnimationSequence(SkeletonAnimation skeletonAnimation, (string animName, bool loop, float delay)[] animations, float timeScale = 1f)
        {
            if (skeletonAnimation == null || skeletonAnimation.AnimationState == null || animations.Length == 0)
                return;

            skeletonAnimation.timeScale = timeScale;

            // Play first animation
            var firstAnim = animations[0];
            skeletonAnimation.AnimationState.SetAnimation(0, firstAnim.animName, firstAnim.loop);

            // Queue remaining animations
            for (int i = 1; i < animations.Length; i++)
            {
                var anim = animations[i];
                skeletonAnimation.AnimationState.AddAnimation(0, anim.animName, anim.loop, anim.delay);
            }
        }

        /// <summary>
        /// Play animation with callback when complete (SkeletonGraphic)
        /// </summary>
        public static TrackEntry PlayAnimationWithCallback(SkeletonGraphic skeletonGraphic, string animationName, bool loop, System.Action onComplete, float timeScale = 1f)
        {
            if (skeletonGraphic == null || skeletonGraphic.AnimationState == null)
                return null;

            if (skeletonGraphic.Skeleton.Data.FindAnimation(animationName) == null)
            {
                Debug.LogWarning($"[SpineHelper] Animation '{animationName}' not found on {skeletonGraphic.name}");
                return null;
            }

            skeletonGraphic.timeScale = timeScale;
            TrackEntry track = skeletonGraphic.AnimationState.SetAnimation(0, animationName, loop);

            if (onComplete != null && !loop)
            {
                track.Complete += (entry) => onComplete?.Invoke();
            }

            return track;
        }

        /// <summary>
        /// Play animation with callback when complete (SkeletonAnimation)
        /// </summary>
        public static TrackEntry PlayAnimationWithCallback(SkeletonAnimation skeletonAnimation, string animationName, bool loop, System.Action onComplete, float timeScale = 1f)
        {
            if (skeletonAnimation == null || skeletonAnimation.AnimationState == null)
                return null;

            if (skeletonAnimation.Skeleton.Data.FindAnimation(animationName) == null)
            {
                Debug.LogWarning($"[SpineHelper] Animation '{animationName}' not found on {skeletonAnimation.name}");
                return null;
            }

            skeletonAnimation.timeScale = timeScale;
            TrackEntry track = skeletonAnimation.AnimationState.SetAnimation(0, animationName, loop);

            if (onComplete != null && !loop)
            {
                track.Complete += (entry) => onComplete?.Invoke();
            }

            return track;
        }

        #endregion
        
        

        #region COLOR MANIPULATION

        /// <summary>
        /// Set color for entire skeleton (SkeletonGraphic)
        /// </summary>
        public static void SetColor(SkeletonGraphic skeletonGraphic, Color color)
        {
            if (skeletonGraphic == null || skeletonGraphic.Skeleton == null)
                return;

            skeletonGraphic.Skeleton.SetColor(color);
        }

        /// <summary>
        /// Set color for entire skeleton (SkeletonAnimation)
        /// </summary>
        public static void SetColor(SkeletonAnimation skeletonAnimation, Color color)
        {
            if (skeletonAnimation == null || skeletonAnimation.Skeleton == null)
                return;

            skeletonAnimation.Skeleton.SetColor(color);
        }

        /// <summary>
        /// Set alpha (transparency) only (SkeletonGraphic)
        /// </summary>
        public static void SetAlpha(SkeletonGraphic skeletonGraphic, float alpha)
        {
            if (skeletonGraphic == null || skeletonGraphic.Skeleton == null)
                return;

            var color = skeletonGraphic.Skeleton.GetColor();
            color.a = Mathf.Clamp01(alpha);
            skeletonGraphic.Skeleton.SetColor(color);
        }

        /// <summary>
        /// Set alpha (transparency) only (SkeletonAnimation)
        /// </summary>
        public static void SetAlpha(SkeletonAnimation skeletonAnimation, float alpha)
        {
            if (skeletonAnimation == null || skeletonAnimation.Skeleton == null)
                return;

            var color = skeletonAnimation.Skeleton.GetColor();
            color.a = Mathf.Clamp01(alpha);
            skeletonAnimation.Skeleton.SetColor(color);
        }

        /// <summary>
        /// PrimeTween implementation of Fade for SkeletonGraphic
        /// </summary>
        public static Tween Fade(SkeletonGraphic skeletonGraphic, float endAlpha, float duration, float delay = 0)
        {
            if (skeletonGraphic == null || skeletonGraphic.Skeleton == null)
                return default;

            float startAlpha = skeletonGraphic.Skeleton.A;
            return Tween.Custom(startAlpha, endAlpha, duration: duration, startDelay: delay, onValueChange: alpha =>
            {
                var color = skeletonGraphic.Skeleton.GetColor();
                color.a = alpha;
                skeletonGraphic.Skeleton.SetColor(color);
            });
        }

        /// <summary>
        /// PrimeTween implementation of Fade for SkeletonAnimation
        /// </summary>
        public static Tween Fade(SkeletonAnimation skeletonAnimation, float endAlpha, float duration, float delay = 0)
        {
            if (skeletonAnimation == null || skeletonAnimation.Skeleton == null)
                return default;

            float startAlpha = skeletonAnimation.Skeleton.A;
            return Tween.Custom(startAlpha, endAlpha, duration: duration, startDelay: delay, onValueChange: alpha =>
            {
                var color = skeletonAnimation.Skeleton.GetColor();
                color.a = alpha;
                skeletonAnimation.Skeleton.SetColor(color);
            });
        }

        /// <summary>
        /// Fade to color over time (SkeletonGraphic) - use with coroutine
        /// </summary>
        public static IEnumerator FadeToColor(SkeletonGraphic skeletonGraphic, Color targetColor, float duration)
        {
            if (skeletonGraphic == null || skeletonGraphic.Skeleton == null)
                yield break;

            Color startColor = skeletonGraphic.Skeleton.GetColor();
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                Color lerpedColor = Color.Lerp(startColor, targetColor, t);
                skeletonGraphic.Skeleton.SetColor(lerpedColor);
                yield return null;
            }

            skeletonGraphic.Skeleton.SetColor(targetColor);
        }

        /// <summary>
        /// Fade to color over time (SkeletonAnimation) - use with coroutine
        /// </summary>
        public static IEnumerator FadeToColor(SkeletonAnimation skeletonAnimation, Color targetColor, float duration)
        {
            if (skeletonAnimation == null || skeletonAnimation.Skeleton == null)
                yield break;

            Color startColor = skeletonAnimation.Skeleton.GetColor();
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                Color lerpedColor = Color.Lerp(startColor, targetColor, t);
                skeletonAnimation.Skeleton.SetColor(lerpedColor);
                yield return null;
            }

            skeletonAnimation.Skeleton.SetColor(targetColor);
        }

        /// <summary>
        /// Fade in (from transparent to visible) - SkeletonGraphic
        /// </summary>
        public static IEnumerator FadeIn(SkeletonGraphic skeletonGraphic, float duration)
        {
            if (skeletonGraphic == null || skeletonGraphic.Skeleton == null)
                yield break;

            Color color = skeletonGraphic.Skeleton.GetColor();
            color.a = 0f;
            skeletonGraphic.Skeleton.SetColor(color);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsed / duration);
                color.a = alpha;
                skeletonGraphic.Skeleton.SetColor(color);
                yield return null;
            }

            color.a = 1f;
            skeletonGraphic.Skeleton.SetColor(color);
        }

        /// <summary>
        /// Fade in (from transparent to visible) - SkeletonAnimation
        /// </summary>
        public static IEnumerator FadeIn(SkeletonAnimation skeletonAnimation, float duration)
        {
            if (skeletonAnimation == null || skeletonAnimation.Skeleton == null)
                yield break;

            Color color = skeletonAnimation.Skeleton.GetColor();
            color.a = 0f;
            skeletonAnimation.Skeleton.SetColor(color);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsed / duration);
                color.a = alpha;
                skeletonAnimation.Skeleton.SetColor(color);
                yield return null;
            }

            color.a = 1f;
            skeletonAnimation.Skeleton.SetColor(color);
        }

        /// <summary>
        /// Fade out (from visible to transparent) - SkeletonGraphic
        /// </summary>
        public static IEnumerator FadeOut(SkeletonGraphic skeletonGraphic, float duration)
        {
            if (skeletonGraphic == null || skeletonGraphic.Skeleton == null)
                yield break;

            Color color = skeletonGraphic.Skeleton.GetColor();
            float startAlpha = color.a;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
                color.a = alpha;
                skeletonGraphic.Skeleton.SetColor(color);
                yield return null;
            }

            color.a = 0f;
            skeletonGraphic.Skeleton.SetColor(color);
        }

        /// <summary>
        /// Fade out (from visible to transparent) - SkeletonAnimation
        /// </summary>
        public static IEnumerator FadeOut(SkeletonAnimation skeletonAnimation, float duration, Action onComplete = null)
        {
            if (skeletonAnimation == null || skeletonAnimation.Skeleton == null)
                yield break;

            Color color = skeletonAnimation.Skeleton.GetColor();
            float startAlpha = color.a;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
                color.a = alpha;
                skeletonAnimation.Skeleton.SetColor(color);
                yield return null;
            }

            color.a = 0f;
            skeletonAnimation.Skeleton.SetColor(color);
            onComplete?.Invoke();
        }

        /// <summary>
        /// Flash color effect (damage hit, power-up, etc.) - SkeletonGraphic
        /// </summary>
        public static IEnumerator FlashColor(SkeletonGraphic skeletonGraphic, Color flashColor, float duration)
        {
            if (skeletonGraphic == null || skeletonGraphic.Skeleton == null)
                yield break;

            Color originalColor = skeletonGraphic.Skeleton.GetColor();

            // Flash to target color
            skeletonGraphic.Skeleton.SetColor(flashColor);
            yield return new WaitForSeconds(duration);

            // Return to original color
            skeletonGraphic.Skeleton.SetColor(originalColor);
        }

        /// <summary>
        /// Flash color effect - SkeletonAnimation
        /// </summary>
        public static IEnumerator FlashColor(SkeletonAnimation skeletonAnimation, Color flashColor, float duration)
        {
            if (skeletonAnimation == null || skeletonAnimation.Skeleton == null)
                yield break;

            Color originalColor = skeletonAnimation.Skeleton.GetColor();

            // Flash to target color
            skeletonAnimation.Skeleton.SetColor(flashColor);
            yield return new WaitForSeconds(duration);

            // Return to original color
            skeletonAnimation.Skeleton.SetColor(originalColor);
        }

        /// <summary>
        /// Pulse color effect (breathing glow) - SkeletonGraphic
        /// </summary>
        public static IEnumerator PulseColor(SkeletonGraphic skeletonGraphic, Color pulseColor, float duration, int pulseCount = 3)
        {
            if (skeletonGraphic == null || skeletonGraphic.Skeleton == null)
                yield break;

            Color originalColor = skeletonGraphic.Skeleton.GetColor();
            float pulseTime = duration / pulseCount;

            for (int i = 0; i < pulseCount; i++)
            {
                // Fade to pulse color
                float elapsed = 0f;
                while (elapsed < pulseTime / 2f)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / (pulseTime / 2f);
                    Color lerpedColor = Color.Lerp(originalColor, pulseColor, t);
                    skeletonGraphic.Skeleton.SetColor(lerpedColor);
                    yield return null;
                }

                // Fade back to original
                elapsed = 0f;
                while (elapsed < pulseTime / 2f)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / (pulseTime / 2f);
                    Color lerpedColor = Color.Lerp(pulseColor, originalColor, t);
                    skeletonGraphic.Skeleton.SetColor(lerpedColor);
                    yield return null;
                }
            }

            skeletonGraphic.Skeleton.SetColor(originalColor);
        }

        /// <summary>
        /// Pulse color effect - SkeletonAnimation
        /// </summary>
        public static IEnumerator PulseColor(SkeletonAnimation skeletonAnimation, Color pulseColor, float duration, int pulseCount = 3)
        {
            if (skeletonAnimation == null || skeletonAnimation.Skeleton == null)
                yield break;

            Color originalColor = skeletonAnimation.Skeleton.GetColor();
            float pulseTime = duration / pulseCount;

            for (int i = 0; i < pulseCount; i++)
            {
                // Fade to pulse color
                float elapsed = 0f;
                while (elapsed < pulseTime / 2f)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / (pulseTime / 2f);
                    Color lerpedColor = Color.Lerp(originalColor, pulseColor, t);
                    skeletonAnimation.Skeleton.SetColor(lerpedColor);
                    yield return null;
                }

                // Fade back to original
                elapsed = 0f;
                while (elapsed < pulseTime / 2f)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / (pulseTime / 2f);
                    Color lerpedColor = Color.Lerp(pulseColor, originalColor, t);
                    skeletonAnimation.Skeleton.SetColor(lerpedColor);
                    yield return null;
                }
            }

            skeletonAnimation.Skeleton.SetColor(originalColor);
        }

        /// <summary>
        /// Set color for specific slot (SkeletonGraphic)
        /// </summary>
        public static void SetSlotColor(SkeletonGraphic skeletonGraphic, string slotName, Color color)
        {
            if (skeletonGraphic == null || skeletonGraphic.Skeleton == null)
                return;

            var slot = skeletonGraphic.Skeleton.FindSlot(slotName);
            if (slot != null)
            {
                slot.SetColor(color);
            }
            else
            {
                Debug.LogWarning($"[SpineHelper] Slot '{slotName}' not found");
            }
        }

        /// <summary>
        /// Set color for specific slot (SkeletonAnimation)
        /// </summary>
        public static void SetSlotColor(SkeletonAnimation skeletonAnimation, string slotName, Color color)
        {
            if (skeletonAnimation == null || skeletonAnimation.Skeleton == null)
                return;

            var slot = skeletonAnimation.Skeleton.FindSlot(slotName);
            if (slot != null)
            {
                slot.SetColor(color);
            }
            else
            {
                Debug.LogWarning($"[SpineHelper] Slot '{slotName}' not found");
            }
        }

        #endregion
    }
}
