using UnityEngine;
using UnityEngine.SceneManagement;

public class Core : MonoBehaviour
{
    public void Exit() {
        SceneManager.LoadScene("MenuScene");
    }
}
