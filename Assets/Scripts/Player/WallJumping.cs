using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WallJumping : MonoBehaviour
{
    public bool IsSliding 
    { 
        get
        {
            return Player.instance.wall != null && !Player.instance.IsGrounded && slideTimer > 0;
        }
    }
    public bool IsJumping { get; set; }
    public float slideSpeed = 0.2f;
    [SerializeField] private float jumpDuration = 0.2f;
    [SerializeField] private float slideTime = 2f;
    private float slideTimer;
    [SerializeField] private int maxJumps = 3;
    private int jumpsLeft;
    [SerializeField] private Vector2 jumpPower = new Vector2(10f, 12f);
    private Coroutine wallJumpRoutine;

    [SerializeField] private float regularJumpBlockTime = 0.5f;
    private bool canJump = true;
    private Coroutine blockJumpRoutine;

    [SerializeField] private UnityEvent onSlideStart;
    [SerializeField] private UnityEvent endSlideEnd;
    [SerializeField] private UnityEvent onJump;

    // Start is called before the first frame update
    private void Start()
    {
        ResetJumps();
    }

    // Update is called once per frame
    private void Update()
    {
        if(Time.timeScale > 0)
        {
            // While player holds space
            if (Player.instance.input.Player.Jump.ReadValue<float>() >= 1f)
            {
                if (canJump && IsSliding && jumpsLeft > 0 && !IsJumping)
                    wallJumpRoutine = StartCoroutine(WallJump());
            }
            if (IsSliding)
            {
                if(slideTimer == slideTime)
                    onSlideStart?.Invoke();
                slideTimer -= Time.deltaTime;
                if(slideTimer <= 0)
                    endSlideEnd?.Invoke();
            }
        }
    }

    private IEnumerator WallJump()
    {
        onJump?.Invoke();
        IsJumping = true;
        float direction = transform.position.x > Player.instance.wall.position.x ? 1f : -1f;
        Player.instance.rb.velocity = new Vector2(jumpPower.x * direction, jumpPower.y);
        yield return new WaitForSeconds(jumpDuration);
        slideTimer = slideTime;
        IsJumping = false;
        jumpsLeft--;
    }

    public void ResetJumps()
    {
        slideTimer = slideTime;
        jumpsLeft = maxJumps;
    }

    public void BlockWallJump()
    {
        if (blockJumpRoutine != null)
            StopCoroutine(blockJumpRoutine);
        blockJumpRoutine = StartCoroutine(BlockWallJumpRoutine());
    }

    private IEnumerator BlockWallJumpRoutine()
    {
        canJump = false;
        yield return new WaitForSeconds(regularJumpBlockTime);
        canJump = true;
    }

    public void Cancel()
    {
        if (wallJumpRoutine != null)
            StopCoroutine(wallJumpRoutine);
        canJump = true;
    }
}
