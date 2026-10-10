using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMouse : MonoBehaviour
{
    // Objeto que a câmera deve seguir, neste caso o jogador
    public Transform alvo;

    // Configurações da câmera
    public float distancia = 6f;
    public float altura = 2f;
    public float sensibilidade = 0.10f;

    // Valores usados para controlar a rotação da câmera
    private float rotacaoHorizontal = 0f;
    private float rotacaoVertical = 15f;

    void Start()
    {
        // Trava o cursor no centro da tela e deixa ele invisível
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        // Impede erros caso o mouse ou o alvo não estejam disponíveis
        if (Mouse.current == null || alvo == null)
        {
            return;
        }

        // Lê o movimento do mouse
        Vector2 movimentoMouse = Mouse.current.delta.ReadValue();

        // Atualiza a rotação horizontal da câmera
        rotacaoHorizontal =
            rotacaoHorizontal +
            movimentoMouse.x * sensibilidade;

        // Atualiza a rotação vertical da câmera
        rotacaoVertical =
            rotacaoVertical -
            movimentoMouse.y * sensibilidade;

        // Limita o quanto a câmera pode olhar para cima e para baixo
        rotacaoVertical =
            Mathf.Clamp(rotacaoVertical, -20f, 70f);

        // Cria a rotação da câmera
        Quaternion rotacao =
            Quaternion.Euler(
                rotacaoVertical,
                rotacaoHorizontal,
                0f
            );

        // Calcula a posição da câmera em relação ao jogador
        Vector3 posicaoCamera =
            alvo.position +
            Vector3.up * altura +
            rotacao * new Vector3(
                0f,
                0f,
                -distancia
            );

        // Atualiza a posição da câmera
        transform.position = posicaoCamera;

        // Faz a câmera continuar olhando para o jogador
        transform.LookAt(
            alvo.position + Vector3.up * 1f
        );
    }
}
}