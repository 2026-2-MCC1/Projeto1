using UnityEngine;
using UnityEngine.InputSystem;

// Faz o mob usar as animações
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

    // Tecla do giro do jogador e distância em que ele alcança o mob
    public Key teclaGiro = Key.Q;
    public float distanciaGiro = 2f;

    private Animator animator;
    private CapsuleCollider colisor;
    private CharacterController jogador;
    private Vector3 centro;

    // Lado para onde está andando: 1 ou -1
    private float sentido = 1f;
    private float esperaPatrulha = 0f;
    private float esperaAtaque = 0f;
    private bool morto = false;

    // Ataque e perda de vida
    private InventarioJogador inventario;
    public float atrasoGolpe = 0.45f;
    public GameObject efeitoAtaque; // explosão do golpe
    private bool golpePendente = false;
    private float tempoGolpe = 0f;

    // Plutonita: tempo entre começar o ataque e explodir
    public float tempoAteExplodir = 1.6f;
    public float raioExplosao = 2.6f;
    public Color corCarga = new Color(0.55f, 1f, 0.2f);
    private Renderer[] partes;
    private MaterialPropertyBlock bloco;
    private float fasePisca = 0f;
    private CameraCrash cameraJogo;

    void Start()
    {
        animator = GetComponent<Animator>();
        colisor = GetComponent<CapsuleCollider>();
        centro = transform.position;
        cameraJogo = FindFirstObjectByType<CameraCrash>();

        // peças que piscam enquanto a Plutonita carrega
        if (efeitoAtaque != null)
        {
            bloco = new MaterialPropertyBlock();
            partes = GetComponentsInChildren<Renderer>();
        }

        // Começa em pontos diferentes do caminho, para os mobs não andarem iguais
        sentido = Random.value < 0.5f ? 1f : -1f;
        esperaPatrulha = Random.Range(0f, tempoParado);

        // O mob é um prefab, então procura o jogador quando entra na cena
        PlayerMovement movimento = FindFirstObjectByType<PlayerMovement>();
        if (movimento != null)
        {
            jogador = movimento.GetComponent<CharacterController>();
            inventario = movimento.GetComponent<InventarioJogador>();
        }
    }

    void Update()
    {
        if (morto)
        {
            return;
        }

        esperaAtaque -= Time.deltaTime;

        if (golpePendente && jogador != null)
        {
            tempoGolpe -= Time.deltaTime;

            if (efeitoAtaque != null && tempoGolpe > 0f)
            {
                Carregar(1f - tempoGolpe / tempoAteExplodir);
            }

            if (tempoGolpe <= 0f)
            {
                golpePendente = false;

                if (efeitoAtaque != null)
                {
                    Instantiate(efeitoAtaque, transform.position + Vector3.up * 0.8f, Quaternion.identity);
                    ArcorFactory.Sons.TocarEm("explosao_plutonita", transform.position, 1f, 0.04f, 40f);

                    // treme a câmera, mais forte quanto mais perto a tartaruga estiver
                    if (cameraJogo != null)
                    {
                        float perto = 1f - Mathf.Clamp01(Vector3.Distance(jogador.transform.position, transform.position) / 14f);
                        cameraJogo.Tremer(0.4f * perto, 0.45f);
                    }
                }

                Vector3 distancia = jogador.transform.position - transform.position;
                float altura = Mathf.Abs(distancia.y);
                distancia.y = 0f;

                // só tira vida se a tartaruga ainda estiver no alcance
                float alcanceGolpe = efeitoAtaque != null ? raioExplosao : distanciaAtaque;
                if (distancia.magnitude <= alcanceGolpe && altura <= 2.5f && inventario != null)
                {
                    inventario.PerderVida(transform.position);
                }

                // mob com explosão (Plutonita) explode junto com o ataque e some
                if (efeitoAtaque != null)
                {
                    morto = true;
                    Destroy(gameObject);
                    return;
                }
            }
        }

        if (jogador != null)
        {
            if (JogadorAcertou())
            {
                Morrer();
                return;
            }

            Vector3 paraJogador = jogador.transform.position - transform.position;
            float alturaJogador = Mathf.Abs(paraJogador.y);
            paraJogador.y = 0f;

            // carregando a explosão: fica parado olhando para o jogador
            if (golpePendente && efeitoAtaque != null)
            {
                animator.SetFloat("Velocidade", 0f);
                Virar(paraJogador);
                return;
            }

            if (paraJogador.magnitude <= distanciaAtaque && alturaJogador <= 2.5f)
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

        // Giro (chão ou ar): vale o giro central do AtaqueJogador, uma vez por giro
        if (distancia <= distanciaGiro && altura <= 1.6f && GiroAcertou())
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

    private int ultimoGiro = -1;

    bool GiroAcertou()
    {
        ArcorFactory.AtaqueJogador ataque = ArcorFactory.AtaqueJogador.Instancia;
        if (ataque == null)
        {
            return Keyboard.current != null && Keyboard.current[teclaGiro].wasPressedThisFrame && jogador.isGrounded;
        }
        if (!ataque.Girando || ataque.IdGiro == ultimoGiro)
        {
            return false;
        }
        ultimoGiro = ataque.IdGiro;
        return true;
    }

    void Atacar(Vector3 paraJogador)
    {
        animator.SetFloat("Velocidade", 0f);
        Virar(paraJogador);

        if (esperaAtaque <= 0f)
        {
            animator.SetTrigger("Atacar");
            esperaAtaque = intervaloAtaque;
            golpePendente = true;
            tempoGolpe = efeitoAtaque != null ? tempoAteExplodir : atrasoGolpe;
            fasePisca = 0f;
        }
    }

    // k vai de 0 a 1 até a explosão
    void Carregar(float k)
    {
        fasePisca += Time.deltaTime * Mathf.Lerp(2.5f, 12f, k * k);
        float pulso = Mathf.Sin(fasePisca * Mathf.PI * 2f) * 0.5f + 0.5f;
        Pintar(pulso * Mathf.Lerp(0.25f, 0.8f, k));
    }

    void Pintar(float quanto)
    {
        if (partes == null)
        {
            return;
        }

        Color cor = new Color(corCarga.r, corCarga.g, corCarga.b, quanto);
        foreach (Renderer parte in partes)
        {
            if (quanto <= 0f)
            {
                parte.SetPropertyBlock(null);
                continue;
            }

            parte.GetPropertyBlock(bloco);
            bloco.SetColor("_CorPiscar", cor);
            parte.SetPropertyBlock(bloco);
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
        colisor.enabled = false;
        ArcorFactory.EstatisticasFase.MobDerrotado();
        ArcorFactory.Efeitos.Estrelas(transform.position + Vector3.up);
        ArcorFactory.Sons.TocarEm("mob_golpe", transform.position, 0.9f);

        // se morreu carregando a explosão, desarma e para de piscar
        golpePendente = false;
        Pintar(0f);

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
