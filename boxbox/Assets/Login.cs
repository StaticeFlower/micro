
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    public void GoToGame()
    {
        SceneManager.LoadScene("Sensor");
    }
}
