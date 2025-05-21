using Epitaph.Scripts.Player.ScriptableObjects;

namespace Epitaph.Scripts.Player
{
    public interface IPlayerSubController
    {
        void InitializeBehaviours(PlayerController playerController, PlayerData playerData);
        void PlayerAwake();
        void PlayerOnEnable();
        void PlayerStart();
        void PlayerUpdate();
        void PlayerLateUpdate();
        void PlayerFixedUpdate();
        void PlayerOnDisable();
        void PlayerOnDestroy();
        
#if UNITY_EDITOR
        void PlayerOnDrawGizmos();
#endif
    }
}