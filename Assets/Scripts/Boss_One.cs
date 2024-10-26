using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Animations;

public class Boss_One : BaseMovement
{
    enum BossState{
        Jump, Charge, Slash, Idle
    }

    BossState CurrentState = BossState.Idle;
    [SerializeField] float minWalk = 0.5f;
    [SerializeField] float maxWalk = 2f;
    [SerializeField] float TotalIdleTime = 2.5f;

    private float OriginalTotalIdleTime;

    private float Waiting = 0f;

    private float WaitingConst;

    [SerializeField] float SlashIdleTime = 1.0f;

    [SerializeField] float ChargeTime = .5f;

    [SerializeField] float JumpChargeTime = .5f;
    [SerializeField] float SlashChargeTime = .3f;

    [SerializeField] float arcTime = 3.0f;

    [SerializeField] Animator animator;

    [SerializeField] float KnockbackDuration = .3f;

    [SerializeField] float KnockbackDrag = 1f;

    [SerializeField] float KnockbackSpeed = 5f;

    [SerializeField] float YKnockback = 0.1f;

    [SerializeField] float ChargeSpeed = 6f;

    [SerializeField] float AccelTime = .8f;

    [SerializeField] float ChargeKnockback = 6f;

    [SerializeField] float ChargeKnockbackDuration = .3f;
    [SerializeField] float ChargeDrag = 1f;

    [SerializeField] float ChargeXConst = 50f;

    public float ImmunityTime = .3f;

    BossState State;

    BossState LastAttack;

    [SerializeField] Transform PlayerTransform;

    private float attackCount = 0;

    [SerializeField] Collider2D SwordCollider;

    [SerializeField] GameObject shield;

    float timer = 0;

    void Start()
    {
        OriginalTotalIdleTime = TotalIdleTime;
        timer = ImmunityTime;
        

        base.Start();
        StartCoroutine(StartIdle());
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if(myCollider.bounds.Intersects(Player.player.myCollider.bounds) && timer>ImmunityTime){
            timer = 0;
            Vector2 direction = Player.player.transform.position - transform.position;
            direction.Normalize();
            direction.y = 0;
            switch (CurrentState){
                case BossState.Slash:
                    StartCoroutine(Player.player.ApplyKnockback(direction * KnockbackSpeed, KnockbackDuration, KnockbackDrag));
                    break;
                case BossState.Jump:
                    StartCoroutine(Player.player.ApplyKnockback(direction * KnockbackSpeed, KnockbackDuration, KnockbackDrag));
                break;
                case BossState.Charge:
                    Vector2 Knockback = new Vector2(moveVelocity.x * ChargeXConst, ChargeKnockback);
                    StartCoroutine(Player.player.ApplyKnockback(Knockback, ChargeKnockbackDuration, ChargeDrag));
                break;
                case BossState.Idle:
                    StartCoroutine(Player.player.ApplyKnockback(direction * KnockbackSpeed, KnockbackDuration, KnockbackDrag));
                break;
            }
        } 
    }

    IEnumerator StartIdle(){
        yield return new WaitForEndOfFrame();
        Physics2D.IgnoreCollision(Player.player.myCollider, myCollider, false);
        shield.GetComponent<Renderer>().material.color = Color.white;
        RotateTowardsPlayer();
        CurrentState = GetRandomEnum<BossState>(); //picks random attack (used later)
        while(CurrentState == LastAttack && CurrentState!=BossState.Slash && CurrentState == BossState.Idle){ // Doesn't pick the same attack in a row (besides slash)
            CurrentState = GetRandomEnum<BossState>();
        }
            switch(CurrentState){ //Sets reaction time values to each attack
            case BossState.Slash:
                WaitingConst = .4f;
            break;
            case BossState.Jump:
                WaitingConst = .5f;
            break;
            case BossState.Charge:
                WaitingConst = .5f;
            break;
            }
        Vector2 Direction = UnityEngine.Random.Range(0,2) ==0? Vector2.right:Vector2.left;
        moveVelocity = Direction * walkSpeed;
        float walkTime = UnityEngine.Random.Range(Mathf.Min(minWalk, TotalIdleTime), Mathf.Max(maxWalk, TotalIdleTime));
        float waitTime = TotalIdleTime-walkTime;
        while(walkTime>0){ //walks while idle
            rb.velocity = new Vector2(moveVelocity.x, rb.velocity.y);
            yield return new WaitForEndOfFrame();
            walkTime -= Time.deltaTime;
            Waiting += Time.deltaTime;
            if (Waiting + WaitingConst >= TotalIdleTime){
                switch(CurrentState){ //fixes color of shield
                case BossState.Slash:
                    shield.GetComponent<Renderer>().material.color = Color.red;
                break;
                case BossState.Jump:
                    shield.GetComponent<Renderer>().material.color = Color.yellow;
                break;
                case BossState.Charge:
                    shield.GetComponent<Renderer>().material.color = Color.black;
                break;
                }
            }
        }  
        while(waitTime>0){
            yield return new WaitForEndOfFrame();
            waitTime -= Time.deltaTime;
            Waiting += Time.deltaTime;
            if (Waiting + WaitingConst >= TotalIdleTime){
                switch(CurrentState){ //fixes color of shield
                case BossState.Slash:
                    shield.GetComponent<Renderer>().material.color = Color.red;
                break;
                case BossState.Jump:
                    shield.GetComponent<Renderer>().material.color = Color.yellow;
                break;
                case BossState.Charge:
                    shield.GetComponent<Renderer>().material.color = Color.black;
                break;
                }
            }
        }
        Waiting = 0;
        attackCount += 1;
        switch(CurrentState){ //runs coroutine of correct attack
            case BossState.Slash:
                TotalIdleTime = SlashIdleTime;
                LastAttack = BossState.Slash;
                Debug.Log("Slash");
                StartCoroutine(Slash());
            break;
            case BossState.Jump:
                TotalIdleTime = OriginalTotalIdleTime;
                LastAttack = BossState.Jump;
                Debug.Log("Jump");
                StartCoroutine(Jump());
            break;
            case BossState.Charge:
                TotalIdleTime = OriginalTotalIdleTime;
                LastAttack = BossState.Charge;
                Debug.Log("Charge");
                StartCoroutine(Charge());
            break;
            }
    }

    void RotateTowardsPlayer(){
        Vector2 PlayerDir = Player.player.transform.position - transform.position;
            PlayerDir.y = 0;
            transform.right = PlayerDir.normalized;
    }

    IEnumerator Charge(){
        RotateTowardsPlayer();
        moveVelocity = transform.right * ChargeSpeed;
        float Chargetimer = 0;
        while(Chargetimer < AccelTime){
            rb.velocity = new Vector2(Mathf.Lerp(0, moveVelocity.x, timer/AccelTime), 0);
            Chargetimer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        while(wall == null){
            rb.velocity = moveVelocity;
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(KnockbackDuration);
        StartCoroutine(StartIdle());
    }

    IEnumerator Slash(){
        animator.SetTrigger("Slash");
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        StartCoroutine(StartIdle());
    }

    public void KnockbackPlayer(){
        if(timer<ImmunityTime) return;
        timer = 0;
        Debug.Log("knockback");
        Vector2 direction = Player.player.transform.position - transform.position;
        direction.Normalize();
        direction.y = 0;
        StartCoroutine(Player.player.ApplyKnockback(direction*KnockbackSpeed, KnockbackDuration, KnockbackDrag));
    }

    IEnumerator Jump(){
        Physics2D.IgnoreCollision(Player.player.myCollider, myCollider, true);
        float distanceX = Player.player.transform.position.x - transform.position.x;
        float distanceY = Player.player.transform.position.y - transform.position.y;
        float velocityX = distanceX / arcTime;
        float velocityY = distanceY / arcTime + 0.5f * rb.gravityScale * -Physics2D.gravity.y * arcTime;
        rb.velocity = new Vector2(velocityX, velocityY);
        yield return new WaitForSeconds(arcTime/2);
        animator.SetTrigger("Slash");
        float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animationLength);
        yield return new WaitUntil(()=> IsGrounded);
        StartCoroutine(StartIdle());
    }

    public static T GetRandomEnum<T>()
    {
        System.Array A = System.Enum.GetValues(typeof(T));
        T V = (T)A.GetValue(UnityEngine.Random.Range(0,A.Length));
        return V;
    }
}