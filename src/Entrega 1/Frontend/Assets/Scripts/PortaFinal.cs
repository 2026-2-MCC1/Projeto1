using UnityEngine;
using UnityEngine.InputSystem;

public class PortaFinal : MonoBehaviour
{
    // Referência do jogador
    public Transform jogador;

    // Distância máxima para interagir com a porta
    public float distanciaInteracao = 3f;

    // Referência para mostrar mensagens na tela
    public MensagemUI mensagemUI;

    // Guarda se o jogador estava perto da porta no frame anterior
    private bool estavaPerto = false;

    // Controla se uma mensagem temporária está sendo exibida
    private bool mensagemTemporaria = false;

    void Update()
    {
        // Calcula a distância entre o jogador e a porta
        float distancia = Vector3.Distance(
            transform.position,
            jogador.position
        );

        // Verifica se o jogador está perto da porta
        bool estaPerto = distancia <= distanciaInteracao;

        // Mostra a mensagem de interação quando o jogador está perto
        if (estaPerto == true && mensagemTemporaria == false)
        {
            mensagemUI.MostrarMensagem("Pressione E para usar o cartão");

            // Se o jogador apertar E, tenta abrir a porta
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                TentarAbrir();
            }
        }

        // Limpa a mensagem quando o jogador se afasta
        if (estaPerto == false && estavaPerto == true)
        {
            mensagemUI.LimparMensagem();
        }

        // Atualiza a informação de proximidade
        estavaPerto = estaPerto;
    }

    void TentarAbrir()
    {
        // Acessa o inventário do jogador
        InventarioJogador inventario =
            jogador.GetComponent<InventarioJogador>();

        // Verifica se o jogador possui o cartão de acesso
        if (inventario.temCartao == true)
        {
            // Informa que a fase foi concluída
            mensagemUI.MostrarMensagem("Porta aberta! Fase concluída!");

            // Desativa a porta
            gameObject.SetActive(false);
        }
        else
        {
            // Impede que outra mensagem substitua o aviso imediatamente
            mensagemTemporaria = true;

            // Informa que o jogador ainda precisa encontrar o cartão
            mensagemUI.MostrarMensagem(
                "Você precisa encontrar o cartão de acesso!"
            );

            // Após 2 segundos, libera novamente a mensagem padrão da porta
            Invoke("LiberarMensagem", 2f);
        }
    }

    void LiberarMensagem()
    {
        // Permite que a mensagem de interação volte a aparecer
        mensagemTemporaria = false;
    }
}