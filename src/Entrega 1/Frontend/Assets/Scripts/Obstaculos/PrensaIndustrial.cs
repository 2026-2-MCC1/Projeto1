using UnityEngine;

// Prensa da fábrica (fase 1): fica parada em cima, dá uma tremida de aviso
public class PrensaIndustrial : MonoBehaviour
{
    [Header("Altura (Y no mundo, igual ao ObstaculoVertical)")]
    public float limiteBaixo = 1f;
    public float limiteAlto = 4.9f;

    // Ligado: os limites contam a partir de onde a prensa foi colocada na cena
    public bool limitesRelativos = false;

    [Header("Tempos")]
    // Atraso antes do primeiro ciclo, para as prensas não baterem todas juntas
    public float atrasoInicial = 0f;
    public float tempoParadaEmCima = 1.2f;
    public float tempoTremendo = 0.55f;
    public float tempoParadaEmbaixo = 0.5f;

    [Header("Movimento")]
    // A queda começa rápida e acelera até a velocidade máxima
    public float velocidadeQuedaInicial = 6f;
    public float aceleracaoQueda = 70f;
    public float velocidadeQuedaMaxima = 22f;
    public float velocidadeSubida = 2.2f;

    // Tremida de aviso (para os lados) e quanto ela "afunda" antes de cair
    public float forcaTremida = 0.06f;
    public float afundadaAviso = 0.12f;

    [Header("Impacto")]
    // Distância até onde a câmera treme quando a prensa bate
    public float alcanceTremorCamera = 9f;

    private enum Estado { EmCima, Tremendo, Caindo, Embaixo, Subindo }
    private Estado estado = Estado.EmCima;

    private float tempo = 0f;
    private float velocidade = 0f;
    private float origem = 0f;
    private Vector3 posicaoBase;

    private Collider colisor;
    private CharacterController jogador;
    private InventarioJogador inventario;
    private CameraCrash cameraJogo;
    private SomPrensa som;

    float Alto { get { return origem + limiteAlto; } }
    float Baixo { get { return origem + limiteBaixo; } }

    void Start()
    {
        if (limitesRelativos == true)
        {
            origem = transform.position.y;
        }

        colisor = GetComponent<Collider>();
        cameraJogo = FindFirstObjectByType<CameraCrash>();

        PlayerMovement movimento = FindFirstObjectByType<PlayerMovement>();
        if (movimento != null)
        {
            jogador = movimento.GetComponent<CharacterController>();
            inventario = movimento.GetComponent<InventarioJogador>();
        }

        // O som da batida passa a ser tocado por este script, na hora exata do impacto
        som = GetComponent<SomPrensa>();
        if (som != null)
        {
            som.automatico = false;
        }

        // Começa lá em cima
        posicaoBase = transform.position;
        posicaoBase.y = Alto;
        transform.position = posicaoBase;
        tempo = -atrasoInicial;
    }

    void Update()
    {
        tempo += Time.deltaTime;

        switch (estado)
        {
            case Estado.EmCima:
                if (tempo >= tempoParadaEmCima)
                {
                    Trocar(Estado.Tremendo);
                }
                break;

            case Estado.Tremendo:
                // Treme para os lados e afunda um pouquinho: é o aviso para o jogador sair
                float t = Mathf.Clamp01(tempo / tempoTremendo);
                Vector3 tremida = new Vector3(
                    Mathf.Sin(tempo * 70f) * forcaTremida,
                    -afundadaAviso * t + Mathf.Sin(tempo * 95f) * forcaTremida * 0.5f,
                    Mathf.Cos(tempo * 83f) * forcaTremida);
                transform.position = posicaoBase + tremida;

                if (tempo >= tempoTremendo)
                {
                    transform.position = posicaoBase;
                    velocidade = velocidadeQuedaInicial;
                    Trocar(Estado.Caindo);
                }
                break;

            case Estado.Caindo:
                velocidade = Mathf.Min(velocidade + aceleracaoQueda * Time.deltaTime, velocidadeQuedaMaxima);
                Vector3 posicao = transform.position;
                posicao.y -= velocidade * Time.deltaTime;

                bool bateu = posicao.y <= Baixo;
                if (bateu)
                {
                    posicao.y = Baixo;
                }
                transform.position = posicao;

                VerificarEsmagamento();

                if (bateu)
                {
                    Impacto();
                    Trocar(Estado.Embaixo);
                }
                break;

            case Estado.Embaixo:
                if (tempo >= tempoParadaEmbaixo)
                {
                    Trocar(Estado.Subindo);
                }
                break;

            case Estado.Subindo:
                Vector3 subindo = transform.position;
                subindo.y = Mathf.MoveTowards(subindo.y, Alto, velocidadeSubida * Time.deltaTime);
                transform.position = subindo;

                if (subindo.y >= Alto)
                {
                    posicaoBase = subindo;
                    Trocar(Estado.EmCima);
                }
                break;
        }
    }

    void Trocar(Estado novo)
    {
        estado = novo;
        tempo = 0f;
    }

    // Tartaruga embaixo da prensa durante a queda
    void VerificarEsmagamento()
    {
        if (jogador == null || inventario == null || colisor == null)
        {
            return;
        }

        Bounds prensa = colisor.bounds;
        Bounds corpo = jogador.bounds;

        // Precisa estar embaixo da prensa (no chão) e com a prensa já encostando nela
        bool embaixoNaHorizontal =
            corpo.max.x > prensa.min.x + 0.1f && corpo.min.x < prensa.max.x - 0.1f &&
            corpo.max.z > prensa.min.z - 0.15f && corpo.min.z < prensa.max.z + 0.15f;
        bool prensaEncostou = prensa.min.y <= corpo.max.y && prensa.max.y > corpo.min.y;

        if (embaixoNaHorizontal && prensaEncostou)
        {
            // Empurra para fora, para o lado em que a tartaruga já estava
            Vector3 origemGolpe = prensa.center;
            origemGolpe.y = corpo.center.y;
            inventario.PerderVida(origemGolpe);
        }
    }

    void Impacto()
    {
        if (som != null)
        {
            som.Tocar();
        }

        if (cameraJogo != null && jogador != null)
        {
            float distancia = Vector3.Distance(jogador.transform.position, transform.position);
            float perto = 1f - Mathf.Clamp01(distancia / alcanceTremorCamera);
            if (perto > 0f)
            {
                cameraJogo.Tremer(0.22f * perto, 0.22f);
            }
        }
    }
}
