using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StatsUI : MonoBehaviour
{
    Color deathsStartColor;
    Color deathsEndColor;
    [SerializeField] Image deathsIcon;
    [SerializeField] TextMeshProUGUI deathsText;

    Color soulsStartColor;
    Color soulsEndColor;
    [SerializeField] Image soulsIcon;
    [SerializeField] TextMeshProUGUI soulsText;

    Coroutine popupCoroutine;

    private void Start()
    {
        deathsStartColor = deathsIcon.color;
        deathsEndColor = deathsStartColor;
        deathsEndColor.a = 0;

        soulsStartColor = soulsIcon.color;
        soulsEndColor = soulsStartColor;
        soulsEndColor.a = 0;

        deathsIcon.color = deathsEndColor;
        soulsIcon.color = soulsEndColor;
    }

    public void PopupUI(int deaths, int souls, float time)
    {
        if(popupCoroutine != null) 
            StopCoroutine(popupCoroutine);

        deathsText.text = "X " + deaths.ToString();
        soulsText.text = "X " + souls.ToString();
        popupCoroutine = StartCoroutine(Popup(time));
    }

    IEnumerator Popup(float time)
    {
        // Fade icons and text from 0 alpha to 1
        float timer = 0;
        while(timer < time)
        {
            Color lerpedDeathsColor = Color.Lerp(deathsEndColor, deathsStartColor, timer / time);
            deathsIcon.color = lerpedDeathsColor;
            deathsText.color = lerpedDeathsColor;

            Color lerpedSpiritsColor = Color.Lerp(soulsEndColor, soulsStartColor, timer / time);
            soulsIcon.color = lerpedSpiritsColor;
            soulsText.color = lerpedSpiritsColor;
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(3f);

        // Fade icons and text from 1 alpha to 0
        timer = 0;
        while (timer < time)
        {
            Color lerpedDeathsColor = Color.Lerp(deathsStartColor, deathsEndColor, timer / time);
            deathsIcon.color = lerpedDeathsColor;
            deathsText.color = lerpedDeathsColor;

            Color lerpedSpiritsColor = Color.Lerp(soulsStartColor, soulsEndColor, timer / time);
            soulsIcon.color = lerpedSpiritsColor;
            soulsText.color = lerpedSpiritsColor;
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
}
