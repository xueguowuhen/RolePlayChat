using UnityEngine;
using UnityEngine.EventSystems;

namespace RainbowArt.CleanFlatUI
{
    public class TransitionDown : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField]
        Animator animator;

        public void OnPointerDown(PointerEventData eventData)
        {
            animator.Play("Transition", 0, 0);
        }
    }
}