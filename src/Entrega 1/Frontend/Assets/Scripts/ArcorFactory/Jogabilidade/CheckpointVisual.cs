using UnityEngine;

namespace ArcorFactory
{
    // Visual do checkpoint
    public class CheckpointVisual : MonoBehaviour
    {
        public Transform bandeira;
        public float alturaBaixa = 0.6f, alturaAlta = 2.25f;
        public Renderer lampada;
        public Vector3 pontoLuz;
        public CheckpointFase checkpoint;

        bool ativo;
        float tempoAtivo = -1f;
        Light luz;
        MaterialPropertyBlock bloco;
        Vector3 posBandeira;
        Quaternion rotBandeira;

        static readonly Color Ambar = new Color(1f, 0.62f, 0.15f);
        static readonly Color VerdeAtivo = new Color(0.3f, 1f, 0.45f);

        void Start()
        {
            if (checkpoint == null)
            {
                // o CheckpointFase mais perto (o visual é colocado em cima dele)
                float melhor = 9f;
                foreach (var c in FindObjectsByType<CheckpointFase>(FindObjectsSortMode.None))
                {
                    float d = (c.transform.position - transform.position).sqrMagnitude;
                    if (d < melhor) { melhor = d; checkpoint = c; }
                }
            }
            bloco = new MaterialPropertyBlock();
            if (bandeira != null)
            {
                posBandeira = bandeira.localPosition;
                rotBandeira = bandeira.localRotation;
            }
            var go = new GameObject("Luz_Checkpoint");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = pontoLuz;
            luz = go.AddComponent<Light>();
            luz.type = LightType.Point;
            luz.range = 4.5f;
            luz.intensity = 1f;
            luz.color = Ambar;
            luz.shadows = LightShadows.None;
            if (checkpoint != null && checkpoint.Ativado) Ativar(false);
        }

        void Update()
        {
            if (!ativo && checkpoint != null && checkpoint.Ativado) Ativar(true);

            float t = Time.time;
            if (!ativo)
            {
                // esperando: lâmpada âmbar piscando devagar, bandeira baixa balançando
                float p = 0.5f + 0.5f * Mathf.Sin(t * 3f);
                luz.intensity = 0.4f + p * 0.9f;
                Tingir(Ambar, 0.55f + p * 0.3f);
                if (bandeira != null)
                {
                    bandeira.localPosition = new Vector3(posBandeira.x, alturaBaixa, posBandeira.z);
                    bandeira.localRotation = rotBandeira * Quaternion.Euler(0f, Mathf.Sin(t * 1.6f) * 8f, 0f);
                }
                return;
            }

            float idade = t - tempoAtivo;
            luz.intensity = 1.6f + Mathf.Sin(t * 2f) * 0.3f + Mathf.Max(0f, 3f - idade * 4f);
            Tingir(VerdeAtivo, 0.85f);
            if (bandeira != null)
            {
                float k = Mathf.Clamp01(idade / 0.9f);
                float e = 1f - Mathf.Pow(1f - k, 3f);
                float y = Mathf.Lerp(alturaBaixa, alturaAlta, e);
                bandeira.localPosition = new Vector3(posBandeira.x, y, posBandeira.z);
                float giro = (1f - e) * 720f;
                bandeira.localRotation = rotBandeira * Quaternion.Euler(0f, giro + Mathf.Sin(t * 2.2f) * 12f, Mathf.Sin(t * 3.1f) * 2f);
            }
        }

        void Tingir(Color cor, float forca)
        {
            if (lampada == null) return;
            lampada.GetPropertyBlock(bloco);
            bloco.SetColor("_CorPiscar", new Color(cor.r, cor.g, cor.b, forca));
            lampada.SetPropertyBlock(bloco);
        }

        void Ativar(bool comFesta)
        {
            ativo = true;
            tempoAtivo = comFesta ? Time.time : Time.time - 10f;
            luz.color = VerdeAtivo;
            if (!comFesta) return;
            Vector3 topo = transform.TransformPoint(pontoLuz);
            Efeitos.Confete(transform.position + Vector3.up * 1.5f, 40, 0.9f);
            Efeitos.Brilhos(topo, VerdeAtivo, 24, 0.4f, 3f);
            Efeitos.Anel(transform.position + Vector3.up * 0.2f, new Color(0.4f, 1f, 0.6f, 0.8f), 3.5f);
            var feixe = Efeitos.Feixe(transform, new Color(0.45f, 1f, 0.6f, 0.6f), 6f, 0.9f);
            Destroy(feixe.gameObject, 2.5f);
            var em = feixe.emission;
            em.rateOverTime = 40f;
            Sons.TocarEm("estrela", transform.position, 0.6f, 0f);
            HUDArcor.Notificar("CHECKPOINT!", "Você volta daqui se cair", VerdeAtivo, 2.6f);
        }
    }
}
