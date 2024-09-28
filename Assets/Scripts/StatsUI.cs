using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class StatsUI : MonoBehaviour
{
    Color deathsStartColor;
    Color deathsEndColor;
    [SerializeField] Image deathsIcon;
    [SerializeField] TextMeshProUGUI deathsText;

    Color spiritsStartColor;
    Color spiritsEndColor;
    [SerializeField] Image spiritsIcon;
    [SerializeField] TextMeshProUGUI spiritsText;

    Coroutine popupCoroutine;

    private void Start()
    {
        deathsStartColor = deathsIcon.color;
        deathsEndColor = deathsStartColor;
        deathsEndColor.a = 0;

        spiritsStartColor = spiritsIcon.color;
        spiritsEndColor = spiritsStartColor;
        spiritsEndColor.a = 0;

        deathsIcon.color = deathsEndColor;
        spiritsIcon.color = spiritsEndColor;
    }

    public void PopupUI(int deaths, int spirits, float time)
    {
        if(popupCoroutine != null) 
            StopCoroutine(popupCoroutine);

        deathsText.text = "X " + deaths.ToString();
        spiritsText.text = "X " + spirits.ToString();
        popupCoroutine = StartCoroutine(Popup(time));
    }

    IEnumerator Popup(float time)
    {
        float timer = 0;
        while(timer < time)
        {
            Color lerpedDeathsColor = Color.Lerp(deathsEndColor, deathsStartColor, timer / time);
            deathsIcon.color = lerpedDeathsColor;
            deathsText.color = lerpedDeathsColor;

            Color lerpedSpiritsColor = Color.Lerp(spiritsEndColor, spiritsStartColor, timer / time);
            spiritsIcon.color = lerpedSpiritsColor;
            spiritsText.color = lerpedSpiritsColor;
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(3f);

        timer = 0;
        while (timer < time)
        {
            Color lerpedDeathsColor = Color.Lerp(deathsStartColor, deathsEndColor, timer / time);
            deathsIcon.color = lerpedDeathsColor;
            deathsText.color = lerpedDeathsColor;

            Color lerpedSpiritsColor = Color.Lerp(spiritsStartColor, spiritsEndColor, timer / time);
            spiritsIcon.color = lerpedSpiritsColor;
            spiritsText.color = lerpedSpiritsColor;
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
}
