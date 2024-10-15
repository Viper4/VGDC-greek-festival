using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class Dashing : MonoBehaviour
{
    Player player;

    [SerializeField] float dashSpeed = 15;
    [SerializeField] float dashDuration = 0.25f;
    [SerializeField] float dashCooldown = 0.5f;
    [SerializeField] float dashDrag = 0.5f;
    [SerializeField] SpriteRenderer dashIndicator;
    private bool canDash = true;
    private bool tryDash = false;
    [HideInInspector] public Vector2 velocity = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.timeScale > 0)
        {
            Vector2 moveInput = Player.playerInput.Player.Move.ReadValue<Vector2>();
            if (Player.playerInput.Player.Dash.ReadValue<float>() >= 1f && moveInput != Vector2.zero)
            {
                tryDash = true;
                if (canDash)
                    StartCoroutine(Dash(moveInput));
            }
            else
            {
                tryDash = false;
            }
        }
    }

    IEnumerator Dash(Vector2 moveInput)
    {
        canDash = false;
        player.movementAudio.PlayDash();
        velocity = moveInput * dashSpeed;
        dashIndicator.color = Color.red;
        yield return new WaitForSeconds(dashDuration);
        if (!tryDash || player.IsGrounded)
        {
            // Stop dash momentum if we aren't bunny hopping
            velocity = Vector2.zero;
            player.rb.velocity = Vector2.zero;
        }
        else
        {
            // If we're bunny hopping, preserve x velocity but stop vertical dash momentum
            if (velocity.y != 0)
            {
                velocity.y = 0;
                player.rb.velocity = new Vector2(player.rb.velocity.x, 0);
            }
        }
        yield return new WaitForSeconds(dashCooldown - dashDuration);
        while (!player.IsGrounded)
        {
            if (Mathf.Abs(velocity.x) < Mathf.Abs(player.moveVelocity.x))
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
