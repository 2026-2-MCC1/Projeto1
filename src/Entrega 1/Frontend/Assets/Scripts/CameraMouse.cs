using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMouse : MonoBehaviour
{
    public Transform alvo;

    public float distancia = 6f;
    public float altura = 2f;
    public float sensibilidade = 0.10f;

    private float rotacaoHorizontal = 0f;
    private float rotacaoVertical = 15f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (Mouse.current == null || alvo == null)
        {
            return;
        }

        Vector2 movimentoMouse = Mouse.current.delta.ReadValue();

        rotacaoHorizontal = rotacaoHorizontal + movimentoMouse.x * sensibilidade;
        
        rotacaoVertical = rotacaoVertical - movimentoMouse.y * sensibilidade;

        rotacaoVertical = Mathf.Clamp(rotacaoVertical, -20f, 70f);

        Quaternion rotacao = Quaternion.Euler(rotacaoVertical, rotacaoHorizontal, 0f);

        Vector3 posicaoCamera = alvo.position + Vector3.up * altura + rotacao * new Vector3(0f, 0f, -distancia);
                
        transform.position = posicaoCamera;

        transform.LookAt(alvo.position + Vector3.up * 1f);
    }
}
