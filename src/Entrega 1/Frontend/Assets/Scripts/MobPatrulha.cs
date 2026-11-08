using UnityEngine;
using UnityEngine.InputSystem;

// Faz o mob usar as animações: anda de um lado para o outro, ataca quando o jogador chega perto
// e morre quando o jogador gira ao lado dele ou pula em cima.
// Quando ataca, machuca o jogador (tira uma vida; se as vidas acabarem, abre o Game Over).
[RequireComponent(typeof(Animator))]
public class MobPatrulha : MonoBehaviour
{
    // Até onde o mob anda para cada lado do ponto inicial, e em qual direção
    public float alcance = 2.5f;
    public Vector3 eixo = Vector3.right;

    // Velocidade da caminhada e tempo parado em cada ponta do caminho
    public float velocidade = 1.4f;
    public float tempoParado = 1f;

    // Graus por segundo que o mob vira
    public float velocidadeGiro = 360f;

    // Distância em que ele ataca e tempo entre um ataque e outro
    public float distanciaAtaque = 2.2f;
    public float intervaloAtaque = 2.2f;

    // Dano: marque para o ataque tirar uma vida do jogador
    public bool causarDano = true;

    // Segundos entre o início da animação de ataque e o golpe acertar (ajuste pela animação)
    public float atrasoDano = 0.6f;

    // Folga extra de distância na hora do golpe (o jogador ainda leva dano se recuar um pouco)
    public float folgaDano = 0.5f;

    // Escreve no Console o que acontece com o dano (desmarque quando tudo estiver funcionando)
    public bool mostrarLogs = true;

    // Tecla do giro do jogador e distância em que ele alcança o mob
    public Key teclaGiro = Key.Q;
    public float distanciaGiro = 2f;

    private Animator animator;
    private CapsuleCollider colisor;
    private CharacterController jogador;
    private RespawnJogador respawnJogador;
    private Vector3 centro;

    // Lado para onde está andando: 1 ou -1
    private float sentido = 1f;
    private float esperaPatrulha = 0f;
    private float esperaAtaque = 0f;
    private bool morto = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        colisor = GetComponent<CapsuleCollider>();
        centro = transform.position;

        // Começa em pontos diferentes do caminho, para os mobs não andarem iguais
        sentido = Random.value < 0.5f ? 1f : -1f;
        esperaPatrulha = Random.Range(0f, tempoParado);

        // O mob é um prefab, então procura o jogador quando entra na cena
        PlayerMovement movimento = FindFirstObjectByType<PlayerMovement>();
        if (movimento != null)
        {
            jogador = movimento.GetComponent<CharacterController>();

            // Procura o RespawnJogador no mesmo objeto (ou no pai)
            respawnJogador = movimento.GetComponentInParent<RespawnJogador>();
        }

        // Se não achou ali, procura em qualquer lugar da cena
        if (respawnJogador == null)
        {
            respawnJogador = FindFirstObjectByType<RespawnJogador>();
        }

        if (jogador == null)
        {
            Debug.LogWarning("[MobPatrulha] Não achei o jogador (PlayerMovement). O mob não vai atacar.", this);
        }
        if (respawnJogador == null)
        {
            Debug.LogWarning("[MobPatrulha] Não achei o RespawnJogador na cena. O mob não vai causar dano.", this);
        }
    }

    void Update()
    {
        if (morto)
        {
            return;
        }

        esperaAtaque -= Time.deltaTime;

        if (jogador != null)
        {
            if (JogadorAcertou())
            {
                Morrer();
                return;
            }

            Vector3 paraJogador = jogador.transform.position - transform.position;
            float altura = Mathf.Abs(paraJogador.y);
            paraJogador.y = 0f;

            if (paraJogador.magnitude <= distanciaAtaque && altura <= 2.5f)
            {
                Atacar(paraJogador);
                return;
            }
        }

        Patrulhar();
    }

    bool JogadorAcertou()
    {
        Bounds corpo = jogador.bounds;
        Bounds limites = colisor.bounds;

        Vector3 diferenca = corpo.center - limites.center;
        float altura = Mathf.Abs(diferenca.y);
        diferenca.y = 0f;
        float distancia = diferenca.magnitude;

        // Giro: só no chão, igual à animação da tartaruga
        if (Keyboard.current != null
            && Keyboard.current[teclaGiro].wasPressedThisFrame
            && jogador.isGrounded
            && distancia <= distanciaGiro
            && altura <= 1.6f)
        {
            return true;
        }

        // Pulo em cima: a folga é maior porque o pulo mal passa da altura do mob
        float pesAcimaDoTopo = corpo.min.y - limites.max.y;
        return jogador.velocity.y < -0.5f
            && distancia <= 1f
            && pesAcimaDoTopo >= -0.45f
            && pesAcimaDoTopo <= 0.25f;
    }

    void Atacar(Vector3 paraJogador)
    {
        animator.SetFloat("Velocidade", 0f);
        Virar(paraJogador);

        if (esperaAtaque <= 0f)
        {
            animator.SetTrigger("Atacar");
            esperaAtaque = intervaloAtaque;

            // Quando o golpe acertar, verifica se o jogador ainda está ao alcance
            if (causarDano == true)
            {
                Invoke(nameof(CausarDano), atrasoDano);
            }
            else if (mostrarLogs == true)
            {
                Debug.Log("[MobPatrulha] Atacou, mas 'Causar Dano' está DESMARCADO no Inspector.", this);
            }
        }
    }

    // Machuca o jogador se ele ainda estiver ao alcance quando o golpe acerta
    void CausarDano()
    {
        if (morto || jogador == null)
        {
            return;
        }

        // Tenta achar o RespawnJogador de novo, caso não tenha achado no Start
        if (respawnJogador == null)
        {
            respawnJogador = FindFirstObjectByType<RespawnJogador>();
        }

        if (respawnJogador == null)
        {
            Debug.LogWarning("[MobPatrulha] O golpe acertou, mas não existe RespawnJogador na cena. Adicione o componente no Player.", this);
            return;
        }

        Vector3 paraJogador = jogador.transform.position - transform.position;
        float altura = Mathf.Abs(paraJogador.y);
        paraJogador.y = 0f;
        float distancia = paraJogador.magnitude;

        if (distancia <= distanciaAtaque + folgaDano && altura <= 2.5f)
        {
            if (mostrarLogs == true)
            {
                Debug.Log("[MobPatrulha] Golpe acertou! Distância: " + distancia.ToString("F2") + ". Chamando LevarDano().", this);
            }

            // Tira uma vida e volta ao checkpoint; se as vidas acabarem, abre o Game Over
            respawnJogador.TomarDano();
        }
        else if (mostrarLogs == true)
        {
            Debug.Log("[MobPatrulha] Golpe errou: o jogador estava longe (distância " + distancia.ToString("F2") +
                      ", altura " + altura.ToString("F2") + ").", this);
        }
    }

    void Patrulhar()
    {
        if (esperaPatrulha > 0f)
        {
            esperaPatrulha -= Time.deltaTime;
            animator.SetFloat("Velocidade", 0f);
            return;
        }

        Vector3 direcao = eixo.normalized;
        Vector3 alvo = centro + direcao * alcance * sentido;
        Vector3 caminho = alvo - transform.position;
        caminho.y = 0f;

        // Chegou na ponta: para um pouco e volta
        if (caminho.magnitude < 0.05f)
        {
            sentido = -sentido;
            esperaPatrulha = tempoParado;
            animator.SetFloat("Velocidade", 0f);
            return;
        }

        Vector3 passo = Vector3.ClampMagnitude(caminho, velocidade * Time.deltaTime);
        transform.position += passo;
        Virar(caminho);
        AcompanharChao();
        animator.SetFloat("Velocidade", velocidade);
    }

    void Virar(Vector3 direcao)
    {
        if (direcao.sqrMagnitude < 0.0001f)
        {
            return;
        }

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            Quaternion.LookRotation(direcao),
            velocidadeGiro * Time.deltaTime
        );
    }

    // Mantém os pés no chão quando o caminho tem relevo
    void AcompanharChao()
    {
        Vector3 origem = transform.position + Vector3.up * 1.2f;
        float melhor = float.NegativeInfinity;

        foreach (RaycastHit ponto in Physics.RaycastAll(origem, Vector3.down, 4f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (ponto.collider == colisor || ponto.collider is CharacterController)
            {
                continue;
            }

            melhor = Mathf.Max(melhor, ponto.point.y);
        }

        if (melhor > float.NegativeInfinity)
        {
            Vector3 posicao = transform.position;
            posicao.y = melhor;
            transform.position = posicao;
        }
    }

    public void Morrer()
    {
        if (morto)
        {
            return;
        }

        morto = true;

        // Se o mob morrer antes do golpe acertar, cancela o dano que estava agendado
        CancelInvoke(nameof(CausarDano));

        colisor.enabled = false;
        animator.SetFloat("Velocidade", 0f);
        animator.SetTrigger("Morrer");

        // Espera a animação de morte terminar antes de remover o mob
        float duracao = 2f;
        foreach (AnimationClip clipe in animator.runtimeAnimatorController.animationClips)
        {
            if (clipe.name.Contains("Morrer"))
            {
                duracao = clipe.length + 0.2f;
            }
        }

        Destroy(gameObject, duracao);
    }
}