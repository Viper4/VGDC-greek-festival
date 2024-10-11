using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public class AbilityScript : MonoBehaviour
{
    public Rigidbody2D OwnerRigidbody;
    public Transform LaunchOffset;
    public Transform TwoLaunchOffset;
    public Transform ThreeLaunchOffset;
    public GameObject ProjectilePrefab;
    public float ProjectileSpeed = 20f;

    void OnEnable(){
        Player.playerInput.Player.Ability1.performed += AbilityOneScript;
    }

[SerializeField] private Cooldown cooldown;

    void AbilityOneScript(InputAction.CallbackContext context){
        
        if (cooldown.IsCoolingDown) return;

        Collider2D cloneOne = Instantiate(ProjectilePrefab,LaunchOffset.position,LaunchOffset.rotation).GetComponent<Collider2D>();
        Collider2D cloneTwo = Instantiate(ProjectilePrefab,TwoLaunchOffset.position, TwoLaunchOffset.rotation).GetComponent<Collider2D>();
        Collider2D cloneThree = Instantiate(ProjectilePrefab,ThreeLaunchOffset.position,ThreeLaunchOffset.rotation).GetComponent<Collider2D>();
    
        Physics2D.IgnoreCollision(cloneOne, cloneTwo);
        Physics2D.IgnoreCollision(cloneOne, cloneThree);
        Physics2D.IgnoreCollision(cloneThree, cloneTwo);
        Vector2 InitialVelocity = OwnerRigidbody.velocity;
        InitialVelocity.y = 0;
        cloneOne.GetComponent<Rigidbody2D>().velocity=InitialVelocity + new Vector2(LaunchOffset.up.x, LaunchOffset.up.y) * ProjectileSpeed;
        cloneTwo.GetComponent<Rigidbody2D>().velocity=InitialVelocity + new Vector2(TwoLaunchOffset.up.x, TwoLaunchOffset.up.y) * ProjectileSpeed;
        cloneThree.GetComponent<Rigidbody2D>().velocity=InitialVelocity + new Vector2(ThreeLaunchOffset.up.x, ThreeLaunchOffset.up.y) * ProjectileSpeed;
        
        cooldown.StartCooldown();
    }

}
