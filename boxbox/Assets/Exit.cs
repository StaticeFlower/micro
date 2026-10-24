using UnityEngine;
using UnityEngine.SceneManagement;

public class Exit : MonoBehaviour
{
    public void GoToGame()
    {
        SceneManager.LoadScene("Sensor");
    }
}
