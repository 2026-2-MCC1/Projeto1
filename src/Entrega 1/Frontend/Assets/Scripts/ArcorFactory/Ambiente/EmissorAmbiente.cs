using UnityEngine;

namespace ArcorFactory
{
    // Partículas de ambiente que dão vida ao cenário
    public class EmissorAmbiente : MonoBehaviour
    {
        public enum Tipo { Vapor, Vagalumes, Folhas, Faiscas, GotasChocolate, Borboletas, Bolhas, PoeiraNaLuz, FumacaChamine, RespingosRio, Petalas, Confeitos }

        public Tipo tipo = Tipo.Vapor;
        public Vector3 area = new Vector3(4f, 2f, 4f);
        public float intensidade = 1f;

        ParticleSystem ps;
        float proximaFaisca;

        void Start()
        {
            ps = Efeitos.Novo(tipo.ToString(), transform.position, Efeitos.MatSuave, true, transform);
            ps.transform.localPosition = Vector3.zero;
            ps.transform.localRotation = Quaternion.identity;
            var m = ps.main;
            var em = ps.emission;
            var forma = ps.shape;
            var r = ps.GetComponent<ParticleSystemRenderer>();
            forma.shapeType = ParticleSystemShapeType.Box;
            forma.scale = area;
            m.prewarm = true;
            m.duration = 5f;

            switch (tipo)
            {
                case Tipo.Vapor:
                    m.startLifetime = new ParticleSystem.MinMaxCurve(1.6f, 2.6f);
                    m.startSpeed = new ParticleSystem.MinMaxCurve(1.2f, 2.2f);
                    m.startSize = new ParticleSystem.MinMaxCurve(0.5f, 1f);
                    m.startColor = new Color(1f, 1f, 1f, 0.45f);
                    em.rateOverTime = 9f * intensidade;
                    forma.shapeType = ParticleSystemShapeType.Cone;
                    forma.angle = 12f;
                    forma.radius = 0.15f;
                    forma.rotation = new Vector3(-90f, 0f, 0f);
                    Efeitos.Tamanho(ps, 0.4f, 1.2f, 2.2f);
                    Efeitos.Desbotar(ps, Color.white, Color.white, 0.45f);
                    Ruido(0.6f, 0.6f);
                    break;

                case Tipo.FumacaChamine:
                    m.startLifetime = new ParticleSystem.MinMaxCurve(5f, 8f);
                    m.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 2.5f);
                    m.startSize = new ParticleSystem.MinMaxCurve(2f, 3.5f);
                    m.startColor = new Color(0.85f, 0.85f, 0.88f, 0.5f);
                    em.rateOverTime = 3f * intensidade;
                    forma.shapeType = ParticleSystemShapeType.Cone;
                    forma.angle = 8f;
                    forma.radius = 0.6f;
                    forma.rotation = new Vector3(-90f, 0f, 0f);
                    var vel = ps.velocityOverLifetime;
                    vel.enabled = true;
                    vel.space = ParticleSystemSimulationSpace.World;
                    vel.x = new ParticleSystem.MinMaxCurve(0.6f, 1.2f);
                    vel.y = new ParticleSystem.MinMaxCurve(0f, 0f);
                    vel.z = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
                    Efeitos.Tamanho(ps, 0.5f, 1.4f, 2.6f);
                    Efeitos.Desbotar(ps, Color.white, new Color(0.8f, 0.8f, 0.85f), 0.5f);
                    break;

                case Tipo.Vagalumes:
                    r.sharedMaterial = Efeitos.MatAditiva;
                    m.startLifetime = new ParticleSystem.MinMaxCurve(3f, 6f);
                    m.startSpeed = 0.1f;
                    m.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.16f);
                    m.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.95f, 0.5f), new Color(0.7f, 1f, 0.5f));
                    em.rateOverTime = 4f * intensidade;
                    Ruido(0.8f, 0.4f);
                    Efeitos.Desbotar(ps, Color.white, Color.white);
                    break;

                case Tipo.PoeiraNaLuz:
                    r.sharedMaterial = Efeitos.MatAditiva;
                    m.startLifetime = new ParticleSystem.MinMaxCurve(5f, 9f);
                    m.startSpeed = 0.05f;
                    m.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.07f);
                    m.startColor = new Color(1f, 0.92f, 0.75f, 0.6f);
                    em.rateOverTime = 6f * intensidade;
                    Ruido(0.15f, 0.2f);
                    Efeitos.Desbotar(ps, Color.white, Color.white, 0.7f);
                    break;

                case Tipo.Folhas:
                case Tipo.Petalas:
                    bool petala = tipo == Tipo.Petalas;
                    m.startLifetime = new ParticleSystem.MinMaxCurve(5f, 8f);
                    m.startSpeed = 0.2f;
                    m.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.25f);
                    m.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
                    m.startColor = petala
                        ? new ParticleSystem.MinMaxGradient(new Color(1f, 0.6f, 0.8f), new Color(1f, 0.85f, 0.9f))
                        : new ParticleSystem.MinMaxGradient(new Color(0.35f, 0.65f, 0.2f), new Color(0.85f, 0.6f, 0.15f));
                    m.gravityModifier = 0.03f;
                    em.rateOverTime = 2.5f * intensidade;
                    var rot = ps.rotationOverLifetime;
                    rot.enabled = true;
                    rot.z = new ParticleSystem.MinMaxCurve(-2f, 2f);
                    Ruido(0.9f, 0.3f);
                    Efeitos.Desbotar(ps, Color.white, Color.white);
                    break;

                case Tipo.Borboletas:
                    m.startLifetime = new ParticleSystem.MinMaxCurve(6f, 10f);
                    m.startSpeed = 0.3f;
                    m.startSize = new ParticleSystem.MinMaxCurve(0.18f, 0.3f);
                    m.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.85f, 0.2f), new Color(0.4f, 0.75f, 1f)) { mode = ParticleSystemGradientMode.TwoColors };
                    em.rateOverTime = 0.8f * intensidade;
                    var tam = ps.sizeOverLifetime;
                    tam.enabled = true;
                    tam.separateAxes = true;
                    var bater = new AnimationCurve();
                    for (int i = 0; i <= 40; i++) bater.AddKey(i / 40f, i % 2 == 0 ? 1f : 0.25f);
                    tam.x = new ParticleSystem.MinMaxCurve(1f, bater);
                    tam.y = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Constant(0f, 1f, 1f));
                    tam.z = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Constant(0f, 1f, 1f));
                    Ruido(1.4f, 0.35f);
                    Efeitos.Desbotar(ps, Color.white, Color.white);
                    break;

                case Tipo.Faiscas:
                    r.sharedMaterial = Efeitos.MatAditiva;
                    r.renderMode = ParticleSystemRenderMode.Stretch;
                    r.velocityScale = 0.05f;
                    r.lengthScale = 1.5f;
                    m.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.7f);
                    m.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
                    m.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.08f);
                    m.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.85f, 0.4f), new Color(1f, 0.5f, 0.15f));
                    m.gravityModifier = 1.4f;
                    em.rateOverTime = 0f;
                    em.SetBursts(new[] { new ParticleSystem.Burst(0f, 6, 14, 1, 1.6f) });
                    forma.shapeType = ParticleSystemShapeType.Cone;
                    forma.angle = 35f;
                    forma.radius = 0.05f;
                    m.prewarm = false;
                    break;

                case Tipo.GotasChocolate:
                    m.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.2f);
                    m.startSpeed = 0f;
                    m.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.18f);
                    m.startColor = new Color(0.36f, 0.19f, 0.09f, 1f);
                    m.gravityModifier = 1f;
                    em.rateOverTime = 3f * intensidade;
                    r.renderMode = ParticleSystemRenderMode.Stretch;
                    r.velocityScale = 0.08f;
                    r.lengthScale = 1.2f;
                    break;

                case Tipo.Bolhas:
                    m.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 3f);
                    m.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.6f);
                    m.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.45f);
                    m.startColor = new Color(0.45f, 0.25f, 0.12f, 0.9f);
                    em.rateOverTime = 4f * intensidade;
                    forma.scale = new Vector3(area.x, 0.05f, area.z);
                    Efeitos.Tamanho(ps, 0.2f, 1f, 1.3f);
                    Efeitos.Desbotar(ps, Color.white, Color.white, 0.9f);
                    break;

                case Tipo.RespingosRio:
                    m.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1f);
                    m.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3f);
                    m.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.25f);
                    m.startColor = new Color(0.5f, 0.28f, 0.14f, 0.9f);
                    m.gravityModifier = 0.9f;
                    em.rateOverTime = 8f * intensidade;
                    forma.scale = new Vector3(area.x, 0.05f, area.z);
                    break;

                case Tipo.Confeitos:
                    m.startLifetime = new ParticleSystem.MinMaxCurve(4f, 7f);
                    m.startSpeed = 0.15f;
                    m.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.12f);
                    m.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
                    var g = new Gradient();
                    g.SetKeys(new[] { new GradientColorKey(Cores.Rosa, 0f), new GradientColorKey(Cores.AzulClaro, 0.33f), new GradientColorKey(Cores.Ouro, 0.66f), new GradientColorKey(Cores.Verde, 1f) },
                              new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
                    m.startColor = new ParticleSystem.MinMaxGradient(g) { mode = ParticleSystemGradientMode.RandomColor };
                    m.gravityModifier = 0.02f;
                    em.rateOverTime = 3f * intensidade;
                    Ruido(0.5f, 0.3f);
                    Efeitos.Desbotar(ps, Color.white, Color.white);
                    break;
            }
            ps.Play();
        }

        void Ruido(float forca, float frequencia)
        {
            var n = ps.noise;
            n.enabled = true;
            n.strength = forca;
            n.frequency = frequencia;
            n.scrollSpeed = 0.2f;
            n.quality = ParticleSystemNoiseQuality.Medium;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.8f, 0.3f, 0.4f);
            Gizmos.DrawWireCube(transform.position, area);
        }
    }
}
