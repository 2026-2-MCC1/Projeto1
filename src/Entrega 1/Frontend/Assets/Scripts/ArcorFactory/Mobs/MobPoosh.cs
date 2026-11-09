using UnityEngine;

namespace ArcorFactory
{
    // Poosh (voador): bolha de doce roxa e rosa com asinhas de fada
    public class MobPoosh : MobBase
    {
        public float alturaVoo = 2.6f;
        public float raioCirculo = 3.2f;
        public float velocidadeMergulho = 11f;
        public Transform grito;

        enum Fase_ { Patrulha, Mirando, Mergulho, Tonto, Subindo }
        Fase_ fase = Fase_.Patrulha;
        float timer, angulo, chaoOrigem, gritoAte;
        Vector3 alvoMergulho, escalaGrito = Vector3.one;
        AudioSource zumbido;
        ParticleSystem bolhas;

        protected override void Start()
        {
            base.Start();
            if (nomeExibicao == "Mob") nomeExibicao = "Poosh";
            angulo = Random.value * Mathf.PI * 2f;
            if (!AlturaChao(origem, out chaoOrigem, 1f, 8f)) chaoOrigem = origem.y - alturaVoo;
            if (grito != null) { escalaGrito = grito.localScale; grito.gameObject.SetActive(false); }

            zumbido = gameObject.AddComponent<AudioSource>();
            zumbido.clip = Sons.Clip("asas_loop");
            zumbido.loop = true;
            zumbido.spatialBlend = 1f;
            zumbido.minDistance = 1.5f;
            zumbido.maxDistance = 12f;
            zumbido.rolloffMode = AudioRolloffMode.Linear;
            zumbido.volume = 0.4f;
            zumbido.pitch = Random.Range(0.95f, 1.1f);
            if (zumbido.clip != null) zumbido.Play();

            bolhas = Efeitos.Aura(transform, new Color(1f, 0.7f, 0.95f), 0.35f, 5f);
            var m = bolhas.main;
            m.gravityModifier = -0.25f;
            m.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.2f);
            Fase(FasePoosh.Voar);
        }

        protected override bool PodeCausarDano() { return fase == Fase_.Mergulho; }

        // Estados do Mob_Poosh.controller na ordem de FasePoosh
        static readonly string[] estadosPoosh = { "Voar", "Mirar", "Mergulho", "Tonto" };
        protected override string[] EstadosAnimacao { get { return estadosPoosh; } }
        protected override float MisturaAnimacao(int f)
        {
            return f == FasePoosh.Voar ? 0.18f : f == FasePoosh.Mirar ? 0.12f : f == FasePoosh.Mergulho ? 0.06f : 0.08f;
        }

        // giro só pega o Poosh de perto (antes acertava de ~2,4 m)
        protected override float AlcanceGiroExtra { get { return -1.05f; } }
        protected override float AlturaGiroMax { get { return 0.7f; } }

        protected override void Pensar()
        {
            AnimarGrito();
            timer -= Time.deltaTime;

            switch (fase)
            {
                case Fase_.Patrulha:
                {
                    Fase(FasePoosh.Voar);
                    angulo += velocidade / Mathf.Max(1f, raioCirculo) * Time.deltaTime;
                    float flut = Mathf.Sin(Time.time * 2.4f) * 0.22f;
                    Vector3 alvo = new Vector3(origem.x + Mathf.Cos(angulo) * raioCirculo, chaoOrigem + alturaVoo + flut, origem.z + Mathf.Sin(angulo) * raioCirculo);
                    Vector3 antes = transform.position;
                    Mover(Vector3.MoveTowards(transform.position, alvo, velocidade * 1.5f * Time.deltaTime) - transform.position);
                    Virar(alvo - antes, 300f);
                    Inclinar(0.15f, Mathf.Sin(Time.time * 1.3f) * 8f);
                    if (JogadorPerto(raioDeteccao) && (jogador.transform.position - origem).magnitude < raioDeteccao + raioCirculo)
                    {
                        fase = Fase_.Mirando;
                        timer = TemposMobs.PooshMirar;
                        gritoAte = Time.time + TemposMobs.PooshMirar + 0.35f;
                        Sons.TocarEm("poosh_alerta", transform.position, 0.9f, 0.04f);
                    }
                    break;
                }
                case Fase_.Mirando:
                {
                    Fase(FasePoosh.Mirar);
                    Vector3 sobre = jogador.transform.position;
                    float yChao = jogador.Corpo.bounds.min.y;
                    Vector3 alvo = new Vector3(sobre.x, yChao + alturaVoo + 0.4f, sobre.z) - ParaJogador().normalized * 0.8f;
                    Mover(Vector3.MoveTowards(transform.position, alvo, velocidade * 1.3f * Time.deltaTime) - transform.position);
                    Virar(ParaJogador(), 600f);
                    float k = 1f - Mathf.Clamp01(timer / TemposMobs.PooshMirar);
                    Inclinar(-0.35f * k, 0f);
                    ForcaPisca(Mathf.Repeat(Time.time * 9f, 1f) < 0.5f ? 0.3f * k : 0f, new Color(1f, 0.25f, 0.5f));
                    if (timer <= 0f)
                    {
                        alvoMergulho = new Vector3(sobre.x, yChao + 0.45f, sobre.z);
                        fase = Fase_.Mergulho;
                        Sons.TocarEm("poosh_mergulho", transform.position, 0.9f);
                        ForcaPisca(0f, Color.white);
                    }
                    break;
                }
                case Fase_.Mergulho:
                {
                    Fase(FasePoosh.Mergulho);
                    Vector3 dir = alvoMergulho - transform.position;
                    bool livreMergulho = Mover(Vector3.MoveTowards(transform.position, alvoMergulho, velocidadeMergulho * Time.deltaTime) - transform.position, true);
                    Vector3 plano = new Vector3(dir.x, 0f, dir.z);
                    Quaternion rumo = Quaternion.LookRotation(plano.sqrMagnitude > 0.01f ? plano : transform.forward) * Quaternion.Euler(40f, 0f, 0f);
                    transform.rotation = Quaternion.Slerp(transform.rotation, rumo, 10f * Time.deltaTime);
                    if (dir.magnitude < 0.1f || !livreMergulho) Bater();
                    break;
                }
                case Fase_.Tonto:
                    Fase(FasePoosh.Tonto);
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, transform.eulerAngles.y, 0f), 6f * Time.deltaTime);
                    if (Random.value < 0.04f) Efeitos.Estrelas(transform.position + Vector3.up * 0.9f, Cores.Ouro);
                    if (timer <= 0f)
                    {
                        fase = Fase_.Subindo;
                        estado = Estado.Patrulha;
                        if (zumbido != null) zumbido.pitch = 1f;
                    }
                    break;

                case Fase_.Subindo:
                {
                    Fase(FasePoosh.Voar);
                    Vector3 alvo = transform.position;
                    alvo.y = chaoOrigem + alturaVoo;
                    Vector3 volta = new Vector3(origem.x + Mathf.Cos(angulo) * raioCirculo, alvo.y, origem.z + Mathf.Sin(angulo) * raioCirculo);
                    Mover(Vector3.MoveTowards(transform.position, Vector3.Lerp(alvo, volta, 0.5f), velocidade * 2f * Time.deltaTime) - transform.position);
                    Inclinar(0f, 0f);
                    if (Mathf.Abs(transform.position.y - alvo.y) < 0.1f) fase = Fase_.Patrulha;
                    break;
                }
            }
        }

        // Chegou ao chão no mergulho: impacto e fica tonto
        void Bater()
        {
            fase = Fase_.Tonto;
            timer = TemposMobs.PooshTonto;
            Atordoar(TemposMobs.PooshTonto);
            Efeitos.Poeira(transform.position, 1f);
            Efeitos.Brilhos(transform.position, new Color(1f, 0.55f, 0.85f), 14, 0.5f, 2f);
            Sons.TocarEm("poosh_batida", transform.position, 0.9f);
            if (zumbido != null) zumbido.pitch = 0.6f;
            if (CameraJogo != null) CameraJogo.Tremer(0.06f, 0.15f);
        }

        // Levou golpe sem morrer: cai tonto (mesmo fluxo do mergulho)
        public override void ReceberGolpe(int dano, Vector3 deOnde, bool porCima)
        {
            base.ReceberGolpe(dano, deOnde, porCima);
            if (Vivo && fase != Fase_.Tonto) { fase = Fase_.Tonto; timer = 0.9f; }
        }

        // Placa "POOSH!": pula, treme e fica sempre virada para a câmera
        void AnimarGrito()
        {
            if (grito == null) return;
            bool ativo = Time.time < gritoAte && estado != Estado.Morto;
            if (grito.gameObject.activeSelf != ativo) grito.gameObject.SetActive(ativo);
            if (!ativo) return;
            float resta = gritoAte - Time.time;
            float idade = TemposMobs.PooshMirar + 0.35f - resta;
            float k = Mathf.Clamp01(idade / 0.18f);
            float s = k < 1f ? Mathf.Lerp(0f, 1.25f, k) : Mathf.Lerp(1.25f, 1f, Mathf.Clamp01((idade - 0.18f) / 0.12f));
            if (resta < 0.15f) s *= resta / 0.15f;
            grito.localScale = escalaGrito * s;
            grito.position = transform.position + Vector3.up * (0.95f + Mathf.Sin(idade * 30f) * 0.02f);
            var cam = UnityEngine.Camera.main;
            if (cam != null) grito.rotation = Quaternion.LookRotation(cam.transform.position - grito.position) * Quaternion.Euler(0f, 0f, Mathf.Sin(idade * 40f) * 4f);
        }

        void Inclinar(float frente, float lado)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(frente * 60f, transform.eulerAngles.y, lado), 5f * Time.deltaTime);
        }

        protected override void Morrer(Vector3 deOnde, bool porCima)
        {
            if (bolhas != null) bolhas.Stop();
            if (zumbido != null) zumbido.Stop();
            if (grito != null) grito.gameObject.SetActive(false);
            Efeitos.Brilhos(colisor.bounds.center, new Color(0.75f, 0.45f, 1f), 20, 0.6f, 3f);
            base.Morrer(deOnde, porCima);
        }
    }
}
