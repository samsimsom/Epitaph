using Epitaph.Scripts.Player;
using UnityEngine;

namespace Epitaph.Scripts.HealPoint
{
    public class FoodPoint : MonoBehaviour
    {
        [SerializeField] private Material foodMaterial;
        [SerializeField] private Collider foodCollider;
        
        [SerializeField] private float foodAmount = 10f;

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("Food Point Entered!");
            if (other.gameObject.TryGetComponent<PlayerController>(out var playerController))
            {
                foodMaterial.color = Color.white;
                playerController.HealthController.Eat(foodAmount);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            foodMaterial.color = Color.red;
        }
    }
}