using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [SerializeField] GameObject loadingScreen;
    [SerializeField] Slider loadingBar;
    [SerializeField] TextMeshProUGUI loadingText;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void LoadScene(string sceneName)
    {
        string scenePath = $"Assets/Scenes/{sceneName}.unity";
        StartCoroutine(LoadRoutine(SceneUtility.GetBuildIndexByScenePath(scenePath)));
    }

    public void LoadScene(int buildIndex)
    {
        StartCoroutine(LoadRoutine(buildIndex));
    }

    IEnumerator LoadRoutine(int buildIndex)
    {
        Time.timeScale = 0;
        loadingScreen.SetActive(true);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(buildIndex);
        asyncLoad.allowSceneActivation = false;
        // Async load progress goes from 0 to 0.9
        float normalizedProgress = asyncLoad.progress / 0.9f;
        Debug.Log("Start");
        while (normalizedProgress < 0.9f)
        {
            normalizedProgress = asyncLoad.progress / 0.9f;
            loadingBar.value = normalizedProgress;
            loadingText.text = Mathf.RoundToInt(normalizedProgress * 100f) + "%";
            yield return new WaitForEndOfFrame();
        }
        Debug.Log("End");
        loadingScreen.SetActive(false);
        loadingBar.value = 1;
        loadingText.text = "100%";
        asyncLoad.allowSceneActivation = true;
        Time.timeScale = 1;
    }
}
