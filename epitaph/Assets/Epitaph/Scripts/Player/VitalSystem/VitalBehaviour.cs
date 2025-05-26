using Cysharp.Threading.Tasks;
using Epitaph.Scripts.GameTimeManager;
using Epitaph.Scripts.Player.BaseBehaviour;
using UnityEngine;

namespace Epitaph.Scripts.Player.VitalSystem
{
    public class VitalBehaviour : PlayerBehaviour
    {
        private VitalFactory _vitalStats;
        
        private bool _isUpdating;
        private int _lastMinute = -1;
        
        // ---------------------------------------------------------------------------- //
        
        public VitalBehaviour(PlayerController playerController) 
            : base(playerController) { }
        
        // ---------------------------------------------------------------------------- //
        
        public override void Awake()
        {
            InitializeVitals();
        }
        
        public override void OnEnable()
        {
            _isUpdating = true;
        }

        public override void OnDisable()
        {
            _isUpdating = false; 
        }

        public override void Start()
        {
            FrameBasedUpdates().Forget(); // Handles continuous PlayerData sync
            // MinuteBasedUpdates().Forget(); // Handles per-minute stat degradation
        }
        
        private void InitializeVitals()
        {
            _vitalStats = new VitalFactory(this);
        }
        
        private async UniTaskVoid FrameBasedUpdates()
        {
            while (_isUpdating)
            {
                
                UpdateVitalStats(Time.deltaTime);
                
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }
        
        private async UniTaskVoid MinuteBasedUpdates()
        {
            while (_isUpdating)
            {
                if (GameTime.Instance == null)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update); 
                    continue;
                }

                var currentMinute = GameTime.Instance.GameMinute;
                if (currentMinute != _lastMinute)
                {
                    _lastMinute = currentMinute;
                    // Pass 1.0f as deltaTime, signifying one minute has passed.
                    // The BaseIncreaseRate in Hunger, Thirst, Fatigue
                    // should be scaled accordingly (e.g., units per minute).
                    UpdateVitalStats(1.0f); 
                }
                
                await GameTime.Instance.WaitForGameSecond();
            }
        }
        
        private void UpdateVitalStats(float deltaTime)
        {
            _vitalStats.StaminaVital().UpdateVital(deltaTime);
        }
        
    }
}