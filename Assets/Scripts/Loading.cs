using UnityEngine;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour
{
   private void Start() {
      SceneManager.LoadScene("MenuScene");
   }
}
