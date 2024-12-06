using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Dashing : MonoBehaviour
{
    [SerializeField] private float inputWait = 0.05f;
    [SerializeField] private float dashSpeed = 15;
    [SerializeField] private float dashDuration = 0.25f;
    [SerializeField] private float dashCooldown = 0.1f;
    [SerializeField] private float dashDrag = 0.5f;
    [SerializeField] private SpriteRenderer dashIndicator;

    private Coroutine dashRoutine;
    [HideInInspector] public bool canDash = true;
    [HideInInspector] public Vector2 velocity = Vector2.zero;
    private Vector2 lastInput;

    [SerializeField] private UnityEvent onDash;

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
                    if (canDash)
                    {
                        StartCoroutine(WaitForInput(moveInput));
                    }
                }
                else if(lastInput != Vector2.zero)
                {
                    if (canDash)
                    {
                        StartCoroutine(WaitForInput(lastInput));
                    }
                }
            }
            if(moveInput != Vector2.zero)
            {
                lastInput = moveInput;
            }
        }
    }

    public void TryResetDash()
    {
        if (dashRoutine == null)
        {
            canDash = true;
            UpdateDashIndicator();
        }
    }

    public void UpdateDashIndicator()
    {
        dashIndicator.color = canDash ? Color.green : Color.red;
    }

    IEnumerator WaitForInput(Vector2 initialInput)
    {
        bool jumped = Player.instance.input.Player.Jump.ReadValue<float>() >= 1f;
        canDash = false;
        UpdateDashIndicator();
        yield return new WaitForSecondsRealtime(inputWait);
        Vector2 moveInput = Player.instance.input.Player.Move.ReadValue<Vector2>();

        jumped = jumped || Player.instance.input.Player.Jump.ReadValue<float>() >= 1f;

        if (moveInput != Vector2.zero)
        {
            dashRoutine = StartCoroutine(Dash(moveInput, jumped && moveInput.y == 0));
        }
        else
        {
            dashRoutine = StartCoroutine(Dash(initialInput, jumped && initialInput.y == 0));
        }
    }

    IEnumerator Dash(Vector2 moveInput, bool bunnyHop)
    {
        onDash?.Invoke();
        bool waveDash = moveInput.y < 0 && moveInput.x != 0;
        Player.instance.movementAudio.PlayDash();
        velocity = moveInput * dashSpeed;
        if (moveInput.y < 0)
        {
            // Stop downward dash if we hit the ground
            float timer = 0;
            while(timer < dashDuration)
            {
                if (Player.instance.IsGrounded)
                {
                    break;
                }
                timer += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }
        else if(moveInput.y == 0 && moveInput.x != 0)
        {
            // Stop horizontal dash if we hit a wall
            float timer = 0;
            while (timer < dashDuration)
            {
                if (Player.instance.input.Player.Jump.ReadValue<float>() >= 1f)
                    bunnyHop = true;
                if (Player.instance.wall != null)
                {
                    break;
                }
                timer += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            yield return new WaitForSeconds(dashDuration);
        }

        if(bunnyHop && !Player.instance.IsGrounded)
        {
            // If we're bunny hopping, preserve x velocity but stop vertical dash momentum
            if (velocity.y != 0)
            {
                velocity.y = 0;
                Player.instance.rb.velocity = new Vector2(Player.instance.rb.velocity.x, 0);
            }
        }
        else
        {
            // Stop dash momentum if we aren't bunny hopping
            velocity = Vector2.zero;
            Player.instance.rb.velocity = Vector2.zero;
        }

        if(!waveDash)
            yield return new WaitForSeconds(dashCooldown);

        while (!Player.instance.IsGrounded)
        {
            if(Player.instance.wall != null || Mathf.Abs(velocity.x) < Mathf.Abs(Player.instance.moveVelocity.x))
            {
                velocity = Vector2.zero;
            }
            else
            {
                velocity.x = Mathf.Lerp(velocity.x, 0, Time.fixedDeltaTime * dashDrag);
            }
            yield return new WaitForFixedUpdate();
        }
        velocity = Vector2.zero;
        canDash = true;
        UpdateDashIndicator();
        dashRoutine = null;
        if (waveDash && Player.instance.input.Player.Jump.ReadValue<float>() >= 1f)
        {
            moveInput.y = 0;
            moveInput.Normalize();
            dashRoutine = StartCoroutine(Dash(moveInput, true));
        }
    }
}
