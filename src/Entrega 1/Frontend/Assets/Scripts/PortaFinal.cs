using UnityEngine;
using UnityEngine.InputSystem;

public class PortaFinal : MonoBehaviour
{
    public Transform jogador;
    public float distanciaInteracao = 3f;
    public MensagemUI mensagemUI;

    private bool estavaPerto = false;
    private bool mensagemTemporaria = false;

    void Update()
    {
        float distancia = Vector3.Distance(
            transform.position,
            jogador.position
        );

        bool estaPerto = distancia <= distanciaInteracao;

        if (estaPerto == true && mensagemTemporaria == false)
        {
            mensagemUI.MostrarMensagem("Pressione E para usar o cartão");

            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                TentarAbrir();
            }
        }

        if (estaPerto == false && estavaPerto == true)
        {
            mensagemUI.LimparMensagem();
        }

        estavaPerto = estaPerto;
    }

    void TentarAbrir()
    {
        InventarioJogador inventario =
            jogador.GetComponent<InventarioJogador>();

        if (inventario.temCartao == true)
        {
            mensagemUI.MostrarMensagem("Porta aberta! Fase concluída!");

            gameObject.SetActive(false);
        }
        else
        {
            mensagemTemporaria = true;

            mensagemUI.MostrarMensagem(
                "Você precisa encontrar o cartão de acesso!"
            );

            Invoke("LiberarMensagem", 2f);
        }
    }

    void LiberarMensagem()
    {
        mensagemTemporaria = false;
    }
}