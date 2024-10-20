using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;

public class Boss_One : BaseMovement
{
    enum BossState{
        Jump, Charge, Slash
    }

    [SerializeField] float minWalk = 0.5f;
    [SerializeField] float maxWalk = 2f;
    [SerializeField] float TotalIdleTime = 2.5f;

    [SerializeField] float ChargeTime = .5f;

    [SerializeField] float JumpChargeTime = .5f;
    [SerializeField] float SlashChargeTime = .3f;

    [SerializeField] float arcTime = 3.0f;

    [SerializeField] Animator animator;
    BossState State;

    BossState LastAttack;

    void Start()
    {
        base.Start();
        StartCoroutine(StartIdle());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator StartIdle(){
        Vector2 Direction = UnityEngine.Random.Range(0,2) ==0? Vector2.right:Vector2.left;
        moveVelocity = Direction * walkSpeed;
        float walkTime = UnityEngine.Random.Range(Mathf.Min(minWalk, TotalIdleTime), Mathf.Max(maxWalk, TotalIdleTime));
        float waitTime = TotalIdleTime-walkTime;
        while(walkTime>0){
            rb.velocity = moveVelocity;
            yield return new WaitForEndOfFrame();
            walkTime -= Time.deltaTime;
        }
        yield return new WaitForSeconds(waitTime);
        BossState RandomState = GetRandomEnum<BossState>();
        while(RandomState == LastAttack){
            RandomState = GetRandomEnum<BossState>();
        }
        switch(RandomState){
            case BossState.Slash:
                LastAttack = BossState.Slash;
                Debug.Log("Slash");
                StartCoroutine(Slash());
            break;
            case BossState.Jump:
                LastAttack = BossState.Jump;
                Debug.Log("Jump");
                StartCoroutine(Jump());
            break;
            case BossState.Charge:
                LastAttack = BossState.Charge;
                Debug.Log("Charge");
                //StartCoroutine(Charge());
                StartCoroutine(StartIdle());
            break;
            }
    }

    IEnumerator Slash(){
        animator.SetTrigger("Slash");
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        StartCoroutine(StartIdle());
    }

    IEnumerator Jump(){
        float arcTime = 2f;
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