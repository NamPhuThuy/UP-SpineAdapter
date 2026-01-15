using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using Spine.Unity;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.SpineAdapter
{
    /// <summary>
    /// Enum for all pooled Spine animations - ADD YOUR ANIMATIONS HERE
    /// </summary>
    public enum SpineAnimationType
    {
        NONE = 0,
        LIKE = 1,
        HitEffect,
        LevelComplete,
        PowerUp,
        // Add more as needed
    }
    
    /// <summary>
    /// Enum for all pooled Spine graphics - ADD YOUR UI ANIMATIONS HERE
    /// </summary>
    public enum SpineGraphicType
    {
        LikeEffect,
        ComboText,
        ScorePopup,
        ButtonPress,
        // Add more as needed
    }
    
    public partial class SpineManager : Singleton<SpineManager>
    {
        #region Pool Configuration
        
        [System.Serializable]
        public class SpinePoolConfig
        {
            public SpineAnimationType type;
            public SkeletonAnimation prefab;
            public int preloadCount = 5;
            public int maxPoolSize = 20;
            public bool expandable = true;
        }
        
        [System.Serializable]
        public class SpineGraphicPoolConfig
        {
            public SpineGraphicType type;
            public SkeletonGraphic prefab;
            public int preloadCount = 5;
            public int maxPoolSize = 20;
            public bool expandable = true;
        }
        
        #endregion
        
        #region Private Serializable Fields
        
        [Header("Example Prefabs")] 
        public SkeletonAnimation like_skeAnim;
        public SkeletonGraphic like_skeGrap;
        
        [Header("Pool Configuration")]
        [SerializeField] private SpinePoolConfig[] animationPools;
        [SerializeField] private SpineGraphicPoolConfig[] graphicPools;
        
        [Header("Pool Settings")]
        [SerializeField] private Transform poolContainer;
        [SerializeField] private bool autoCreateContainer = true;
        [SerializeField] private bool warmupOnAwake = true;
        
        #endregion
        
        #region Private Fields
        
        // SkeletonAnimation pools - using enum as key (faster than string!)
        private Dictionary<SpineAnimationType, Queue<SkeletonAnimation>> _animationPools;
        private Dictionary<SpineAnimationType, SpinePoolConfig> _animationConfigs;
        private Dictionary<SkeletonAnimation, SpineAnimationType> _activeAnimations;
        
        // SkeletonGraphic pools
        private Dictionary<SpineGraphicType, Queue<SkeletonGraphic>> _graphicPools;
        private Dictionary<SpineGraphicType, SpineGraphicPoolConfig> _graphicConfigs;
        private Dictionary<SkeletonGraphic, SpineGraphicType> _activeGraphics;
        
        // Statistics - using enum as key
        private Dictionary<SpineAnimationType, int> _animSpawnCounts;
        private Dictionary<SpineAnimationType, int> _animRecycleCounts;
        private Dictionary<SpineGraphicType, int> _graphicSpawnCounts;
        private Dictionary<SpineGraphicType, int> _graphicRecycleCounts;
        
        #endregion
        
        #region MonoBehaviour Callbacks
        
        protected override void Awake()
        {
            base.Awake();
            MMEventManager.RegistCurrentEvents(this);
            
            // Initialize containers
            _animationPools = new Dictionary<SpineAnimationType, Queue<SkeletonAnimation>>();
            _animationConfigs = new Dictionary<SpineAnimationType, SpinePoolConfig>();
            _activeAnimations = new Dictionary<SkeletonAnimation, SpineAnimationType>();
            
            _graphicPools = new Dictionary<SpineGraphicType, Queue<SkeletonGraphic>>();
            _graphicConfigs = new Dictionary<SpineGraphicType, SpineGraphicPoolConfig>();
            _activeGraphics = new Dictionary<SkeletonGraphic, SpineGraphicType>();
            
            _animSpawnCounts = new Dictionary<SpineAnimationType, int>();
            _animRecycleCounts = new Dictionary<SpineAnimationType, int>();
            _graphicSpawnCounts = new Dictionary<SpineGraphicType, int>();
            _graphicRecycleCounts = new Dictionary<SpineGraphicType, int>();
            
            // Create pool container
            if (poolContainer == null && autoCreateContainer)
            {
                GameObject container = new GameObject("[Spine Pools]");
                container.transform.SetParent(transform);
                poolContainer = container.transform;
            }
            
            // Warmup pools
            if (warmupOnAwake)
            {
                WarmupAllPools();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            MMEventManager.UnregistCurrentEvents(this);
        }

        #endregion
        
        #region Public Methods - Spawn & Recycle
        
        /// <summary>
        /// Spawn SkeletonAnimation from pool using enum
        /// </summary>
        public SkeletonAnimation Spawn(SpineAnimationType type, Vector3 position, Transform parent = null)
        {
            if (!_animationPools.ContainsKey(type))
            {
                Debug.LogError($"[SpineManager] Animation pool '{type}' not found!");
                return null;
            }
            
            SkeletonAnimation anim;
            Queue<SkeletonAnimation> pool = _animationPools[type];
            
            // Get from pool or create new
            if (pool.Count > 0)
            {
                anim = pool.Dequeue();
            }
            else
            {
                SpinePoolConfig config = _animationConfigs[type];
                
                if (!config.expandable)
                {
                    Debug.LogWarning($"[SpineManager] Pool '{type}' is full and not expandable!");
                    return null;
                }
                
                anim = CreateNewAnimation(config);
                Debug.Log($"[SpineManager] Pool '{type}' expanded. New instance created.");
            }
            
            // Setup
            anim.transform.position = position;
            anim.transform.SetParent(parent);
            anim.gameObject.SetActive(true);
            
            // Track
            _activeAnimations[anim] = type;
            _animSpawnCounts[type]++;
            
            return anim;
        }
        
        /// <summary>
        /// Spawn SkeletonGraphic from pool using enum
        /// </summary>
        public SkeletonGraphic Spawn(SpineGraphicType type, Transform parent = null)
        {
            if (!_graphicPools.ContainsKey(type))
            {
                Debug.LogError($"[SpineManager] Graphic pool '{type}' not found!");
                return null;
            }
            
            SkeletonGraphic graphic;
            Queue<SkeletonGraphic> pool = _graphicPools[type];
            
            // Get from pool or create new
            if (pool.Count > 0)
            {
                graphic = pool.Dequeue();
            }
            else
            {
                SpineGraphicPoolConfig config = _graphicConfigs[type];
                
                if (!config.expandable)
                {
                    Debug.LogWarning($"[SpineManager] Pool '{type}' is full and not expandable!");
                    return null;
                }
                
                graphic = CreateNewGraphic(config);
                Debug.Log($"[SpineManager] Pool '{type}' expanded. New instance created.");
            }
            
            // Setup
            graphic.transform.SetParent(parent);
            graphic.gameObject.SetActive(true);
            
            // Track
            _activeGraphics[graphic] = type;
            _graphicSpawnCounts[type]++;
            
            return graphic;
        }
        
        /// <summary>
        /// Recycle SkeletonAnimation back to pool
        /// </summary>
        public void Recycle(SkeletonAnimation anim)
        {
            if (anim == null) return;
            
            if (!_activeAnimations.TryGetValue(anim, out SpineAnimationType type))
            {
                Debug.LogWarning("[SpineManager] Trying to recycle animation not from pool!");
                Destroy(anim.gameObject);
                return;
            }
            
            // Reset state
            anim.gameObject.SetActive(false);
            anim.transform.SetParent(poolContainer);
            anim.AnimationState.ClearTracks();
            anim.Skeleton.SetToSetupPose();
            
            // Return to pool
            Queue<SkeletonAnimation> pool = _animationPools[type];
            SpinePoolConfig config = _animationConfigs[type];
            
            if (pool.Count < config.maxPoolSize)
            {
                pool.Enqueue(anim);
                _animRecycleCounts[type]++;
            }
            else
            {
                // Pool is full, destroy
                Destroy(anim.gameObject);
            }
            
            _activeAnimations.Remove(anim);
        }
        
        /// <summary>
        /// Recycle SkeletonGraphic back to pool
        /// </summary>
        public void Recycle(SkeletonGraphic graphic)
        {
            if (graphic == null) return;
            
            if (!_activeGraphics.TryGetValue(graphic, out SpineGraphicType type))
            {
                Debug.LogWarning("[SpineManager] Trying to recycle graphic not from pool!");
                Destroy(graphic.gameObject);
                return;
            }
            
            // Reset state
            graphic.gameObject.SetActive(false);
            graphic.transform.SetParent(poolContainer);
            graphic.AnimationState.ClearTracks();
            graphic.Skeleton.SetToSetupPose();
            
            // Return to pool
            Queue<SkeletonGraphic> pool = _graphicPools[type];
            SpineGraphicPoolConfig config = _graphicConfigs[type];
            
            if (pool.Count < config.maxPoolSize)
            {
                pool.Enqueue(graphic);
                _graphicRecycleCounts[type]++;
            }
            else
            {
                // Pool is full, destroy
                Destroy(graphic.gameObject);
            }
            
            _activeGraphics.Remove(graphic);
        }
        
        /// <summary>
        /// Spawn animation and auto-recycle after duration
        /// </summary>
        public SkeletonAnimation SpawnTimed(SpineAnimationType type, Vector3 position, float duration, Transform parent = null)
        {
            SkeletonAnimation anim = Spawn(type, position, parent);
            if (anim != null)
            {
                StartCoroutine(AutoRecycleAnimation(anim, duration));
            }
            return anim;
        }
        
        /// <summary>
        /// Spawn animation and auto-recycle when animation completes
        /// </summary>
        public SkeletonAnimation SpawnAutoRecycle(SpineAnimationType type, Vector3 position, string animationName, bool loop = false, Transform parent = null)
        {
            SkeletonAnimation anim = Spawn(type, position, parent);
            if (anim != null)
            {
                var trackEntry = anim.AnimationState.SetAnimation(0, animationName, loop);
                if (!loop)
                {
                    trackEntry.Complete += (entry) => Recycle(anim);
                }
            }
            return anim;
        }
        
        /// <summary>
        /// Spawn graphic and auto-recycle when animation completes
        /// </summary>
        public SkeletonGraphic SpawnAutoRecycle(SpineGraphicType type, string animationName, bool loop = false, Transform parent = null)
        {
            SkeletonGraphic graphic = Spawn(type, parent);
            if (graphic != null)
            {
                var trackEntry = graphic.AnimationState.SetAnimation(0, animationName, loop);
                if (!loop)
                {
                    trackEntry.Complete += (entry) => Recycle(graphic);
                }
            }
            return graphic;
        }
        
        #endregion
        
        #region Public Methods - Pool Management
        
        /// <summary>
        /// Preload all configured pools
        /// </summary>
        public void WarmupAllPools()
        {
            // Warmup animation pools
            foreach (var config in animationPools)
            {
                if (config.prefab == null)
                {
                    Debug.LogWarning($"[SpineManager] Animation pool '{config.type}' has null prefab!");
                    continue;
                }
                
                CreateAnimationPool(config);
            }
            
            // Warmup graphic pools
            foreach (var config in graphicPools)
            {
                if (config.prefab == null)
                {
                    Debug.LogWarning($"[SpineManager] Graphic pool '{config.type}' has null prefab!");
                    continue;
                }
                
                CreateGraphicPool(config);
            }
        }
        
        /// <summary>
        /// Clear and recycle all active objects
        /// </summary>
        public void RecycleAll()
        {
            // Recycle animations
            List<SkeletonAnimation> activeAnims = new List<SkeletonAnimation>(_activeAnimations.Keys);
            foreach (var anim in activeAnims)
            {
                Recycle(anim);
            }
            
            // Recycle graphics
            List<SkeletonGraphic> activeGraphics = new List<SkeletonGraphic>(_activeGraphics.Keys);
            foreach (var graphic in activeGraphics)
            {
                Recycle(graphic);
            }
        }
        
        /// <summary>
        /// Check if pool has available instances
        /// </summary>
        public bool HasAvailable(SpineAnimationType type)
        {
            if (!_animationPools.TryGetValue(type, out Queue<SkeletonAnimation> pool))
                return false;
            
            return pool.Count > 0 || _animationConfigs[type].expandable;
        }
        
        /// <summary>
        /// Check if pool has available instances
        /// </summary>
        public bool HasAvailable(SpineGraphicType type)
        {
            if (!_graphicPools.TryGetValue(type, out Queue<SkeletonGraphic> pool))
                return false;
            
            return pool.Count > 0 || _graphicConfigs[type].expandable;
        }
        
        /// <summary>
        /// Get pool statistics
        /// </summary>
        public void LogPoolStatistics()
        {
            Debug.Log("=== Spine Pool Statistics ===");
            
            foreach (var kvp in _animationPools)
            {
                SpineAnimationType type = kvp.Key;
                int pooled = kvp.Value.Count;
                int active = 0;
                
                foreach (var activeKvp in _activeAnimations)
                {
                    if (activeKvp.Value == type) active++;
                }
                
                int spawned = _animSpawnCounts.ContainsKey(type) ? _animSpawnCounts[type] : 0;
                int recycled = _animRecycleCounts.ContainsKey(type) ? _animRecycleCounts[type] : 0;
                
                Debug.Log($"[Animation] {type}: Pooled={pooled}, Active={active}, Spawned={spawned}, Recycled={recycled}");
            }
            
            foreach (var kvp in _graphicPools)
            {
                SpineGraphicType type = kvp.Key;
                int pooled = kvp.Value.Count;
                int active = 0;
                
                foreach (var activeKvp in _activeGraphics)
                {
                    if (activeKvp.Value == type) active++;
                }
                
                int spawned = _graphicSpawnCounts.ContainsKey(type) ? _graphicSpawnCounts[type] : 0;
                int recycled = _graphicRecycleCounts.ContainsKey(type) ? _graphicRecycleCounts[type] : 0;
                
                Debug.Log($"[Graphic] {type}: Pooled={pooled}, Active={active}, Spawned={spawned}, Recycled={recycled}");
            }
        }
        
        #endregion
        
        #region Private Methods
        
        private void CreateAnimationPool(SpinePoolConfig config)
        {
            if (_animationPools.ContainsKey(config.type))
            {
                Debug.LogWarning($"[SpineManager] Animation pool '{config.type}' already exists!");
                return;
            }
            
            Queue<SkeletonAnimation> pool = new Queue<SkeletonAnimation>();
            
            // Preload instances
            for (int i = 0; i < config.preloadCount; i++)
            {
                SkeletonAnimation anim = CreateNewAnimation(config);
                pool.Enqueue(anim);
            }
            
            _animationPools[config.type] = pool;
            _animationConfigs[config.type] = config;
            _animSpawnCounts[config.type] = 0;
            _animRecycleCounts[config.type] = 0;
            
            Debug.Log($"[SpineManager] Created animation pool '{config.type}' with {config.preloadCount} instances");
        }
        
        private void CreateGraphicPool(SpineGraphicPoolConfig config)
        {
            if (_graphicPools.ContainsKey(config.type))
            {
                Debug.LogWarning($"[SpineManager] Graphic pool '{config.type}' already exists!");
                return;
            }
            
            Queue<SkeletonGraphic> pool = new Queue<SkeletonGraphic>();
            
            // Preload instances
            for (int i = 0; i < config.preloadCount; i++)
            {
                SkeletonGraphic graphic = CreateNewGraphic(config);
                pool.Enqueue(graphic);
            }
            
            _graphicPools[config.type] = pool;
            _graphicConfigs[config.type] = config;
            _graphicSpawnCounts[config.type] = 0;
            _graphicRecycleCounts[config.type] = 0;
            
            Debug.Log($"[SpineManager] Created graphic pool '{config.type}' with {config.preloadCount} instances");
        }
        
        private SkeletonAnimation CreateNewAnimation(SpinePoolConfig config)
        {
            SkeletonAnimation anim = Instantiate(config.prefab, poolContainer);
            anim.name = $"{config.type}_anim";
            anim.gameObject.SetActive(false);
            return anim;
        }
        
        private SkeletonGraphic CreateNewGraphic(SpineGraphicPoolConfig config)
        {
            SkeletonGraphic graphic = Instantiate(config.prefab, poolContainer);
            graphic.name = $"{config.type}_graphic";
            graphic.gameObject.SetActive(false);
            return graphic;
        }
        
        private IEnumerator AutoRecycleAnimation(SkeletonAnimation anim, float duration)
        {
            yield return new WaitForSeconds(duration);
            Recycle(anim);
        }
        
        #endregion
        
        #region Editor Methods
        
        public void ResetValues()
        {
            RecycleAll();
        }
        
        #endregion

    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(SpineManager))]
    [CanEditMultipleObjects]
    public class SpineManagerEditor : Editor
    {
        private SpineManager script;
        private Texture2D frogIcon;
        
        private void OnEnable()
        {
            frogIcon = Resources.Load<Texture2D>("frog");
            script = (SpineManager)target;
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);
            
            EditorGUI.BeginDisabledGroup(!Application.isPlaying);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Warmup All Pools"))
            {
                script.WarmupAllPools();
            }
           
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("Log Pool Statistics"))
            {
                script.LogPoolStatistics();
            }
            
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.Space(10);
            ButtonResetValues();
        }
        
        private void ButtonResetValues()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("Reset Values", frogIcon), GUILayout.Width(150)))
            {
                script.ResetValues();
                EditorUtility.SetDirty(script);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
    #endif
}