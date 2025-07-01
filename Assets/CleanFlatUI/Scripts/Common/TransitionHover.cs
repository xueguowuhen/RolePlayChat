using UnityEngine;
using UnityEngine.EventSystems;

namespace RainbowArt.CleanFlatUI
{
    public class TransitionHover : MonoBehaviour, IPointerEnterHandler
    {
        [SerializeField]
        Animator animator;

        public void OnPointerEnter(PointerEventData eventData)
        {
            animator.Play("Transition", 0, 0);
        }
    }
}