// AppManager.cs - ������ڵ�
using UnityEngine;

public class AppManager : MonoBehaviour
{
    private GameRoot _gameRoot;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        _gameRoot = new GameRoot();
        _gameRoot.Initialize();
    }
}