using System;
using UnityEngine;

namespace Epitaph.Scripts.Player.HealthSystem.HealPoint
{
    public class FoodPoint : MonoBehaviour
    {
        [SerializeField] private Material foodMaterial;
        [SerializeField] private Collider foodCollider;
        
        [SerializeField] private float foodAmount = 10f;

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("Food Point Triggered!");
            if (other.gameObject.TryGetComponent<PlayerCondition>(out var playerCondition))
            {
                foodMaterial.color = Color.white;
                playerCondition.Eat(foodAmount);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            Debug.Log("Food Point Triggered!");
            foodMaterial.color = Color.red;
        }
    }
}