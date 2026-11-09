using System.Collections;
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

    // Mensagem (dica) que aparece quando o baú está vazio
    public string mensagemVazio = "Baú vazio!";

    // Referências para atualizar as mensagens e o objetivo na tela
    public MensagemUI mensagemUI;
    public ObjetivoUI objetivoUI;

    [Header("Visual do baú")]
    // Tampa do baú (o pivô fica na dobradiça, atrás)
    public Transform tampa;

    // Quanto a tampa gira ao abrir e quanto tempo leva
    public float anguloAberto = 105f;
    public float tempoAbrir = 0.45f;

    // Cartão que sobe de dentro do baú (fica desligado até abrir)
    public GameObject cartaoVisual;

    // Guarda se o baú já foi aberto
    private bool aberto = false;

    // Guarda se o jogador estava perto no frame anterior
    private bool estavaPerto = false;

    void Start()
    {
        // O baú é um prefab: se algo não foi ligado no Inspector, procura na cena
        if (jogador == null)
        {
            PlayerMovement movimento = FindFirstObjectByType<PlayerMovement>();
            if (movimento != null)
            {
                jogador = movimento.transform;
            }
        }
        if (mensagemUI == null)
        {
            mensagemUI = FindFirstObjectByType<MensagemUI>();
        }
        if (objetivoUI == null)
        {
            objetivoUI = FindFirstObjectByType<ObjetivoUI>();
        }
        if (cartaoVisual != null)
        {
            cartaoVisual.SetActive(false);
        }
    }

    void Update()
    {
        // Depois de aberto o baú fica parado aberto e não mostra mais mensagens
        if (aberto == true || jogador == null)
        {
            return;
        }

        // Calcula a distância entre o jogador e o baú
        float distancia = Vector3.Distance(
            transform.position,
            jogador.position
        );

        // Verifica se o jogador está perto
        bool estaPerto = distancia <= distanciaInteracao;

        if (estaPerto == true)
        {
            // Mostra a mensagem de interação
            mensagemUI.MostrarMensagem("Pressione E para abrir");

            // Se o jogador apertar E, abre o baú
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                AbrirBau();
                return;
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

    public void AbrirBau()
    {
        if (aberto == true)
        {
            return;
        }

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
            mensagemUI.MostrarMensagem(mensagemVazio);
        }

        if (tampa != null)
        {
            // Baú com modelo: a tampa abre e ele continua na fase, aberto
            StartCoroutine(AnimarAbertura());
        }
        else
        {
            // Baú antigo sem tampa: só some
            gameObject.SetActive(false);
        }
    }

    IEnumerator AnimarAbertura()
    {
        Quaternion fechada = tampa.localRotation;

        // Tremidinha antes de abrir, estilo Crash
        Vector3 posicaoBase = transform.position;
        float tempo = 0f;
        while (tempo < 0.15f)
        {
            tempo += Time.deltaTime;
            transform.position = posicaoBase + new Vector3(Mathf.Sin(tempo * 90f) * 0.03f, 0f, 0f);
            yield return null;
        }
        transform.position = posicaoBase;

        if (contemCartao == true && cartaoVisual != null)
        {
            StartCoroutine(AnimarCartao());
        }

        // A tampa abre passando um pouco do ângulo e volta (efeito de mola)
        tempo = 0f;
        while (tempo < tempoAbrir)
        {
            tempo += Time.deltaTime;
            float t = Mathf.Clamp01(tempo / tempoAbrir);
            float mola = 1f + Mathf.Sin(t * Mathf.PI) * 0.12f * t;
            tampa.localRotation = fechada * Quaternion.Euler(anguloAberto * EaseOut(t) * mola, 0f, 0f);
            yield return null;
        }
        tampa.localRotation = fechada * Quaternion.Euler(anguloAberto, 0f, 0f);
    }

    // O cartão sobe girando de dentro do baú, brilha e some
    IEnumerator AnimarCartao()
    {
        Transform cartao = cartaoVisual.transform;
        Vector3 inicio = cartao.localPosition;
        Vector3 escala = cartao.localScale;
        cartaoVisual.SetActive(true);

        GameObject objetoLuz = new GameObject("Luz_Cartao");
        objetoLuz.transform.SetParent(cartao, false);
        Light luz = objetoLuz.AddComponent<Light>();
        luz.type = LightType.Point;
        luz.color = new Color(1f, 0.85f, 0.4f);
        luz.range = 3f;

        float tempo = 0f;
        while (tempo < 1.6f)
        {
            tempo += Time.deltaTime;
            float subida = EaseOut(Mathf.Clamp01(tempo / 0.6f));
            cartao.localPosition = inicio + Vector3.up * (0.9f * subida + Mathf.Sin(tempo * 4f) * 0.04f);
            cartao.Rotate(0f, 360f * Time.deltaTime, 0f, Space.Self);
            luz.intensity = 3f * subida;

            // Encolhe no final, como se fosse para o bolso da tartaruga
            float sumir = Mathf.Clamp01((tempo - 1.2f) / 0.4f);
            cartao.localScale = escala * (1f - sumir);
            yield return null;
        }

        cartaoVisual.SetActive(false);
        Destroy(objetoLuz);
    }

    float EaseOut(float t)
    {
        return 1f - (1f - t) * (1f - t) * (1f - t);
    }
}
