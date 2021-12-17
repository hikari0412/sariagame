using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;
public class StartManager : MonoBehaviour
{
    public GameObject loading;
    public void StartGame() {
        loading.SetActive(true);
        SceneManager.LoadSceneAsync("Saria_Main Scene");
    }
    public void EndGame() {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif  
    }
}
