using UnityEngine;

// Solta árvores no topo da rampa em horários e posições aleatórias
public class GeradorArvores : MonoBehaviour
{
    public ArvoreRolante prefabArvore;

    // Tempo aleatório entre uma árvore e outra
    public float intervaloMinimo = 2.2f;
    public float intervaloMaximo = 5f;

    // Quanto a árvore pode nascer para cada lado do centro
    public float larguraNascer = 2f;
    public float larguraMaxima = 2.5f;

    // Distância que a árvore percorre até se quebrar
    public float percurso = 36f;

    // Velocidade aleatória de cada árvore
    public float velocidadeMinima = 5f;
    public float velocidadeMaxima = 8f;

    [Header("Carregamento por proximidade")]
    // Região de ativação (opcional)
    public Collider regiaoAtivacao;
    // Liga quando a tartaruga chega a esta distância do caminho das árvores
    public float distanciaAtivar = 30f;
    // Desliga quando ela se afasta mais do que isso
    public float distanciaDesligar = 55f;
    // Espera depois de ligar até a primeira árvore
    public float esperaPrimeira = 0.6f;

    // Vira true quando o jogador passa pelo trigger FimArvores (topo da rampa)
    private bool parado = false;
    private bool ativo = false;
    private float espera = 1f;
    private float proximaChecagem = 0f;
    private Transform jogador;

    public bool Ativo { get { return ativo; } }

    // Chamado pelo FimArvores: o jogador já passou, não precisa mais soltar árvores
    public void Parar()
    {
        parado = true;
        ativo = false;
        enabled = false;
    }

    void Update()
    {
        if (parado || prefabArvore == null)
        {
            return;
        }

        // checagem de proximidade (barata e espaçada)
        if (Time.time >= proximaChecagem)
        {
            proximaChecagem = Time.time + 0.25f;
            bool perto = JogadorNaRegiao(ativo ? distanciaDesligar : distanciaAtivar);
            if (perto && !ativo)
            {
                ativo = true;
                espera = esperaPrimeira;
            }
            else if (!perto && ativo)
            {
                ativo = false;
            }
        }

        if (!ativo)
        {
            return;
        }

        espera -= Time.deltaTime;
        if (espera > 0f)
        {
            return;
        }

        espera = Random.Range(intervaloMinimo, intervaloMaximo);
        SoltarArvore();
    }

    void SoltarArvore()
    {
        Vector3 lateral = Vector3.Cross(Vector3.up, transform.forward).normalized;
        float desvio = Random.Range(-larguraNascer, larguraNascer);

        ArvoreRolante arvore = Instantiate(prefabArvore, transform.position + lateral * desvio, Quaternion.identity);
        arvore.Iniciar(
            transform.forward,
            percurso,
            Random.Range(velocidadeMinima, velocidadeMaxima),
            transform.position,
            larguraMaxima
        );
    }

    bool JogadorNaRegiao(float distancia)
    {
        if (jogador == null)
        {
            PlayerMovement movimento = FindFirstObjectByType<PlayerMovement>();
            if (movimento == null)
            {
                return false;
            }
            jogador = movimento.transform;
        }

        Vector3 p = jogador.position;
        if (regiaoAtivacao != null)
        {
            return regiaoAtivacao.bounds.Contains(p);
        }

        // distância até o segmento do caminho
        Vector3 a = transform.position;
        Vector3 b = a + transform.forward * percurso;
        Vector3 ab = b - a;
        float t = Mathf.Clamp01(Vector3.Dot(p - a, ab) / Mathf.Max(0.0001f, ab.sqrMagnitude));
        return (p - (a + ab * t)).sqrMagnitude <= distancia * distancia;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 1f, 0.3f, 0.6f);
        Vector3 a = transform.position;
        Vector3 b = a + transform.forward * percurso;
        Gizmos.DrawLine(a, b);
        Gizmos.DrawWireSphere(a, distanciaAtivar);
        Gizmos.DrawWireSphere(b, distanciaAtivar);
    }
}
