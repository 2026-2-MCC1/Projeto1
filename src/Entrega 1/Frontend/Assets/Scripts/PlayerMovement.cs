using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float velocidade = 5f;
    public float velocidadeCorrida = 15f;
    public float forcaPulo = 5f;
    public float gravidade = -9.8f;

    private float velocidadeVertical = 0f;
    private CharacterController controller;
    private Transform cameraTransform;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        float x = 0f;
        float z = 0f;

        if (Keyboard.current.wKey.isPressed)
        {
            z = 1f;
        }

        if (Keyboard.current.sKey.isPressed)
        {
            z = -1f;
        }

        if (Keyboard.current.aKey.isPressed)
        {
            x = -1f;
        }

        if (Keyboard.current.dKey.isPressed)
        {
            x = 1f;
        }

        float velocidadeAtual = velocidade;

        if (Keyboard.current.leftShiftKey.isPressed)
        {
            velocidadeAtual = velocidadeCorrida;
        }

        if (controller.isGrounded && velocidadeVertical < 0)
        {
            velocidadeVertical = -2f;
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame && controller.isGrounded)
        {
            velocidadeVertical = forcaPulo;
        }

        velocidadeVertical = velocidadeVertical + gravidade * Time.deltaTime;
        
        Vector3 frenteCamera = cameraTransform.forward;
        Vector3 direitaCamera = cameraTransform.right;

        frenteCamera.y = 0f;
        direitaCamera.y = 0f;

        frenteCamera.Normalize();
        direitaCamera.Normalize();

        Vector3 direcao = frenteCamera * z + direitaCamera * x;
        
        if (direcao.magnitude > 1f)
        {
            direcao.Normalize();
        }

        if (direcao != Vector3.zero)
        {
            transform.forward = direcao;
        }

        Vector3 movimento = direcao * velocidadeAtual + Vector3.up * velocidadeVertical;
      
        controller.Move(movimento * Time.deltaTime);
    }
}