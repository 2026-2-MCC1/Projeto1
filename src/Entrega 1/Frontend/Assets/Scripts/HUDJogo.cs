using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// HUD da fase: vidas (cascos), tempo da partida (conta a partir de 00:00) e Rocklets coletados.
// Só LÊ o GameManager (Vidas e Pontuacao); quem muda os valores continua sendo ele.
public class HUDJogo : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private GameManager gameManager;

    [Header("Vidas")]
    [SerializeField] private Transform containerCascos;
    [SerializeField] private UnityEngine.UI.Image modeloCasco;   // casco usado como molde
    [SerializeField] private Sprite cascoCheio;
    [SerializeField] private Sprite cascoVazio;
    [SerializeField] private int vidasSemGameManager = 3;        // só para testar a cena sem GameManager

    [Header("Tempo")]
    [SerializeField] private TMP_Text textoTempo;
    [SerializeField] private bool contarTempo = true;           // conta quanto tempo o jogador leva até o final

    [Header("Rocklets")]
    [SerializeField] private TMP_Text textoQuantidade;            // "x 18"
    [SerializeField] private TMP_Text textoMeta;                  // "18 / 25"
    [SerializeField] private UnityEngine.UI.Image barraDoces;     // Image tipo Filled
    [SerializeField] private RectTransform iconeDoces;            // pula quando pega um Rocklet
    [SerializeField] private int metaDoces = 25;                  // igual ao minimoProximaFase do GameManager

    private readonly List<UnityEngine.UI.Image> cascos = new List<UnityEngine.UI.Image>();
    private int ultimasVidas = -1;
    private int ultimosPontos = -1;
    private float tempoDecorrido;

    // tempo que o jogador levou (em segundos), para mostrar no final da fase
    public float TempoDecorrido => tempoDecorrido;

    void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        tempoDecorrido = 0f;
        MontarCascos(gameManager != null ? gameManager.Vidas : vidasSemGameManager);
        AtualizarTudo(true);
    }

    void Update()
    {
        if (contarTempo)
        {
            tempoDecorrido += Time.deltaTime;
        }

        AtualizarTudo(false);
    }

    // ------------------------------------------------------------ vidas
    private void MontarCascos(int quantidade)
    {
        if (containerCascos == null || modeloCasco == null)
        {
            return;
        }

        // apaga os cascos de exemplo e deixa só o molde
        for (int i = containerCascos.childCount - 1; i >= 0; i--)
        {
            Transform filho = containerCascos.GetChild(i);
            if (filho != modeloCasco.transform)
            {
                Destroy(filho.gameObject);
            }
        }

        cascos.Clear();
        cascos.Add(modeloCasco);
        for (int i = 1; i < quantidade; i++)
        {
            cascos.Add(Instantiate(modeloCasco, containerCascos));
        }
    }

    private void AtualizarVidas(int vidas, bool inicio)
    {
        for (int i = 0; i < cascos.Count; i++)
        {
            Sprite alvo = i < vidas ? cascoCheio : cascoVazio;
            if (cascos[i].sprite != alvo)
            {
                cascos[i].sprite = alvo;
                if (!inicio)
                {
                    StartCoroutine(Pulo(cascos[i].rectTransform, 1.35f));
                }
            }
        }
    }

    // ------------------------------------------------------------ tempo
    private void AtualizarTempo()
    {
        if (textoTempo == null)
        {
            return;
        }

        int total = Mathf.FloorToInt(tempoDecorrido);
        textoTempo.text = (total / 60).ToString("00") + ":" + (total % 60).ToString("00");
    }

    // ------------------------------------------------------------ Rocklets
    private void AtualizarDoces(int pontos, bool inicio)
    {
        if (textoQuantidade != null)
        {
            textoQuantidade.text = "x " + pontos;
        }
        if (textoMeta != null)
        {
            textoMeta.text = pontos + " / " + metaDoces;
        }
        if (barraDoces != null)
        {
            barraDoces.fillAmount = metaDoces > 0 ? Mathf.Clamp01((float)pontos / metaDoces) : 0f;
        }
        if (!inicio && iconeDoces != null)
        {
            StartCoroutine(Pulo(iconeDoces, 1.2f));
        }
    }

    private void AtualizarTudo(bool inicio)
    {
        int vidas = gameManager != null ? gameManager.Vidas : vidasSemGameManager;
        int pontos = gameManager != null ? gameManager.Pontuacao : 0;

        if (vidas != ultimasVidas)
        {
            AtualizarVidas(vidas, inicio);
            ultimasVidas = vidas;
        }
        if (pontos != ultimosPontos)
        {
            AtualizarDoces(pontos, inicio);
            ultimosPontos = pontos;
        }
        AtualizarTempo();
    }

    // Chamar quando o jogador atravessar o portal (para o relógio parar)
    public void PararTempo()
    {
        contarTempo = false;
    }

    public void ZerarTempo()
    {
        tempoDecorrido = 0f;
        contarTempo = true;
    }

    // ------------------------------------------------------------ animação
    private IEnumerator Pulo(RectTransform alvo, float escalaMaxima)
    {
        const float duracao = 0.25f;
        float t = 0f;
        while (t < duracao)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Sin(Mathf.Clamp01(t / duracao) * Mathf.PI);
            alvo.localScale = Vector3.one * Mathf.Lerp(1f, escalaMaxima, k);
            yield return null;
        }
        alvo.localScale = Vector3.one;
    }
}
