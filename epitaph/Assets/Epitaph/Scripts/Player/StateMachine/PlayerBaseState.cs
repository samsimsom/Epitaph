namespace Epitaph.Scripts.Player.StateMachine
{
    public abstract class PlayerBaseState
    {
        protected PlayerStateMachine Ctx; // Context (PlayerStateMachine) referansı
        protected PlayerStateFactory Factory; // State'leri oluşturmak için factory referansı

        public PlayerBaseState(PlayerStateMachine currentContext, 
            PlayerStateFactory playerStateFactory)
        {
            Ctx = currentContext;
            Factory = playerStateFactory;
        }

        public abstract void EnterState();
        public abstract void UpdateState();
        public abstract void FixedUpdateState(); // Fizik güncellemeleri için
        public abstract void ExitState();
        public abstract void CheckSwitchStates(); // Durum geçişlerini kontrol et
        public abstract void InitializeSubState(); // Alt durumları başlatmak için (opsiyonel)
    }
}