using UnityEngine;
using UnityEngine.InputSystem;

// Liga o movimento do jogador (CharacterController) às animações da tartaruga.
// Fica no prefab da tartaruga, que deve ser filha do objeto do jogador.
[RequireComponent(typeof(Animator))]
public class TartarugaAnimador : MonoBehaviour
{
    // Tecla que dispara a animação de giro
    public Key teclaGiro = Key.Q;

    // Velocidade de caminhada usada como referência para o ritmo da animação
    public float velocidadeBase = 5f;

    // Tempo no ar (em segundos) antes de considerar que o jogador está pulando
    public float tempoParaAr = 0.08f;

    // Referência ao Animator da tartaruga
    private Animator animator;

    // Referência ao CharacterController do jogador (objeto pai)
    private CharacterController controller;

    // Guarda há quanto tempo o jogador está fora do chão
    private float tempoNoAr = 0f;

    void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponentInParent<CharacterController>();
    }

    void Update()
    {
        if (controller == null)
        {
            return;
        }

        // Velocidade horizontal do jogador (ignora a queda/pulo)
        Vector3 velocidade = controller.velocity;
        velocidade.y = 0f;
        float velocidadeAtual = velocidade.magnitude;

        // Conta o tempo no ar para evitar tremida quando o chão falha por um frame
        if (controller.isGrounded)
        {
            tempoNoAr = 0f;
        }
        else
        {
            tempoNoAr += Time.deltaTime;
        }

        bool noChao = tempoNoAr < tempoParaAr;

        animator.SetFloat("Velocidade", velocidadeAtual, 0.05f, Time.deltaTime);
        animator.SetFloat("MultAndar", Mathf.Clamp(velocidadeAtual / velocidadeBase, 0.8f, 2f));
        animator.SetBool("NoChao", noChao);

        // Giro só é permitido no chão
        if (noChao && Keyboard.current != null && Keyboard.current[teclaGiro].wasPressedThisFrame)
        {
            animator.SetTrigger("Girar");
        }
    }
}
