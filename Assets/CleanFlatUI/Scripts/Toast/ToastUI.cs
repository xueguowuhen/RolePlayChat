using UnityEngine;
using UnityEngine.UI;
namespace RainbowArt.CleanFlatUI
{
    public class ToastUI : MonoBehaviour
    {
        [SerializeField]
        Button button;

        [SerializeField]
        Toast toast;

        void Start()
        {
            toast.gameObject.SetActive(false);
            button.onClick.AddListener(OnButtonClick);
        }

        public void OnButtonClick()
        {
            toast.ShowToast();
        }
    }
}