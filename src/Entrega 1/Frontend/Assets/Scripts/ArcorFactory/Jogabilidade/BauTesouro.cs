using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ArcorFactory
{
    // Baú de tesouro com raridade (Comum, Raro, Épico, Lendário)
    public class BauTesouro : MonoBehaviour
    {
        public Raridade raridade = Raridade.Comum;
        public Transform tampa;
        public float distanciaInteracao = 2.3f;
        public float anguloAberto = 108f;

        bool aberto;
        Light luz;
        ParticleSystem aura, feixe;
        Collider colisor;
        float fase;
        Quaternion tampaFechada;

        public static Color CorDe(Raridade r)
        {
            switch (r)
            {
                case Raridade.Raro: return new Color(0.25f, 0.62f, 1f);
                case Raridade.Epico: return new Color(0.72f, 0.35f, 1f);
                case Raridade.Lendario: return new Color(1f, 0.78f, 0.15f);
                default: return new Color(0.95f, 0.85f, 0.65f);
            }
        }

        public static string NomeDe(Raridade r)
        {
            switch (r)
            {
                case Raridade.Raro: return "BAÚ RARO";
                case Raridade.Epico: return "BAÚ ÉPICO";
                case Raridade.Lendario: return "BAÚ LENDÁRIO";
                default: return "BAÚ COMUM";
            }
        }

        void Start()
        {
            colisor = GetComponent<Collider>();
            if (tampa != null) tampaFechada = tampa.localRotation;
            EstatisticasFase.RegistrarBau();
            fase = Random.value * 10f;

            Color cor = CorDe(raridade);
            if (raridade >= Raridade.Raro)
            {
                var go = new GameObject("Luz_Bau");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, 1.1f, -0.6f);
                luz = go.AddComponent<Light>();
                luz.type = LightType.Point;
                luz.color = cor;
                luz.range = raridade == Raridade.Lendario ? 7f : 4.5f;
                luz.intensity = raridade == Raridade.Lendario ? 2.6f : 1.6f;
                luz.shadows = LightShadows.None;
                aura = Efeitos.Aura(transform, cor, 0.75f, raridade == Raridade.Raro ? 4f : 9f);
                aura.transform.localPosition = Vector3.up * 0.6f;
            }
            if (raridade == Raridade.Lendario)
            {
                feixe = Efeitos.Feixe(transform, new Color(1f, 0.85f, 0.3f, 0.8f), 9f, 0.7f);
                feixe.transform.localPosition = Vector3.up * 0.5f;
            }
        }

        void Update()
        {
            if (aberto) return;
            fase += Time.deltaTime;
            if (luz != null) luz.intensity = (raridade == Raridade.Lendario ? 2.6f : 1.6f) * (0.8f + Mathf.Sin(fase * 3f) * 0.2f);

            var ataque = AtaqueJogador.Instancia;
            if (ataque == null || colisor == null) return;

            Bounds b = colisor.bounds;
            float distancia = Vector3.Distance(ataque.transform.position, transform.position);
            bool perto = distancia <= distanciaInteracao;
            if (perto) HUDArcor.MostrarDica(this, "E", "Abrir " + NomeDe(raridade).ToLower());
            else HUDArcor.EsconderDica(this);

            var k = Keyboard.current;
            if ((perto && k != null && k.eKey.wasPressedThisFrame) || ataque.GiroAcerta(b) || ataque.PisouEm(b))
            {
                if (ataque.PisouEm(b)) ataque.Quicar(6f);
                Abrir();
            }
        }

        public void Abrir()
        {
            if (aberto) return;
            aberto = true;
            HUDArcor.EsconderDica(this);
            StartCoroutine(Sequencia());
        }

        IEnumerator Sequencia()
        {
            Color cor = CorDe(raridade);
            Vector3 basePos = transform.position;
            int nivel = (int)raridade;

            // tremida de expectativa (mais longa nos raros)
            float tremer = 0.18f + nivel * 0.12f;
            Sons.TocarEm("bau_tremer", basePos, 0.7f);
            float t = 0f;
            while (t < tremer)
            {
                t += Time.deltaTime;
                float f = 0.025f + nivel * 0.012f;
                transform.position = basePos + new Vector3(Mathf.Sin(t * 85f) * f, Mathf.Abs(Mathf.Sin(t * 40f)) * f, 0f);
                yield return null;
            }
            transform.position = basePos;

            // abre
            Sons.TocarEm(raridade >= Raridade.Epico ? "bau_abrir_raro" : "bau_abrir", basePos, 1f, 0.03f);
            Vector3 topo = basePos + Vector3.up * 0.9f;
            Efeitos.Brilhos(topo, cor, 18 + nivel * 10, 0.5f, 4f + nivel);
            Efeitos.Anel(topo, new Color(cor.r, cor.g, cor.b, 0.9f), 3f + nivel);
            if (raridade >= Raridade.Epico) Efeitos.Confete(topo, 30 + nivel * 15);
            var cam = FindFirstObjectByType<CameraCrash>();
            if (cam != null) cam.Tremer(0.05f + nivel * 0.04f, 0.25f);
            if (luz != null) StartCoroutine(Clarao());

            // recompensas
            var inv = AtaqueJogador.Instancia != null ? AtaqueJogador.Instancia.Inventario : FindFirstObjectByType<InventarioJogador>();
            int qtd = Quantidade();
            string detalhes = "+" + qtd + " Rocklets";
            if (inv != null) SoltarRocklets(inv, qtd, topo);
            EstatisticasFase.Bonus(qtd);

            bool ganhaVida = raridade == Raridade.Lendario || (raridade == Raridade.Epico && Random.value < 0.6f) || (raridade == Raridade.Raro && Random.value < 0.35f);
            if (ganhaVida && inv != null)
            {
                if (inv.GanharVida()) detalhes += "  +1 Casco";
                else ganhaVida = false;
            }

            EstatisticasFase.BauAberto(raridade);
            HUDArcor.Notificar(NomeDe(raridade) + "!", detalhes, cor);
            TextoFlutuante.Criar(topo + Vector3.up * 0.8f, "+" + qtd, Cores.Ouro, 4.5f);

            // tampa com mola
            if (tampa != null)
            {
                t = 0f;
                while (t < 0.5f)
                {
                    t += Time.deltaTime;
                    float k = Mathf.Clamp01(t / 0.5f);
                    float e = 1f - Mathf.Pow(1f - k, 3f);
                    float mola = 1f + Mathf.Sin(k * Mathf.PI) * 0.15f * k;
                    tampa.localRotation = tampaFechada * Quaternion.Euler(anguloAberto * e * mola, 0f, 0f);
                    yield return null;
                }
                tampa.localRotation = tampaFechada * Quaternion.Euler(anguloAberto, 0f, 0f);
            }

            if (aura != null) { var em = aura.emission; em.rateOverTime = 2f; }
            if (feixe != null) { feixe.Stop(); Destroy(feixe.gameObject, 2f); }
        }

        IEnumerator Clarao()
        {
            float baseI = luz.intensity;
            float t = 0f;
            while (t < 0.9f)
            {
                t += Time.deltaTime;
                float k = t / 0.9f;
                luz.intensity = Mathf.Lerp(baseI * 4f, baseI * 0.4f, k);
                yield return null;
            }
        }

        int Quantidade()
        {
            switch (raridade)
            {
                case Raridade.Raro: return Random.Range(6, 10);
                case Raridade.Epico: return Random.Range(10, 15);
                case Raridade.Lendario: return Random.Range(16, 23);
                default: return Random.Range(3, 6);
            }
        }

        void SoltarRocklets(InventarioJogador inv, int quantidade, Vector3 de)
        {
            var cfg = ConfigArcor.Instancia;
            for (int i = 0; i < quantidade; i++)
            {
                GameObject prefab = cfg.RockletAleatorio();
                if (prefab == null) { inv.SomarRocklets(quantidade - i); return; }
                Vector3 desvio = new Vector3(Random.Range(-0.6f, 0.6f), Random.Range(0f, 0.4f), Random.Range(-0.6f, 0.6f));
                GameObject r = Instantiate(prefab, de + desvio, Quaternion.identity);
                foreach (var c in r.GetComponentsInChildren<Collider>()) c.enabled = false;
                inv.ColetarVoando(r.transform, 0.12f + i * 0.06f);
            }
        }
    }
}
