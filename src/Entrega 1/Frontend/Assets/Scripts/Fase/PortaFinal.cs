
using UnityEngine;
using UnityEngine.InputSystem;
using FactoryEscape;

public class PortaFinal : MonoBehaviour
{
    // Referência do jogador
    public Transform jogador;

    // Distância máxima para interagir com a porta
    public float distanciaInteracao = 3f;

    // Referência para mostrar mensagens na tela
    public MensagemUI mensagemUI;

    // Quantidade de Rocklets necessária para abrir a porta
    public int metaRocklets = 12;

    // Tempo entre abrir a porta e mudar de cena
    public float atrasoVitoria = 0.5f;

    // Som de porta abrindo
    public AudioClip somPorta;

    private bool estavaPerto = false;
    private bool mensagemTemporaria = false;
    private bool concluida = false;

    void Start()
    {
        // Procura automaticamente o jogador
        if (jogador == null)
        {
            PlayerMovement movimento =
                FindFirstObjectByType<PlayerMovement>();

            if (movimento != null)
            {
                jogador = movimento.transform;
            }
            else
            {
                Debug.LogError(
                    "PortaFinal: Jogador não encontrado!"
                );

                enabled = false;
                return;
            }
        }

        // Procura automaticamente a interface de mensagens
        if (mensagemUI == null)
        {
            mensagemUI = FindFirstObjectByType<MensagemUI>();
        }

        if (mensagemUI == null)
        {
            Debug.LogWarning(
                "PortaFinal: MensagemUI não encontrada!"
            );
        }
    }

    void Update()
    {
        // Impede novas interações depois que a porta for aberta
        if (concluida)
            return;

        if (jogador == null || mensagemUI == null)
            return;

        // Calcula a distância entre jogador e porta
        float distancia = Vector3.Distance(
            transform.position,
            jogador.position
        );

        bool estaPerto = distancia <= distanciaInteracao;

        // Mostra a mensagem de interação
        if (estaPerto && mensagemTemporaria == false)
        {
            mensagemUI.MostrarMensagem(
                "Pressione E para usar o cartão"
            );

            // Verifica se o jogador apertou E
            if (Keyboard.current != null &&
                Keyboard.current.eKey.wasPressedThisFrame)
            {
                TentarAbrir();
            }
        }

        // Limpa a mensagem quando o jogador se afasta
        if (!estaPerto && estavaPerto)
        {
            mensagemUI.LimparMensagem();
        }

        estavaPerto = estaPerto;
    }

    void TentarAbrir()
    {
        // Acessa o inventário do jogador
        InventarioJogador inventario =
            jogador.GetComponent<InventarioJogador>();

        if (inventario == null)
        {
            Debug.LogError(
                "PortaFinal: InventarioJogador não encontrado!"
            );
            return;
        }

        // Verifica se possui o cartão de acesso
        if (!inventario.temCartao)
        {
            MostrarAviso(
                "Você precisa encontrar o cartão de acesso!"
            );
        }
        // Verifica a quantidade de Rocklets
        else if (inventario.Rocklets < metaRocklets)
        {
            int faltam = metaRocklets - inventario.Rocklets;

            MostrarAviso(
                "Faltam " + faltam + " Rocklets para abrir a porta!"
            );
        }
        else
        {
            // Informa que a fase foi concluída
            mensagemUI.MostrarMensagem(
                "Porta aberta! Fase concluída!"
            );

            concluida = true;

            // Toca o som da porta
            FESfx.Play(somPorta);

            // Faz a porta desaparecer
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                r.enabled = false;
            }

            // Desativa os colisores da porta
            foreach (Collider c in GetComponentsInChildren<Collider>())
            {
                c.enabled = false;
            }

            // Aguarda antes de abrir a tela correspondente
            Invoke("IrParaVitoria", atrasoVitoria);
        }
    }

    void IrParaVitoria()
    {
        // Fases 1 e 2: abre a tela de Vitória.
        // Fase 3: abre a tela de Fim de Jogo.
        GameFlow.Victory();
    }

    void MostrarAviso(string aviso)
    {
        mensagemTemporaria = true;

        mensagemUI.MostrarMensagem(aviso);

        // Libera novamente a mensagem padrão após 2 segundos
        Invoke("LiberarMensagem", 2f);
    }

    void LiberarMensagem()
    {
        mensagemTemporaria = false;
    }
}
