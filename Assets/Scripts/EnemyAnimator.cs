using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(EnemyStateManager))]
public class EnemyAnimator : MonoBehaviour
{
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private EnemyStateManager enemyState;

    void Start()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyState = GetComponent<EnemyStateManager>();

        // Nos suscribimos al evento: "Cuando el estado avise de un ataque, ejecuta TriggerAttackAnimation"
        enemyState.OnAttackEvent += TriggerAttackAnimation;
    }

    void Update()
    {
        Vector2 moveDir = enemyState.MovementDirection;

        // 1. Lógica de Volteo (Flip)
        // Asumiendo que tu sprite original mira hacia la derecha
        if (moveDir.x < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (moveDir.x > 0)
        {
            spriteRenderer.flipX = false;
        }

        // 2. Animación de Movimiento
        // Si no está atacando y se está moviendo hacia algún lado
        if (enemyState.currentState != EnemyStateManager.EnemyState.Attack && moveDir != Vector2.zero)
        {
            anim.SetBool("IsMoving", true);
        }
        else
        {
            anim.SetBool("IsMoving", false);
        }
    }

    private void TriggerAttackAnimation()
    {
        // Disparamos el "Trigger" en el Animator
        anim.SetTrigger("Attack");
    }

    void OnDestroy()
    {
        // Buena práctica: siempre desuscribirse de los eventos cuando el objeto se destruye
        if (enemyState != null)
        {
            enemyState.OnAttackEvent -= TriggerAttackAnimation;
        }
    }
}