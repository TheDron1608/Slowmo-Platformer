using UnityEngine;
using UnityEngine.SceneManagement;

public class OnStartChangeScene : MonoBehaviour
{
    [SerializeField]
    private string _changeSceneName;

    private void Start()
    {
        SceneManager.LoadScene(_changeSceneName);
    }
}
