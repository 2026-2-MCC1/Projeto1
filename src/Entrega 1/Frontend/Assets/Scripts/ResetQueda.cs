using UnityEngine;

public class ResetQueda : MonoBehaviour
{
    // Ponto para onde o jogador volta caso caia
    public Transform pontoRespawn;

    // Altura mínima permitida antes de considerar que o jogador caiu
    public float limiteQueda = -5f;

    // Referência ao CharacterController do jogador
    private CharacterController controller;

    void Start()
    {
        // Busca o CharacterController presente no Player
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Verifica se o jogador caiu abaixo do limite definido
        if (transform.position.y < limiteQueda)
        {
            // Desativa temporariamente o CharacterController
            // para permitir a mudança de posição do jogador
            controller.enabled = false;

            // Retorna o jogador para o ponto de respawn
            transform.position = pontoRespawn.position;

            // Ativa novamente o CharacterController
            controller.enabled = true;
        }
    }
}