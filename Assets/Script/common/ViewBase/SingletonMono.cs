using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    #region 单例
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject(typeof(T).Name);
                DontDestroyOnLoad(go);
                instance = go.GetOrCreatComponet<T>();
            }
            return instance;
        }
    }
    #endregion
    void Awake()
    {
        OnAwake();
    }
    void Start()
    {
        OnStart();
    }
    void Update()
    {
        OnUpdate();
    }
    private void OnDestroy()
    {
        BeforeOnDestroy();
    }
    protected virtual void OnAwake() { }
    protected virtual void OnStart() { }
    protected virtual void OnUpdate() { }
    protected virtual void BeforeOnDestroy() { }

}
