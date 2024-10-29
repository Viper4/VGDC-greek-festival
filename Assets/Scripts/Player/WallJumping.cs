using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallJumping : MonoBehaviour
{
    public bool IsSliding 
    { 
        get
        {
            return Player.instance.wall != null && Player.instance.rb.velocity.y < 0 && slideTimer > 0;
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
        ResetJumps();
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.timeScale > 0)
        {
            // While player holds space
            if (Player.instance.input.Player.Jump.ReadValue<float>() >= 1f)
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
}
