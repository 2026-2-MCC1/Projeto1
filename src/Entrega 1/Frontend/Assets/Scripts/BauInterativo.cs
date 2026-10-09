using UnityEngine;
using UnityEngine.InputSystem;

public class BauInterativo : MonoBehaviour
{
    public Transform jogador;
    public float distanciaInteracao = 2f;
    public bool contemCartao = false;

    public MensagemUI mensagemUI;
    public ObjetivoUI objetivoUI;

    private bool aberto = false;
    private bool estavaPerto = false;

    void Update()
    {
        float distancia = Vector3.Distance(
            transform.position,
            jogador.position
        );

        bool estaPerto = distancia <= distanciaInteracao && aberto == false;

        if (estaPerto == true)
        {
            mensagemUI.MostrarMensagem("Pressione E para abrir");

            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                AbrirBau();
            }
        }

        if (estaPerto == false && estavaPerto == true)
        {
            mensagemUI.LimparMensagem();
        }

        estavaPerto = estaPerto;
    }

    void AbrirBau()
    {
        aberto = true;

        if (contemCartao == true)
        {
            InventarioJogador inventario =
                jogador.GetComponent<InventarioJogador>();

            inventario.temCartao = true;

            mensagemUI.MostrarMensagem("Cartão de acesso encontrado!");

            objetivoUI.CartaoEncontrado();
        }
        else
        {
            mensagemUI.MostrarMensagem("Baú vazio!");
        }

        gameObject.SetActive(false);
    }
}