using UnityEngine;

public class OnStartChangeScene : MonoBehaviour
{
    [SerializeField]
    private string _changeSceneName;

    private void Start()
    {
        UIManager.Instance.LoadSceneWithEffect(_changeSceneName);
    }
}
