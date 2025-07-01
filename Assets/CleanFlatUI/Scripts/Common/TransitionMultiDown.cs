using UnityEngine;
using UnityEngine.EventSystems;

namespace RainbowArt.CleanFlatUI
{
    public class TransitionMultiDown : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField]
        Animator[] animators;

        public void OnPointerDown(PointerEventData eventData)
        {
            for (int i = 0; i < animators.Length; i++)
            {
                Animator animator = animators[i];
                animator.Play("Transition", 0, 0);
            }
        }
    }
}