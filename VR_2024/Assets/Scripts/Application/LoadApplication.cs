using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadApplication : MonoBehaviour
{
    
   public void LoadScene(string sceneName)
       {
           SceneManager.LoadScene(sceneName);
       }
    
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1f;
    }
}
