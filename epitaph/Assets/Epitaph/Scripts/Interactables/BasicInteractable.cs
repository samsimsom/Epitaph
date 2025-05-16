using Epitaph.Scripts.Interaction;
using UnityEngine;

namespace Epitaph.Scripts.Interactables
{
    public class BasicInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string interactionPrompt = "Press E to interact";
        [SerializeField] private string interactionMessage = "You interacted with this object!";
        
        public void Interact()
        {
            Debug.Log(interactionMessage);
            // Add your interaction logic here
        }
        
        public string GetInteractionPrompt()
        {
            return interactionPrompt;
        }
    }
}