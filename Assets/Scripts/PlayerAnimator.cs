using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PlayerStateManager))]
public class PlayerAnimator : MonoBehaviour
{
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private PlayerStateManager stateManager;

    void Start()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        stateManager = GetComponent<PlayerStateManager>();
    }

    void Update()
    {
        // AHORA LEEMOS LA DIRECCIÓN DE ANIMACIÓN, NO LA FÍSICA
        Vector2 currentAnimDir = stateManager.AnimDirection;

        // Lógica de Volteo (Flip)
        if (currentAnimDir.x < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (currentAnimDir.x > 0)
        {
            spriteRenderer.flipX = false;
        }

        // Lógica del Animator
        if (currentAnimDir != Vector2.zero)
        {
            anim.SetFloat("MoveX", Mathf.Abs(currentAnimDir.x));
            anim.SetFloat("MoveY", currentAnimDir.y);
            anim.SetBool("IsMoving", true);
        }
        else
        {
            anim.SetBool("IsMoving", false);
        }

        // --- NUEVO: Le avisamos al Animator si estamos haciendo Dash ---
        anim.SetBool("IsDashing", stateManager.isDashing);
    }

    public void TriggerDeathAnimation()
    {
        anim.SetBool("IsMoving", false);
        anim.SetTrigger("Die");
    }
}