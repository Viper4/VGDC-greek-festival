using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class MovementAudio : MonoBehaviour
{
    private AudioSource audioSource;

    [SerializeField] private AudioClip doubleJumpClip;
    [SerializeField] private AudioClip dashClip;
    [SerializeField] private AudioClip crouchClip;

    // Cant serialize a dictionary so we do this stupid shit
    // We have audio clip arrays for each ground type and their respective walk, run, jump, and land sound effects.
    [SerializeField, Header("Running")] private AudioClip[] dirtRunClips;
    [SerializeField] private AudioClip[] gravelRunClips;
    [SerializeField] private AudioClip[] metalV1RunClips;
    [SerializeField] private AudioClip[] metalV2RunClips;
    [SerializeField] private AudioClip[] mudRunClips;
    [SerializeField] private AudioClip[] rockRunClips;
    [SerializeField] private AudioClip[] sandRunClips;
    [SerializeField] private AudioClip[] tileRunClips;
    private Dictionary<string, AudioClip[]> footstepRunClips = new Dictionary<string, AudioClip[]>();
    [SerializeField] private float runVolume = 1f;
    public float runStepInterval = 0.1f;

    [SerializeField, Header("Walking")] private AudioClip[] dirtWalkClips;
    [SerializeField] private AudioClip[] gravelWalkClips;
    [SerializeField] private AudioClip[] metalV1WalkClips;
    [SerializeField] private AudioClip[] metalV2WalkClips;
    [SerializeField] private AudioClip[] mudWalkClips;
    [SerializeField] private AudioClip[] rockWalkClips;
    [SerializeField] private AudioClip[] sandWalkClips;
    [SerializeField] private AudioClip[] tileWalkClips;
    private Dictionary<string, AudioClip[]> footstepWalkClips = new Dictionary<string, AudioClip[]>();
    [SerializeField] private float walkVolume = 0.75f;
    public float walkStepInterval = 0.25f;

    [SerializeField, Header("Jumping")] private AudioClip[] dirtJumpClips;
    [SerializeField] private AudioClip[] gravelJumpClips;
    [SerializeField] private AudioClip[] metalV1JumpClips;
    [SerializeField] private AudioClip[] metalV2JumpClips;
    [SerializeField] private AudioClip[] mudJumpClips;
    [SerializeField] private AudioClip[] rockJumpClips;
    [SerializeField] private AudioClip[] sandJumpClips;
    [SerializeField] private AudioClip[] tileJumpClips;
    private Dictionary<string, AudioClip[]> jumpClips = new Dictionary<string, AudioClip[]>();
    [SerializeField] private float jumpVolume;

    [SerializeField, Header("Landing")] private AudioClip[] dirtLandClips;
    [SerializeField] private AudioClip[] gravelLandClips;
    [SerializeField] private AudioClip[] metalV1LandClips;
    [SerializeField] private AudioClip[] metalV2LandClips;
    [SerializeField] private AudioClip[] mudLandClips;
    [SerializeField] private AudioClip[] rockLandClips;
    [SerializeField] private AudioClip[] sandLandClips;
    [SerializeField] private AudioClip[] tileLandClips;
    private Dictionary<string, AudioClip[]> landClips = new Dictionary<string, AudioClip[]>();
    [SerializeField] private float maxLandVolume = 2.5f;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        footstepRunClips.Add("Dirt", dirtRunClips);
        footstepRunClips.Add("Gravel", gravelRunClips);
        footstepRunClips.Add("MetalV1", metalV1RunClips);
        footstepRunClips.Add("MetalV2", metalV2RunClips);
        footstepRunClips.Add("Mud", mudRunClips);
        footstepRunClips.Add("Rock", rockRunClips);
        footstepRunClips.Add("Sand", sandRunClips);
        footstepRunClips.Add("Tile", tileRunClips);

        footstepWalkClips.Add("Dirt", dirtWalkClips);
        footstepWalkClips.Add("Gravel", gravelWalkClips);
        footstepWalkClips.Add("MetalV1", metalV1WalkClips);
        footstepWalkClips.Add("MetalV2", metalV2WalkClips);
        footstepWalkClips.Add("Mud", mudWalkClips);
        footstepWalkClips.Add("Rock", rockWalkClips);
        footstepWalkClips.Add("Sand", sandWalkClips);
        footstepWalkClips.Add("Tile", tileWalkClips);

        jumpClips.Add("Dirt", dirtJumpClips);
        jumpClips.Add("Gravel", gravelJumpClips);
        jumpClips.Add("MetalV1", metalV1JumpClips);
        jumpClips.Add("MetalV2", metalV2JumpClips);
        jumpClips.Add("Mud", mudJumpClips);
        jumpClips.Add("Rock", rockJumpClips);
        jumpClips.Add("Sand", sandJumpClips);
        jumpClips.Add("Tile", tileJumpClips);

        landClips.Add("Dirt", dirtLandClips);
        landClips.Add("Gravel", gravelLandClips);
        landClips.Add("MetalV1", metalV1LandClips);
        landClips.Add("MetalV2", metalV2LandClips);
        landClips.Add("Mud", mudLandClips);
        landClips.Add("Rock", rockLandClips);
        landClips.Add("Sand", sandLandClips);
        landClips.Add("Tile", tileLandClips);
    }

    public void PlayDoubleJump()
    {
        audioSource.PlayOneShot(doubleJumpClip);
    }

    public void PlayDash()
    {
        audioSource.PlayOneShot(dashClip);
    }

    public void PlayCrouch()
    {
        audioSource.PlayOneShot(crouchClip);
    }

    public void PlayFootstep(string groundTag, bool walking)
    {
        if (walking)
        {
            if (footstepWalkClips.TryGetValue(groundTag, out var clips))
            {
                audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)], walkVolume);
            }
            else
            {
                // Play dirt sound effect by default if ground tag is unknown
                audioSource.PlayOneShot(dirtWalkClips[Random.Range(0, dirtWalkClips.Length)], walkVolume);
            }
        }
        else
        {
            if (footstepRunClips.TryGetValue(groundTag, out var clips))
            {
                audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)], runVolume);
            }
            else
            {
                // Play dirt sound effect by default if ground tag is unknown
                audioSource.PlayOneShot(dirtRunClips[Random.Range(0, dirtRunClips.Length)], runVolume);
            }
        }
    }

    public void PlayJump(string groundTag)
    {
        if (jumpClips.TryGetValue(groundTag, out var clips))
        {
            audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)], jumpVolume);
        }
        else
        {
            // Play dirt sound effect by default if ground tag is unknown
            audioSource.PlayOneShot(dirtJumpClips[Random.Range(0, dirtJumpClips.Length)], jumpVolume);
        }
    }

    public void PlayLand(string groundTag, float speed)
    {
        if (landClips.TryGetValue(groundTag, out var clips))
        {
            audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)], Mathf.Clamp(speed, 0, maxLandVolume));
        }
        else
        {
            // Play dirt sound effect by default if ground tag is unknown
            audioSource.PlayOneShot(dirtLandClips[Random.Range(0, dirtLandClips.Length)], Mathf.Clamp(speed, 0, maxLandVolume));
        }
    }
}
