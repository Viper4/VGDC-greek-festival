using System.Collections;
using UnityEngine;

public class Dashing : MonoBehaviour
{
    [SerializeField] float dashSpeed = 15;
    [SerializeField] float dashDuration = 0.25f;
    [SerializeField] float dashCooldown = 0.5f;
    [SerializeField] float dashDrag = 0.5f;
    [SerializeField] SpriteRenderer dashIndicator;
    private bool canDash = true;
    private bool tryDash = false;
    [HideInInspector] public Vector2 velocity = Vector2.zero;
    private Vector2 lastInput;

    // Update is called once per frame
    void Update()
    {
        if(Time.timeScale > 0)
        {
            Vector2 moveInput = Player.instance.input.Player.Move.ReadValue<Vector2>();
            if (Player.instance.input.Player.Dash.ReadValue<float>() >= 1f)
            {
                if (moveInput != Vector2.zero)
                {
                    tryDash = true;
                    if(canDash)
                        StartCoroutine(Dash(moveInput));
                }
                else if(lastInput != Vector2.zero)
                {
                    if(canDash)
                        StartCoroutine(Dash(lastInput));
                }
            }
            else
            {
                tryDash = false;
            }
            if (moveInput != Vector2.zero)
                lastInput = moveInput;
        }
    }

    IEnumerator Dash(Vector2 moveInput)
    {
        canDash = false;
        Player.instance.movementAudio.PlayDash();
        velocity = moveInput * dashSpeed;
        dashIndicator.color = Color.red;
        yield return new WaitForSeconds(dashDuration);
        if (!tryDash || Player.instance.IsGrounded)
        {
            // Stop dash momentum if we aren't bunny hopping
            velocity = Vector2.zero;
            Player.instance.rb.velocity = Vector2.zero;
        }
        else
        {
            // If we're bunny hopping, preserve x velocity but stop vertical dash momentum
            if (velocity.y != 0)
            {
                velocity.y = 0;
                Player.instance.rb.velocity = new Vector2(Player.instance.rb.velocity.x, 0);
            }
        }

        yield return new WaitForSeconds(dashCooldown - dashDuration);
        while (!Player.instance.IsGrounded)
        {
            if(Player.instance.wall != null || Mathf.Abs(velocity.x) < Mathf.Abs(Player.instance.moveVelocity.x))
            {
                velocity = Vector2.zero;
            }
            else
            {
                velocity.x = Mathf.Lerp(velocity.x, 0, Time.deltaTime * dashDrag);
            }
            yield return new WaitForFixedUpdate();
        }
        velocity = Vector2.zero;
        dashIndicator.color = Color.green;
        canDash = true;
    }
}
