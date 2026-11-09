using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CaixaDestrutivel : MonoBehaviour
{
    // Soltar rocklets ao quebrar uma caixa
    public GameObject[] rockletsParaSoltar;
    public int minimoRocklets = 2;
    public int maximoRocklets = 5;

    // Tecla do giro e distância em que ele alcança a caixa
    public Key teclaGiro = Key.Q;
    public float distanciaGiro = 2.0f;

    // Em quantas partes a caixa é cortada em cada eixo
    public int divisoes = 3;

    // Velocidade com que os pedaços saem para os lados e para cima
    public float forcaLateral = 3.5f;
    public float forcaCima = 4.5f;

    // Gravidade aplicada aos pedaços e tempo até eles sumirem
    public float gravidade = 14f;
    public float duracao = 1.1f;

    // Liga o clarão e a explosão em cadeia da Plutonita
    public bool explosiva = false;
    public float raioExplosao = 2f;
    public Material materialBrilho;

    // Guarda se a caixa já começou a ser destruída
    private bool destruida = false;

    private Renderer rendererCaixa;
    private Collider colisorCaixa;
    private CharacterController jogador;
    private InventarioJogador inventario;

    // Dados de cada pedaço enquanto ele voa
    private class Pedaco
    {
        public Transform transform;
        public Mesh malha;
        public Vector3 velocidade;
        public Vector3 giro;
        public Vector3 escala;
    }

    void Start()
    {
        rendererCaixa = GetComponent<Renderer>();
        colisorCaixa = GetComponent<Collider>();

        // A caixa é um prefab, então procura o jogador quando entra na cena
        PlayerMovement movimento = FindFirstObjectByType<PlayerMovement>();
        if (movimento != null)
        {
            inventario = movimento.GetComponent<InventarioJogador>();
            jogador = movimento.GetComponent<CharacterController>();
        }
    }

    void Update()
    {
        if (destruida || jogador == null)
        {
            return;
        }

        Bounds limites = colisorCaixa.bounds;
        Bounds corpo = jogador.bounds;

        // Distância no chão e diferença de altura entre o jogador e o centro da caixa
        Vector3 diferenca = corpo.center - limites.center;
        float altura = Mathf.Abs(diferenca.y);
        diferenca.y = 0f;
        float distancia = diferenca.magnitude;

        // Giro (chão ou ar): vale o giro central do AtaqueJogador, uma vez por giro
        if (distancia <= distanciaGiro && altura <= 1.6f && GiroAcertou())
        {
            Destruir();
            return;
        }

        // Pulo em cima: o jogador está caindo e os pés chegaram ao topo da caixa
        float pesAcimaDoTopo = corpo.min.y - limites.max.y;
        if (jogador.velocity.y < -0.5f
            && distancia <= 0.85f
            && pesAcimaDoTopo >= -0.1f
            && pesAcimaDoTopo <= 0.25f)
        {
            Destruir();
        }
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

    void SoltarRocklets()
    {
        // sem jogador achado no Start, procura de novo
        if (inventario == null)
        {
            inventario = FindFirstObjectByType<InventarioJogador>();
        }
        if (inventario == null) return;

        int quantidade = Random.Range(minimoRocklets, maximoRocklets + 1);
        bool temLista = rockletsParaSoltar != null && rockletsParaSoltar.Length > 0;
        ArcorFactory.ConfigArcor cfg = ArcorFactory.ConfigArcor.Instancia;
        for (int i = 0; i < quantidade; i++)
        {
            GameObject prefab = temLista ? rockletsParaSoltar[Random.Range(0, rockletsParaSoltar.Length)] : null;
            if (prefab == null && cfg != null) prefab = cfg.RockletAleatorio();
            if (prefab == null)
            {
                inventario.SomarRocklets(quantidade - i);
                return;
            }
            Vector3 desvio = new Vector3(Random.Range(-0.8f, 0.8f), 0.5f, Random.Range(-0.8f, 0.8f));
            GameObject r = Instantiate(prefab, transform.position + desvio, Quaternion.identity);
            foreach (Collider c in r.GetComponentsInChildren<Collider>()) c.enabled = false; // não conta pelo trigger
            inventario.ColetarVoando(r.transform, i * 0.08f);
        }
    }

    // ArvoreRolante só pode ser destruída por ela mesma (forcar = true)
    public void Destruir(bool forcar = false)
    {
        if (!forcar && GetComponent<ArvoreRolante>() != null)
        {
            return;
        }

        if (destruida)
        {
            return;
        }

        destruida = true;
        ArcorFactory.EstatisticasFase.CaixaQuebrada();
        ArcorFactory.Sons.TocarEm(explosiva ? "explosao_plutonita" : "caixa_quebra", transform.position, explosiva ? 1f : 0.85f, 0.08f, 32f);
        SoltarRocklets();

        // Libera a passagem na hora, antes mesmo dos pedaços aparecerem
        colisorCaixa.enabled = false;
        StartCoroutine(AnimarDestruicao());
    }

    IEnumerator AnimarDestruicao()
    {
        Vector3 posicaoBase = transform.position;
        Vector3 escalaInicial = transform.localScale;

        // A caixa de cima não pode ficar flutuando
        DerrubarCaixasDeCima();

        // Amassa a caixa antes de quebrar, para o golpe ter peso
        const float tempoAmassar = 0.07f;
        float tempo = 0f;
        while (tempo < tempoAmassar)
        {
            tempo += Time.deltaTime;
            float t = Mathf.Clamp01(tempo / tempoAmassar);
            transform.localScale = Vector3.Scale(
                escalaInicial,
                new Vector3(1f + 0.22f * t, 1f - 0.28f * t, 1f + 0.22f * t)
            );
            yield return null;
        }

        transform.localScale = escalaInicial;
        rendererCaixa.enabled = false;

        // Os pedaços caem até o que estiver embaixo da caixa
        float alturaChao = float.NegativeInfinity;
        if (Physics.Raycast(posicaoBase + Vector3.up * 0.05f, Vector3.down, out RaycastHit chao, 30f, ~0, QueryTriggerInteraction.Ignore))
        {
            alturaChao = chao.point.y;
        }

        List<Pedaco> pedacos = CriarPedacos();

        if (explosiva)
        {
            StartCoroutine(AnimarClarao());
            ExplodirVizinhas();
        }

        tempo = 0f;
        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float encolher = Mathf.Clamp01((duracao - tempo) / (duracao * 0.3f));

            foreach (Pedaco p in pedacos)
            {
                p.velocidade += Vector3.down * gravidade * Time.deltaTime;
                p.transform.position += p.velocidade * Time.deltaTime;
                p.transform.Rotate(p.giro * Time.deltaTime, Space.World);
                p.transform.localScale = p.escala * encolher;

                // Quica uma vez ou outra e vai perdendo força
                if (p.transform.position.y < alturaChao + 0.04f && p.velocidade.y < 0f)
                {
                    Vector3 posicao = p.transform.position;
                    posicao.y = alturaChao + 0.04f;
                    p.transform.position = posicao;
                    p.velocidade = new Vector3(p.velocidade.x * 0.55f, -p.velocidade.y * 0.35f, p.velocidade.z * 0.55f);
                    p.giro *= 0.5f;
                }
            }

            yield return null;
        }

        foreach (Pedaco p in pedacos)
        {
            Destroy(p.transform.gameObject);
            Destroy(p.malha);
        }

        Destroy(gameObject);
    }

    // Corta a malha da caixa em blocos, mantendo a textura em cada pedaço
    List<Pedaco> CriarPedacos()
    {
        Mesh original = GetComponent<MeshFilter>().sharedMesh;
        Vector3[] posicoes = original.vertices;
        Vector3[] normais = original.normals;
        Vector2[] uvs = original.uv;
        Color[] cores = original.colors;
        int[] triangulos = original.triangles;
        Bounds limites = original.bounds;
        float tamanhoMaximo = limites.size.magnitude / (divisoes * 1.7f);

        // Agrupa os triângulos pela célula da grade em que cada um cai
        Dictionary<int, List<Vertice>> celulas = new Dictionary<int, List<Vertice>>();
        for (int i = 0; i < triangulos.Length; i += 3)
        {
            Vertice a = LerVertice(posicoes, normais, uvs, cores, triangulos[i]);
            Vertice b = LerVertice(posicoes, normais, uvs, cores, triangulos[i + 1]);
            Vertice c = LerVertice(posicoes, normais, uvs, cores, triangulos[i + 2]);
            Separar(a, b, c, limites, tamanhoMaximo, celulas, 0);
        }

        List<Pedaco> pedacos = new List<Pedaco>();
        Vector3 centroCaixa = transform.TransformPoint(limites.center);

        foreach (List<Vertice> lista in celulas.Values)
        {
            pedacos.Add(CriarPedaco(lista, centroCaixa));
        }

        return pedacos;
    }

    Vertice LerVertice(Vector3[] posicoes, Vector3[] normais, Vector2[] uvs, Color[] cores, int i)
    {
        Vector2 uv = uvs.Length > 0 ? uvs[i] : Vector2.zero;
        Color cor = cores.Length > 0 ? cores[i] : Color.white;
        return new Vertice(posicoes[i], normais[i], uv, cor);
    }

    // Divide os triângulos grandes
    void Separar(Vertice a, Vertice b, Vertice c, Bounds limites, float tamanhoMaximo, Dictionary<int, List<Vertice>> celulas, int nivel)
    {
        float ab = (a.posicao - b.posicao).magnitude;
        float bc = (b.posicao - c.posicao).magnitude;
        float ca = (c.posicao - a.posicao).magnitude;

        if (nivel < 5 && Mathf.Max(ab, bc, ca) > tamanhoMaximo)
        {
            // Corta sempre a maior aresta ao meio
            if (ab >= bc && ab >= ca)
            {
                Vertice m = Vertice.Meio(a, b);
                Separar(a, m, c, limites, tamanhoMaximo, celulas, nivel + 1);
                Separar(m, b, c, limites, tamanhoMaximo, celulas, nivel + 1);
            }
            else if (bc >= ca)
            {
                Vertice m = Vertice.Meio(b, c);
                Separar(a, b, m, limites, tamanhoMaximo, celulas, nivel + 1);
                Separar(a, m, c, limites, tamanhoMaximo, celulas, nivel + 1);
            }
            else
            {
                Vertice m = Vertice.Meio(c, a);
                Separar(a, b, m, limites, tamanhoMaximo, celulas, nivel + 1);
                Separar(m, b, c, limites, tamanhoMaximo, celulas, nivel + 1);
            }
            return;
        }

        Vector3 centro = (a.posicao + b.posicao + c.posicao) / 3f;
        int x = Celula(centro.x, limites.min.x, limites.size.x);
        int y = Celula(centro.y, limites.min.y, limites.size.y);
        int z = Celula(centro.z, limites.min.z, limites.size.z);
        int indice = x + y * divisoes + z * divisoes * divisoes;

        if (!celulas.TryGetValue(indice, out List<Vertice> lista))
        {
            lista = new List<Vertice>();
            celulas[indice] = lista;
        }

        lista.Add(a);
        lista.Add(b);
        lista.Add(c);
    }

    int Celula(float valor, float minimo, float tamanho)
    {
        return Mathf.Clamp(Mathf.FloorToInt((valor - minimo) / tamanho * divisoes), 0, divisoes - 1);
    }

    Pedaco CriarPedaco(List<Vertice> lista, Vector3 centroCaixa)
    {
        // O pivô de cada pedaço fica no meio dele, para girar em torno de si
        Vector3 centro = Vector3.zero;
        foreach (Vertice v in lista)
        {
            centro += v.posicao;
        }
        centro /= lista.Count;

        // As faces são repetidas ao contrário, porque a caixa é oca por dentro
        int total = lista.Count;
        Vector3[] posicoes = new Vector3[total * 2];
        Vector3[] normais = new Vector3[total * 2];
        Vector2[] uvs = new Vector2[total * 2];
        Color[] cores = new Color[total * 2];
        int[] triangulos = new int[total * 2];
        for (int i = 0; i < total; i++)
        {
            posicoes[i] = lista[i].posicao - centro;
            normais[i] = lista[i].normal;
            uvs[i] = lista[i].uv;
            cores[i] = lista[i].cor;
            triangulos[i] = i;

            int verso = total + i;
            posicoes[verso] = posicoes[i];
            normais[verso] = -lista[i].normal;
            uvs[verso] = lista[i].uv;
            cores[verso] = lista[i].cor;
        }
        for (int i = 0; i < total; i += 3)
        {
            triangulos[total + i] = total + i;
            triangulos[total + i + 1] = total + i + 2;
            triangulos[total + i + 2] = total + i + 1;
        }

        Mesh malha = new Mesh();
        malha.vertices = posicoes;
        malha.normals = normais;
        malha.uv = uvs;
        malha.colors = cores;
        malha.triangles = triangulos;
        malha.RecalculateBounds();

        GameObject objeto = new GameObject("Pedaco_" + name);
        objeto.transform.position = transform.TransformPoint(centro);
        objeto.transform.rotation = transform.rotation;
        objeto.transform.localScale = transform.lossyScale;
        objeto.AddComponent<MeshFilter>().sharedMesh = malha;
        objeto.AddComponent<MeshRenderer>().sharedMaterial = rendererCaixa.sharedMaterial;

        // Sai na direção oposta ao centro da caixa, com um pouco de acaso
        Vector3 direcao = objeto.transform.position - centroCaixa;
        direcao.y = 0f;
        direcao = direcao.normalized + Random.insideUnitSphere * 0.25f;
        direcao.y = 0f;

        float multiplicador = explosiva ? 1.8f : 1f;

        Pedaco pedaco = new Pedaco();
        pedaco.transform = objeto.transform;
        pedaco.malha = malha;
        pedaco.escala = objeto.transform.localScale;
        pedaco.velocidade = direcao * forcaLateral * multiplicador * Random.Range(0.6f, 1.2f)
            + Vector3.up * forcaCima * multiplicador * Random.Range(0.7f, 1.2f);
        pedaco.giro = Random.onUnitSphere * Random.Range(260f, 720f);
        return pedaco;
    }

    void DerrubarCaixasDeCima()
    {
        Bounds limites = rendererCaixa.bounds;
        Vector3 centro = new Vector3(limites.center.x, limites.max.y + 0.3f, limites.center.z);
        foreach (Collider outro in Physics.OverlapBox(centro, new Vector3(0.45f, 0.25f, 0.45f)))
        {
            CaixaDestrutivel caixa = outro.GetComponent<CaixaDestrutivel>();
            if (caixa != null && caixa != this)
            {
                caixa.StartCoroutine(caixa.Cair());
            }
        }
    }

    // Faz a caixa cair até o próximo apoio
    IEnumerator Cair()
    {
        // Espera a caixa de baixo perder o colisor, e sai se ainda houver outro apoio
        yield return null;
        if (destruida)
        {
            yield break;
        }

        colisorCaixa.enabled = false;
        bool achouChao = Physics.Raycast(transform.position + Vector3.up * 0.05f, Vector3.down, out RaycastHit chao, 30f, ~0, QueryTriggerInteraction.Ignore);
        colisorCaixa.enabled = true;

        if (!achouChao || chao.distance < 0.1f)
        {
            yield break;
        }

        DerrubarCaixasDeCima();

        float velocidade = 0f;
        while (!destruida && transform.position.y > chao.point.y)
        {
            velocidade += gravidade * Time.deltaTime;
            transform.position += Vector3.down * velocidade * Time.deltaTime;
            yield return null;
        }

        if (!destruida)
        {
            Vector3 posicao = transform.position;
            posicao.y = chao.point.y;
            transform.position = posicao;
        }
    }

    void ExplodirVizinhas()
    {
        foreach (Collider outro in Physics.OverlapSphere(colisorCaixa.bounds.center, raioExplosao))
        {
            // jogador dentro da explosão perde um casco
            if (outro == jogador && inventario != null)
            {
                inventario.PerderVida(colisorCaixa.bounds.center);
            }

            CaixaDestrutivel caixa = outro.GetComponent<CaixaDestrutivel>();
            if (caixa != null && caixa != this)
            {
                caixa.StartCoroutine(caixa.DestruirComAtraso(Random.Range(0.08f, 0.16f)));
            }
        }
    }

    IEnumerator DestruirComAtraso(float atraso)
    {
        yield return new WaitForSeconds(atraso);
        Destruir();
    }

    // Clarão da Plutonita: uma forma facetada que cresce e some, junto com uma luz
    IEnumerator AnimarClarao()
    {
        Vector3 centro = transform.position + Vector3.up * 0.5f;

        GameObject objetoLuz = new GameObject("Luz_Explosao");
        objetoLuz.transform.position = centro;
        Light luz = objetoLuz.AddComponent<Light>();
        luz.type = LightType.Point;
        luz.color = new Color(1f, 0.75f, 0.3f);
        luz.range = 9f;

        GameObject clarao = null;
        Mesh malha = null;
        if (materialBrilho != null)
        {
            malha = CriarOctaedro();
            clarao = new GameObject("Clarao_Explosao");
            clarao.transform.position = centro;
            clarao.AddComponent<MeshFilter>().sharedMesh = malha;
            MeshRenderer visual = clarao.AddComponent<MeshRenderer>();
            visual.sharedMaterial = materialBrilho;
            visual.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        const float duracaoClarao = 0.28f;
        float tempo = 0f;
        while (tempo < duracaoClarao)
        {
            tempo += Time.deltaTime;
            float t = Mathf.Clamp01(tempo / duracaoClarao);
            luz.intensity = 45f * (1f - t);
            if (clarao != null)
            {
                // Cresce rápido e encolhe no final
                float tamanho = raioExplosao * Mathf.Sin(Mathf.Sqrt(t) * Mathf.PI) * 0.8f;
                clarao.transform.localScale = Vector3.one * tamanho;
                clarao.transform.Rotate(0f, 500f * Time.deltaTime, 200f * Time.deltaTime);
            }
            yield return null;
        }

        Destroy(objetoLuz);
        if (clarao != null)
        {
            Destroy(clarao);
            Destroy(malha);
        }
    }

    Mesh CriarOctaedro()
    {
        Vector3[] p =
        {
            Vector3.up, Vector3.right, Vector3.forward, Vector3.left, Vector3.back, Vector3.down
        };
        int[] f =
        {
            0, 2, 1,  0, 3, 2,  0, 4, 3,  0, 1, 4,
            5, 1, 2,  5, 2, 3,  5, 3, 4,  5, 4, 1
        };

        // Vértices separados por face, para manter o visual facetado
        Vector3[] posicoes = new Vector3[f.Length];
        int[] triangulos = new int[f.Length];
        for (int i = 0; i < f.Length; i++)
        {
            posicoes[i] = p[f[i]];
            triangulos[i] = i;
        }

        Mesh malha = new Mesh();
        malha.vertices = posicoes;
        malha.triangles = triangulos;
        malha.RecalculateNormals();
        malha.RecalculateBounds();
        return malha;
    }

    private struct Vertice
    {
        public Vector3 posicao;
        public Vector3 normal;
        public Vector2 uv;
        public Color cor;

        public Vertice(Vector3 posicao, Vector3 normal, Vector2 uv, Color cor)
        {
            this.posicao = posicao;
            this.normal = normal;
            this.uv = uv;
            this.cor = cor;
        }

        public static Vertice Meio(Vertice a, Vertice b)
        {
            return new Vertice(
                (a.posicao + b.posicao) * 0.5f,
                (a.normal + b.normal).normalized,
                (a.uv + b.uv) * 0.5f,
                a.cor
            );
        }
    }
}
