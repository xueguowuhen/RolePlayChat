using UnityEngine;
using UnityEngine.UI;
namespace RainbowArt.CleanFlatUI
{
    public class NotificationContentFitterUI : MonoBehaviour
    {
        [SerializeField]
        Button button;

        [SerializeField]
        NotificationContentFitter notification;

        void Start()
        {
            notification.gameObject.SetActive(false);
            button.onClick.AddListener(OnButtonClick);
        }

        public void OnButtonClick()
        {
            notification.OnCancel.AddListener(NotificationCancel);
            notification.ShowNotification();
            notification.ShowNotification();
        }

        void NotificationCancel()
        {
            Debug.Log("Cancel Button Clicked");
        }
    }
}