using UnityEngine;
using UnityEngine.UI;

public static class GameObjectUtil
{
    /// <summary>
    /// 获取或创建组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="str"></param>
    /// <returns></returns>
    public static T GetOrCreatComponet<T>(this GameObject str) where T : MonoBehaviour
    {
        T t = str.GetComponent<T>();
        if (t == null)
        {
            t = str.AddComponent<T>();
        }

        return t;
    }
    public static void SetNull(this MonoBehaviour[] arr)
    {
        if (arr != null)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = null;
            }
            arr = null;
        }
    }
    public static void SetNull(this Transform[] arr)
    {
        if (arr != null)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = null;
            }
            arr = null;
        }
    }
    public static void SetNull(this Sprite[] arr)
    {
        if (arr != null)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = null;
            }
            arr = null;
        }
    }
    public static void SetInfo(this GameObject obj, Transform parent)
    {
        obj.transform.SetParent(parent);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;
        obj.transform.localEulerAngles = Vector3.zero;
    }

    #region UI扩展

    public static void SetSliderValue(this Slider sliderObj, float value)
    {
        if (sliderObj != null)
        {
            sliderObj.value = value;
        }
    }
    #endregion
}