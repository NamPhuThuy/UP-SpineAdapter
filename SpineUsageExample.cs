using UnityEngine;
using Spine.Unity;
using Spine;
using NamPhuThuy.SpineAdapter;

/// <summary>
/// Example usage of SpineManager for spawning and recycling
/// </summary>
public class SpineUsageExamples : MonoBehaviour
{
    // ============================================================
    // METHOD 1: BASIC - Spawn, Play, Manual Recycle
    // ============================================================
    void Example_BasicSpawn()
    {
        // Spawn from pool
        SkeletonAnimation explosion = SpineManager.Ins.Spawn(
            SpineAnimationType.LIKE,
            transform.position
        );
        
        // Play animation (one-shot, no loop)
        explosion.AnimationState.SetAnimation(0, "explode", false);
        
        // Manual recycle after 2 seconds
        Invoke(nameof(RecycleExplosion), 2f);
        
        void RecycleExplosion()
        {
            SpineManager.Ins.Recycle(explosion);
        }
    }

    // ============================================================
    // METHOD 2: AUTO-RECYCLE (Built-in) - RECOMMENDED!
    // ============================================================
    void Example_AutoRecycle()
    {
        // ✅ BEST: Auto-recycle khi animation kết thúc
        SpineManager.Ins.SpawnAutoRecycle(
            SpineAnimationType.LIKE,
            transform.position,
            "explode",      // Animation name
            loop: false     // IMPORTANT: false để auto-recycle!
        );
        
        // Không cần manual recycle - SpineManager tự động làm!
    }

    // ============================================================
    // METHOD 3: TIMED RECYCLE - For specific duration
    // ============================================================
    void Example_TimedRecycle()
    {
        // Auto-recycle sau 1.5 giây
        SpineManager.Ins.SpawnTimed(
            SpineAnimationType.LIKE,
            transform.position,
            1.5f  // Duration
        );
    }

    // ============================================================
    // METHOD 4: MANUAL CONTROL - Full control over animation
    // ============================================================
    void Example_ManualControl()
    {
        // Spawn
        SkeletonAnimation fx = SpineManager.Ins.Spawn(
            SpineAnimationType.HitEffect,
            transform.position
        );
        
        // Play animation với callback
        TrackEntry track = fx.AnimationState.SetAnimation(0, "hit", false);
        
        // Subscribe to Complete event
        track.Complete += (entry) =>
        {
            Debug.Log("Animation completed!");
            SpineManager.Ins.Recycle(fx);
        };
    }

    // ============================================================
    // METHOD 5: CHAINED ANIMATIONS - Play multiple then recycle
    // ============================================================
    void Example_ChainedAnimations()
    {
        SkeletonAnimation character = SpineManager.Ins.Spawn(
            SpineAnimationType.PowerUp,
            transform.position
        );
        
        // Play animation 1
        TrackEntry intro = character.AnimationState.SetAnimation(0, "intro", false);
        
        // Queue animation 2
        TrackEntry loop = character.AnimationState.AddAnimation(0, "loop", false, 0f);
        
        // Queue animation 3 and recycle when done
        TrackEntry outro = character.AnimationState.AddAnimation(0, "outro", false, 0f);
        outro.Complete += (entry) =>
        {
            SpineManager.Ins.Recycle(character);
        };
    }

    // ============================================================
    // METHOD 6: UI GRAPHICS - For UI animations
    // ============================================================
    void Example_UIGraphic()
    {
        // Spawn UI graphic
        SkeletonGraphic likeEffect = SpineManager.Ins.Spawn(
            SpineGraphicType.LIKE_EFFECT,
            transform  // UI parent
        );
        
        // Play and auto-recycle
        TrackEntry track = likeEffect.AnimationState.SetAnimation(0, "like_anim", false);
        track.Complete += (entry) =>
        {
            SpineManager.Ins.Recycle(likeEffect);
        };
    }

    // ============================================================
    // METHOD 7: CONDITIONAL RECYCLE - Check animation state
    // ============================================================
    void Example_ConditionalRecycle()
    {
        SkeletonAnimation fx = SpineManager.Ins.Spawn(
            SpineAnimationType.LIKE,
            transform.position
        );
        
        TrackEntry track = fx.AnimationState.SetAnimation(0, "explode", false);
        
        track.Complete += (entry) =>
        {
            // Check if animation actually completed (not interrupted)
            if (entry.Animation.Name == "explode")
            {
                SpineManager.Ins.Recycle(fx);
            }
        };
    }

    // ============================================================
    // METHOD 8: POOLED WITH DELAY - Delayed spawn
    // ============================================================
    void Example_DelayedSpawn()
    {
        // Spawn after 1 second delay
        StartCoroutine(SpawnAfterDelay());
        
        System.Collections.IEnumerator SpawnAfterDelay()
        {
            yield return new WaitForSeconds(1f);
            
            SpineManager.Ins.SpawnAutoRecycle(
                SpineAnimationType.LevelComplete,
                transform.position,
                "complete",
                loop: false
            );
        }
    }

    // ============================================================
    // METHOD 9: MULTIPLE INSTANCES - Spawn many at once
    // ============================================================
    void Example_MultipleSpawns()
    {
        // Spawn 10 coin collect effects
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPos = transform.position + Random.insideUnitSphere * 2f;
            
            SpineManager.Ins.SpawnAutoRecycle(
                SpineAnimationType.LIKE,
                randomPos,
                "collect",
                loop: false
            );
        }
    }

    // ============================================================
    // METHOD 10: CUSTOM RECYCLE CONDITION - Advanced control
    // ============================================================
    private SkeletonAnimation currentEffect;
    
    void Example_CustomRecycleCondition()
    {
        currentEffect = SpineManager.Ins.Spawn(
            SpineAnimationType.HitEffect,
            transform.position
        );
        
        TrackEntry track = currentEffect.AnimationState.SetAnimation(0, "hit_loop", true);
        
        // Recycle when health reaches zero
        StartCoroutine(CheckHealthAndRecycle());
    }
    
    System.Collections.IEnumerator CheckHealthAndRecycle()
    {
        while (true)
        {
            // Check some condition
            int health = -9;
            if (health <= 0)
            {
                // Stop animation
                currentEffect.AnimationState.ClearTracks();
                
                // Recycle
                SpineManager.Ins.Recycle(currentEffect);
                yield break;
            }
            
            yield return new WaitForSeconds(0.1f);
        }
    }
}

// ============================================================
// REAL-WORLD GAME EXAMPLES
// ============================================================

/// <summary>
/// Coin collection effect
/// </summary>
public class CoinCollector : MonoBehaviour
{
    void OnCoinCollected(Vector3 coinPosition)
    {
        // Simple one-line spawn with auto-recycle
        SpineManager.Ins.SpawnAutoRecycle(
            SpineAnimationType.LIKE,
            coinPosition,
            "collect",
            loop: false
        );
    }
}

/// <summary>
/// Enemy death effect
/// </summary>
public class Enemy : MonoBehaviour
{
    void OnDeath()
    {
        // Spawn death effect at enemy position
        SpineManager.Ins.SpawnAutoRecycle(
            SpineAnimationType.LIKE,
            transform.position,
            "death_explosion",
            loop: false
        );
        
        // Destroy enemy
        Destroy(gameObject);
    }
}

/// <summary>
/// Player attack with hit effect
/// </summary>
public class Player : MonoBehaviour
{
    void Attack(Vector3 targetPosition)
    {
        // Spawn hit effect
        SkeletonAnimation hitFX = SpineManager.Ins.Spawn(
            SpineAnimationType.HitEffect,
            targetPosition
        );
        
        // Play with speed multiplier
        TrackEntry track = hitFX.AnimationState.SetAnimation(0, "slash", false);
        track.TimeScale = 1.5f;  // 1.5x speed
        
        // Auto-recycle
        track.Complete += (entry) => SpineManager.Ins.Recycle(hitFX);
    }
}

/// <summary>
/// Boss intro with multiple animations
/// </summary>
public class BossController : MonoBehaviour
{
    void PlayIntroSequence()
    {
        SkeletonAnimation boss = SpineManager.Ins.Spawn(
            SpineAnimationType.LevelComplete,
            transform.position
        );
        
        // Entrance animation
        TrackEntry entrance = boss.AnimationState.SetAnimation(0, "entrance", false);
        
        // Roar animation
        TrackEntry roar = boss.AnimationState.AddAnimation(0, "roar", false, 0f);
        
        // Idle loop (don't recycle, boss stays active)
        boss.AnimationState.AddAnimation(0, "idle", true, 0f);
        
        // Store reference for later
        // GetComponent<BossAI>().spineAnimation = boss;
    }
}

/// <summary>
/// UI Like button effect
/// </summary>
public class LikeButton : MonoBehaviour
{
    public Transform effectContainer;
    
    public void OnLikeButtonClicked()
    {
        // Spawn UI graphic effect
        SkeletonGraphic likeEffect = SpineManager.Ins.Spawn(
            SpineGraphicType.LIKE_EFFECT,
            effectContainer
        );
        
        // Play animation
        TrackEntry track = likeEffect.AnimationState.SetAnimation(0, "like", false);
        
        // Add some juice
        track.TimeScale = 1.2f;
        
        // Auto-recycle
        track.Complete += (entry) => SpineManager.Ins.Recycle(likeEffect);
    }
}

/// <summary>
/// Combo counter with pooled text
/// </summary>
public class ComboSystem : MonoBehaviour
{
    private int comboCount = 0;
    
    public void AddCombo()
    {
        comboCount++;
        
        // Spawn combo text
        SkeletonGraphic comboText = SpineManager.Ins.Spawn(
            SpineGraphicType.COMBO_TEXT,
            transform
        );
        
        // Update text (if you have text mesh in spine)
        // comboText.Skeleton.FindSlot("combo_number").SetAttachment($"number_{comboCount}");
        
        // Play animation
        TrackEntry track = comboText.AnimationState.SetAnimation(0, "combo_appear", false);
        track.Complete += (entry) => SpineManager.Ins.Recycle(comboText);
    }
}

/// <summary>
/// Particle-like effects system
/// </summary>
public class EffectSpawner : MonoBehaviour
{
    [SerializeField] private float spawnInterval = 0.1f;
    [SerializeField] private int particleCount = 20;
    
    void Start()
    {
        StartCoroutine(SpawnParticles());
    }
    
    System.Collections.IEnumerator SpawnParticles()
    {
        for (int i = 0; i < particleCount; i++)
        {
            Vector3 randomOffset = Random.insideUnitCircle * 3f;
            Vector3 spawnPos = transform.position + randomOffset;
            
            // Spawn with random duration
            float duration = Random.Range(0.5f, 1.5f);
            SpineManager.Ins.SpawnTimed(
                SpineAnimationType.PowerUp,
                spawnPos,
                duration
            );
            
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}

/// <summary>
/// Level complete sequence
/// </summary>
public class LevelCompleteManager : MonoBehaviour
{
    public void ShowLevelComplete()
    {
        StartCoroutine(LevelCompleteSequence());
    }
    
    System.Collections.IEnumerator LevelCompleteSequence()
    {
        // Spawn level complete animation
        SkeletonAnimation complete = SpineManager.Ins.Spawn(
            SpineAnimationType.LevelComplete,
            Vector3.zero
        );
        
        // Play intro
        TrackEntry intro = complete.AnimationState.SetAnimation(0, "intro", false);
        
        // Wait for intro to finish
        bool introDone = false;
        intro.Complete += (entry) => introDone = true;
        yield return new WaitUntil(() => introDone);
        
        // Play loop for 3 seconds
        complete.AnimationState.SetAnimation(0, "celebrate_loop", true);
        yield return new WaitForSeconds(3f);
        
        // Play outro and recycle
        TrackEntry outro = complete.AnimationState.SetAnimation(0, "outro", false);
        outro.Complete += (entry) => SpineManager.Ins.Recycle(complete);
    }
}

// ============================================================
// HELPER EXTENSION METHODS
// ============================================================
public static class SpinePoolingExtensions
{
    /// <summary>
    /// Extension: Spawn at GameObject position
    /// </summary>
    public static SkeletonAnimation SpawnAt(this SpineAnimationType type, GameObject target, string animName = null, bool loop = false)
    {
        var anim = SpineManager.Ins.Spawn(type, target.transform.position);
        
        if (!string.IsNullOrEmpty(animName))
        {
            var track = anim.AnimationState.SetAnimation(0, animName, loop);
            
            if (!loop)
            {
                track.Complete += (entry) => SpineManager.Ins.Recycle(anim);
            }
        }
        
        return anim;
    }
    
    /// <summary>
    /// Extension: Spawn and follow target
    /// </summary>
    public static SkeletonAnimation SpawnFollowing(this SpineAnimationType type, Transform target, string animName, bool loop = false)
    {
        var anim = SpineManager.Ins.Spawn(type, target.position, target);
        
        var track = anim.AnimationState.SetAnimation(0, animName, loop);
        
        if (!loop)
        {
            track.Complete += (entry) => SpineManager.Ins.Recycle(anim);
        }
        
        return anim;
    }
}

// Usage of extensions:
public class ExtensionUsageExample : MonoBehaviour
{
    void Example()
    {
        // Simple spawn at this GameObject
        SpineAnimationType.LIKE.SpawnAt(gameObject, "explode");
        
        // Spawn and follow target
        SpineAnimationType.PowerUp.SpawnFollowing(transform, "buff", loop: true);
    }
}