using UnityEngine;

namespace ArcorFactory
{
    // Selo Arcor: moeda dourada escondida (3 por fase)
    public class SeloArcor : MonoBehaviour
    {
        public string id = "selo_1";
        public float raioColeta = 1.3f;

        bool pego, jaTinha;
        Vector3 inicio;
        Renderer[] partes;
        Light luz;
        ParticleSystem aura;

        string Chave { get { return "arcor_selo_" + EstatisticasFase.Cena + "_" + id; } }

        void Start()
        {
            inicio = transform.position;
            partes = GetComponentsInChildren<Renderer>();
            jaTinha = PlayerPrefs.GetInt(Chave, 0) == 1;
            EstatisticasFase.RegistrarSelo();

            var go = new GameObject("Luz_Selo");
            go.transform.SetParent(transform, false);
            luz = go.AddComponent<Light>();
            luz.type = LightType.Point;
            luz.color = jaTinha ? new Color(0.6f, 0.75f, 1f) : Cores.Ouro;
            luz.range = 4f;
            luz.intensity = jaTinha ? 0.6f : 1.8f;
            luz.shadows = LightShadows.None;
            aura = Efeitos.Aura(transform, jaTinha ? new Color(0.6f, 0.75f, 1f, 0.5f) : Cores.Ouro, 0.6f, jaTinha ? 2f : 7f);

            if (jaTinha)
            {
                var bloco = new MaterialPropertyBlock();
                bloco.SetColor("_CorPiscar", new Color(0.55f, 0.75f, 1f, 0.65f));
                foreach (var r in partes) r.SetPropertyBlock(bloco);
            }
        }

        void Update()
        {
            if (pego) return;
            float t = Time.time;
            transform.position = inicio + Vector3.up * (Mathf.Sin(t * 2.2f) * 0.18f);
            transform.rotation = Quaternion.Euler(0f, t * 120f, 0f);

            var ataque = AtaqueJogador.Instancia;
            if (ataque != null && Vector3.Distance(ataque.Centro, transform.position) <= raioColeta) Coletar();
        }

        void Coletar()
        {
            pego = true;
            PlayerPrefs.SetInt(Chave, 1);
            PlayerPrefs.Save();
            EstatisticasFase.SeloColetado();
            Sons.Tocar("selo", 0.9f, 0f);
            Efeitos.Brilhos(transform.position, Cores.Ouro, 40, 0.6f, 5f);
            Efeitos.Anel(transform.position, new Color(1f, 0.9f, 0.4f, 0.9f), 4.5f);
            Efeitos.Confete(transform.position, 25, 0.8f);
            string sub = "Selo " + EstatisticasFase.SelosColetados + " de " + EstatisticasFase.SelosTotal + (jaTinha ? " (já era seu)" : "");
            HUDArcor.Notificar("SELO ARCOR!", sub, Cores.Ouro);
            TextoFlutuante.Criar(transform.position + Vector3.up, "SELO!", Cores.Ouro, 5f);
            if (EstatisticasFase.SelosColetados >= EstatisticasFase.SelosTotal)
            {
                HUDArcor.Notificar("TODOS OS SELOS!", "Você achou todos os Selos desta fase", Cores.Ouro, 4f);
            }
            Destroy(gameObject);
        }
    }
}
