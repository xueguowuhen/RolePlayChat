using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfiniteVariableListCore
{
    // ScrollRect 组件，用于监听滑动事件和获取视图区域
    private ScrollRect scrollRect;
    // ScrollRect.content 对应的 RectTransform，所有列表项都作为其子节点
    private RectTransform content;

    // 原始数据列表
    private List<object> dataList = new();
    // 根据数据获取模板 ID 的委托
    private Func<object, string> getTemplateId;
    // 根据模板 ID 获取对应预制体的委托
    private Func<string, RectTransform> getTemplatePrefab;
    // 更新列表项绑定数据的回调
    private Action<RectTransform, object> onBindItem;

    // 缓存所有模板预制体，避免重复加载
    private Dictionary<string, RectTransform> prefabCache = new();
    // 为每种模板类型维护一个对象池
    private Dictionary<string, ObjectPool<RectTransform>> poolMap = new();
    // 为每种模板类型维护一个隐藏的 Probe，用于测量高度
    private Dictionary<string, RectTransform> probeCache = new();

    // 存储每项高度，<=0 表示尚未测量
    private float[] itemHeights;
    // 前缀和列表，第 i+1 个元素为第 0..i 项总高度
    private List<float> prefixSum = new List<float> { 0 };

    // 当前激活（可见）列表项的实例
    private List<RectTransform> activeItems = new();
    // 第一个可见项在 dataList 中的索引
    private int firstVisibleIndex = 0;
    // 池中最多缓存的可见项数量
    private int poolSize = 0;

    // 滚动节流相关：记录上次处理的纵向偏移
    private float lastScrollY;
    // 只有当滚动超过该阈值时才触发重建
    private const float scrollThreshold = 5f;

    /// <summary>
    /// 初始化方法，需传入 ScrollRect、数据、委托等
    /// </summary>
    public void Init(
        ScrollRect scroll,
        IList<object> data,
        Func<object, string> getTemplateId,
        Func<string, RectTransform> getTemplatePrefab,
        Action<RectTransform, object> onBindItem)
    {
        scrollRect = scroll;
        content = scroll.content;
        this.getTemplateId = getTemplateId;
        this.getTemplatePrefab = getTemplatePrefab;
        this.onBindItem = onBindItem;

        // 复制一份数据列表，并初始化高度数组
        dataList = new List<object>(data);
        itemHeights = new float[dataList.Count];
        Array.Fill(itemHeights, -1f); // 用 -1 表示未测量
        prefixSum = new List<float> { 0 };

        // 绑定节流后的滚动回调
        scrollRect.onValueChanged.RemoveAllListeners();
        scrollRect.onValueChanged.AddListener(OnScrollThrottled);

        // 预热模板和对象池，计算前缀和，生成首屏
        InitTemplateCaches();
        RecalculatePrefixSum();
        Rebuild();
    }

    /// <summary>
    /// 向列表末尾添加数据，并扩展结构
    /// </summary>
    public void AddData(object data)
    {
        dataList.Add(data);
        Array.Resize(ref itemHeights, dataList.Count);// 扩展高度数组
        itemHeights[^1] = -1f;
        // 前缀和尾部初始继承上一项累计高度
        prefixSum.Add(prefixSum[^1]);
        InitTemplateCaches();
        Rebuild();
    }

    /// <summary>
    /// 清空列表和缓存
    /// </summary>
    public void Clear()
    {
        dataList.Clear();
        itemHeights = Array.Empty<float>();
        prefixSum = new List<float> { 0 };
        foreach (var item in activeItems)
            item.gameObject.SetActive(false);
        activeItems.Clear();
        firstVisibleIndex = 0;
        content.sizeDelta = Vector2.zero;
    }

    /// <summary>
    /// 重新测量所有项并重建
    /// </summary>
    public void RefreshAll()
    {
        Array.Fill(itemHeights, -1f);
        RecalculatePrefixSum();
        Rebuild();
    }

    public bool Contains(object data) => dataList.Contains(data);
    public int Count => dataList.Count;

    /// <summary>
    /// 更新指定位置的数据和高度
    /// </summary>
    public void UpdateDataAt(int index, object data)
    {
        dataList[index] = data;
        itemHeights[index] = -1f; // 标记需重测
        RecalculatePrefixSum(index);
        // 若该项当前可见，立即刷新显示
        if (index >= firstVisibleIndex && index < firstVisibleIndex + activeItems.Count)
            UpdateItem(activeItems[index - firstVisibleIndex], index);
        // 更新 content 总高度
        content.sizeDelta = new Vector2(content.sizeDelta.x, prefixSum[^1]);
    }

    /// <summary>
    /// 为所有模板预制体创建对象池，限容防止过多缓存
    /// </summary>
    private void InitTemplateCaches()
    {
        for (int i = 0; i < dataList.Count; i++)
        {
            string id = getTemplateId(dataList[i]);
            if (!prefabCache.ContainsKey(id))
            {
                var prefab = getTemplatePrefab(id);
                prefabCache[id] = prefab;
                poolMap[id] = new ObjectPool<RectTransform>(
                    () => UnityEngine.Object.Instantiate(prefab),
                    maxSize: 50, // 池最大缓存
                    parent: content);
            }
        }
    }

    /// <summary>
    /// 获取并缓存第 index 项的高度，若未测量则使用 Probe 实例化测量
    /// </summary>
    private float GetItemHeight(int index)
    {
        if (itemHeights[index] > 0)
            return itemHeights[index];

        string id = getTemplateId(dataList[index]);
        if (!probeCache.TryGetValue(id, out var probe))
        {
            // 第一次为该模板创建一个隐藏 Probe
            probe = UnityEngine.Object.Instantiate(prefabCache[id]);
            probe.name = $"_Probe_{id}";
            probe.SetParent(scrollRect.transform, false);
            probe.gameObject.SetActive(false);
            probeCache[id] = probe;
        }

        // 绑定数据并强制重建布局，获取首选高度
        probe.gameObject.SetActive(true);
        probe.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, content.rect.width);
        onBindItem(probe, dataList[index]);
        LayoutRebuilder.ForceRebuildLayoutImmediate(probe);

        float h = LayoutUtility.GetPreferredHeight(probe);
        itemHeights[index] = h;
        probe.gameObject.SetActive(false);
        return h;
    }

    /// <summary>
    /// 计算或更新 prefixSum 数组
    /// </summary>
    private void RecalculatePrefixSum(int from = 0)
    {
        if (prefixSum.Count != dataList.Count + 1)
        {
            // 全量重建前缀和
            prefixSum = new List<float> { 0 };
            for (int i = 0; i < dataList.Count; i++)
                prefixSum.Add(prefixSum[^1] + Mathf.Max(0, itemHeights[i] > 0 ? itemHeights[i] : 0));
        }
        else
        {
            // 增量更新 from+1 之后的值
            for (int i = from + 1; i < prefixSum.Count; i++)
                prefixSum[i] = prefixSum[i - 1] + (itemHeights[i - 1] > 0 ? itemHeights[i - 1] : GetItemHeight(i - 1));
        }
    }

    /// <summary>
    /// 根据当前滚动状态回收/创建可见项
    /// </summary>
    private void Rebuild()
    {
        // 回收所有现有项
        foreach (var item in activeItems)
        {
            string id = getTemplateId(dataList[firstVisibleIndex + activeItems.IndexOf(item)]);
            poolMap[id].Release(item);
        }
        activeItems.Clear();

        // 确保 prefixSum 最新
        RecalculatePrefixSum();

        // 根据视图高度初算需要的项数
        float viewH = scrollRect.viewport.rect.height;
        float acc = 0; int count = 0;
        while (count < dataList.Count && acc < viewH)
        {
            acc += GetItemHeight(count);
            count++;
        }
        poolSize = Mathf.Min(count + 2, dataList.Count);

        // 更新 content 总高度
        content.sizeDelta = new Vector2(content.sizeDelta.x, prefixSum[^1]);

        // 重置首屏
        firstVisibleIndex = 0;
        for (int i = 0; i < poolSize; i++)
            activeItems.Add(CreateItem(i));
    }

    /// <summary>
    /// 从对象池获取一个项并绑定
    /// </summary>
    private RectTransform CreateItem(int idx)
    {
        var id = getTemplateId(dataList[idx]);
        var item = poolMap[id].Get();
        item.SetParent(content, false);
        item.gameObject.SetActive(true);
        UpdateItem(item, idx);
        return item;
    }

    /// <summary>
    /// 更新单个项的位置和尺寸
    /// </summary>
    private void UpdateItem(RectTransform item, int idx)
    {
        onBindItem(item, dataList[idx]);
        float y = -prefixSum[idx];
        item.anchoredPosition = new Vector2(0, y);
        float h = GetItemHeight(idx);
        item.sizeDelta = new Vector2(content.rect.width, h);
    }

    /// <summary>
    /// 滚动回调（带阈值节流）
    /// </summary>
    private void OnScrollThrottled(Vector2 _)
    {
        float y = content.anchoredPosition.y;
        if (Mathf.Abs(y - lastScrollY) < scrollThreshold) return;
        lastScrollY = y;
        HandleScroll(y);
    }

    /// <summary>
    /// 真正处理滚动：回收屏外项并补齐新项
    /// </summary>
    private void HandleScroll(float scrollY)
    {
        int newFirst = FindFirstVisibleIndex(scrollY);
        if (newFirst == firstVisibleIndex) return;

        // 动态计算 poolSize（防止 viewport 变化或 itemHeight 更新时显示不足）
        float viewH = scrollRect.viewport.rect.height;
        float acc = 0; int count = 0;
        for (int i = newFirst; i < dataList.Count && acc < viewH; i++)
        {
            acc += GetItemHeight(i);
            count++;
        }
        poolSize = Mathf.Min(count + 2, dataList.Count - newFirst);

        // 回收旧的 activeItems
        for (int i = activeItems.Count - 1; i >= 0; i--)
        {
            int dataIdx = firstVisibleIndex + i;
            string id = getTemplateId(dataList[dataIdx]);
            poolMap[id].Release(activeItems[i]);
            activeItems.RemoveAt(i);
        }

        // 更新索引并创建新的项
        firstVisibleIndex = Mathf.Clamp(newFirst, 0, dataList.Count - poolSize);
        for (int i = 0; i < poolSize; i++)
        {
            int idx = firstVisibleIndex + i;
            if (idx >= dataList.Count) break;
            activeItems.Add(CreateItem(idx));
        }
    }


    /// <summary>
    /// 二分查找第一个可见项的索引
    /// </summary>
    private int FindFirstVisibleIndex(float offsetY)
    {
        int low = 0, high = dataList.Count - 1;
        while (low < high)
        {
            int mid = (low + high) / 2;
            if (prefixSum[mid] <= offsetY) low = mid + 1;
            else high = mid;
        }
        return Mathf.Clamp(low - 1, 0, dataList.Count - 1);
    }
}