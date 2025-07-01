using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ͨ�ö���أ�ʵ�ֶ� Unity ����ĸ������޶�����
/// </summary>
public class ObjectPool<T> where T : Component
{
    // ���󹤳��������״δ�����ؿ�ʱ������ʵ��
    private readonly Func<T> factory;
    // ���游�ڵ㣨��ѡ������ȡʱ���ʵ���ҵ��ýڵ���
    private readonly Transform parent;
    // �ص���ÿ�δӳ���ȡ������ʱ����
    private readonly Action<T> onGet;
    // �ص���ÿ�ι黹���󵽳���ʱ����
    private readonly Action<T> onRelease;

    // �ڲ�ջ�ṹ�����ڴ洢�ɸ��ö���
    private readonly Stack<T> pool;
    // ��󻺴�����������ʱ�¹黹�Ķ���ᱻ����
    private readonly int maxSize;

    /// <param name="factory">���󴴽�����</param>
    /// <param name="parent">���ж����Ĭ�ϸ��ڵ�</param>
    /// <param name="maxSize">��󻺴�����<=0 ��ʾ������</param>
    /// <param name="onGet">ȡ��ʱ�ص�</param>
    /// <param name="onRelease">�黹ʱ�ص�</param>
    public ObjectPool(
        Func<T> factory,
        Transform parent = null,
        int maxSize = 0,
        Action<T> onGet = null,
        Action<T> onRelease = null)
    {
        this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        this.parent = parent;
        this.maxSize = maxSize;
        this.onGet = onGet;
        this.onRelease = onRelease;
        pool = new Stack<T>(Mathf.Max(4, maxSize > 0 ? maxSize : 4));
    }

    /// <summary>
    /// ��ȡ�������ȴӳ���ȡ��������ʹ�� factory ����
    /// </summary>
    public T Get()
    {
        T element;
        if (pool.Count > 0)
        {
            element = pool.Pop();
        }
        else
        {
            element = factory();
        }

        // ������ø���
        element.gameObject.SetActive(true);
        if (parent != null)
            element.transform.SetParent(parent, false);

        // �ص�
        onGet?.Invoke(element);

        return element;
    }

    /// <summary>
    /// �黹����ִ�лص����Żس��У�������Ѵ���������������ٶ���
    /// </summary>
    public void Release(T element)
    {
        if (element == null) return;

        // �ص�
        onRelease?.Invoke(element);

        // ���ظ��黹
        if (pool.Count > 0 && pool.Contains(element))
        {
            Debug.LogWarning("Attempting to release an object that is already in the pool.");
            return;
        }

        element.gameObject.SetActive(false);

        if (maxSize > 0 && pool.Count >= maxSize)
        {
            // �����������ޣ�ֱ������
            UnityEngine.Object.Destroy(element.gameObject);
        }
        else
        {
            pool.Push(element);
        }
    }

    /// <summary>
    /// ��ճ�������δʹ�õĶ���
    /// </summary>
    public void Clear()
    {
        while (pool.Count > 0)
        {
            var element = pool.Pop();
            if (element != null)
                UnityEngine.Object.Destroy(element.gameObject);
        }
    }

    /// <summary>
    /// ��ǰ���л�������
    /// </summary>
    public int CountInactive => pool.Count;
}
