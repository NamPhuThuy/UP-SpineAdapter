using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.SpineAdapter
{
    // ============================================================
// USAGE EXAMPLES
// ============================================================
    public class SpineChainExample : MonoBehaviour
    {
        [SerializeField] private SkeletonGraphic skeletonGraphic;
        [SerializeField] private SkeletonAnimation skeletonAnimation;

        // ============================================================
        // EXAMPLE 1: Simple Idle → Loop (YOUR REQUIREMENT)
        // ============================================================
        void PlayIdleThenMain()
        {
            // Play "idle" once, then immediately play "animation" looping
            SpineHelper.PlayAppearThenLoop(
                skeletonGraphic,
                "idle", // Plays once (loop=false)
                "animation", // Loops forever (loop=true)
                timeScale: 1f
            );
        }

        // ============================================================
        // EXAMPLE 2: Character Intro Sequence
        // ============================================================
        void PlayCharacterIntro()
        {
            // Entrance → Idle → Loop
            SpineHelper.PlayAppearThenLoop(
                skeletonGraphic,
                "entrance", // Play entrance once
                "idle_loop", // Then loop idle
                timeScale: 1f
            );
        }

        // ============================================================
        // EXAMPLE 3: Button Press Effect
        // ============================================================
        void OnButtonPressed()
        {
            // Press animation → Return to idle
            SpineHelper.PlayAppearThenLoop(
                skeletonGraphic,
                "button_press", // Press once
                "button_idle", // Back to idle loop
                timeScale: 1.2f // 20% faster
            );
        }

        // ============================================================
        // EXAMPLE 4: Multiple Animations in Sequence
        // ============================================================
        void PlayComplexSequence()
        {
            var sequence = new[]
            {
                ("intro", false, 0f), // Play intro (no loop, no delay)
                ("attack", false, 0f), // Then attack (no loop, no delay)
                ("idle", true, 0.5f) // Then idle loop after 0.5s delay
            };

            SpineHelper.PlayAnimationSequence(skeletonGraphic, sequence);
        }

        // ============================================================
        // EXAMPLE 5: Boss Death Sequence
        // ============================================================
        void PlayBossDeath()
        {
            var deathSequence = new[]
            {
                ("take_damage", false, 0f), // Damage reaction
                ("stagger", false, 0f), // Stagger
                ("death", false, 0f), // Death animation
                ("death_idle", true, 0f) // Stay in death pose
            };

            SpineHelper.PlayAnimationSequence(skeletonAnimation, deathSequence, timeScale: 0.8f);
        }

        // ============================================================
        // EXAMPLE 6: With Callback
        // ============================================================
        void PlayWithCallback()
        {
            // Play animation and get notified when done
            SpineHelper.PlayAnimationWithCallback(
                skeletonGraphic,
                "victory",
                loop: false,
                onComplete: () =>
                {
                    Debug.Log("Victory animation completed!");
                    // Do something after animation finishes
                    ShowRewardScreen();
                }
            );
        }

        // ============================================================
        // EXAMPLE 7: Level Complete Sequence
        // ============================================================
        void PlayLevelComplete()
        {
            // Celebration → Victory Pose → Idle
            SpineHelper.PlayAppearThenLoop(
                skeletonGraphic,
                "celebration",
                "victory_idle"
            );
        }

        // ============================================================
        // EXAMPLE 8: Combo Chain (World Space)
        // ============================================================
        void PlayComboChain()
        {
            var comboSequence = new[]
            {
                ("combo_1", false, 0f),
                ("combo_2", false, 0f),
                ("combo_3", false, 0f),
                ("idle_combat", true, 0f) // Back to combat idle
            };

            SpineHelper.PlayAnimationSequence(skeletonAnimation, comboSequence, timeScale: 1.5f);
        }

        // ============================================================
        // EXAMPLE 9: Chest Opening
        // ============================================================
        void OpenChest()
        {
            // Chest opens, then stays open
            SpineHelper.PlayAppearThenLoop(
                skeletonGraphic,
                "chest_open", // Opening animation
                "chest_opened", // Stays open (looping)
                timeScale: 1f
            );
        }

        // ============================================================
        // EXAMPLE 10: Character State Machine
        // ============================================================
        public enum CharacterState
        {
            Idle,
            Walking,
            Attacking,
            Dead
        }

        private CharacterState currentState;

        void ChangeState(CharacterState newState)
        {
            if (currentState == newState) return;

            switch (newState)
            {
                case CharacterState.Idle:
                    SpineHelper.PlayAnimation(skeletonAnimation, "idle", loop: true);
                    break;

                case CharacterState.Walking:
                    SpineHelper.PlayAnimation(skeletonAnimation, "walk", loop: true);
                    break;

                case CharacterState.Attacking:
                    // Attack once, then return to idle
                    SpineHelper.PlayAppearThenLoop(
                        skeletonAnimation,
                        "attack",
                        "idle"
                    );
                    break;

                case CharacterState.Dead:
                    SpineHelper.PlayAnimation(skeletonAnimation, "death", loop: false);
                    break;
            }

            currentState = newState;
        }

        private void ShowRewardScreen()
        {
        }
    }

// ============================================================
// ADVANCED: MonoBehaviour Wrapper for Easy Use
// ============================================================
    public class SpineAnimationController : MonoBehaviour
    {
        [SerializeField] private SkeletonGraphic skeletonGraphic;
        [SerializeField] private SkeletonAnimation skeletonAnimation;

        // Helper property to determine which spine component to use
        private bool IsGraphic => skeletonGraphic != null;

        /// <summary>
        /// Play intro animation then loop main animation
        /// </summary>
        public void PlayIntroThenLoop(string introAnim, string loopAnim)
        {
            if (IsGraphic)
                SpineHelper.PlayAppearThenLoop(skeletonGraphic, introAnim, loopAnim);
            else
                SpineHelper.PlayAppearThenLoop(skeletonAnimation, introAnim, loopAnim);
        }

        /// <summary>
        /// Play single animation
        /// </summary>
        public void Play(string animName, bool loop = true)
        {
            if (IsGraphic)
                SpineHelper.PlayAnimation(skeletonGraphic, animName, loop);
            else
                SpineHelper.PlayAnimation(skeletonAnimation, animName, loop);
        }

        /// <summary>
        /// Play animation sequence
        /// </summary>
        public void PlaySequence(params (string animName, bool loop, float delay)[] animations)
        {
            if (IsGraphic)
                SpineHelper.PlayAnimationSequence(skeletonGraphic, animations);
            else
                SpineHelper.PlayAnimationSequence(skeletonAnimation, animations);
        }
    }

// Usage of wrapper:
    public class WrapperUsageExample : MonoBehaviour
    {
        [SerializeField] private SpineAnimationController spineController;

        void Start()
        {
            // Simple one-liner!
            spineController.PlayIntroThenLoop("idle", "animation");
        }

        void OnButtonClick()
        {
            spineController.PlayIntroThenLoop("button_press", "button_idle");
        }

        void OnLevelComplete()
        {
            spineController.PlaySequence(
                ("celebration", false, 0f),
                ("victory", false, 0f),
                ("idle", true, 0f)
            );
        }
    }

    // ============================================================
// COLOR MANIPULATION EXAMPLES
// ============================================================
    public class SpineColorExamples : MonoBehaviour
    {
        [SerializeField] private SkeletonGraphic uiSpine;
        [SerializeField] private SkeletonAnimation worldSpine;

        // ============================================================
        // EXAMPLE 1: Instant Color Change
        // ============================================================
        void ChangeColorInstant()
        {
            // Change to red
            SpineHelper.SetColor(worldSpine, Color.red);

            // Change to custom color
            Color customColor = new Color(0.5f, 0.3f, 0.8f, 1f);
            SpineHelper.SetColor(uiSpine, customColor);

            // Change alpha only (transparency)
            SpineHelper.SetAlpha(worldSpine, 0.5f); // 50% transparent
        }

        // ============================================================
        // EXAMPLE 2: Fade In/Out
        // ============================================================
        void FadeInOut()
        {
            // Fade in over 1 second
            StartCoroutine(SpineHelper.FadeIn(worldSpine, 1f));

            // Fade out over 2 seconds
            StartCoroutine(SpineHelper.FadeOut(worldSpine, 2f));
        }

        // ============================================================
        // EXAMPLE 3: Damage Flash (Hit Effect)
        // ============================================================
        void OnTakeDamage()
        {
            // Flash red for 0.1 seconds
            StartCoroutine(SpineHelper.FlashColor(worldSpine, Color.red, 0.1f));
        }

        // ============================================================
        // EXAMPLE 4: Power-Up Glow
        // ============================================================
        void OnPowerUpCollected()
        {
            // Flash yellow
            StartCoroutine(SpineHelper.FlashColor(worldSpine, Color.yellow, 0.2f));

            // Or pulse golden color
            Color goldenGlow = new Color(1f, 0.8f, 0f, 1f);
            StartCoroutine(SpineHelper.PulseColor(worldSpine, goldenGlow, 1f, pulseCount: 3));
        }

        // ============================================================
        // EXAMPLE 5: Poison/Freeze Effect
        // ============================================================
        void ApplyPoisonEffect()
        {
            // Change to green for poison
            Color poisonGreen = new Color(0f, 1f, 0f, 0.8f);
            StartCoroutine(SpineHelper.FadeToColor(worldSpine, poisonGreen, 0.5f));
        }

        void ApplyFreezeEffect()
        {
            // Change to blue-white for freeze
            Color freezeBlue = new Color(0.6f, 0.8f, 1f, 1f);
            StartCoroutine(SpineHelper.FadeToColor(worldSpine, freezeBlue, 0.3f));
        }

        // ============================================================
        // EXAMPLE 6: Death Fade
        // ============================================================
        void OnDeath()
        {
            // Fade to black/gray
            Color deathGray = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            StartCoroutine(DeathSequence());

            IEnumerator DeathSequence()
            {
                // Fade to gray
                yield return SpineHelper.FadeToColor(worldSpine, deathGray, 0.5f);

                // Wait
                yield return new WaitForSeconds(1f);

                // Fade out completely
                yield return SpineHelper.FadeOut(worldSpine, 1f);

                // Destroy
                Destroy(gameObject);
            }
        }

        // ============================================================
        // EXAMPLE 7: Invincibility Blink
        // ============================================================
        void ActivateInvincibility(float duration)
        {
            StartCoroutine(InvincibilityBlink(duration));
        }

        IEnumerator InvincibilityBlink(float duration)
        {
            float elapsed = 0f;
            bool visible = true;

            while (elapsed < duration)
            {
                // Toggle alpha
                SpineHelper.SetAlpha(worldSpine, visible ? 0.3f : 1f);
                visible = !visible;

                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }

            // Restore full opacity
            SpineHelper.SetAlpha(worldSpine, 1f);
        }

        // ============================================================
        // EXAMPLE 8: Rainbow Effect
        // ============================================================
        void PlayRainbowEffect()
        {
            StartCoroutine(RainbowLoop());
        }

        IEnumerator RainbowLoop()
        {
            Color[] rainbowColors = new Color[]
            {
                Color.red,
                new Color(1f, 0.5f, 0f), // Orange
                Color.yellow,
                Color.green,
                Color.cyan,
                Color.blue,
                new Color(0.5f, 0f, 1f) // Purple
            };

            foreach (Color color in rainbowColors)
            {
                yield return SpineHelper.FadeToColor(worldSpine, color, 0.3f);
                yield return new WaitForSeconds(0.1f);
            }

            // Return to white
            yield return SpineHelper.FadeToColor(worldSpine, Color.white, 0.5f);
        }

        // ============================================================
        // EXAMPLE 9: Health-Based Color
        // ============================================================
        void UpdateHealthColor(float healthPercent)
        {
            // Green → Yellow → Red based on health
            Color healthColor;

            if (healthPercent > 0.5f)
            {
                // Green to Yellow
                healthColor = Color.Lerp(Color.yellow, Color.green, (healthPercent - 0.5f) * 2f);
            }
            else
            {
                // Yellow to Red
                healthColor = Color.Lerp(Color.red, Color.yellow, healthPercent * 2f);
            }

            SpineHelper.SetColor(worldSpine, healthColor);
        }

        // ============================================================
        // EXAMPLE 10: Specific Slot Color (Advanced)
        // ============================================================
        void ColorSpecificPart()
        {
            // Color only the weapon slot red
            SpineHelper.SetSlotColor(worldSpine, "weapon", Color.red);

            // Color body blue, weapon stays normal
            SpineHelper.SetSlotColor(worldSpine, "body", Color.blue);
        }

        // ============================================================
        // EXAMPLE 11: Stealth Mode (Fade)
        // ============================================================
        void EnterStealthMode()
        {
            StartCoroutine(SpineHelper.FadeToColor(
                worldSpine,
                new Color(1f, 1f, 1f, 0.3f), // 30% opacity
                0.5f
            ));
        }

        void ExitStealthMode()
        {
            StartCoroutine(SpineHelper.FadeToColor(
                worldSpine,
                Color.white,
                0.5f
            ));
        }

        // ============================================================
        // EXAMPLE 12: Charging Attack (Pulse)
        // ============================================================
        void ChargeAttack()
        {
            StartCoroutine(ChargeEffect());
        }

        IEnumerator ChargeEffect()
        {
            Color chargeColor = new Color(1f, 0f, 0f, 1f); // Red

            // Pulse faster and faster
            yield return SpineHelper.PulseColor(worldSpine, chargeColor, 1.5f, 2);
            yield return SpineHelper.PulseColor(worldSpine, chargeColor, 1f, 3);
            yield return SpineHelper.PulseColor(worldSpine, chargeColor, 0.5f, 5);

            // Flash bright
            yield return SpineHelper.FlashColor(worldSpine, Color.white, 0.1f);
        }

        // ============================================================
        // EXAMPLE 13: UI Button Hover Effect
        // ============================================================
        void OnButtonHover()
        {
            // Brighten on hover
            Color brightColor = new Color(1.2f, 1.2f, 1.2f, 1f);
            StartCoroutine(SpineHelper.FadeToColor(uiSpine, brightColor, 0.1f));
        }

        void OnButtonUnhover()
        {
            // Return to normal
            StartCoroutine(SpineHelper.FadeToColor(uiSpine, Color.white, 0.1f));
        }

        // ============================================================
        // EXAMPLE 14: Enemy Enrage (Red Glow)
        // ============================================================
        void OnEnrage()
        {
            StartCoroutine(EnrageEffect());
        }

        IEnumerator EnrageEffect()
        {
            Color enrageRed = new Color(1.5f, 0.5f, 0.5f, 1f); // Bright red

            // Quick flash
            yield return SpineHelper.FlashColor(worldSpine, Color.red, 0.05f);
            yield return new WaitForSeconds(0.05f);
            yield return SpineHelper.FlashColor(worldSpine, Color.red, 0.05f);

            // Stay red
            yield return SpineHelper.FadeToColor(worldSpine, enrageRed, 0.3f);
        }

        // ============================================================
        // EXAMPLE 15: Combo System (Progressive Color)
        // ============================================================
        private int comboCount = 0;

        void OnComboHit()
        {
            comboCount++;
            UpdateComboColor();
        }

        void UpdateComboColor()
        {
            Color comboColor;

            if (comboCount < 5)
                comboColor = Color.white;
            else if (comboCount < 10)
                comboColor = Color.yellow;
            else if (comboCount < 20)
                comboColor = new Color(1f, 0.5f, 0f); // Orange
            else
                comboColor = Color.red;

            StartCoroutine(SpineHelper.FadeToColor(worldSpine, comboColor, 0.1f));
        }
    }
}