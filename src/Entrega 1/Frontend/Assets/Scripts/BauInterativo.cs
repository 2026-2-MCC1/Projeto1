using UnityEngine;
using UnityEngine.InputSystem;

public class BauInterativo : MonoBehaviour
{
    // Referência do jogador
    public Transform jogador;

    // Distância máxima para conseguir interagir com o baú
    public float distanciaInteracao = 2f;

    // Define se este baú possui ou não o cartão de acesso
    public bool contemCartao = false;

    // Referências para atualizar as mensagens e o objetivo na tela
    public MensagemUI mensagemUI;
    public ObjetivoUI objetivoUI;

    // Guarda se o baú já foi aberto
    private bool aberto = false;

    // Guarda se o jogador estava perto no frame anterior
    private bool estavaPerto = false;

    void Update()
    {
        // Calcula a distância entre o jogador e o baú
        float distancia = Vector3.Distance(
            transform.position,
            jogador.position
        );

        // Verifica se o jogador está perto e se o baú ainda não foi aberto
        bool estaPerto = distancia <= distanciaInteracao && aberto == false;

        if (estaPerto == true)
        {
            // Mostra a mensagem de interação
            mensagemUI.MostrarMensagem("Pressione E para abrir");

            // Se o jogador apertar E, abre o baú
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                AbrirBau();
            }
        }

        // Limpa a mensagem quando o jogador se afasta do baú
        if (estaPerto == false && estavaPerto == true)
        {
            mensagemUI.LimparMensagem();
        }

        // Atualiza a informação de proximidade
        estavaPerto = estaPerto;
    }

    void AbrirBau()
    {
        // Marca o baú como aberto
        aberto = true;

        // Verifica se este baú contém o cartão
        if (contemCartao == true)
        {
            // Acessa o inventário do jogador
            InventarioJogador inventario =
                jogador.GetComponent<InventarioJogador>();

            // Adiciona o cartão ao inventário
            inventario.temCartao = true;

            // Mostra a mensagem de cartão encontrado
            mensagemUI.MostrarMensagem("Cartão de acesso encontrado!");

            // Atualiza o objetivo da fase
            objetivoUI.CartaoEncontrado();
        }
        else
        {
            // Caso o baú não tenha o cartão
            mensagemUI.MostrarMensagem("Baú vazio!");
        }

        // Desativa o baú depois que ele é aberto
        gameObject.SetActive(false);
    }
}