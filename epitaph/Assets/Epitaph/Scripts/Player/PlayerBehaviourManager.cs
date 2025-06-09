using System.Collections.Generic;
using System.Linq;
using Epitaph.Scripts.Player.BaseBehaviour;

namespace Epitaph.Scripts.Player
{
    public class PlayerBehaviourManager<T> where T : PlayerBehaviour
    {
        private readonly List<T> _behaviours = new();
        private readonly PlayerController _playerController;

        public PlayerBehaviourManager(PlayerController playerController)
        {
            _playerController = playerController;
        }

        public TBehaviour AddBehaviour<TBehaviour>(TBehaviour behaviour) where TBehaviour : T
        {
            _behaviours.Add(behaviour);
            return behaviour;
        }

        public TBehaviour GetBehaviour<TBehaviour>() where TBehaviour : T
        {
            return _behaviours.OfType<TBehaviour>().FirstOrDefault();
        }

        public IEnumerable<T> GetAllBehaviours() => _behaviours;

        public void ExecuteOnAll(System.Action<T> action)
        {
            foreach (var behaviour in _behaviours)
            {
                action?.Invoke(behaviour);
            }
        }
    }
}