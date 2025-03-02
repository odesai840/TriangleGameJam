using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Button : MonoBehaviour
{
    // Start is called before the first frame update
    public void Play()
    {
        SceneManager.LoadSceneAsync("Multiverse");
    }

    public void Credits()
    {
        SceneManager.LoadSceneAsync("Credits");
    }

    public void Back()
    {
        SceneManager.LoadSceneAsync("Title");
    }
}
