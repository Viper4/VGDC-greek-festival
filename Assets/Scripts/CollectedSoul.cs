using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;

public class CollectedSoul : BaseMovement
{
    [System.Serializable]
    enum MoveState
    {
        Wander,
        Destination
    }
    [SerializeField] MoveState moveState = MoveState.Wander;

    [SerializeField] Vector2 wanderTime = new Vector2(3, 5);
    Vector3 wanderDirection;

    [SerializeField] float obstacleAvoidance = 2f;
    [SerializeField] LayerMask collisionLayers;

    [SerializeField] Transform[] verticalPoints;
    [SerializeField] Transform[] endPoints;
    List<Vector2> destinations = new List<Vector2>();
    bool atDestination = false;
    [SerializeField] Vector2 pauseTime = new Vector2(1, 2);

    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] float dialogueClearDelay = 1f;
    [SerializeField] string[] dialogue;
    int dialogueIndex = 0;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        Walking = true;
        StartCoroutine(Move());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        switch (moveState)
        {
            case MoveState.Wander:
                if (wanderDirection != Vector3.zero)
                {
                    rb.velocity = new Vector2(wanderDirection.x * walkSpeed, rb.velocity.y);
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, wanderDirection, obstacleAvoidance, collisionLayers);
                    if (hit.transform != null)
                    {
                        wanderDirection = Vector3.zero;
                        rb.velocity = Vector2.zero;
                    }
                }
                break;
            case MoveState.Destination:
                if (destinations.Count > 0)
                {
                    Vector2 direction = (destinations[^1] - new Vector2(transform.position.x, transform.position.y)).normalized;
                    if (Climbing)
                    {
                        rb.velocity = new Vector2(direction.x * walkSpeed, direction.y * climbSpeed);
                    }
                    else
                    {
                        rb.velocity = new Vector2(direction.x * walkSpeed, rb.velocity.y);
                    }

                    if ((destinations[^1] - new Vector2(transform.position.x, transform.position.y)).sqrMagnitude < 1)
                    {
                        if(destinations.Count == 1)
                        {
                            atDestination = true;
                        }
                        else
                        {
                            if (destinations[^2].y < destinations[^1].y)
                            {
                                GoDown();
                            }
                        }
                        destinations.RemoveAt(destinations.Count - 1);
                    }
                }
                break;
        }
    }

    bool CheckLineOfSight(Vector2 from, Vector2 to)
    {
        RaycastHit2D hit = Physics2D.Linecast(from, to, collisionLayers);
        return hit.transform == null;
    }

    IEnumerator Move()
    {
        switch (moveState)
        {
            case MoveState.Wander:
                wanderDirection = Random.Range(0, 2) == 0 ? -transform.right : Vector3.right;
                transform.right = wanderDirection;
                yield return new WaitForSeconds(Random.Range(wanderTime.x, wanderTime.y));
                wanderDirection = Vector3.zero;
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

    public void EnterDialogue()
    {
        if(dialogue.Length > 0)
            dialogueText.text = dialogue[0];
    }

    public void ExitDialogue()
    {
        if(dialogue.Length > 0)
            dialogueText.text = dialogue[^1];
        StartCoroutine(ClearDialogue(dialogueClearDelay));
    }

    IEnumerator ClearDialogue(float delay)
    {
        yield return new WaitForSeconds(delay);
        dialogueText.text = "";
    }
}
