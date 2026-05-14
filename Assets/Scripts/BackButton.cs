using UnityEngine;
using UnityEngine.SceneManagement;
public class BackButton : MonoBehaviour
{

    public void GoBack()
    {
        SceneManager.LoadScene("LoginScene");
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
