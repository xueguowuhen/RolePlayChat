using System;
using UnityEngine;
using UnityEngine.UI;

public class UIViewBase : MonoBehaviour
{
    public Action OnShow;
    public void Awake()
    {

        OnAWake();
    }
    protected void BtnClick(GameObject gameObject)
    {
        if (!gameObject.name.Equals("BtnSkill1", StringComparison.CurrentCultureIgnoreCase)
            && !gameObject.name.Equals("BtnSkill2", StringComparison.CurrentCultureIgnoreCase)
            && !gameObject.name.Equals("BtnSkill3", StringComparison.CurrentCultureIgnoreCase)
            && !gameObject.name.Equals("BtnAddHP", StringComparison.CurrentCultureIgnoreCase))
        {
            //AudioEffectMgr.Instance.PlayUIAudioEffect(UIAudioEffectType.ButtonClick);
        }
        OnBtnClick(gameObject);
    }
    public void Start()
    {
        Button[] btnArr = GetComponentsInChildren<Button>(true);
        for (int i = 0; i < btnArr.Length; i++)
        {
            EventTriggerListener.Get(btnArr[i].gameObject).onClick += BtnClick;
        }
        OnStart();
        if (OnShow != null) OnShow();
    }

    public void Update()
    {
        OnUpdate();
    }
    private void OnDestroy()
    {
        BeforeOnDestroy();
    }
    protected virtual void OnAWake()
    {
    }
    protected virtual void OnStart()
    {
    }
    protected virtual void OnUpdate()
    {
    }
    protected virtual void BeforeOnDestroy() { }
    protected virtual void OnBtnClick(GameObject gameObject) { }
}