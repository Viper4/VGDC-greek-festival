using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider loadingBar;
    [SerializeField] private TextMeshProUGUI loadingText;

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

    private IEnumerator LoadRoutine(int buildIndex)
    {
        Time.timeScale = 0;
        loadingScreen.SetActive(true);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(buildIndex);
        asyncLoad.allowSceneActivation = false;
        // Async load progress goes from 0 to 0.9
        float progress = asyncLoad.progress / 0.9f;
        while (progress < 0.9f)
        {
            progress = asyncLoad.progress / 0.9f;
            loadingBar.value = progress;
            loadingText.text = Mathf.RoundToInt(progress * 100f) + "%";
            yield return new WaitForEndOfFrame();
        }
        loadingScreen.SetActive(false);
        loadingBar.value = 1;
        loadingText.text = "100%";
        asyncLoad.allowSceneActivation = true;
        Time.timeScale = 1;
    }
}
