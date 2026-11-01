using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    // Velocidade do jogador, sempre a mesma, como no Crash
    public float velocidadeAndar = 6.5f;

    // Força aplicada no pulo
    public float forcaPulo = 5f;

    // Valor usado para simular a gravidade
    public float gravidade = -9.8f;

    // Velocidade com que o jogador vira para a direção do movimento, em graus por segundo
    public float velocidadeGiro = 540f;

    // Rapidez com que o jogador ganha e perde velocidade
    public float aceleracao = 30f;

    // Guarda a velocidade vertical do jogador
    private float velocidadeVertical = 0f;

    // Guarda a velocidade horizontal atual, que muda aos poucos
    private Vector3 velocidadeHorizontal = Vector3.zero;

    // Referência ao CharacterController do jogador
    private CharacterController controller;

    // Referência à câmera principal
    private Transform cameraTransform;

    void Start()
    {
        // Busca o CharacterController do jogador
        controller = GetComponent<CharacterController>();

        // Guarda a posição e rotação da câmera principal
        cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        // Valores usados para controlar o movimento
        float x = 0f;
        float z = 0f;

        // Movimento para frente
        if (Keyboard.current.wKey.isPressed)
        {
            z = 1f;
        }

        // Movimento para trás
        if (Keyboard.current.sKey.isPressed)
        {
            z = -1f;
        }

        // Movimento para a esquerda
        if (Keyboard.current.aKey.isPressed)
        {
            x = -1f;
        }

        // Movimento para a direita
        if (Keyboard.current.dKey.isPressed)
        {
            x = 1f;
        }

        // Mantém o jogador próximo ao chão
        if (controller.isGrounded && velocidadeVertical < 0)
        {
            velocidadeVertical = -2f;
        }

        // Permite o pulo somente quando o jogador está no chão
        if (Keyboard.current.spaceKey.wasPressedThisFrame && controller.isGrounded)
        {
            velocidadeVertical = forcaPulo;
        }

        // Aplica a gravidade ao jogador
        velocidadeVertical = velocidadeVertical + gravidade * Time.deltaTime;

        // Pega as direções da câmera
        Vector3 frenteCamera = cameraTransform.forward;
        Vector3 direitaCamera = cameraTransform.right;

        // Remove o movimento vertical da direção da câmera
        frenteCamera.y = 0f;
        direitaCamera.y = 0f;

        // Normaliza as direções
        frenteCamera.Normalize();
        direitaCamera.Normalize();

        // Calcula a direção do movimento em relação à câmera
        Vector3 direcao = frenteCamera * z + direitaCamera * x;

        // Evita que o movimento diagonal seja mais rápido
        if (direcao.magnitude > 1f)
        {
            direcao.Normalize();
        }

        // Faz o jogador virar aos poucos para a direção em que está andando
        if (direcao != Vector3.zero)
        {
            Quaternion rotacaoAlvo = Quaternion.LookRotation(direcao);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                rotacaoAlvo,
                velocidadeGiro * Time.deltaTime
            );
        }

        // Aproxima a velocidade atual da velocidade desejada, sem mudança brusca
        velocidadeHorizontal = Vector3.MoveTowards(
            velocidadeHorizontal,
            direcao * velocidadeAndar,
            aceleracao * Time.deltaTime
        );

        // Junta o movimento horizontal com o movimento vertical
        Vector3 movimento =
            velocidadeHorizontal +
            Vector3.up * velocidadeVertical;

        // Move o jogador
        controller.Move(movimento * Time.deltaTime);
    }
}