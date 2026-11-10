using UnityEngine;
using System.Collections;

public class InventarioJogador : MonoBehaviour
{
    // Indica se o jogador já encontrou o cartão de acesso
    public bool temCartao = false;

    public RectTransform iconeRocklets;

    // Som quando o jogador pega um Rocklet
    public AudioClip somRocklet;
    public float volumeRocklet = 0.6f;

    public void ColetarVoando(Transform item, float atraso)
    {
        StartCoroutine(VoarParaHud(item, atraso));
    }

    IEnumerator VoarParaHud(Transform item, float atraso)
    {
        yield return new WaitForSeconds(atraso);
        Camera cam = Camera.main;
        if (item == null) yield break;
        // o Animator do Rocklet (giro/flutuar) não pode brigar com o voo até o HUD
        foreach (Animator an in item.GetComponentsInChildren<Animator>()) an.enabled = false;
        Vector3 inicio = item.position;
        Vector3 escala = item.localScale;

        float t = 0f;
        while (t < 0.25f)
        {
            t += Time.deltaTime;
            item.position = inicio + Vector3.up * Mathf.Sin(t / 0.25f * Mathf.PI * 0.5f) * 1.2f;
            yield return null;
        }

        Vector3 de = item.position;
        t = 0f;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / 0.5f);
            // Sem ícone ligado no Inspector, o Rocklet voa para o próprio jogador
            Vector3 alvo = iconeRocklets != null
                ? cam.ScreenToWorldPoint(new Vector3(iconeRocklets.position.x, iconeRocklets.position.y, 2f))
                : transform.position + Vector3.up * 0.5f;
            item.position = Vector3.Lerp(de, alvo, k * k);
            item.localScale = escala * (1f - k * 0.7f);
            yield return null;
        }

        rocklets++;
        TocarSomRocklet();
        Destroy(item.gameObject);
    }

    private int rocklets = 0;
    public int Rocklets { get { return rocklets; } }

    // Vidas (cascos) do jogador
    public int vidasIniciais = 3;
    private int vidas;
    public int Vidas { get { return vidas; } }
    public bool Invencivel { get { return invencivel > 0f; } }

    public float tempoInvencivel = 1.2f;
    public float forcaEmpurrao = 5.5f;
    public float puloEmpurrao = 2.5f;
    public Color corDano = new Color(1f, 0.22f, 0.18f);
    private float invencivel = 0f;

    private PlayerMovement movimento;
    private TartarugaAnimador animadorTartaruga;
    private Renderer[] partesTartaruga;
    private MaterialPropertyBlock bloco;
    private CameraCrash cameraJogo;

    // Ao concluir a fase o jogador congela: sem mover, sem dano, sem Game Over
    public static bool Congelado;

    public static void CongelarJogador()
    {
        InventarioJogador inv = FindFirstObjectByType<InventarioJogador>();
        if (inv != null) inv.Congelar();
    }

    public void Congelar()
    {
        Congelado = true;
        CancelInvoke(nameof(IrParaGameOver));
        invencivel = 99999f;
        if (movimento != null) movimento.enabled = false;
        ArcorFactory.AtaqueJogador ataque = GetComponent<ArcorFactory.AtaqueJogador>();
        if (ataque != null) ataque.enabled = false;
    }

    void Awake()
    {
        Congelado = false;
        vidas = vidasIniciais;
        movimento = GetComponent<PlayerMovement>();
        animadorTartaruga = GetComponentInChildren<TartarugaAnimador>();
        if (animadorTartaruga != null)
        {
            partesTartaruga = animadorTartaruga.GetComponentsInChildren<Renderer>();
        }
        bloco = new MaterialPropertyBlock();

        // Se o som não foi colocado no Inspector, pega o "SomRocklet" da pasta Resources
        if (somRocklet == null)
        {
            somRocklet = Resources.Load<AudioClip>("SomRocklet");
        }
        if (somRocklet == null)
        {
            somRocklet = ArcorFactory.Sons.Clip("rocklet");
        }
    }

    // Toca o som do Rocklet usando o RespawnJogador (que também fica no Player)
    private float ultimoSomRocklet = -10f;
    private int sequenciaRocklet = 0;

    void TocarSomRocklet()
    {
        if (somRocklet == null)
        {
            somRocklet = ArcorFactory.Sons.Clip("rocklet");
        }
        if (somRocklet == null)
        {
            return;
        }
        sequenciaRocklet = Time.time - ultimoSomRocklet < 0.6f ? Mathf.Min(sequenciaRocklet + 1, 8) : 0;
        ultimoSomRocklet = Time.time;
        AudioSource fonte = ArcorFactory.Sons.TocarClip(somRocklet, transform.position, false, volumeRocklet, 0.02f, 0f);
        if (fonte != null)
        {
            fonte.pitch = 1f + sequenciaRocklet * 0.04f;
        }
    }

    void Start()
    {
        cameraJogo = FindFirstObjectByType<CameraCrash>();
    }

    void Update()
    {
        if (invencivel <= 0f)
        {
            return;
        }

        invencivel -= Time.deltaTime;
        float passou = tempoInvencivel - invencivel;

        // clarão branco no impacto, depois pisca vermelho até acabar a invencibilidade
        Color cor = corDano;
        float forca = 0f;
        if (passou < 0.06f)
        {
            cor = Color.white;
            forca = 0.9f;
        }
        else if (passou < 0.2f)
        {
            forca = 0.75f;
        }
        else if (Mathf.Repeat(passou, 0.16f) < 0.08f)
        {
            forca = 0.5f;
        }

        if (invencivel <= 0f)
        {
            forca = 0f;
        }

        Pintar(cor, forca);
    }

    void Pintar(Color cor, float forca)
    {
        if (partesTartaruga == null)
        {
            return;
        }

        cor.a = forca;
        foreach (Renderer parte in partesTartaruga)
        {
            if (forca <= 0f)
            {
                parte.SetPropertyBlock(null);
                continue;
            }

            parte.GetPropertyBlock(bloco);
            bloco.SetColor("_CorPiscar", cor);
            parte.SetPropertyBlock(bloco);
        }
    }

    public bool GanharVida()
    {
        if (vidas <= 0 || vidas >= vidasIniciais)
        {
            return false;
        }
        vidas++;
        ArcorFactory.Sons.Tocar("vida", 0.8f, 0f);
        ArcorFactory.Efeitos.Brilhos(transform.position + Vector3.up, new Color(0.4f, 1f, 0.5f), 20);
        return true;
    }

    // Soma Rocklets direto (quando não dá para criar o Rocklet voando)
    public void SomarRocklets(int quantidade)
    {
        rocklets += Mathf.Max(0, quantidade);
    }

    public void PerderVida()
    {
        PerderVida(transform.position + transform.forward);
    }

    // origem = de onde veio o golpe, para empurrar a tartaruga para o lado contrário
    public void PerderVida(Vector3 origem)
    {
        if (Congelado || vidas <= 0 || invencivel > 0f)
        {
            return;
        }

        vidas--;
        ArcorFactory.EstatisticasFase.DanoRecebido(origem);
        ArcorFactory.Sons.Tocar("dano", 0.7f, 0.04f);
        invencivel = tempoInvencivel;

        // Acabaram as vidas: mostra o aviso e abre a tela de Game Over
        if (vidas <= 0)
        {
            FimDeJogo();
        }

        if (animadorTartaruga != null)
        {
            if (vidas <= 0)
            {
                animadorTartaruga.Morrer();
            }
            else
            {
                animadorTartaruga.LevarDano();
            }
        }

        if (cameraJogo != null)
        {
            cameraJogo.Tremer(0.18f, 0.25f);
        }

        if (movimento != null)
        {
            Vector3 direcao = transform.position - origem;
            direcao.y = 0f;
            if (direcao.sqrMagnitude < 0.01f)
            {
                direcao = -transform.forward;
            }
            // No golpe final ela só é jogada para trás, sem pulinho, e cai ali mesmo
            if (vidas <= 0)
            {
                movimento.Empurrar(direcao.normalized * forcaEmpurrao * 0.6f, 0f);
            }
            else
            {
                movimento.Empurrar(direcao.normalized * forcaEmpurrao, puloEmpurrao);
            }
        }
    }

    private bool fimDeJogo = false;

    void FimDeJogo()
    {
        if (fimDeJogo)
        {
            return;
        }
        fimDeJogo = true;

        MensagemUI mensagem = FindFirstObjectByType<MensagemUI>();
        if (mensagem != null)
        {
            mensagem.MostrarMensagem("Suas vidas acabaram!");
        }

        // O jogador perde o controle (o empurrão do último golpe ainda acontece)
        if (movimento != null)
        {
            movimento.tempoAtordoado = 99f;
        }

        // Espera a animação de morte (queda + piscar) antes do Game Over
        float espera = animadorTartaruga != null ? animadorTartaruga.DuracaoMorte + 0.4f : 1.5f;
        Invoke(nameof(IrParaGameOver), espera);
    }

    void IrParaGameOver()
    {
        if (Congelado) return;
        // Tela de Game Over: "Tentar de novo" recarrega a fase do começo
        FactoryEscape.GameFlow.GameOver();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Rocklet"))
        {
            ArcorFactory.Efeitos.Brilhos(other.transform.position, new Color(1f, 0.9f, 0.5f), 6, 0.2f, 1.5f);
            rocklets++;
            TocarSomRocklet();
            Destroy(other.gameObject);
        }
    }
}
