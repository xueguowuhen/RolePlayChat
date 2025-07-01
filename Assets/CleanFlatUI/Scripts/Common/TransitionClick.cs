using UnityEngine;
using UnityEngine.EventSystems;

namespace RainbowArt.CleanFlatUI
{
    public class TransitionClick : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        Animator animator;

        public void OnPointerClick(PointerEventData eventData)
        {
            animator.Play("Transition", 0, 0);
        }
    }
}