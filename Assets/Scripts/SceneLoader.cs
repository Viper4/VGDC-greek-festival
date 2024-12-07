using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader instance { get; private set; }

    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider loadingBar;
    [SerializeField] private TextMeshProUGUI loadingText;
    public bool isLoading { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void TriggerLoadScene(string sceneName)
    {
        LoadScene(sceneName, true);
    }

    public void LoadScene(string sceneName, bool trigger = false)
    {
        string scenePath = $"Assets/Scenes/{sceneName}.unity";
        StartCoroutine(LoadRoutine(SceneUtility.GetBuildIndexByScenePath(scenePath), trigger));
    }

    public void LoadScene(int buildIndex)
    {
        StartCoroutine(LoadRoutine(buildIndex));
    }

    private IEnumerator LoadRoutine(int buildIndex, bool trigger = false)
    {
        if(trigger)
            Player.instance.useSavedTransform = false;
        isLoading = true;
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
        isLoading = false;
        if(buildIndex != 0)
        {
            yield return new WaitUntil(() => SceneManager.GetActiveScene().buildIndex == buildIndex);
            SaveSystem.instance.StoreSaveableEntities();
            SaveSystem.instance.RestoreEntityStates();
        }
    }
}
