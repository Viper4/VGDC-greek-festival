using UnityEngine;
using UnityEngine.InputSystem;

public class NailAttack : MonoBehaviour
{
    [SerializeField] Collider2D hitbox;

    [SerializeField] LayerMask HitLayers;
    Collider2D[] hits = new Collider2D[10];

    [SerializeField] float knockbackDuration = .5f;

    public BaseMovement knockbackTarget;
    [SerializeField] float knockback = 1f;
    [SerializeField] float drag = 1f;
    [SerializeField] float cooldown = 3f;
    Coroutine knockbackRoutine;
    float timer = 0;

    [SerializeField] float damage = 1f;

    void OnEnable()
    {
        Player.playerInput.Player.Nail.performed += Attack;
    }

    private void OnDisable()
    {
        Player.playerInput.Player.Nail.performed -= Attack;
    }

    private void Start()
    {
        timer = cooldown;
    }

    private void Update()
    {
        timer += Time.deltaTime;
    }

    void Attack(InputAction.CallbackContext context)
    {
        if (timer >= cooldown)
        {
            timer = 0;
            ContactFilter2D Filter = new ContactFilter2D() { layerMask = HitLayers, useLayerMask = true };
            int numHits = hitbox.OverlapCollider(Filter, hits);
            for(int i = 0; i < numHits; i++)
            {
                if (hits[i].TryGetComponent(out HealthSystem healthSystem))
                {
                    healthSystem.AddHealth(-damage, 0, true);
                }
            }

            if (numHits > 0)
            {
                Vector2 direction = -(hitbox.transform.position - transform.position).normalized;
                direction.y = 0;
                if (knockbackRoutine != null) StopCoroutine(knockbackRoutine);
                knockbackRoutine = StartCoroutine(knockbackTarget.ApplyKnockback(direction * knockback, knockbackDuration, drag));
            }
        }
    }
}
