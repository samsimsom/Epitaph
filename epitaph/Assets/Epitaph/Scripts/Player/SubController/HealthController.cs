using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Epitaph.Scripts.GameTimeManager;
using Epitaph.Scripts.Player.HealthSystem;
using Epitaph.Scripts.Player.ScriptableObjects;
using UnityEngine;

namespace Epitaph.Scripts.Player.SubController
{
    public class HealthController : IPlayerSubController // PlayerBehaviour yerine IPlayerSubController
    {
        // PlayerController ve PlayerData alanları InitializeBehaviours içinde atanacak
        private PlayerController _playerController;
        private PlayerData _playerData;

        // Yapıcı metot (constructor) IPlayerSubController deseni için genellikle kaldırılır
        // veya parametresiz hale getirilir. Gerekli bağımlılıklar InitializeBehaviours ile enjekte edilir.
        // public HealthController(PlayerController playerController, PlayerData playerData)
        // {
        //     _playerController = playerController;
        //     _playerData = playerData;
        // }
        
        #region Public Properties
        public Health Health
        {
            get => _health;
            private set => _health = value;
        }
        public Stamina Stamina
        {
            get => _stamina;
            private set => _stamina = value;
        }
        public Hunger Hunger
        {
            get => _hunger;
            private set => _hunger = value;
        }
        public Thirst Thirst
        {
            get => _thirst;
            private set => _thirst = value;
        }
        public Fatigue Fatigue
        {
            get => _fatigue;
            private set => _fatigue = value;
        }
        #endregion

        #region Private Fields
        // private PlayerData _playerData; // InitializeBehaviours'da atanacak
        
        private Health _health;
        private Stamina _stamina;
        private Hunger _hunger;
        private Thirst _thirst;
        private Fatigue _fatigue;
        private List<ICondition> _allStats;
        
        private int _lastMinute = -1;
        // private int _lastSecond = -1;
        
        private bool _isUpdating;
        #endregion

        #region IPlayerSubController Implementation
        public void InitializeBehaviours(PlayerController playerController, PlayerData playerData)
        {
            _playerController = playerController;
            _playerData = playerData;
        }

        public void PlayerAwake()
        {
            InitializeConditions();
        }
        
        public void PlayerStart()
        {
            StartUpdates();
        }

        public void PlayerOnEnable()
        {
            _isUpdating = true;
            
            if (GameTime.Instance != null)
            {
                GameTime.Instance.OnTimeSkipped += OnTimeSkipped;
            }
            else
            {
                Debug.LogWarning("GameTime instance not found. " +
                                 "Player conditions will not update on time skip.");
            }
        }

        public void PlayerUpdate()
        {
            // FrameBasedUpdates zaten kendi döngüsünde çalışıyor.
            // PlayerController.Update'den buraya sürekli çağrı yapılmasına gerek yok
            // eğer FrameBasedUpdates içindeki gibi bir async loop kullanılıyorsa.
            // Ancak, PlayerData'ya değer atamaları gibi senkronize işler varsa burada yapılabilir.
            // Şimdilik FrameBasedUpdates'in bu işi yaptığı varsayılıyor.
        }

        public void PlayerLateUpdate() { /* Gerekirse implementasyon eklenebilir */ }
        public void PlayerFixedUpdate() { /* Gerekirse implementasyon eklenebilir */ }

        public void PlayerOnDisable()
        {
            _isUpdating = false; // Async loop'ların durmasını sağlar
            
            if (GameTime.Instance != null)
            {
                GameTime.Instance.OnTimeSkipped -= OnTimeSkipped;
            }
        }
        
        public void PlayerOnDestroy() { /* Gerekirse implementasyon eklenebilir */ }

#if UNITY_EDITOR
        public void PlayerOnDrawGizmos() { /* Gerekirse implementasyon eklenebilir */ }
#endif
        #endregion
        
        private void InitializeConditions()
        {
            Health = new Health(100f, 1.0f);
            Stamina = new Stamina(100f, 1f, 1f);
            Hunger = new Hunger(100f, 0.1f);
            Thirst = new Thirst(100f, 0.3f);
            Fatigue = new Fatigue(100f, 0.1f);
            
            _allStats = new List<ICondition> { Health, Stamina, Hunger, Thirst, Fatigue };
        }
        
        private void StartUpdates()
        {
            FrameBasedUpdates().Forget();
            MinuteBasedUpdates().Forget();
            // SecondBasedUpdates().Forget(); // Kullanılmıyorsa kaldırıldı
        }
        
        #region Update Methods
        private async UniTaskVoid FrameBasedUpdates()
        {
            while (_isUpdating) // _isUpdating kontrolü eklendi
            {
                var delta = Time.deltaTime;
                Health.UpdateStat(delta);
                
                // PlayerData güncellemeleri
                _playerData.health = Health.Value;
                _playerData.stamina = Stamina.Value;
                _playerData.hunger = Hunger.Value;
                _playerData.thirst = Thirst.Value;
                _playerData.fatigue = Fatigue.Value;
                
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }
        
        // SecondBasedUpdates kaldırıldı, MinuteBasedUpdates benzer işlevi görüyor gibi.
        // Eğer saniye bazlı ayrı bir mantık gerekirse geri eklenebilir.
        // private async UniTaskVoid SecondBasedUpdates() ...
        
        private async UniTaskVoid MinuteBasedUpdates()
        {
            while (_isUpdating) // _isUpdating kontrolü eklendi
            {
                if (GameTime.Instance == null)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update); // GameTime yoksa bekle
                    continue;
                }

                var currentMinute = GameTime.Instance.GameMinute;
                if (currentMinute != _lastMinute)
                {
                    _lastMinute = currentMinute;
                    UpdateNonVitalStats(1.0f); // Her oyun dakikasında 1 birim etki
                }
                
                // Bir sonraki oyun saniyesini bekle (veya daha uygun bir bekleme süresi)
                // Eğer bu çok sık ise ve performansı etkiliyorsa, bekleme süresi artırılabilir.
                await GameTime.Instance.WaitForGameSecond();
            }
        }
        
        private void UpdateNonVitalStats(float amount)
        {
            foreach (var stat in _allStats)
            {
                if (stat != Health && stat != Stamina) // Stamina otomatik güncellenmiyor, kendi mantığı var
                    stat.UpdateStat(amount);
            }
        }
        #endregion

        #region Public Interaction Methods
        public void Eat(float amount) => Hunger.Decrease(amount);
        public void Drink(float amount) => Thirst.Decrease(amount);
        public void Sleep(float hours) => Fatigue.Decrease(hours * 20f); // Yorgunluğu saat başına 20 azaltır
        #endregion

        #region Condition Modifiers
        public void SetRunning(bool isRunning)
        {
            Hunger.Modifier = isRunning ? 1.2f : 1f;
            Thirst.Modifier = isRunning ? 1.6f : 1f;
            // Stamina tüketimi zaten MovementController veya Stamina class'ı tarafından yönetiliyor olabilir.
            // Eğer HealthController'dan da yönetilmesi gerekiyorsa Stamina.IsSprinting gibi bir özellik kullanılabilir.
        }
        
        public void SetOutsideTemperature(float temperature)
        {
            Thirst.Modifier = temperature > 35f ? 1.5f : 1f;
        }
        
        public void OnTimeChanged(int gameHour)
        {
            Fatigue.Modifier = (gameHour >= 22 || gameHour <= 6) ? 1.4f : 1f;
        }
        #endregion
        
        #region Time Skip Handler
        private void OnTimeSkipped(float hoursSkipped)
        {
            Debug.Log($"Time skipped: {hoursSkipped} hours. Updating player conditions...");
    
            var minutesSkipped = hoursSkipped * 60f;
    
            foreach (var stat in _allStats)
            {
                if (stat != Health && stat != Stamina)
                {
                    // Her dakika için 1.0f değerinde bir etki (UpdateNonVitalStats ile aynı mantık)
                    // Bu, stat'ın kendi UpdateStat metodundaki deltaTime (veya benzeri) mantığına göre olmalı.
                    // Mevcut UpdateNonVitalStats(1.0f) çağrısı her oyun dakikası için.
                    // Bu yüzden minutesSkipped kadar bu etkiyi uygulamalıyız.
                    // ICondition.UpdateStat genellikle deltaTime alır.
                    // Burada her bir dakika için simülasyon yapıyoruz.
                    // Eğer ICondition.UpdateStat doğrudan ne kadar azaltılacağını/artırılacağını alıyorsa,
                    // o zaman her stat için (DecreaseRate * Modifier * minutesSkipped) gibi bir hesaplama yapılmalı.
                    // Mevcut Hunger, Thirst, Fatigue UpdateStat'ları deltaTime alıyor ve kendi içlerinde rate * modifier * deltaTime yapıyor.
                    // Bu durumda, doğrudan minutesSkipped * (dakika başına etki) kadar güncellemeliyiz.
                    // UpdateNonVitalStats'taki 'amount' parametresi, her çağrıldığında statların ne kadar değişeceğini belirtiyor.
                    // Bu durumda, OnTimeSkipped'de, her atlanan dakika için bu 'amount' kadar güncelleme yapmalıyız.
                    // Eğer UpdateStat(float deltaTime) ise ve her oyun dakikasında UpdateNonVitalStats(1.0f) çağrılıyorsa,
                    // ve UpdateNonVitalStats içindeki UpdateStat(amount) ise, bu durumda 'amount' 1.0f oluyor.
                    // Yani her oyun dakikasında statlar 'BaseDecreaseRate * Modifier * 1.0f' kadar azalıyor.
                    // Öyleyse, atlanılan her dakika için bu etkiyi uygulamalıyız.
                    // stat.Decrease(stat.BaseDecreaseRate * stat.Modifier * minutesSkipped); // Bu daha doğru olabilir.
                    // Ancak mevcut UpdateStat'lar deltaTime alıp rate ile çarpıyor.
                    // Ve UpdateNonVitalStats(1.0f) çağrısı ile 1.0f * rate * mod kadar azalıyorlar her dakika.
                    // Bu durumda, minutesSkipped kadar bu işlemi tekrarlamış gibi olmalıyız.
                    
                    // Mevcut UpdateStat, bir "tick" için ne kadar etkileneceğini belirtir.
                    // MinuteBasedUpdates'te UpdateNonVitalStats(1.0f) çağrılıyor,
                    // ve UpdateNonVitalStats içindeki stat.UpdateStat(amount) çağrısı
                    // örneğin Hunger için: Value -= BaseDecreaseRate * Modifier * amount;
                    // Burada amount = 1.0f. Yani her oyun dakikası için Value -= BaseDecreaseRate * Modifier;
                    // Dolayısıyla, atlanan her dakika için bu işlemi uygulamalıyız.
                    // var decreaseAmount = 0f; // Bu, ICondition'a göre değişir.
                                             // Şimdilik, stat.UpdateStat(minutesSkipped) çağrısını varsayalım,
                                             // ve ICondition implementasyonları (Hunger, Thirst, Fatigue)
                                             // bu 'minutesSkipped' değerini uygun şekilde yorumlasın.
                                             // Örneğin, UpdateStat(float timePassedInMinutes) gibi.
                                             // Mevcut UpdateStat(float deltaTime) bunu doğrudan desteklemiyor.

                    // En basit yaklaşım, her atlanan dakika için bir kez UpdateNonVitalStats(1.0f)'in etkisini simüle etmek:
                    if (stat is Hunger hunger) hunger.Decrease(hunger.BaseDecreaseRate * hunger.Modifier * minutesSkipped);
                    else if (stat is Thirst thirst) thirst.Decrease(thirst.BaseDecreaseRate * thirst.Modifier * minutesSkipped);
                    else if (stat is Fatigue fatigue) fatigue.Decrease(fatigue.BaseDecreaseRate * fatigue.Modifier * minutesSkipped);
                    // Ya da ICondition.UpdateStat metodunu, geçen süreyi (dakika cinsinden) alacak şekilde yeniden tasarlamak.
                    // Şimdilik yukarıdaki gibi doğrudan azaltma yapalım.
                    // Veya, ICondition.UpdateStat'ı şu şekilde kullanabiliriz, her bir stat için rate'leri farklı olduğundan:
                    // stat.UpdateStat(minutesSkipped); // Eğer UpdateStat, BaseDecreaseRate * Modifier * deltaTime yapıyorsa ve biz deltaTime yerine minutesSkipped veriyorsak.
                    // Bu, her stat'ın kendi BaseDecreaseRate ve Modifier'ını kullanarak azalmasını sağlar.
                    
                    // Önceki `stat.UpdateStat(minutesSkipped);` çağrısı doğruydu, çünkü ICondition'daki
                    // Hunger, Thirst, Fatigue sınıflarının UpdateStat metotları
                    // Value -= BaseDecreaseRate * Modifier * deltaTime; şeklinde çalışıyor.
                    // Burada deltaTime yerine minutesSkipped vererek doğru hesaplamayı yapmış oluruz.
                    stat.UpdateStat(minutesSkipped);

                    Debug.Log($"Updated {stat.GetType().Name} after time skip. New value: {stat.Value}");
                }
            }
        }
        #endregion
    }
}