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
    [SerializeField] Player player;
    [SerializeField] TextMeshProUGUI timePlayedText;
    [SerializeField] TextMeshProUGUI deathsText;
    [SerializeField] TextMeshProUGUI soulsText;

    private void Start()
    {
        holder = transform.Find("Holder").gameObject;
    }

    private void OnEnable()
    {
        Player.playerInput.UI.Pause.performed += (context) => TogglePause();
    }

    private void OnDisable()
    {
        Player.playerInput.UI.Pause.performed -= (context) => TogglePause();
    }

    public void TogglePause()
    {
        if (holder.activeSelf)
        {
            Time.timeScale = 1;
            holder.SetActive(false);
            screenBlur.SetActive(false);
        }
        else
        {
            timePlayedText.text = "Time played: " + Time.time.ToString("0.00");
            deathsText.text = "X " + player.deaths.ToString();
            soulsText.text = "X " + player.soulsSaved.ToString();
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
