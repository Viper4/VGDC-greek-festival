using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthEffects : MonoBehaviour
{
    [System.Serializable]
    private struct AnimationInfo
    {
        public string name;
        public float fadeInDuration;
        public float fadeOutDuration;
        public Color startColor;
        public Color endColor;
        public float startTimeScale;
        public float endTimeScale;
        public bool smoothTimeScale;
        public AudioClip audioClip;
        public GameObject particles;
    }

    private AudioSource audioSource;
    [SerializeField] private Image healthChangeImage;
    [SerializeField] private AnimationInfo[] animations;
    private Dictionary<string, AnimationInfo> animationsByName = new Dictionary<string, AnimationInfo>();
    private bool playingAnimation;

    private float previousHealth = 1f;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        foreach (AnimationInfo animation in animations)
        {
            animationsByName.Add(animation.name, animation);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateHealth(float health)
    {
        if (playingAnimation)
            return;
        float change = health - previousHealth;
        if (change > 0)
        {
            StartCoroutine(PlayAnimation("Heal"));
        }
        else if (change < 0)
        {
            StartCoroutine(PlayAnimation("Damage"));
        }
        previousHealth = health;
    }

    private IEnumerator PlayAnimation(string name)
    {
        playingAnimation = true;
        if (animationsByName[name].audioClip != null)
            audioSource.PlayOneShot(animationsByName[name].audioClip);
        if (animationsByName[name].particles != null)
        {
            Transform newParticles = Instantiate(animationsByName[name].particles).transform;
            newParticles.position = transform.position;
            Destroy(newParticles.gameObject, 5);
        }
        Time.timeScale = animationsByName[name].startTimeScale;
        float timer = 0;
        while (timer < animationsByName[name].fadeInDuration)
        {
            healthChangeImage.color = Color.Lerp(animationsByName[name].startColor, animationsByName[name].endColor, timer / animationsByName[name].fadeInDuration);
            if (animationsByName[name].smoothTimeScale)
            {
                Time.timeScale = Mathf.Lerp(animationsByName[name].startTimeScale, animationsByName[name].endTimeScale, timer / animationsByName[name].fadeInDuration);
            }
            timer += Time.unscaledDeltaTime;
            yield return new WaitForEndOfFrame();
        }
        healthChangeImage.color = animationsByName[name].endColor;
        Time.timeScale = animationsByName[name].endTimeScale;

        timer = 0;
        while (timer < animationsByName[name].fadeOutDuration)
        {
            healthChangeImage.color = Color.Lerp(animationsByName[name].endColor, animationsByName[name].startColor, timer / animationsByName[name].fadeOutDuration);
            timer += Time.unscaledDeltaTime;
            yield return new WaitForEndOfFrame();
        }
        healthChangeImage.color = animationsByName[name].startColor;
        playingAnimation = false;
    }
}
