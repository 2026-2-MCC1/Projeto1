
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    // Velocidade do jogador
    public float velocidadeAndar = 6.5f;

    // Força aplicada no pulo
    public float forcaPulo = 5f;

    // Gravidade
    public float gravidade = -9.8f;

    // Velocidade de giro
    public float velocidadeGiro = 540f;

    // Aceleração
    public float aceleracao = 30f;

    // Velocidade vertical
    private float velocidadeVertical = 0f;

    // Velocidade horizontal
    private Vector3 velocidadeHorizontal = Vector3.zero;

    // CharacterController
    private CharacterController controller;

    // Câmera principal
    private Transform cameraTransform;

    // Informa se o jogador está recebendo um empurrão
    private bool recebendoEmpurrao = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (Camera.main != null)
            cameraTransform = Camera.main.transform;
        else
            Debug.LogError("Câmera principal não encontrada!");
    }

    // Chamado pelo RespawnJogador durante o empurrão
    public void DefinirEmpurrao(bool estado)
    {
        recebendoEmpurrao = estado;
    }

    void Update()
    {
        // Impede o movimento normal durante o empurrão
        if (recebendoEmpurrao)
            return;

        if (controller == null || cameraTransform == null)
            return;

        float x = 0f;
        float z = 0f;

        // Movimento para frente
        if (Keyboard.current.wKey.isPressed)
            z = 1f;

        // Movimento para trás
        if (Keyboard.current.sKey.isPressed)
            z = -1f;

        // Movimento para esquerda
        if (Keyboard.current.aKey.isPressed)
            x = -1f;

        // Movimento para direita
        if (Keyboard.current.dKey.isPressed)
            x = 1f;

        // Mantém o jogador próximo ao chão
        if (controller.isGrounded && velocidadeVertical < 0)
            velocidadeVertical = -2f;

        // Pulo
        if (Keyboard.current.spaceKey.wasPressedThisFrame && controller.isGrounded)
            velocidadeVertical = forcaPulo;

        // Gravidade
        velocidadeVertical += gravidade * Time.deltaTime;

        // Direções da câmera
        Vector3 frenteCamera = cameraTransform.forward;
        Vector3 direitaCamera = cameraTransform.right;

        frenteCamera.y = 0f;
        direitaCamera.y = 0f;

        frenteCamera.Normalize();
        direitaCamera.Normalize();

        // Movimento relativo à câmera
        Vector3 direcao = frenteCamera * z + direitaCamera * x;

        // Evita velocidade diagonal maior
        if (direcao.magnitude > 1f)
            direcao.Normalize();

        // Rotação do jogador
        if (direcao != Vector3.zero)
        {
            Quaternion rotacaoAlvo = Quaternion.LookRotation(direcao);

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                rotacaoAlvo,
                velocidadeGiro * Time.deltaTime
            );
        }

        // Aceleração horizontal
        velocidadeHorizontal = Vector3.MoveTowards(
            velocidadeHorizontal,
            direcao * velocidadeAndar,
            aceleracao * Time.deltaTime
        );

        // Movimento completo
        Vector3 movimento =
            velocidadeHorizontal +
            Vector3.up * velocidadeVertical;

        controller.Move(movimento * Time.deltaTime);
    }
}
