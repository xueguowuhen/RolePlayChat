using System;
using UnityEngine;

public class GameRoot : MonoBehaviour
{
    public static GameRoot Instance = null;

    [Header("���Ĺ�����")]
    public DataPersistenceManager dataPersistenceManager;
    public SentisInference SentisInference;
    public MemoryManager memoryManager;               // ���� MemoryManager ����
    public DialogueController dialogueController;
    private void Awake()
    {
        // ������ֵ����ֻ֤����һ�� GameRoot
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitAllModules();
    }

    /// <summary>
    /// ������˳�����γ�ʼ��������ģ��
    /// </summary>
    private void InitAllModules()
    {
        // 1. �־û������ʼ����������ʷ�Ի����������ݵȣ�
        dataPersistenceManager.Init();

        // 2. �ı���������ʼ�����ṩ����������
        SentisInference.Init();

        // 3. �����������ʼ�������� DataPersistenceManager �� SentisInference��
        memoryManager.Init();
      
        //����Ի�����
        UIViewMgr.Instance.OpenWindow(WindowUIType.DialogMain);
    }
}
