using UnityEngine;

public interface IInfiniteVariableListHandler
{
    string GetTemplateId(object data);
    RectTransform GetTemplatePrefab(string templateId);
    void OnBind(RectTransform item, object data);
}
