using UnityEngine;
using UnityEngine.SceneManagement;

public class logout : MonoBehaviour
{
    public void GoToGame()
    {
        SceneManager.LoadScene("login scren");
    }
}
