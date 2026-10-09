using UnityEngine;
using UnityEngine.InputSystem;

public class BauInterativo : MonoBehaviour
{
    public Transform jogador;
    public float distanciaInteracao = 2f;
    public bool contemCartao = false;

    private bool aberto = false;

    void Update()
    {
        float distancia = Vector3.Distance(transform.position, jogador.position);


        if (distancia <= distanciaInteracao)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame && aberto == false)
            {
                AbrirBau();
            }
        }
    }

    void AbrirBau()
    {
        aberto = true;

        if (contemCartao == true)
        {
            InventarioJogador inventario = jogador.GetComponent<InventarioJogador>();

            inventario.temCartao = true;

            Debug.Log("Cartão de acesso encontrado!");
        }
        else
        {
            Debug.Log("Baú vazio!");
        }

        gameObject.SetActive(false);
    }
}
