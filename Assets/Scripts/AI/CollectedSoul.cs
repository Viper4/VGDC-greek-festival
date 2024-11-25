using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CollectedSoul : BaseMovement
{
    [System.Serializable]
    private enum MoveState
    {
        Wander,
        Destination,
        Idle,
    }
    [Header("CollectedSoul")]
    [SerializeField] private MoveState moveState = MoveState.Wander;
    private MoveState currentMoveState;

    [SerializeField] private Vector2 wanderTime = new Vector2(3, 5);

    [SerializeField] private Transform[] verticalPoints;
    [SerializeField] private Transform[] endPoints;
    private List<Vector2> destinations = new List<Vector2>();
    private bool atDestination = false;
    [SerializeField] private Vector2 pauseTime = new Vector2(1, 2);

    private bool inDialogue = false;
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private float dialogueClearDelay = 1f;
    [SerializeField] private float dialogueClearSpeed = 0.025f;
    [SerializeField] private string[] dialogue;
    [SerializeField] private float[] timesPerChar;
    private int dialogueIndex = 0;
    private Coroutine dialogueAnimationRoutine = null;
    private Coroutine clearDialogueRoutine = null;

    private void OnEnable()
    {
        Player.instance.input.Player.Dialogue.performed += ctx => NextDialogue();
    }

    private void OnDisable()
    {
        Player.instance.input.Player.Dialogue.performed -= ctx => NextDialogue();
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        Walking = true;
        currentMoveState = moveState;
        StartCoroutine(Move());
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        switch (currentMoveState)
        {
            case MoveState.Wander:
                if (moveVelocity != Vector2.zero)
                {
                    RaycastHit2D hit = GetFutureObstruction(moveVelocity);
                    if (hit.transform != null)
                    {
                        moveVelocity = Vector3.zero;
                        rb.velocity = Vector2.zero;
                    }
                    ApplyVelocity(new Vector2(moveVelocity.x * walkSpeed, rb.velocity.y));
                }
                break;
            case MoveState.Destination:
                if (destinations.Count > 0)
                {
                    moveVelocity = (destinations[^1] - new Vector2(transform.position.x, transform.position.y)).normalized;
                    if (Climbing)
                    {
                        ApplyVelocity(new Vector2(moveVelocity.x * walkSpeed, moveVelocity.y * climbSpeed));
                    }
                    else
                    {
                        ApplyVelocity(new Vector2(moveVelocity.x * walkSpeed, rb.velocity.y));
                    }

                    if ((destinations[^1] - new Vector2(transform.position.x, transform.position.y)).sqrMagnitude < 0.5f)
                    {
                        if(destinations.Count == 1)
                        {
                            atDestination = true;
                        }
                        else
                        {
                            if (destinations[^2].y < destinations[^1].y)
                            {
                                Descend();
                            }
                        }
                        destinations.RemoveAt(destinations.Count - 1);
                    }
                }
                break;
            case MoveState.Idle:
                moveVelocity = Vector2.zero;
                break;
        }
    }

    private bool CheckLineOfSight(Vector2 from, Vector2 to)
    {
        RaycastHit2D hit = Physics2D.Linecast(from, to, collisionLayers);
        return hit.transform == null;
    }

    private IEnumerator Move()
    {
        switch (currentMoveState)
        {
            case MoveState.Wander:
                moveVelocity = Random.Range(0, 2) == 0 ? -transform.right : Vector2.right;
                yield return new WaitForSeconds(Random.Range(wanderTime.x, wanderTime.y));
                moveVelocity = Vector2.zero;
                rb.velocity = Vector2.zero;
                break;
            case MoveState.Destination:
                // TODO: Make this recursive
                destinations.Clear();
                atDestination = false;
                Vector2 finalDestination = endPoints[Random.Range(0, endPoints.Length)].position;
                destinations.Add(finalDestination);

                if (!CheckLineOfSight(transform.position, finalDestination))
                {
                    // Look for vertical nav end points with LOS to destination
                    for (int i = 0; i < verticalPoints.Length; i++)
                    {
                        Transform firstEnd = verticalPoints[i].Find("End");
                        if (CheckLineOfSight(finalDestination, firstEnd.position))
                        {
                            Transform firstStart = verticalPoints[i].Find("Start");

                            if (!CheckLineOfSight(transform.position, firstStart.position))
                            {
                                for (int j = 0; j < verticalPoints.Length; j++)
                                {
                                    if (i != j)
                                    {
                                        Transform secondEnd = verticalPoints[j].Find("End");
                                        if (CheckLineOfSight(firstStart.position, secondEnd.position))
                                        {
                                            Transform secondStart = verticalPoints[j].Find("Start");

                                            if (CheckLineOfSight(transform.position, secondStart.position))
                                            {
                                                destinations.Add(firstEnd.position);
                                                destinations.Add(firstStart.position);
                                                destinations.Add(secondEnd.position);
                                                destinations.Add(secondStart.position);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                destinations.Add(firstEnd.position);
                                destinations.Add(firstStart.position);
                            }
                        }
                    }
                }
                yield return new WaitUntil(() => atDestination);
                break;
        }
        yield return new WaitForSeconds(Random.Range(pauseTime.x, pauseTime.y));
        StartCoroutine(Move());
    }

    private IEnumerator AnimatedDialogue(string text, float timePerChar)
    {
        for (int i = 0; i < text.Length; i++)
        {
            dialogueText.text += text[i];
            yield return new WaitForSeconds(timePerChar);
        }
        dialogueAnimationRoutine = null;
    }

    private void SkipDialogue(int index)
    {
        StopCoroutine(dialogueAnimationRoutine);
        dialogueText.text = "";
        for (int i = 0; i < index; i++)
        {
            dialogueText.text += dialogue[i];
            if(i < index - 1)
            {
                dialogueText.text += "\n";
            }
        }
        dialogueAnimationRoutine = null;
    }

    public void EnterDialogue()
    {
        dialogueIndex = 0;
        inDialogue = true;
        if (clearDialogueRoutine != null)
            StopCoroutine(clearDialogueRoutine);
        dialogueBox.SetActive(true);
        currentMoveState = MoveState.Idle;
        rb.velocity = new Vector2(0, rb.velocity.y);
        if (dialogue.Length > 0)
        {
            dialogueText.text = "";
            if (dialogueAnimationRoutine != null)
                StopCoroutine(dialogueAnimationRoutine);
            dialogueAnimationRoutine = StartCoroutine(AnimatedDialogue(dialogue[0], timesPerChar[0]));
        }
    }

    public void NextDialogue()
    {
        if(inDialogue)
        {
            if(dialogueAnimationRoutine != null)
            {
                SkipDialogue(Mathf.Min(dialogueIndex + 1, dialogue.Length - 1));
            }
            else 
            {
                if (dialogueIndex < dialogue.Length - 2)
                {
                    dialogueIndex++;
                    dialogueText.text += "\n";
                    dialogueAnimationRoutine = StartCoroutine(AnimatedDialogue(dialogue[dialogueIndex], timesPerChar[dialogueIndex]));
                }
            }
        }
    }

    public void ExitDialogue()
    {
        inDialogue = false;
        currentMoveState = moveState;
        if(dialogue.Length > 0)
        {
            if (dialogueAnimationRoutine != null)
                StopCoroutine(dialogueAnimationRoutine);
            dialogueText.text = "";
            dialogueAnimationRoutine = StartCoroutine(AnimatedDialogue(dialogue[^1], timesPerChar[^1]));
        }
        clearDialogueRoutine = StartCoroutine(ClearDialogue(dialogueClearDelay, dialogueClearSpeed));
    }

    private IEnumerator ClearDialogue(float delay, float timerPerChar)
    {
        yield return new WaitUntil(() => dialogueAnimationRoutine == null);
        yield return new WaitForSeconds(delay);
        while (dialogueText.text.Length > 0)
        {
            dialogueText.text = dialogueText.text[..^1];
            yield return new WaitForSeconds(timerPerChar);
        }
        dialogueBox.SetActive(false);
        clearDialogueRoutine = null;
    }
}
