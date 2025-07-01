using UnityEngine;
using UnityEngine.EventSystems;

namespace RainbowArt.CleanFlatUI
{
    public class EventForward : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField]
        GameObject targetGameObject;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (targetGameObject != null)
            {
                ExecuteEvents.Execute(targetGameObject, eventData, ExecuteEvents.pointerDownHandler);
            }
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            if (targetGameObject != null)
            {
                ExecuteEvents.Execute(targetGameObject, eventData, ExecuteEvents.pointerUpHandler);
            }
        }
    }
}
