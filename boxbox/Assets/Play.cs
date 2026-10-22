
using UnityEngine;
using UnityEngine.SceneManagement;

public class Play : MonoBehaviour
{
    public void GoToGame()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
