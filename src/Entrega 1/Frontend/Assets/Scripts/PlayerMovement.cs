
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float velocidade = 5f;
    public float forcaPulo = 5f;
    public float gravidade = -9.8f;
    public float velocidadeCorrida = 15f;

    private float velocidadeVertical = 0f;
    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float x = 0f;
        float z = 0f;

        //Movimento para frente
        if (Keyboard.current.wKey.isPressed)
        {
            z = 1f;
        }

        //Movimento para tras
        if (Keyboard.current.sKey.isPressed)
        {
            z = -1f;
        }

        //Movimento para esquerda
        if (Keyboard.current.aKey.isPressed)
        {
            x = -1f;
        }

        //Movimento para direita
        if (Keyboard.current.dKey.isPressed)
        {
            x = 1f;
        }

        // Define qual velocidade será usada
        float velocidadeAtual = velocidade;

        // Se apertar Shift, usa a velocidade de corrida
        if (Keyboard.current.leftShiftKey.isPressed)
        {
            velocidadeAtual = velocidadeCorrida;
        }

        // Verifica se o personagem está no chão
        if (controller.isGrounded && velocidadeVertical < 0)
        {
            velocidadeVertical = -2f;
        }

        // Pulo
        if (Keyboard.current.spaceKey.wasPressedThisFrame && controller.isGrounded)
        {
            velocidadeVertical = forcaPulo;
        }

        // Gravidade
        velocidadeVertical = velocidadeVertical + gravidade * Time.deltaTime;

        // Agora usa velocidadeAtual
        Vector3 movimento = new Vector3(
            x * velocidadeAtual,
            velocidadeVertical,
            z * velocidadeAtual
        );

        // Move o personagem
        controller.Move(movimento * Time.deltaTime);
    }
}