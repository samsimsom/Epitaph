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

        public virtual void EnterState() {  }
        public virtual void UpdateState() {  }
        public virtual void FixedUpdateState() {  } // Fizik güncellemeleri için
        public virtual void ExitState() {  }
        public virtual void CheckSwitchStates() {  } // Durum geçişlerini kontrol et
        public virtual void InitializeSubState() {  } // Alt durumları başlatmak için (opsiyonel)
    }
}