using System.Collections;
using UnityEngine;

// Animação da árvore: brota, treme, tomba, quica e desce rolando até se quebrar.
// O tronco fica no eixo Y do modelo, com o pivô no meio dele.
[RequireComponent(typeof(CaixaDestrutivel))]
public class ArvoreRolante : MonoBehaviour
{
    public float comprimento = 4.6f;
    public float raio = 0.5f;

    public float tempoBrotar = 0.3f;
    public float tempoTremer = 0.35f;
    public float tempoTombar = 0.75f;

    // Rapidez com que ganha velocidade ao começar a rolar
    public float aceleracao = 5f;

    // Pulos e desvios aleatórios durante a descida
    public float gravidade = 13f;
    public float intervaloPulos = 1.6f;
    public float forcaPulo = 3f;
    public float forcaDesvio = 0.6f;

    private Vector3 direcao = Vector3.back;
    private float percurso = 36f;
    private float velocidadeMaxima = 6f;
    private Vector3 linhaCentral;
    private float larguraMaxima = 2.5f;

    private Collider colisor;
    private CharacterController jogador;

    // Chamado por quem cria a árvore, antes do primeiro quadro dela
    public void Iniciar(Vector3 direcaoRolar, float distancia, float velocidade, Vector3 centroCaminho, float largura)
    {
        direcao = direcaoRolar.normalized;
        percurso = distancia;
        velocidadeMaxima = velocidade;
        linhaCentral = centroCaminho;
        larguraMaxima = largura;
    }

    void Start()
    {
        colisor = GetComponent<Collider>();

        PlayerMovement movimento = FindFirstObjectByType<PlayerMovement>();
        if (movimento != null)
        {
            jogador = movimento.GetComponent<CharacterController>();
        }

        StartCoroutine(Animar());
    }

    IEnumerator Animar()
    {
        Vector3 eixoRolar = Vector3.Cross(Vector3.up, direcao).normalized;
        float lado = Random.value < 0.5f ? 1f : -1f;
        Vector3 eixoDeitado = eixoRolar * lado;
        Vector3 centroFinal = transform.position;

        // A base fica de um lado e a copa cai para o outro, terminando no centro sorteado
        Vector3 pe = centroFinal - eixoDeitado * comprimento * 0.5f;
        pe.y = AlturaChao(pe);

        // Brota do chão, passando um pouco do tamanho final
        Vector3 escalaFinal = transform.localScale;
        float tempo = 0f;
        while (tempo < tempoBrotar)
        {
            tempo += Time.deltaTime;
            float t = Mathf.Clamp01(tempo / tempoBrotar);
            float tamanho = Mathf.Sin(t * Mathf.PI * 0.75f) / Mathf.Sin(Mathf.PI * 0.75f);
            transform.localScale = escalaFinal * tamanho;
            transform.position = pe + Vector3.up * comprimento * 0.5f * tamanho;
            yield return null;
        }
        transform.localScale = escalaFinal;

        // Treme antes de cair
        tempo = 0f;
        while (tempo < tempoTremer)
        {
            if (!colisor.enabled) yield break;
            tempo += Time.deltaTime;
            float angulo = Mathf.Sin(tempo * 55f) * 3.5f * (tempo / tempoTremer);
            transform.rotation = Quaternion.AngleAxis(angulo, direcao);
            transform.position = pe + transform.up * comprimento * 0.5f;
            yield return null;
        }

        // Tomba acelerando, como uma árvore cortada
        Quaternion deitada = Quaternion.FromToRotation(Vector3.up, eixoDeitado);
        tempo = 0f;
        while (tempo < tempoTombar)
        {
            if (!colisor.enabled) yield break;
            tempo += Time.deltaTime;
            float t = Mathf.Clamp01(tempo / tempoTombar);
            float curva = t * t * t;
            transform.rotation = Quaternion.Slerp(Quaternion.identity, deitada, curva);
            transform.position = pe + transform.up * comprimento * 0.5f + Vector3.up * raio * curva;
            yield return null;
        }

        // Quica ao bater no chão e começa a rolar
        float alturaPulo = 0f;
        float subida = 2.4f;
        float velocidade = 0f;
        float rodado = 0f;
        float percorrido = 0f;
        float proximoPulo = Random.Range(0.5f, intervaloPulos);
        float desvio = 0f;
        Vector3 centro = transform.position;
        Vector3 eixoGiro = Vector3.Cross(Vector3.up, direcao).normalized;

        while (percorrido < percurso)
        {
            if (!colisor.enabled) yield break;

            velocidade = Mathf.MoveTowards(velocidade, velocidadeMaxima, aceleracao * Time.deltaTime);
            float passo = velocidade * Time.deltaTime;
            percorrido += passo;

            // Muda de rumo aos poucos, sem encostar nas paredes
            desvio = Mathf.MoveTowards(desvio, Mathf.Sin(percorrido * 0.4f + lado * 2f) * forcaDesvio, Time.deltaTime);
            centro += direcao * passo + eixoRolar * desvio * Time.deltaTime;
            float lateral = Vector3.Dot(centro - linhaCentral, eixoRolar);
            centro -= eixoRolar * (lateral - Mathf.Clamp(lateral, -larguraMaxima, larguraMaxima));

            // De vez em quando dá um pulo, como se batesse em uma pedra
            proximoPulo -= Time.deltaTime;
            if (proximoPulo <= 0f && alturaPulo <= 0f)
            {
                subida = forcaPulo * Random.Range(0.6f, 1.2f);
                proximoPulo = Random.Range(intervaloPulos * 0.6f, intervaloPulos * 1.6f);
            }
            subida -= gravidade * Time.deltaTime;
            alturaPulo += subida * Time.deltaTime;
            if (alturaPulo <= 0f)
            {
                alturaPulo = 0f;
                subida = 0f;
            }

            centro.y = AlturaChao(centro) + raio + alturaPulo;
            transform.position = centro;

            // O giro acompanha a distância percorrida, para não parecer que escorrega
            rodado += passo / raio * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(rodado, eixoGiro) * deitada;

            EmpurrarJogador(centro, eixoRolar, passo);
            yield return null;
        }

        GetComponent<CaixaDestrutivel>().Destruir();
    }

    // Altura do chão embaixo de um ponto, ignorando a própria árvore e o jogador
    float AlturaChao(Vector3 ponto)
    {
        float melhor = ponto.y;
        bool achou = false;
        Vector3 origem = new Vector3(ponto.x, ponto.y + 4f, ponto.z);

        foreach (RaycastHit h in Physics.RaycastAll(origem, Vector3.down, 12f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (h.collider is CharacterController || h.collider.GetComponent<ArvoreRolante>() != null)
            {
                continue;
            }

            if (!achou || h.point.y > melhor)
            {
                melhor = h.point.y;
                achou = true;
            }
        }

        return melhor;
    }

    // Leva o jogador junto rampa abaixo; quem pula por cima passa sem ser empurrado
    void EmpurrarJogador(Vector3 centro, Vector3 eixoRolar, float passo)
    {
        if (jogador == null)
        {
            return;
        }

        Bounds corpo = jogador.bounds;
        Vector3 diferenca = corpo.center - centro;
        float naFrente = Vector3.Dot(diferenca, direcao);
        float noLado = Mathf.Abs(Vector3.Dot(diferenca, eixoRolar));
        float limite = raio + jogador.radius;

        bool mesmaAltura = corpo.min.y < centro.y + raio && corpo.max.y > centro.y - raio;
        if (mesmaAltura && naFrente > -0.1f && naFrente < limite && noLado < comprimento * 0.5f + jogador.radius)
        {
            jogador.Move(direcao * passo);
        }
    }
}
