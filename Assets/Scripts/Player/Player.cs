using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : BaseMovement, ISaveable
{
    public static Player instance;
    public PlayerInput input;

    private SpriteRenderer spriteRenderer;
    private HealthSystem healthSystem;
    private Healing healing;

    private WallJumping wallJumping;
    private Dashing dashing;
    private GroundPound groundPound;

    // Coyote time and double jumping
    [Header("Player")]
    [SerializeField] private float coyoteTime = 0.1f;
    private float airTime = 0;
    private bool canCoyoteJump = false;
    private bool canDoubleJump = false;

    // Checkpoints, souls, and stats
    private Checkpoint lastCheckpoint;
    public List<Soul> followingSouls = new List<Soul>();
    [SerializeField] private float deathTime = 1f;
    private bool dying = false;
    [SerializeField] private StatsUI statsUI;
    [SerializeField] private PauseUI pauseUI;

    private HealthSystem previousKill;
    public int totalSoulsCollected = 0;
    public int levelSoulsCollected = 0;
    public int deaths = 0;

    public bool useSavedTransform = true;

    private void OnEnable()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
            input = new PlayerInput();

            input.Enable();

            // Add listeners
            input.Player.Crouch.performed += ctx => Crouch();
            input.Player.Crouch.canceled += ctx => Uncrouch();
            input.Player.Jump.performed += ctx => DoubleJump();
            pauseUI.AddListener();
        }
        else
        {
            instance.followingSouls.Clear();
            instance.lastCheckpoint = null;
            if (!instance.useSavedTransform)
            {
                instance.transform.SetPositionAndRotation(transform.position, transform.rotation);
            }

            Destroy(transform.root.gameObject);
        }
    }

    private void OnDisable()
    {
        if(instance == this)
        {
            input.Disable();

            // Remove listeners
            input.Player.Crouch.performed -= ctx => Crouch();
            input.Player.Crouch.canceled -= ctx => Uncrouch();
            input.Player.Jump.performed -= ctx => DoubleJump();
            pauseUI.RemoveListener();
        }
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        spriteRenderer = GetComponent<SpriteRenderer>();
        healthSystem = GetComponent<HealthSystem>();
        wallJumping = GetComponent<WallJumping>();
        dashing = GetComponent<Dashing>();
        groundPound = GetComponent<GroundPound>();
        healing = GetComponent<Healing>();
    }

    // Called every time the Physics engine updates (not tied to framerate, fixed rate)
    private void FixedUpdate()
    {
        if (Time.timeScale > 0)
        {
            Vector2 newVelocity;
            if (wallJumping.IsJumping)
            {
                newVelocity = rb.velocity;
            }
            else if (dashing.velocity != Vector2.zero)
            {
                newVelocity = dashing.velocity;
                if (dashing.velocity.y == 0 && (dashing.velocity.x > 0 && moveVelocity.x < 0 || dashing.velocity.x < 0 && moveVelocity.x > 0))
                {
                    dashing.velocity = Vector2.zero;
                    newVelocity = moveVelocity;
                }
            }
            else
            {
                newVelocity = moveVelocity;
            }

            if (!Climbing && dashing.velocity.y == 0 && !wallJumping.IsJumping && knockbackVelocity.y == 0)
            {
                newVelocity += new Vector2(0, rb.velocity.y);
            }

            if (wallJumping.IsSliding)
            {
                if(newVelocity.y < -wallJumping.slideSpeed)
                    newVelocity.y = -wallJumping.slideSpeed;
            }

            if (groundPound.velocity != 0)
            {
                newVelocity.x = 0;
                newVelocity.y = groundPound.velocity;
            }

            newVelocity += knockbackVelocity;

            ApplyVelocity(newVelocity);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        MovementUpdate();
        if(Time.timeScale > 0)
        {
            // Read player input for movement
            Crouching = input.Player.Crouch.ReadValue<float>() >= 1f;
            Walking = !Crouching && input.Player.Walk.ReadValue<float>() >= 1f;
            Vector2 moveInput = input.Player.Move.ReadValue<Vector2>();
            moveVelocity = moveInput;
            if (Walking)
            {
                moveVelocity *= walkSpeed;
            }
            else if (Crouching)
            {
                moveVelocity *= crouchSpeed;
            }
            else
            {
                moveVelocity *= runSpeed;
            }

            if (Climbing)
                moveVelocity.y = moveInput.y * climbSpeed;
            else
                moveVelocity.y = 0;

            if (input.Player.Jump.ReadValue<float>() >= 1f)
            {
                Jump();
            }

            if(!IsGrounded)
                airTime += Time.deltaTime;

            if(stairs != null && moveInput.y < 0)
            {
                stairs.Descend(_collider);
                stairs = null;
            }
        }
    }

    private void Crouch()
    {
        if (Time.timeScale > 0)
        {
            movementAudio.PlayCrouch();
            transform.localScale = new Vector3(1, 0.5f, 1);
            transform.position -= new Vector3(0, 0.5f);
            if (!IsGrounded && groundPound.velocity == 0)
                groundPound.StartGroundPound();
        }
    }

    private void Uncrouch()
    {
        if (Time.timeScale > 0 && groundPound.velocity == 0)
        {
            UncrouchAnimation();
        }
    }

    public void UncrouchAnimation(){
        movementAudio.PlayCrouch();
        float deltaY = 1 - transform.localScale.y;
        transform.localScale = new Vector3(1, 1, 1);
        transform.position += new Vector3(0, deltaY);
    }

    private void Jump()
    {
        if (Time.timeScale > 0 && Mathf.Round(rb.velocity.y * 100) / 100 <= 0 && (IsGrounded || (canCoyoteJump && airTime <= coyoteTime)))
        {
            canCoyoteJump = false;
            if(IsGrounded)
                PlayJumpSound();
            rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
            wallJumping.BlockWallJump();
        }
    }

    private void DoubleJump()
    {
        if (Time.timeScale > 0 && !IsGrounded && canDoubleJump && (airTime > coyoteTime || !canCoyoteJump) && dashing.velocity == Vector2.zero)
        {
            canDoubleJump = false;
            movementAudio.PlayDoubleJump();
            rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
        }
    }

    public void KillPlayer()
    {
        if (!dying)
            StartCoroutine(DieAnimation());
    }

    private IEnumerator DieAnimation()
    {
        dying = true; // Stop code from playing animation over and over again
        rb.constraints = RigidbodyConstraints2D.FreezeAll; // Stop player movement

        // Change sprite color to red
        Color oldColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(deathTime);
        // Respawn player at lastCheckpoint
        spriteRenderer.color = oldColor;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        transform.position = lastCheckpoint.transform.position;
        lastCheckpoint.Respawn();

        // Reset all souls that were following
        foreach (Soul followingSoul in followingSouls)
        {
            followingSoul.ResetSoul();
        }
        followingSouls.Clear();
        deaths++;
        statsUI.PopupUI(deaths, levelSoulsCollected, 0.5f); // Show stats
        dying = false;
        IsGrounded = false;
        healthSystem.ResetHealth();
    }

    public void CheckKill(HealthSystem healthSystem)
    {
        if (previousKill != healthSystem && healthSystem.health <= 0)
        {
            healing.unsavedHealing += healthSystem.KillHealAmount;
            previousKill = healthSystem;
        }
    }

    public void SetCheckpoint(Checkpoint checkpoint)
    {
        if(checkpoint.canSaveSouls)
        {
            foreach (Soul followingSoul in followingSouls)
            {
                levelSoulsCollected++;
                totalSoulsCollected++;
                followingSoul.Fade();
            }
            // If we saved new souls show stats
            if (followingSouls.Count > 0)
                statsUI.PopupUI(deaths, levelSoulsCollected, 0.5f);
        }

        // If we're setting a new checkpoint
        if (lastCheckpoint != checkpoint)
        {
            // Show stats
            statsUI.PopupUI(deaths, levelSoulsCollected, 0.5f);

            // If the last checkpoint isn't null, unselect it
            if (lastCheckpoint != null)
                lastCheckpoint.Deselect();

            lastCheckpoint = checkpoint;
        }
        checkpoint.Select(followingSouls.Count);
        followingSouls.Clear();
    }

    public void PickupSoul(Soul soul)
    {
        if (followingSouls.Count > 0)
        {
            // Get this soul to start following the last soul in the chain
            soul.StartFollow(followingSouls[^1].transform);
            followingSouls.Add(soul);
        }
        else
        {
            // This soul is the first soul so its first in the chain and follows the player
            soul.StartFollow(transform);
            followingSouls.Add(soul);
        }
    }

    public void OnLand()
    {
        canDoubleJump = true;
        canCoyoteJump = true;
        airTime = 0;
        wallJumping.ResetJumps();
        dashing.TryResetDash();
    }

    public override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);

        switch (collision.transform.tag)
        {
            case "DeathZone":
                KillPlayer();
                break;
        }
    }

    public override object CaptureState()
    {
        float[] position = { transform.position.x, transform.position.y, transform.position.z };
        float[] eulerAngles = { transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z };
        float[] velocity = { rb.velocity.x, rb.velocity.y };
        string[] followingSoulIDs = new string[followingSouls.Count];
        for (int i = 0; i < followingSouls.Count; i++)
        {
            followingSoulIDs[i] = followingSouls[i].GetComponent<SaveableEntity>().UniqueId;
        }
        string lastCheckpointID = null;
        if (lastCheckpoint != null)
        {
            lastCheckpointID = lastCheckpoint.GetComponent<SaveableEntity>().UniqueId;
        }
        return new object[] { position, eulerAngles, velocity, followingSoulIDs, lastCheckpointID, canCoyoteJump, canDoubleJump, dashing.canDash, levelSoulsCollected };
    }

    public override void RestoreState(object state)
    {
        object[] data = (object[])state;
        float[] position = (float[])data[0];
        float[] eulerAngles = (float[])data[1];
        float[] velocity = (float[])data[2];
        string[] followingSoulIDs = (string[])data[3];
        string lastCheckpointID = (string)data[4];
        canCoyoteJump = (bool)data[5];
        canDoubleJump = (bool)data[6];
        dashing.canDash = (bool)data[7];
        dashing.UpdateDashIndicator();

        if (useSavedTransform)
        {
            transform.position = new Vector3(position[0], position[1], position[2]);
            transform.eulerAngles = new Vector3(eulerAngles[0], eulerAngles[1], eulerAngles[2]);
            rb.velocity = new Vector2(velocity[0], velocity[1]);
            useSavedTransform = false;
        }

        for(int i = 0; i < followingSoulIDs.Length; i++)
        {
            Soul soul = SaveSystem.instance.saveableEntityDict[followingSoulIDs[i]].GetComponent<Soul>();
            if(i == 0)
            {
                soul.StartFollow(transform);
            }
            else
            {
                soul.StartFollow(followingSouls[i-1].transform);
            }
            followingSouls.Add(soul);
        }

        levelSoulsCollected = (int)data[8];
        if (lastCheckpointID != null)
        {
            lastCheckpoint = SaveSystem.instance.saveableEntityDict[lastCheckpointID].GetComponent<Checkpoint>();
            lastCheckpoint.Select(levelSoulsCollected, false);
            StartCoroutine(lastCheckpoint.SaveCooldown());
        }
    }
}
