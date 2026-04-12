using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Loading_Screen : MonoBehaviour
{
    [SerializeField]
    GameObject LoadingScreen;
    [SerializeField]
    public Image LoadFillBar;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
       
    }


    public void LoadScene(int sceneID)
    {
        StartCoroutine(LoadSceneAsync(sceneID));
    }
    // Update is called once per frame
    IEnumerator LoadSceneAsync(int sceneID)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneID);

        LoadingScreen.SetActive(true);

        while (!operation.isDone)
        {

            float progressValue = Mathf.Clamp01(operation.progress/0.9f);
            LoadFillBar.fillAmount = progressValue;

            yield return null;
           

        }

        LoadingScreen.SetActive(false);
        


    }
}
