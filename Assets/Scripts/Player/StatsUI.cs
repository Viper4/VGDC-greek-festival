using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StatsUI : MonoBehaviour
{
    [SerializeField] RectTransform healthMask;
    [SerializeField] RectTransform heartsParent;
    [SerializeField] GameObject heartPrefab;

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
        deathsStartColor.a = 1;
        deathsEndColor = deathsStartColor;
        deathsEndColor.a = 0;

        soulsStartColor = soulsIcon.color;
        soulsStartColor.a = 1;
        soulsEndColor = soulsStartColor;
        soulsEndColor.a = 0;

        deathsIcon.color = deathsEndColor;
        deathsText.color = deathsEndColor;
        soulsIcon.color = soulsEndColor;
        soulsText.color = soulsEndColor;
    }

    public void SetHealth(float healthPercent)
    {
        healthMask.sizeDelta = new Vector2(healthPercent * heartsParent.sizeDelta.x, healthMask.sizeDelta.y);
    }

    public void SetMaxHealth(float maxHealth)
    {
        for (int i = heartsParent.childCount - 1; i >= 0; i--)
        {
            Destroy(heartsParent.GetChild(i).gameObject);
        }
        for (int i = 0; i < maxHealth; i++)
        {
            Instantiate(heartPrefab, heartsParent);
        }
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
