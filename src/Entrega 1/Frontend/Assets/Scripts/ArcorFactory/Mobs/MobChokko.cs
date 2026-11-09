using UnityEngine;

namespace ArcorFactory
{
    // Chokko: golem de blocos de chocolate com ombreiras de biscoito e punhos enormes
    public class MobChokko : MobBase
    {
        public float alcancePatrulha = 3.5f;
        public float distanciaSoco = 2.3f;
        public float alcanceSoco = 2.7f;
        // Raio fixo (não serializado, para valer também nos mobs já colocados nas fases)
        public float raioPancada { get { return 4f; } }
        public float distanciaPancada = 7f;
        public float intervaloPancada = 4.5f;

        enum Acao { Andando, Socando, Preparando, Pancada }
        Acao acao = Acao.Andando;
        float timer, sentido = 1f, esperaPancada, cicloAnterior;
        bool socoEsquerdo, acertouSoco;
        Vector3 eixo;
        Transform aviso;
        Renderer avisoRend;
        MaterialPropertyBlock blocoAviso;

        protected override void Start()
        {
            base.Start();
            if (nomeExibicao == "Mob") nomeExibicao = "Chokko";
            eixo = transform.right;
            sentido = Random.value < 0.5f ? 1f : -1f;
            esperaPancada = Random.Range(1.5f, intervaloPancada);
        }

        bool cansado; // true só no descanso depois da pancada

        // Levou golpe: cancela o que estava fazendo (soco/preparo) e estremece
        public override void ReceberGolpe(int dano, Vector3 deOnde, bool porCima)
        {
            if (!cansado) { acao = Acao.Andando; EsconderAviso(); timer = 0f; }
            base.ReceberGolpe(dano, deOnde, porCima);
            if (Vivo && !cansado) Atordoar(TemposMobs.ChokkoAtingido);
        }

        // Estados do Mob_Chokko.controller na ordem de FaseChokko
        static readonly string[] estadosChokko = { "Idle", "Andar", "SocoE", "SocoD", "Preparar", "Pancada", "Cansado", "Atingido" };
        protected override string[] EstadosAnimacao { get { return estadosChokko; } }
        protected override float MisturaAnimacao(int fase)
        {
            switch (fase)
            {
                case FaseChokko.Andar: return 0.2f;
                case FaseChokko.SocoE: case FaseChokko.SocoD: return 0.06f;
                case FaseChokko.Preparar: return 0.08f;
                case FaseChokko.Pancada: return 0.02f;
                case FaseChokko.Cansado: return 0.15f;
                case FaseChokko.Atingido: return 0.04f;
                default: return 0.25f;
            }
        }

        protected override void Pensar()
        {
            if (estado == Estado.Atordoado)
            {
                acao = Acao.Andando;
                EsconderAviso();
                int alvoAnim = cansado ? FaseChokko.Cansado : FaseChokko.Atingido;
                if (FaseAtual != alvoAnim)
                {
                    Fase(alvoAnim);
                }
                return;
            }
            cansado = false;
            timer -= Time.deltaTime;
            esperaPancada -= Time.deltaTime;

            switch (acao)
            {
                case Acao.Andando: Andando(); break;
                case Acao.Socando: Socando(); break;
                case Acao.Preparando:
                    Virar(ParaJogador(), 200f);
                    Fase(FaseChokko.Preparar);
                    AtualizarAviso(1f - Mathf.Clamp01(timer / TemposMobs.ChokkoPreparo));
                    if (Random.value < 0.3f) Efeitos.Farelos(transform.position + Vector3.up * 0.2f, Cores.Chocolate, 2);
                    if (timer <= 0f) { acao = Acao.Pancada; timer = TemposMobs.ChokkoPancada; Fase(FaseChokko.Pancada); }
                    break;
                case Acao.Pancada:
                    Fase(FaseChokko.Pancada);
                    if (timer <= 0f)
                    {
                        Pancada();
                        esperaPancada = intervaloPancada;
                        acao = Acao.Andando;
                        cansado = true;
                        Atordoar(TemposMobs.ChokkoCansado);
                    }
                    break;
            }
        }

        void Andando()
        {
            bool ve = JogadorPerto(raioDeteccao);
            estado = ve ? Estado.Alerta : Estado.Patrulha;
            Vector3 pj = ParaJogador(); pj.y = 0f;
            float dist = pj.magnitude;

            if (ve && dist <= distanciaSoco)
            {
                Virar(pj, 500f);
                acao = Acao.Socando;
                timer = TemposMobs.ChokkoSocoTotal;
                socoEsquerdo = !socoEsquerdo;
                acertouSoco = false;
                Fase(socoEsquerdo ? FaseChokko.SocoE : FaseChokko.SocoD);
                Sons.TocarEm("chokko_preparo", transform.position, 0.45f, 0.08f);
                return;
            }
            if (ve && dist <= distanciaPancada && dist > distanciaSoco + 0.6f && esperaPancada <= 0f)
            {
                acao = Acao.Preparando;
                timer = TemposMobs.ChokkoPreparo;
                Fase(FaseChokko.Preparar);
                Sons.TocarEm("chokko_preparo", transform.position, 0.9f, 0f);
                MostrarAviso();
                return;
            }

            bool andou;
            if (ve) andou = Andar(pj.normalized * velocidade * 1.15f, 260f);
            else
            {
                Vector3 alvo = origem + eixo * alcancePatrulha * sentido;
                Vector3 d = alvo - transform.position; d.y = 0f;
                if (d.magnitude < 0.15f) { sentido = -sentido; andou = false; }
                else { andou = Andar(d.normalized * velocidade, 200f); if (!andou) sentido = -sentido; }
            }
            if (ve && !andou) Virar(pj, 300f);
            Fase(andou ? FaseChokko.Andar : FaseChokko.Idle);
            RitmoPassos(andou ? velocidade * (ve ? 1.15f : 1f) : 0f);
            if (andou) PassosNoChao();
        }

        // O soco tem três partes (puxar, golpe, voltar); só machuca durante o golpe
        void Socando()
        {
            float decorrido = TemposMobs.ChokkoSocoTotal - timer;
            bool golpe = decorrido >= TemposMobs.ChokkoPuxarSoco && decorrido < TemposMobs.ChokkoPuxarSoco + TemposMobs.ChokkoSoco;
            if (decorrido < TemposMobs.ChokkoPuxarSoco)
            {
                Virar(ParaJogador(), 500f);
                ForcaPisca(0.3f * decorrido / TemposMobs.ChokkoPuxarSoco, new Color(1f, 0.55f, 0.2f));
            }
            else if (golpe)
            {
                if (!acertouSoco && decorrido - Time.deltaTime < TemposMobs.ChokkoPuxarSoco)
                {
                    ForcaPisca(0f, Color.white);
                    Sons.TocarEm("chokko_soco", transform.position, 0.9f);
                }
                Andar(transform.forward * 4f, 0f); // avança junto com o soco
                if (!acertouSoco && jogador != null)
                {
                    Vector3 d = jogador.Centro - (transform.position + Vector3.up * 1.1f);
                    Vector3 dh = new Vector3(d.x, 0f, d.z);
                    if (dh.magnitude < alcanceSoco && Mathf.Abs(d.y) < 1.6f && Vector3.Dot(transform.forward, dh.normalized) > 0.2f)
                    {
                        acertouSoco = true;
                        var inv = jogador.Inventario;
                        if (inv != null && !inv.Invencivel)
                        {
                            inv.PerderVida(transform.position);
                            jogador.Empurrar(transform.position, 9f, 3f);
                            Efeitos.Estrelas(jogador.Centro, Cores.Ouro);
                        }
                    }
                }
            }
            if (timer <= 0f)
            {
                acao = Acao.Andando;
                Efeitos.Poeira(transform.position + transform.forward * 1.2f, 0.4f, new Color(0.55f, 0.35f, 0.2f, 0.7f));
            }
        }

        // "Tum" quando cada pé pisa: as pernas cruzam o meio do passo em 0 e 0,5 do ciclo
        void PassosNoChao()
        {
            float c = CicloAnimacao();
            if ((cicloAnterior < 0.5f && c >= 0.5f) || c < cicloAnterior)
            {
                Sons.TocarEm("chokko_passo", transform.position, 0.55f, 0.08f, 18f);
                Efeitos.Poeira(transform.position, 0.3f, new Color(0.55f, 0.38f, 0.25f, 0.5f));
            }
            cicloAnterior = c;
        }

        void Pancada()
        {
            Vector3 c = transform.position;
            EsconderAviso();
            Sons.TocarEm("chokko_boom", c, 1f, 0.04f, 40f);
            Efeitos.Poeira(c + transform.forward * 0.8f, 2.2f, new Color(0.55f, 0.38f, 0.25f, 0.8f));
            Efeitos.Anel(c + Vector3.up * 0.15f, new Color(1f, 0.75f, 0.45f, 0.9f), raioPancada * 2.2f);
            Efeitos.Anel(c + Vector3.up * 0.1f, new Color(1f, 1f, 1f, 0.6f), raioPancada * 1.4f);
            Efeitos.Farelos(c + transform.forward * 0.8f, Cores.Chocolate, 30);
            Efeitos.Faiscas(c + transform.forward * 0.8f, new Color(1f, 0.8f, 0.5f), 18, 8f);
            if (jogador == null) return;

            float dist = Vector3.Distance(jogador.transform.position, c);
            if (CameraJogo != null) CameraJogo.Tremer(Mathf.Lerp(0.35f, 0.08f, Mathf.Clamp01(dist / 18f)), 0.45f);

            // onda de choque: só pega quem está no chão dentro do círculo
            Bounds b = jogador.Corpo.bounds;
            Vector3 d = new Vector3(b.center.x, b.min.y, b.center.z) - c;
            bool noChao = jogador.Corpo.isGrounded || Mathf.Abs(d.y) < 0.45f;
            if (noChao && new Vector2(d.x, d.z).magnitude < raioPancada && Mathf.Abs(d.y) < 1.5f)
            {
                var inv = jogador.Inventario;
                if (inv != null && !inv.Invencivel)
                {
                    inv.PerderVida(c);
                    jogador.Empurrar(c, 8f, 5f);
                }
            }
        }

        // círculo de aviso no chão
        void MostrarAviso()
        {
            if (aviso == null)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Destroy(go.GetComponent<Collider>());
                go.name = "Aviso_Pancada";
                aviso = go.transform;
                avisoRend = go.GetComponent<Renderer>();
                avisoRend.sharedMaterial = Efeitos.MatAditiva;
                avisoRend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                blocoAviso = new MaterialPropertyBlock();
            }
            aviso.gameObject.SetActive(true);
            float y = transform.position.y;
            if (AlturaChao(transform.position, out float ch, 1f, 2f)) y = ch;
            aviso.position = new Vector3(transform.position.x, y + 0.05f, transform.position.z);
            aviso.rotation = Quaternion.LookRotation(Vector3.down);
            aviso.localScale = Vector3.zero;
        }

        void AtualizarAviso(float k)
        {
            if (aviso == null) return;
            aviso.localScale = Vector3.one * raioPancada * 2.3f * Mathf.Clamp01(k * 2.5f);
            float pisca = 0.35f + 0.35f * Mathf.Abs(Mathf.Sin(Time.time * (8f + k * 14f)));
            avisoRend.GetPropertyBlock(blocoAviso);
            blocoAviso.SetColor("_BaseColor", new Color(1f, 0.15f, 0.1f, 1f) * pisca);
            avisoRend.SetPropertyBlock(blocoAviso);
        }

        void EsconderAviso()
        {
            if (aviso != null) aviso.gameObject.SetActive(false);
        }

        protected override void Morrer(Vector3 deOnde, bool porCima)
        {
            EsconderAviso();
            Efeitos.Farelos(colisor.bounds.center, Cores.Chocolate, 30);
            Efeitos.Farelos(colisor.bounds.center, new Color(0.85f, 0.65f, 0.4f), 16);
            base.Morrer(deOnde, porCima);
        }

        void OnDestroy()
        {
            if (aviso != null) Destroy(aviso.gameObject);
        }
    }
}
