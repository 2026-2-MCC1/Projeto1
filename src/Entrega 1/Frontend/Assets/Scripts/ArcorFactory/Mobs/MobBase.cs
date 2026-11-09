using System.Collections;
using UnityEngine;

namespace ArcorFactory
{
    // Base dos mobs novos
    [RequireComponent(typeof(Collider))]
    public abstract class MobBase : MonoBehaviour
    {
        public enum Estado { Patrulha, Alerta, Atacando, Atordoado, Morto }

        [Header("Atributos")]
        public string nomeExibicao = "Mob";
        public int vidaMaxima = 1;
        public float velocidade = 2.2f;
        public float raioDeteccao = 9f;
        public float alturaDeteccao = 4f;
        public bool danoPorContato = true;
        public bool podeSerPisado = true;
        public bool podeLevarGiro = true;
        public float quiqueAoPisar = 7.5f;
        public int recompensaRocklets = 2;
        public Color corEfeito = new Color(1f, 0.6f, 0.8f);

        [Header("Peças animadas (opcional)")]
        public Transform visual;

        [Header("Desempenho")]
        public float distanciaAtiva = 55f;

        protected Estado estado = Estado.Patrulha;
        protected int vida;
        protected Vector3 origem;
        protected Collider colisor;
        protected AtaqueJogador jogador;
        protected float atordoadoAte;
        protected Vector3 escalaVisual = Vector3.one;

        Renderer[] partes;
        MaterialPropertyBlock bloco;
        float piscarAte;
        Color corPiscar = Color.white;
        int ultimoGiro = -1;
        float ultimaForcaPisca = -1f;

        protected Animator animador;
        int faseAnimacao = int.MinValue;
        Transform olhoE, olhoD;
        float proximoPiscar, piscarOlhos;
        bool dormindo;

        // Buffers reaproveitados (sem alocar a cada quadro)
        static readonly RaycastHit[] acertos = new RaycastHit[16];
        static CameraCrash cameraJogo;

        protected static CameraCrash CameraJogo
        {
            get
            {
                if (cameraJogo == null) cameraJogo = FindFirstObjectByType<CameraCrash>();
                return cameraJogo;
            }
        }

        public Estado EstadoAtual { get { return estado; } }
        public bool Vivo { get { return estado != Estado.Morto; } }

        protected virtual void Start()
        {
            vida = vidaMaxima;
            origem = transform.position;
            colisor = GetComponent<Collider>();
            partes = GetComponentsInChildren<Renderer>();
            bloco = new MaterialPropertyBlock();
            if (visual == null && transform.childCount > 0) visual = transform.GetChild(0);
            if (visual != null) escalaVisual = visual.localScale;
            animador = GetComponent<Animator>();
            if (animador != null)
            {
                animador.applyRootMotion = false;
                // anima sempre
                animador.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animador.keepAnimatorStateOnDisable = true;
                // As animações são trocadas pelo script (Fase)
                if (EstadosAnimacao != null) animador.SetInteger(ParamMob.Fase, -999);
                // A camada "Reacao"
                for (int c = 1; c < animador.layerCount; c++) animador.SetLayerWeight(c, 0f);
            }
            ossoRaiz = Osso("Raiz");
            olhoE = Osso("Olho_E");
            olhoD = Osso("Olho_D");
            proximoPiscar = Time.time + Random.Range(1f, 3.5f);
        }

        // Procura um osso pelo nome em toda a hierarquia do mob
        protected Transform Osso(string nome)
        {
            foreach (var t in GetComponentsInChildren<Transform>(true))
                if (t.name == nome) return t;
            return null;
        }

        // Estado de animação (parâmetro "Fase" do Animator); só envia quando muda
        protected virtual string[] EstadosAnimacao { get { return null; } }
        protected virtual float MisturaAnimacao(int fase) { return 0.15f; }

        protected void Fase(int fase)
        {
            if (animador == null) return;
            var nomes = EstadosAnimacao;
            if (nomes == null || fase < 0 || fase >= nomes.Length)
            {
                if (fase == faseAnimacao && (!animador.isActiveAndEnabled || animador.GetInteger(ParamMob.Fase) == fase)) return;
                faseAnimacao = fase;
                animador.SetInteger(ParamMob.Fase, fase);
                return;
            }
            if (!animador.isActiveAndEnabled) { faseAnimacao = int.MinValue; return; }
            bool mudou = fase != faseAnimacao;
            if (!mudou)
            {
                // vigia: se o Animator não está (nem indo) para o estado certo, corrige na hora
                if (animador.IsInTransition(0)) { if (animador.GetNextAnimatorStateInfo(0).IsName(nomes[fase])) return; }
                else if (animador.GetCurrentAnimatorStateInfo(0).IsName(nomes[fase])) return;
            }
            faseAnimacao = fase;
            animador.CrossFadeInFixedTime(nomes[fase], mudou ? MisturaAnimacao(fase) : 0.1f, 0);
        }

        protected int FaseAtual { get { return faseAnimacao; } }

        // Toca um estado do Animator agora
        protected void TocarEstado(string nome, float mistura = 0.05f)
        {
            if (animador == null || !animador.isActiveAndEnabled) return;
            animador.CrossFadeInFixedTime(nome, mistura, 0, 0f);
        }

        // Ritmo dos passos acompanhando a velocidade real (pés não escorregam)
        protected void RitmoPassos(float velocidadeAtual)
        {
            if (animador != null) animador.SetFloat(ParamMob.MultAndar, Mathf.Clamp(velocidadeAtual / Mathf.Max(0.01f, velocidade), 0.5f, 2.2f));
        }

        // Posição (0..1) dentro do ciclo do estado atual da camada base
        protected float CicloAnimacao()
        {
            if (animador == null) return 0f;
            return Mathf.Repeat(animador.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f);
        }

        // Piscar dos olhos: roda depois do Animator e multiplica a escala que ele deixou
        protected virtual void LateUpdate()
        {
            if (dormindo || estado == Estado.Morto) return;
            ReacaoGolpe();
            if (olhoE == null && olhoD == null) return;
            if (Time.time >= proximoPiscar)
            {
                piscarOlhos = 0.14f;
                proximoPiscar = Time.time + Random.Range(2f, 4.5f);
            }
            if (piscarOlhos <= 0f) return;
            piscarOlhos -= Time.deltaTime;
            float k = Mathf.Clamp01(1f - Mathf.Sin(Mathf.Clamp01(1f - piscarOlhos / 0.14f) * Mathf.PI) * 0.92f);
            if (olhoE != null) { var e = olhoE.localScale; olhoE.localScale = new Vector3(e.x, e.y * k, e.z); }
            if (olhoD != null) { var e = olhoD.localScale; olhoD.localScale = new Vector3(e.x, e.y * k, e.z); }
        }

        // Achatada elástica ao levar golpe (no osso Raiz, por cima da animação)
        Transform ossoRaiz;
        float reacaoInicio = -10f;
        bool reacaoAtiva;

        void ReacaoGolpe()
        {
            if (ossoRaiz == null) return;
            float u = (Time.time - reacaoInicio) / TemposMobs.Dano;
            if (u >= 1f)
            {
                if (reacaoAtiva) { reacaoAtiva = false; ossoRaiz.localScale = Vector3.one; ossoRaiz.localRotation = Quaternion.identity; }
                return;
            }
            reacaoAtiva = true;
            float sobe = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.3f, 1f, u));
            float a = Mathf.Sin(Mathf.Clamp01(u / 0.3f) * Mathf.PI) * (1f - sobe);
            float b = Mathf.Sin(Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.25f, 1f, u)) * Mathf.PI) * 0.6f;
            float lado = 1f + 0.24f * a - 0.08f * b;
            ossoRaiz.localScale = new Vector3(lado, 1f - 0.3f * a + 0.12f * b, lado);
            ossoRaiz.localRotation = Quaternion.Euler(-10f * Mathf.Sin(u * Mathf.PI), 0f, 6f * Mathf.Sin(u * Mathf.PI * 3f) * (1f - u));
        }

        protected virtual void Update()
        {
            if (jogador == null) jogador = AtaqueJogador.Instancia;
            if (estado == Estado.Morto) return;

            // longe do jogador: não pensa nem anima
            bool longe = jogador != null && (jogador.transform.position - transform.position).sqrMagnitude > distanciaAtiva * distanciaAtiva;
            if (longe != dormindo)
            {
                dormindo = longe;
                if (animador != null) animador.enabled = !longe;
                faseAnimacao = int.MinValue; // ao acordar, reenvia o estado de animação
            }
            if (dormindo) return;

            AtualizarPisca();
            if (jogador != null && VerificarGolpes()) return;

            if (estado == Estado.Atordoado && Time.time >= atordoadoAte)
            {
                estado = Estado.Patrulha;
                faseAnimacao = int.MinValue; // sai da pose de golpe/cansaço na hora
            }
            Pensar();
            SepararDeOutrosMobs();

            if (danoPorContato && estado != Estado.Atordoado && jogador != null && PodeCausarDano() && jogador.Encostou(colisor.bounds, 0.05f))
            {
                CausarDano();
            }
        }

        protected abstract void Pensar();

        // Alguns mobs só machucam em certos momentos (ex.: o drone no mergulho)
        protected virtual bool PodeCausarDano() { return true; }

        // Ajuste do alcance do giro da tartaruga contra este mob
        protected virtual float AlcanceGiroExtra { get { return 0f; } }
        protected virtual float AlturaGiroMax { get { return 1.4f; } }

        protected void CausarDano()
        {
            var inv = jogador.Inventario;
            if (inv == null || inv.Invencivel) return;
            inv.PerderVida(transform.position);
            AoAcertarJogador();
        }

        protected virtual void AoAcertarJogador() { }

        bool VerificarGolpes()
        {
            Bounds b = colisor.bounds;
            if (podeSerPisado && jogador.PisouEm(b))
            {
                jogador.Quicar(quiqueAoPisar);
                Sons.TocarEm("mob_pisao", transform.position, 0.9f);
                ReceberGolpe(1, jogador.transform.position, true);
                return true;
            }
            if (jogador.IdGiro != ultimoGiro && jogador.GiroAcerta(b, AlcanceGiroExtra, AlturaGiroMax))
            {
                ultimoGiro = jogador.IdGiro;
                if (podeLevarGiro && AceitaGiro(jogador.transform.position))
                {
                    ReceberGolpe(1, jogador.transform.position, false);
                }
                else
                {
                    // blindado: o giro ricocheteia
                    Sons.TocarEm("ricochete", transform.position, 0.8f);
                    Efeitos.Faiscas(b.center, Color.white, 12, 6f);
                    jogador.Empurrar(transform.position, 6f, 2f);
                }
                return true;
            }
            return false;
        }

        // Alguns mobs só aceitam o giro de certos lados
        protected virtual bool AceitaGiro(Vector3 deOnde) { return true; }

        public virtual void ReceberGolpe(int dano, Vector3 deOnde, bool porCima)
        {
            if (estado == Estado.Morto) return;
            vida -= dano;
            Piscar(Color.white, 0.18f);
            Efeitos.Estrelas(colisor.bounds.center);
            Sons.TocarEm("mob_golpe", transform.position, 0.9f);
            reacaoInicio = Time.time;
            if (CameraJogo != null) CameraJogo.Tremer(0.08f, 0.12f);

            if (vida <= 0)
            {
                Morrer(deOnde, porCima);
            }
            else
            {
                Atordoar(0.9f);
                Vector3 dir = transform.position - deOnde; dir.y = 0f;
                if (dir.sqrMagnitude > 0.01f) StartCoroutine(Recuar(dir.normalized * 2.2f));
            }
        }

        protected void Atordoar(float segundos)
        {
            estado = Estado.Atordoado;
            atordoadoAte = Time.time + segundos;
        }

        IEnumerator Recuar(Vector3 deslocamento)
        {
            float t = 0f;
            Vector3 a = transform.position;
            while (t < 0.18f)
            {
                t += Time.deltaTime;
                Vector3 alvo = a + deslocamento * Mathf.Sin(Mathf.Clamp01(t / 0.18f) * Mathf.PI * 0.5f);
                Mover(alvo - transform.position);
                yield return null;
            }
        }

        protected virtual void Morrer(Vector3 deOnde, bool porCima)
        {
            estado = Estado.Morto;
            colisor.enabled = false;
            EstatisticasFase.MobDerrotado();
            Sons.TocarEm("mob_morre", transform.position, 1f);
            Vector3 centro = colisor.bounds.center;
            Efeitos.Poeira(centro, 1.2f, new Color(1f, 1f, 1f, 0.8f));
            Efeitos.Brilhos(centro, corEfeito, 16, 0.5f, 3f);
            Efeitos.Farelos(centro, corEfeito, 18);
            TextoFlutuante.Criar(centro + Vector3.up * 1.2f, "+" + recompensaRocklets, Cores.Ouro, 4f);
            var inv = jogador != null ? jogador.Inventario : null;
            if (inv != null && recompensaRocklets > 0)
            {
                var cfg = ConfigArcor.Instancia;
                for (int i = 0; i < recompensaRocklets; i++)
                {
                    var prefab = cfg.RockletAleatorio();
                    if (prefab == null) { inv.SomarRocklets(recompensaRocklets - i); break; }
                    var r = Instantiate(prefab, centro + Random.insideUnitSphere * 0.4f, Quaternion.identity);
                    foreach (var c in r.GetComponentsInChildren<Collider>()) c.enabled = false;
                    inv.ColetarVoando(r.transform, 0.1f + i * 0.08f);
                }
            }
            StartCoroutine(AnimacaoMorte(porCima));
        }

        protected virtual IEnumerator AnimacaoMorte(bool porCima)
        {
            Transform v = visual != null ? visual : transform;
            Vector3 escala = v.localScale;
            float t = 0f;
            if (porCima)
            {
                // achatado
                while (t < 0.12f)
                {
                    t += Time.deltaTime;
                    float k = t / 0.12f;
                    v.localScale = Vector3.Scale(escala, new Vector3(1f + 0.5f * k, 1f - 0.8f * k, 1f + 0.5f * k));
                    yield return null;
                }
                yield return new WaitForSeconds(0.25f);
            }
            else
            {
                // voa girando para longe
                Vector3 dir = jogador != null ? transform.position - jogador.transform.position : transform.forward;
                dir.y = 0f;
                dir = dir.sqrMagnitude > 0.01f ? dir.normalized : Vector3.forward;
                Vector3 vel = dir * 7f + Vector3.up * 6f;
                while (t < 0.6f)
                {
                    t += Time.deltaTime;
                    vel += Vector3.down * 20f * Time.deltaTime;
                    transform.position += vel * Time.deltaTime;
                    transform.Rotate(Vector3.right, 720f * Time.deltaTime, Space.Self);
                    yield return null;
                }
            }
            t = 0f;
            Vector3 e2 = v.localScale;
            while (t < 0.15f)
            {
                t += Time.deltaTime;
                v.localScale = e2 * (1f - t / 0.15f);
                yield return null;
            }
            Efeitos.Poeira(transform.position, 0.8f);
            Destroy(gameObject);
        }

        // ajudantes
        protected void Piscar(Color cor, float duracao)
        {
            corPiscar = cor;
            piscarAte = Time.time + duracao;
        }

        void AtualizarPisca()
        {
            if (partes == null) return;
            float forca = 0f;
            if (Time.time < piscarAte) forca = 0.85f;
            else if (estado == Estado.Atordoado) forca = Mathf.Repeat(Time.time * 6f, 1f) < 0.5f ? 0.35f : 0f;
            ForcaPisca(forca, Time.time < piscarAte ? corPiscar : new Color(1f, 1f, 0.6f));
        }

        protected void ForcaPisca(float forca, Color cor)
        {
            if (Mathf.Approximately(forca, ultimaForcaPisca) && forca <= 0f) return; // nada mudou
            ultimaForcaPisca = forca;
            foreach (var r in partes)
            {
                if (r == null || r is ParticleSystemRenderer) continue;
                if (forca <= 0f) { r.SetPropertyBlock(null); continue; }
                r.GetPropertyBlock(bloco);
                bloco.SetColor("_CorPiscar", new Color(cor.r, cor.g, cor.b, forca));
                r.SetPropertyBlock(bloco);
            }
        }

        protected Vector3 ParaJogador()
        {
            if (jogador == null) return Vector3.zero;
            Vector3 d = jogador.transform.position - transform.position;
            return d;
        }

        protected bool JogadorPerto(float raio)
        {
            if (jogador == null) return false;
            Vector3 d = ParaJogador();
            if (Mathf.Abs(d.y) > alturaDeteccao) return false;
            d.y = 0f;
            return d.magnitude <= raio;
        }

        protected void Virar(Vector3 direcao, float grausPorSegundo = 400f)
        {
            direcao.y = 0f;
            if (direcao.sqrMagnitude < 0.0001f) return;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direcao), grausPorSegundo * Time.deltaTime);
        }

        // Altura do chão embaixo de um ponto (ignora o próprio mob e o jogador)
        protected bool AlturaChao(Vector3 ponto, out float y, float de = 2f, float ate = 6f)
        {
            y = 0f;
            float melhor = float.NegativeInfinity;
            int n = Physics.RaycastNonAlloc(ponto + Vector3.up * de, Vector3.down, acertos, de + ate, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < n; i++)
            {
                var h = acertos[i];
                if (h.collider == colisor || h.collider is CharacterController || h.collider.transform.IsChildOf(transform)) continue;
                if (h.point.y > melhor) melhor = h.point.y;
            }
            if (float.IsNegativeInfinity(melhor)) return false;
            y = melhor;
            return true;
        }

        // Anda pela velocidade dada (no chão)
        protected float passo;

        // Movimento livre (3D) com colisão contra o mapa
        protected bool Mover(Vector3 delta, bool ignoraChao = false)
        {
            float dist = delta.magnitude;
            if (dist < 1e-5f) return true;
            bool livre = true;
            for (int tentativa = 0; tentativa < 2 && dist > 1e-5f; tentativa++)
            {
                Vector3 dir = delta / dist;
                Bounds bc = colisor.bounds;
                float raio = Mathf.Max(0.15f, Mathf.Min(Mathf.Max(bc.extents.x, bc.extents.z), bc.extents.y) * 0.8f);
                int n = Physics.SphereCastNonAlloc(bc.center, raio, dir, acertos, dist + 0.03f, ~0, QueryTriggerInteraction.Ignore);
                float melhor = float.MaxValue; Vector3 normal = Vector3.zero;
                for (int i = 0; i < n; i++)
                {
                    var h = acertos[i];
                    if (h.collider == colisor || h.collider is CharacterController || h.collider.transform.IsChildOf(transform)) continue;
                    if (ignoraChao && h.normal.y > 0.6f) continue;
                    if (h.distance < melhor) { melhor = h.distance; normal = h.normal; }
                }
                if (melhor == float.MaxValue) { transform.position += delta; return livre; }
                livre = false;
                float anda = Mathf.Max(0f, melhor - 0.03f);
                transform.position += dir * Mathf.Min(anda, dist);
                // o que sobra desliza ao longo da superfície
                Vector3 resto = dir * (dist - anda);
                delta = Vector3.ProjectOnPlane(resto, normal);
                dist = delta.magnitude;
            }
            return livre;
        }

        // Mobs não se atravessam
        static readonly Collider[] vizinhos = new Collider[12];
        void SepararDeOutrosMobs()
        {
            if (colisor == null || !colisor.enabled) return;
            Bounds bc = colisor.bounds;
            int n = Physics.OverlapSphereNonAlloc(bc.center, bc.extents.magnitude, vizinhos, ~0, QueryTriggerInteraction.Collide);
            for (int i = 0; i < n; i++)
            {
                var o = vizinhos[i];
                if (o == null || o == colisor) continue;
                var outro = o.GetComponentInParent<MobBase>();
                if (outro == null || outro == this || outro.estado == Estado.Morto) continue;
                if (Physics.ComputePenetration(colisor, transform.position, transform.rotation, o, o.transform.position, o.transform.rotation, out Vector3 dir, out float prof))
                {
                    dir.y = 0f;
                    if (dir.sqrMagnitude < 1e-6f) dir = transform.position - outro.transform.position;
                    dir.y = 0f;
                    if (dir.sqrMagnitude < 1e-6f) dir = Vector3.right;
                    Mover(dir.normalized * (prof * 0.55f + 0.01f));
                }
            }
        }

        protected bool Andar(Vector3 vel, float grausVirar = 540f)
        {
            Vector3 d = vel * Time.deltaTime;
            d.y = 0f;
            if (d.sqrMagnitude < 1e-6f) return true;
            Vector3 dir = d.normalized;
            Bounds bc = colisor.bounds;
            float raio = Mathf.Max(bc.extents.x, bc.extents.z) * 0.64f;
            int n = Physics.SphereCastNonAlloc(bc.center, raio, dir, acertos, d.magnitude + 0.15f, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < n; i++)
            {
                var h = acertos[i];
                if (h.collider == colisor || h.collider is CharacterController || h.collider.transform.IsChildOf(transform)) continue;
                if (h.normal.y > 0.6f) continue; // chão
                return false;
            }
            Vector3 novo = transform.position + d;
            if (!AlturaChao(novo + dir * 0.4f, out float y, 1f, 1.2f)) return false; // beirada
            novo.y = y;
            transform.position = novo;
            Virar(dir, grausVirar);
            passo += d.magnitude;
            return true;
        }

        // Escala de "gelatina" para dar vida (squash & stretch)
        protected void Gelatina(float estica)
        {
            if (visual == null) return;
            visual.localScale = Vector3.Scale(escalaVisual, new Vector3(1f - estica * 0.5f, 1f + estica, 1f - estica * 0.5f));
        }
    }
}
