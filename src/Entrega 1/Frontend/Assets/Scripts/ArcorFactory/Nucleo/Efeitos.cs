using UnityEngine;

namespace ArcorFactory
{
    // Cada chamada cria um ParticleSystem já configurado
    public static class Efeitos
    {
        static Material matAditiva, matSuave, matEstrela;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reiniciar() { matAditiva = matSuave = matEstrela = null; }

        public static Material MatAditiva { get { Carregar(); return matAditiva; } }
        public static Material MatSuave { get { Carregar(); return matSuave; } }
        public static Material MatEstrela { get { Carregar(); return matEstrela; } }

        static void Carregar()
        {
            if (matAditiva != null) return;
            var cfg = ConfigArcor.Instancia;
            matAditiva = cfg.particulaAditiva;
            matSuave = cfg.particulaSuave;
            matEstrela = cfg.particulaEstrela;
            if (matAditiva == null || matSuave == null)
            {
                var reserva = new Material(Shader.Find("Sprites/Default"));
                if (matAditiva == null) matAditiva = reserva;
                if (matSuave == null) matSuave = reserva;
            }
            if (matEstrela == null) matEstrela = matAditiva;
        }

        // construtor base
        public static ParticleSystem Novo(string nome, Vector3 pos, Material mat, bool loop, Transform pai = null)
        {
            var go = new GameObject("FX_" + nome);
            if (pai != null) go.transform.SetParent(pai, false);
            go.transform.position = pos;
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.loop = loop;
            main.playOnAwake = false;
            main.duration = loop ? 5f : 1f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.maxParticles = 400;
            main.stopAction = loop ? ParticleSystemStopAction.None : ParticleSystemStopAction.Destroy;

            var emissao = ps.emission;
            emissao.rateOverTime = 0f;

            var forma = ps.shape;
            forma.enabled = true;
            forma.shapeType = ParticleSystemShapeType.Sphere;
            forma.radius = 0.2f;

            var r = go.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = mat != null ? mat : MatAditiva;
            r.renderMode = ParticleSystemRenderMode.Billboard;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            r.minParticleSize = 0f;
            r.maxParticleSize = 2f;
            return ps;
        }

        public static void Rajada(ParticleSystem ps, int quantidade)
        {
            var emissao = ps.emission;
            emissao.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)Mathf.Clamp(quantidade, 1, 30000)) });
        }

        public static void Desbotar(ParticleSystem ps, Color inicio, Color fim, float alfaMeio = 1f)
        {
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(inicio, 0f), new GradientColorKey(fim, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(alfaMeio, 0.08f), new GradientAlphaKey(alfaMeio * 0.8f, 0.6f), new GradientAlphaKey(0f, 1f) });
            col.color = new ParticleSystem.MinMaxGradient(g);
        }

        public static void Tamanho(ParticleSystem ps, float inicio, float meio, float fim)
        {
            var t = ps.sizeOverLifetime;
            t.enabled = true;
            t.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, inicio), new Keyframe(0.35f, meio), new Keyframe(1f, fim)));
        }

        public static void Freio(ParticleSystem ps, float quanto)
        {
            var lim = ps.limitVelocityOverLifetime;
            lim.enabled = true;
            lim.limit = 0.1f;
            lim.dampen = quanto;
        }

        static void Tocar(ParticleSystem ps) { ps.Play(true); }

        // efeitos prontos
        public static void Explosao(Vector3 pos, float escala = 1f)
        {
            // fogo
            var fogo = Novo("Explosao_Fogo", pos, MatAditiva, false);
            var m = fogo.main;
            m.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.7f);
            m.startSpeed = new ParticleSystem.MinMaxCurve(3f * escala, 8f * escala);
            m.startSize = new ParticleSystem.MinMaxCurve(0.6f * escala, 1.4f * escala);
            m.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.85f, 0.3f), new Color(1f, 0.4f, 0.1f));
            Rajada(fogo, Mathf.RoundToInt(28 * escala));
            Freio(fogo, 0.25f);
            Tamanho(fogo, 0.6f, 1f, 0.2f);
            Desbotar(fogo, Color.white, new Color(1f, 0.3f, 0.1f));
            Tocar(fogo);

            // fumaça
            var fumaca = Novo("Explosao_Fumaca", pos, MatSuave, false);
            m = fumaca.main;
            m.startLifetime = new ParticleSystem.MinMaxCurve(0.9f, 1.6f);
            m.startSpeed = new ParticleSystem.MinMaxCurve(1.5f * escala, 4f * escala);
            m.startSize = new ParticleSystem.MinMaxCurve(1.2f * escala, 2.4f * escala);
            m.startColor = new ParticleSystem.MinMaxGradient(new Color(0.35f, 0.33f, 0.32f, 0.8f), new Color(0.55f, 0.52f, 0.5f, 0.7f));
            m.gravityModifier = -0.12f;
            Rajada(fumaca, Mathf.RoundToInt(14 * escala));
            Freio(fumaca, 0.2f);
            Tamanho(fumaca, 0.5f, 1f, 1.3f);
            Desbotar(fumaca, Color.white, Color.white, 0.8f);
            Tocar(fumaca);

            Faiscas(pos, new Color(1f, 0.75f, 0.25f), Mathf.RoundToInt(16 * escala), 9f * escala);
            Anel(pos, new Color(1f, 0.8f, 0.4f, 0.8f), 4f * escala);
        }

        public static void Poeira(Vector3 pos, float escala = 1f, Color? cor = null)
        {
            Color c = cor ?? new Color(0.78f, 0.72f, 0.62f, 0.75f);
            var ps = Novo("Poeira", pos, MatSuave, false);
            var m = ps.main;
            m.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.9f);
            m.startSpeed = new ParticleSystem.MinMaxCurve(1.2f * escala, 3f * escala);
            m.startSize = new ParticleSystem.MinMaxCurve(0.45f * escala, 0.9f * escala);
            m.startColor = c;
            m.gravityModifier = -0.05f;
            var forma = ps.shape;
            forma.shapeType = ParticleSystemShapeType.Circle;
            forma.radius = 0.35f * escala;
            forma.rotation = new Vector3(90f, 0f, 0f);
            Rajada(ps, Mathf.RoundToInt(12 * escala));
            Freio(ps, 0.3f);
            Tamanho(ps, 0.5f, 1f, 1.2f);
            Desbotar(ps, Color.white, Color.white, c.a);
            Tocar(ps);
        }

        public static void Confete(Vector3 pos, int quantidade = 45, float forca = 1f)
        {
            var ps = Novo("Confete", pos, MatSuave, false);
            var m = ps.main;
            m.startLifetime = new ParticleSystem.MinMaxCurve(1.1f, 1.8f);
            m.startSpeed = new ParticleSystem.MinMaxCurve(4f * forca, 8.5f * forca);
            m.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.2f);
            m.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            m.gravityModifier = 0.9f;
            var g = new Gradient();
            g.SetKeys(new[]
            {
                new GradientColorKey(Cores.Vermelho, 0f), new GradientColorKey(Cores.Ouro, 0.25f),
                new GradientColorKey(Cores.Verde, 0.5f), new GradientColorKey(Cores.AzulClaro, 0.75f),
                new GradientColorKey(Cores.Rosa, 1f)
            }, new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            m.startColor = new ParticleSystem.MinMaxGradient(g) { mode = ParticleSystemGradientMode.RandomColor };
            var forma = ps.shape;
            forma.shapeType = ParticleSystemShapeType.Cone;
            forma.angle = 32f;
            forma.radius = 0.2f;
            forma.rotation = new Vector3(-90f, 0f, 0f);
            var rot = ps.rotationOverLifetime;
            rot.enabled = true;
            rot.z = new ParticleSystem.MinMaxCurve(-8f, 8f);
            Freio(ps, 0.12f);
            Rajada(ps, quantidade);
            var cor = ps.colorOverLifetime;
            cor.enabled = true;
            var ga = new Gradient();
            ga.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                       new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.75f), new GradientAlphaKey(0f, 1f) });
            cor.color = new ParticleSystem.MinMaxGradient(ga);
            Tocar(ps);
        }

        public static void Brilhos(Vector3 pos, Color cor, int quantidade = 18, float raio = 0.6f, float subida = 2.5f)
        {
            var ps = Novo("Brilhos", pos, MatEstrela, false);
            var m = ps.main;
            m.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.1f);
            m.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, subida);
            m.startSize = new ParticleSystem.MinMaxCurve(0.18f, 0.42f);
            m.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            m.startColor = new ParticleSystem.MinMaxGradient(cor, Color.Lerp(cor, Color.white, 0.6f));
            m.gravityModifier = -0.15f;
            var forma = ps.shape;
            forma.radius = raio;
            Rajada(ps, quantidade);
            Tamanho(ps, 0.2f, 1f, 0f);
            Desbotar(ps, Color.white, Color.white);
            Tocar(ps);
        }

        public static void Estrelas(Vector3 pos, Color? cor = null)
        {
            Color c = cor ?? Cores.Ouro;
            var ps = Novo("Estrelas", pos, MatEstrela, false);
            var m = ps.main;
            m.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.6f);
            m.startSpeed = new ParticleSystem.MinMaxCurve(4f, 7f);
            m.startSize = new ParticleSystem.MinMaxCurve(0.35f, 0.6f);
            m.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            m.startColor = new ParticleSystem.MinMaxGradient(c, Color.white);
            var forma = ps.shape;
            forma.shapeType = ParticleSystemShapeType.Circle;
            forma.radius = 0.1f;
            forma.rotation = new Vector3(90f, 0f, 0f);
            Freio(ps, 0.35f);
            Rajada(ps, 10);
            Tamanho(ps, 1f, 0.9f, 0f);
            Tocar(ps);
            Anel(pos, new Color(1f, 1f, 1f, 0.7f), 2.2f);
        }

        public static void Anel(Vector3 pos, Color cor, float tamanho = 3f)
        {
            var ps = Novo("Anel", pos, MatAditiva, false);
            var m = ps.main;
            m.startLifetime = 0.28f;
            m.startSpeed = 0f;
            m.startSize = tamanho;
            m.startColor = cor;
            var forma = ps.shape;
            forma.enabled = false;
            Rajada(ps, 1);
            Tamanho(ps, 0.15f, 0.8f, 1f);
            Desbotar(ps, Color.white, Color.white, cor.a);
            Tocar(ps);
        }

        public static void Faiscas(Vector3 pos, Color cor, int quantidade = 14, float velocidade = 7f)
        {
            var ps = Novo("Faiscas", pos, MatAditiva, false);
            var m = ps.main;
            m.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.55f);
            m.startSpeed = new ParticleSystem.MinMaxCurve(velocidade * 0.5f, velocidade);
            m.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.12f);
            m.startColor = cor;
            m.gravityModifier = 1.2f;
            var r = ps.GetComponent<ParticleSystemRenderer>();
            r.renderMode = ParticleSystemRenderMode.Stretch;
            r.velocityScale = 0.06f;
            r.lengthScale = 1.5f;
            Rajada(ps, quantidade);
            Desbotar(ps, Color.white, cor);
            Tocar(ps);
        }

        public static void Fumaca(Vector3 pos, float escala = 1f, Color? cor = null)
        {
            Color c = cor ?? new Color(0.5f, 0.5f, 0.52f, 0.6f);
            var ps = Novo("Fumaca", pos, MatSuave, false);
            var m = ps.main;
            m.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.4f);
            m.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.4f);
            m.startSize = new ParticleSystem.MinMaxCurve(0.5f * escala, 1.1f * escala);
            m.startColor = c;
            m.gravityModifier = -0.2f;
            Rajada(ps, Mathf.RoundToInt(8 * escala));
            Tamanho(ps, 0.4f, 1f, 1.4f);
            Desbotar(ps, Color.white, Color.white, c.a);
            Tocar(ps);
        }

        // Pedacinhos coloridos que caem (doce quebrando, biscoito esfarelando)
        public static void Farelos(Vector3 pos, Color cor, int quantidade = 16)
        {
            var ps = Novo("Farelos", pos, MatSuave, false);
            var m = ps.main;
            m.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1f);
            m.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 4f);
            m.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.2f);
            m.startColor = new ParticleSystem.MinMaxGradient(cor, Color.Lerp(cor, Color.black, 0.25f));
            m.gravityModifier = 1.4f;
            var forma = ps.shape;
            forma.shapeType = ParticleSystemShapeType.Box;
            forma.scale = new Vector3(1f, 0.2f, 1f);
            Rajada(ps, quantidade);
            Tocar(ps);
        }

        // efeitos contínuos (presos a um objeto)
        public static ParticleSystem Feixe(Transform alvo, Color cor, float altura = 7f, float largura = 0.5f)
        {
            var ps = Novo("Feixe", alvo.position, MatAditiva, true, alvo);
            var m = ps.main;
            m.simulationSpace = ParticleSystemSimulationSpace.Local;
            m.startLifetime = 1.4f;
            m.startSpeed = altura / 1.4f;
            m.startSize = new ParticleSystem.MinMaxCurve(largura * 0.8f, largura * 1.6f);
            m.startColor = cor;
            var em = ps.emission;
            em.rateOverTime = 26f;
            var forma = ps.shape;
            forma.shapeType = ParticleSystemShapeType.Circle;
            forma.radius = largura * 0.35f;
            forma.rotation = new Vector3(-90f, 0f, 0f);
            Desbotar(ps, Color.white, Color.white, 0.55f);
            Tamanho(ps, 1f, 0.8f, 0.2f);
            ps.Play();
            return ps;
        }

        public static ParticleSystem Aura(Transform alvo, Color cor, float raio = 0.8f, float taxa = 10f)
        {
            var ps = Novo("Aura", alvo.position, MatEstrela, true, alvo);
            var m = ps.main;
            m.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.3f);
            m.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.7f);
            m.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.3f);
            m.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            m.startColor = new ParticleSystem.MinMaxGradient(cor, Color.Lerp(cor, Color.white, 0.5f));
            m.gravityModifier = -0.08f;
            var em = ps.emission;
            em.rateOverTime = taxa;
            var forma = ps.shape;
            forma.radius = raio;
            Tamanho(ps, 0.2f, 1f, 0f);
            Desbotar(ps, Color.white, Color.white);
            ps.Play();
            return ps;
        }

        // Redemoinho de partículas girando para dentro de um disco (portal)
        public static ParticleSystem Vortice(Transform pai, Color cor, float raio = 2f, float taxa = 60f)
        {
            var ps = Novo("Vortice", pai.position, MatEstrela, true, pai);
            ps.transform.localRotation = Quaternion.identity;
            var m = ps.main;
            m.simulationSpace = ParticleSystemSimulationSpace.Local;
            m.startLifetime = 0.9f;
            m.startSpeed = 0f;
            m.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.28f);
            m.startColor = new ParticleSystem.MinMaxGradient(cor, Color.white);
            var em = ps.emission;
            em.rateOverTime = taxa;
            var forma = ps.shape;
            forma.shapeType = ParticleSystemShapeType.Circle;
            forma.radius = raio;
            forma.radiusThickness = 0.15f;
            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.Local;
            // todas as curvas de um mesmo grupo precisam estar no mesmo modo
            vel.x = new ParticleSystem.MinMaxCurve(0f, 0f);
            vel.y = new ParticleSystem.MinMaxCurve(0f, 0f);
            vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);
            vel.orbitalX = new ParticleSystem.MinMaxCurve(0f, 0f);
            vel.orbitalY = new ParticleSystem.MinMaxCurve(0f, 0f);
            vel.orbitalZ = new ParticleSystem.MinMaxCurve(2.6f, 4f);
            vel.radial = new ParticleSystem.MinMaxCurve(-raio / 0.9f, -raio / 0.9f);
            Tamanho(ps, 0.4f, 1f, 0.1f);
            Desbotar(ps, Color.white, cor);
            ps.Play();
            return ps;
        }

        public static ParticleSystem Rastro(Transform alvo, Color cor)
        {
            var ps = Novo("Rastro", alvo.position, MatAditiva, true, alvo);
            var m = ps.main;
            m.startLifetime = 0.35f;
            m.startSpeed = 0f;
            m.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.45f);
            m.startColor = cor;
            var em = ps.emission;
            em.rateOverTime = 0f;
            em.rateOverDistance = 9f;
            var forma = ps.shape;
            forma.radius = 0.25f;
            Desbotar(ps, Color.white, Color.white, 0.6f);
            Tamanho(ps, 1f, 0.7f, 0f);
            ps.Play();
            return ps;
        }
    }

    public static class Cores
    {
        public static readonly Color AzulArcor = new Color(0.00f, 0.33f, 0.64f);
        public static readonly Color AzulClaro = new Color(0.30f, 0.65f, 1.00f);
        public static readonly Color Ouro = new Color(1.00f, 0.80f, 0.20f);
        public static readonly Color Vermelho = new Color(0.92f, 0.20f, 0.18f);
        public static readonly Color Verde = new Color(0.30f, 0.85f, 0.35f);
        public static readonly Color Rosa = new Color(1.00f, 0.42f, 0.62f);
        public static readonly Color Roxo = new Color(0.62f, 0.32f, 0.95f);
        public static readonly Color Laranja = new Color(1.00f, 0.55f, 0.12f);
        public static readonly Color Chocolate = new Color(0.42f, 0.23f, 0.12f);
        public static readonly Color Creme = new Color(0.97f, 0.92f, 0.80f);
        public static readonly Color Tinta = new Color(0.01f, 0.09f, 0.22f);
    }
}
