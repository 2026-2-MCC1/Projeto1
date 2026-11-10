using UnityEngine;

// Câmera no estilo Crash
public class CameraCrash : MonoBehaviour
{
    public static CameraCrash Instancia { get; private set; }

    // Objeto que a câmera deve seguir, neste caso o jogador
    public Transform alvo;

    // Posição da câmera em relação ao jogador
    public float distanciaAtras = 8f;
    public float alturaCamera = 5.4f;

    // Para onde a câmera olha: acima dos pés e um pouco à frente do jogador
    public float alturaOlhar = 1.1f;
    public float olharAFrente = 3f;

    // Tempo para alcançar o jogador; na altura é maior, para não pular junto
    public float suavizacao = 0.12f;
    public float suavizacaoAltura = 0.3f;

    // Direção do caminho, em graus
    public float direcaoCaminho = 0f;
    public float velocidadeGiro = 90f;

    // Acima desta distância a câmera vai direto, como depois de uma queda
    public float distanciaSalto = 15f;

    private Vector3 pontoSeguido;
    private Vector3 velocidadeLateral;
    private float velocidadeAltura;
    private float direcaoAtual;
    private float velocidadeDirecao;

    // Tremida da câmera (explosão, dano)
    private float forcaTremor = 0f;
    private float duracaoTremor = 0f;
    private float tempoTremor = 0f;
    private float sementeTremor;

    // Pose cinematográfica (cutscenes)
    private Vector3 posCinema;
    private Quaternion rotCinema = Quaternion.identity;
    private float pesoCinema = 0f;
    private float fovCinema = -1f;
    private float fovBase = 60f;
    private Camera cam;

    // Anti-obstrução
    public LayerMask camadasObstaculo = ~0;
    public float raioColisao = 0.25f;
    public float distanciaMinima = 1.6f;
    private float fatorDistancia = 1f;
    private float velFator;

    // Pose de jogo do último quadro (sem tremor e sem cinema)
    public Vector3 PosicaoDeJogo { get; private set; }
    public Quaternion RotacaoDeJogo { get; private set; }
    public float PesoCinema { get { return pesoCinema; } }

    void Awake()
    {
        Instancia = this;
        cam = GetComponent<Camera>();
        if (cam != null) fovBase = cam.fieldOfView;
        sementeTremor = Random.value * 100f;
    }

    void OnDestroy()
    {
        if (Instancia == this) Instancia = null;
    }

    // Uma tremida mais fraca não substitui uma mais forte que ainda está acontecendo
    public void Tremer(float forca, float duracao)
    {
        float forcaAtual = tempoTremor > 0f ? forcaTremor * (tempoTremor / duracaoTremor) : 0f;
        if (forca >= forcaAtual)
        {
            forcaTremor = forca;
            duracaoTremor = duracao;
            tempoTremor = duracao;
        }
    }

    // Cutscene: pose desejada e quanto ela vale
    public void DefinirCinema(Vector3 posicao, Quaternion rotacao, float peso, float fov = -1f)
    {
        posCinema = posicao;
        rotCinema = rotacao;
        pesoCinema = Mathf.Clamp01(peso);
        fovCinema = fov;
    }

    public void LiberarCinema()
    {
        pesoCinema = 0f;
        fovCinema = -1f;
    }

    // Recomeça a perseguição exatamente em cima do alvo
    public void Encaixar()
    {
        if (alvo == null) return;
        pontoSeguido = alvo.position;
        velocidadeLateral = Vector3.zero;
        velocidadeAltura = 0f;
        direcaoAtual = direcaoCaminho;
    }

    void Start()
    {
        // Trava o cursor no centro da tela e deixa ele invisível
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        direcaoAtual = direcaoCaminho;
        if (alvo != null)
        {
            pontoSeguido = alvo.position;
        }
        CalcularPoseDeJogo();
    }

    void LateUpdate()
    {
        // Impede erros caso o alvo não esteja disponível
        if (alvo == null)
        {
            return;
        }

        if (Vector3.Distance(pontoSeguido, alvo.position) > distanciaSalto && pesoCinema <= 0f)
        {
            Encaixar();
        }

        // Segue no chão e na altura em ritmos diferentes
        Vector3 noChao = new Vector3(pontoSeguido.x, 0f, pontoSeguido.z);
        Vector3 alvoNoChao = new Vector3(alvo.position.x, 0f, alvo.position.z);
        noChao = Vector3.SmoothDamp(noChao, alvoNoChao, ref velocidadeLateral, suavizacao);
        float altura = Mathf.SmoothDamp(pontoSeguido.y, alvo.position.y, ref velocidadeAltura, suavizacaoAltura);
        pontoSeguido = new Vector3(noChao.x, altura, noChao.z);

        // O rumo acelera e freia suavemente (curvas da fase não dão tranco)
        float tempoGiro = Mathf.Max(0.05f, Mathf.Abs(Mathf.DeltaAngle(direcaoAtual, direcaoCaminho)) / Mathf.Max(1f, velocidadeGiro));
        direcaoAtual = Mathf.SmoothDampAngle(direcaoAtual, direcaoCaminho, ref velocidadeDirecao, Mathf.Min(0.35f, tempoGiro), velocidadeGiro * 1.5f);

        CalcularPoseDeJogo();

        Vector3 pos = PosicaoDeJogo;
        Quaternion rot = RotacaoDeJogo;
        float fov = fovBase;
        if (pesoCinema > 0f)
        {
            float k = pesoCinema;
            pos = Vector3.Lerp(pos, posCinema, k);
            rot = Quaternion.Slerp(rot, rotCinema, k);
            if (fovCinema > 0f) fov = Mathf.Lerp(fovBase, fovCinema, k);
        }

        // Tremor suave: ruído contínuo (não pula de um valor aleatório para outro)
        if (tempoTremor > 0f)
        {
            tempoTremor -= Time.deltaTime;
            float k = Mathf.Clamp01(tempoTremor / duracaoTremor);
            float f = forcaTremor * k * k;
            float t = Time.time * 28f;
            Vector3 ruido = new Vector3(
                Mathf.PerlinNoise(sementeTremor, t) - 0.5f,
                Mathf.PerlinNoise(sementeTremor + 13.1f, t) - 0.5f,
                Mathf.PerlinNoise(sementeTremor + 27.7f, t) - 0.5f) * 2f;
            pos += rot * ruido * f;
            rot = rot * Quaternion.Euler(0f, 0f, (Mathf.PerlinNoise(sementeTremor + 41.3f, t) - 0.5f) * 6f * f);
        }

        transform.SetPositionAndRotation(pos, rot);
        if (cam != null && !Mathf.Approximately(cam.fieldOfView, fov)) cam.fieldOfView = fov;
    }

    // Obstáculo "sólido" para a câmera
    bool BloqueiaCamera(Collider c, bool ignorarElevadas = true)
    {
        if (c == null) return false;
        if (alvo != null && c.transform.IsChildOf(alvo)) return false;
        if (c.GetComponentInParent<MobPatrulha>() != null) return false;
        if (c.GetComponentInParent<ArcorFactory.MobBase>() != null) return false;
        if (c.GetComponentInParent<CaixaDestrutivel>() != null) return false;
        Vector3 tam = c.bounds.size;
        float a1 = Mathf.Min(tam.x, Mathf.Min(tam.y, tam.z));
        float a3 = Mathf.Max(tam.x, Mathf.Max(tam.y, tam.z));
        float a2 = tam.x + tam.y + tam.z - a1 - a3;
        if (a3 < 1.2f) return false; // itens e detalhes pequenos
        if (ignorarElevadas && c.bounds.min.y > (alvo != null ? alvo.position.y : 0f) + 2.0f) return false; // estruturas elevadas
        if (a2 < 0.7f) return false; // postes e vigas finas
        return true;
    }

    float subida = 0f; // quanto a câmera está mais alta que o normal
    float velTeto;
    float limiteAltura = -1f; // altura máxima permitida pelo teto
    float velLimite;

    // Há um teto cobrindo o jogador e a área ao redor? (diferente de uma passarela estreita)
    bool TetoFechado(Vector3 cabeca, Quaternion rumo, float alcance, out float distMin)
    {
        distMin = alcance;
        Vector3 frente = rumo * Vector3.forward * 3.5f;
        Vector3 lado = rumo * Vector3.right * 1.5f;
        Vector3[] deslocs = { Vector3.zero, frente, -frente, lado, -lado };
        int cobertos = 0;
        for (int i = 0; i < deslocs.Length; i++)
        {
            RaycastHit[] hs = Physics.SphereCastAll(cabeca + deslocs[i], 0.3f, Vector3.up, alcance, camadasObstaculo, QueryTriggerInteraction.Ignore);
            float d = alcance;
            bool achou = false;
            for (int j = 0; j < hs.Length; j++)
            {
                if (hs[j].distance <= 0f) continue;
                if (!BloqueiaCamera(hs[j].collider, false)) continue;
                d = Mathf.Min(d, hs[j].distance);
                achou = true;
            }
            if (achou) { cobertos++; distMin = Mathf.Min(distMin, d); }
        }
        return cobertos >= 4;
    }

    void CalcularPoseDeJogo()
    {
        Quaternion rumo = Quaternion.Euler(0f, direcaoAtual, 0f);
        float yBase = alvo != null ? alvo.position.y : pontoSeguido.y;

        // 1) Teto/passarela por cima do jogador
        float livreAcima = alturaCamera + 1.5f;
        Vector3 cabeca = pontoSeguido + Vector3.up * 1.4f;
        RaycastHit[] cima = Physics.SphereCastAll(cabeca, 0.3f, Vector3.up, livreAcima, camadasObstaculo, QueryTriggerInteraction.Ignore);
        float teto = livreAcima;
        for (int i = 0; i < cima.Length; i++)
        {
            if (cima[i].distance <= 0f) continue;
            if (!BloqueiaCamera(cima[i].collider, false)) continue;
            teto = Mathf.Min(teto, cima[i].distance);
        }
        // Teto fechado (corredor, sala): a câmera fica ABAIXO dele, nunca atravessa para fora do mapa
        float distTeto;
        bool tetoFechado = TetoFechado(cabeca, rumo, livreAcima, out distTeto);
        float limiteAlvo = alturaCamera + 3.5f;
        if (tetoFechado) limiteAlvo = Mathf.Max(1.8f, distTeto + 1.4f - 0.7f);
        limiteAltura = Mathf.SmoothDamp(limiteAltura < 0f ? limiteAlvo : limiteAltura, limiteAlvo, ref velLimite, 0.25f);

        // Embaixo de uma estrutura aberta (passarela): sobe um pouco
        float alvoSubida = (teto >= livreAcima || tetoFechado) ? 0f : 3.4f;
        subida = Mathf.SmoothDamp(subida, alvoSubida, ref velTeto, alvoSubida > subida ? 0.35f : 0.7f);
        float alturaUsada = Mathf.Min(alturaCamera + subida, limiteAltura);

        Vector3 deslocamento = rumo * new Vector3(0f, alturaUsada, -distanciaAtras);
        Vector3 origem = pontoSeguido + Vector3.up * alturaOlhar;
        Vector3 alvoPos = pontoSeguido + deslocamento;

        // 2) Algo entre o jogador e a câmera: aproxima
        float total = (alvoPos - origem).magnitude;
        float livre = 1f;
        if (total > 0.01f)
        {
            Vector3 dir = (alvoPos - origem) / total;
            RaycastHit[] hits = Physics.SphereCastAll(origem, raioColisao, dir, total, camadasObstaculo, QueryTriggerInteraction.Ignore);
            float menor = total;
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].distance <= 0f) continue;
                if (!BloqueiaCamera(hits[i].collider)) continue;
                menor = Mathf.Min(menor, hits[i].distance);
            }
            livre = Mathf.Clamp01(Mathf.Max(distanciaMinima, menor) / total);
        }
        float tempo = livre < fatorDistancia ? 0.1f : 0.4f;
        fatorDistancia = Mathf.SmoothDamp(fatorDistancia, livre, ref velFator, tempo);

        Vector3 pos = origem + (alvoPos - origem) * fatorDistancia;
        Vector3 olhar = pontoSeguido + Vector3.up * alturaOlhar + rumo * Vector3.forward * olharAFrente;
        PosicaoDeJogo = pos;
        RotacaoDeJogo = Quaternion.LookRotation(olhar - pos);
    }
}
