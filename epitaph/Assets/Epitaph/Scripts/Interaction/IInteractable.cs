using UnityEngine;

namespace Epitaph.Scripts.Interaction
{
    public interface IInteractable
    {
        /// <summary>
        /// Called when the player interacts with this object
        /// </summary>
        void Interact();
        
        /// <summary>
        /// Optional method to get the interaction prompt text
        /// </summary>
        /// <returns>Text to display when looking at this interactable</returns>
        string GetInteractionPrompt();
    }
}