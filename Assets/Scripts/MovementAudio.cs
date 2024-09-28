using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class MovementAudio : MonoBehaviour
{
    AudioSource audioSource;

    [SerializeField] AudioClip doubleJumpClip;
    [SerializeField] AudioClip dashClip;
    [SerializeField] AudioClip crouchClip;

    // Cant serialize a dictionary so we do this stupid shit
    [SerializeField, Header("Running")] AudioClip[] dirtRunClips;
    [SerializeField] AudioClip[] gravelRunClips;
    [SerializeField] AudioClip[] metalV1RunClips;
    [SerializeField] AudioClip[] metalV2RunClips;
    [SerializeField] AudioClip[] mudRunClips;
    [SerializeField] AudioClip[] rockRunClips;
    [SerializeField] AudioClip[] sandRunClips;
    [SerializeField] AudioClip[] tileRunClips;
    Dictionary<string, AudioClip[]> footstepRunClips = new Dictionary<string, AudioClip[]>();
    [SerializeField] float runVolume = 1f;
    public float runStepInterval = 0.1f;

    [SerializeField, Header("Walking")] AudioClip[] dirtWalkClips;
    [SerializeField] AudioClip[] gravelWalkClips;
    [SerializeField] AudioClip[] metalV1WalkClips;
    [SerializeField] AudioClip[] metalV2WalkClips;
    [SerializeField] AudioClip[] mudWalkClips;
    [SerializeField] AudioClip[] rockWalkClips;
    [SerializeField] AudioClip[] sandWalkClips;
    [SerializeField] AudioClip[] tileWalkClips;
    Dictionary<string, AudioClip[]> footstepWalkClips = new Dictionary<string, AudioClip[]>();
    [SerializeField] float walkVolume = 0.75f;
    public float walkStepInterval = 0.25f;

    [SerializeField, Header("Jumping")] AudioClip[] dirtJumpClips;
    [SerializeField] AudioClip[] gravelJumpClips;
    [SerializeField] AudioClip[] metalV1JumpClips;
    [SerializeField] AudioClip[] metalV2JumpClips;
    [SerializeField] AudioClip[] mudJumpClips;
    [SerializeField] AudioClip[] rockJumpClips;
    [SerializeField] AudioClip[] sandJumpClips;
    [SerializeField] AudioClip[] tileJumpClips;
    Dictionary<string, AudioClip[]> jumpClips = new Dictionary<string, AudioClip[]>();
    [SerializeField] float jumpVolume;

    [SerializeField, Header("Landing")] AudioClip[] dirtLandClips;
    [SerializeField] AudioClip[] gravelLandClips;
    [SerializeField] AudioClip[] metalV1LandClips;
    [SerializeField] AudioClip[] metalV2LandClips;
    [SerializeField] AudioClip[] mudLandClips;
    [SerializeField] AudioClip[] rockLandClips;
    [SerializeField] AudioClip[] sandLandClips;
    [SerializeField] AudioClip[] tileLandClips;
    Dictionary<string, AudioClip[]> landClips = new Dictionary<string, AudioClip[]>();
    [SerializeField] float maxLandVolume = 2.5f;

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
            audioSource.PlayOneShot(dirtLandClips[Random.Range(0, dirtLandClips.Length)], Mathf.Clamp(speed, 0, maxLandVolume));
        }
    }
}
