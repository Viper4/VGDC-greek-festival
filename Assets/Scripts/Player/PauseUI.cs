using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PauseUI : MonoBehaviour
{
    GameObject holder;
    [SerializeField] GameObject screenBlur;
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject settingsMenu;
    [SerializeField] TextMeshProUGUI timePlayedText;
    [SerializeField] TextMeshProUGUI deathsText;
    [SerializeField] TextMeshProUGUI soulsText;

    private void Start()
    {
        holder = transform.Find("Holder").gameObject;
    }

    public void AddListener()
    {
        Player.instance.input.UI.Pause.performed += (context) => TogglePause();
    }

    public void RemoveListener()
    {
        Player.instance.input.UI.Pause.performed -= (context) => TogglePause();
    }

    public void TogglePause()
    {
        if (transform == null)
        {
            Destroy(this);
            return;
        }
        if (holder.activeSelf)
        {
            Time.timeScale = 1;
            holder.SetActive(false);
            screenBlur.SetActive(false);
        }
        else
        {
            timePlayedText.text = "Time played: " + Time.time.ToString("0.00");
            deathsText.text = "X " + Player.instance.deaths.ToString();
            soulsText.text = "X " + Player.instance.soulsCollected.ToString();
            Time.timeScale = 0;
            screenBlur.SetActive(true);
            pauseMenu.SetActive(true);
            settingsMenu.SetActive(false);
            holder.SetActive(true);
        }
    }

    public void Quit()
    {
        Application.Quit();
    }
}
