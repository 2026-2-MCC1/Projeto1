using UnityEngine;
using UnityEngine.InputSystem;

public class PortaFinal : MonoBehaviour
{
    public Transform jogador;
    public float distanciaInteracao = 3f;

    void Update()
    {
        float distancia = Vector3.Distance(
            transform.position,
            jogador.position
        );

        if (distancia <= distanciaInteracao)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                TentarAbrir();
            }
        }
    }

    void TentarAbrir()
    {
        InventarioJogador inventario =
            jogador.GetComponent<InventarioJogador>();

        if (inventario.temCartao == true)
        {
            Debug.Log("Porta aberta! Fase concluída!");

            gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("Você precisa encontrar o cartão de acesso!");
        }
    }
}