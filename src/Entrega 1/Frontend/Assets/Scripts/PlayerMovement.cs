using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    // Velocidade normal do jogador
    public float velocidade = 5f;

    // Velocidade usada quando o jogador corre
    public float velocidadeCorrida = 10f;

    // Força aplicada no pulo
    public float forcaPulo = 5f;

    // Valor usado para simular a gravidade
    public float gravidade = -9.8f;

    // Guarda a velocidade vertical do jogador
    private float velocidadeVertical = 0f;

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

        // Define a velocidade normal como padrão
        float velocidadeAtual = velocidade;

        // Se Shift estiver pressionado, o jogador corre
        if (Keyboard.current.leftShiftKey.isPressed)
        {
            velocidadeAtual = velocidadeCorrida;
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

        // Faz o jogador virar para a direção em que está andando
        if (direcao != Vector3.zero)
        {
            transform.forward = direcao;
        }

        // Junta o movimento horizontal com o movimento vertical
        Vector3 movimento =
            direcao * velocidadeAtual +
            Vector3.up * velocidadeVertical;

        // Move o jogador
        controller.Move(movimento * Time.deltaTime);
    }
}