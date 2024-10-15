using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class WallJumping : MonoBehaviour
{
    Player player;

    public bool IsSliding 
    { 
        get
        {
            return player.wall != null && !player.IsGrounded && slideTimer > 0;
        }
    }
    public bool IsJumping { get; set; }
    public float slideSpeed = 0.2f;
    [SerializeField] float jumpDuration = 0.2f;
    [SerializeField] float slideTime = 2f;
    float slideTimer;
    [SerializeField] int maxJumps = 3;
    int jumpsLeft;
    [SerializeField] Vector2 jumpPower = new Vector2(10f, 12f);

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<Player>();
        ResetJumps();
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.timeScale > 0)
        {
            // While player holds space
            if (Player.playerInput.Player.Jump.ReadValue<float>() >= 1f)
            {
                if (IsSliding && jumpsLeft > 0 && !IsJumping)
                    StartCoroutine(WallJump());
            }
            if (IsSliding)
            {
                slideTimer -= Time.deltaTime;
            }
        }
    }

    IEnumerator WallJump()
    {
        IsJumping = true;
        float direction = transform.position.x > player.wall.position.x ? 1f : -1f;
        player.rb.velocity = new Vector2(jumpPower.x * direction, jumpPower.y);
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
}
