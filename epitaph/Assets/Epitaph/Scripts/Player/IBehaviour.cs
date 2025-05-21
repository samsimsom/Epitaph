namespace Epitaph.Scripts.Player
{
    public interface IBehaviour
    {
        public void Awake();
        public void OnEnable() ;
        public void Start();
        public void Update();
        public void LateUpdate();
        public void FixedUpdate();
        public void OnDisable();
        public void OnDestroy();
        
#if UNITY_EDITOR
        public void OnDrawGizmos();
#endif
    }
}