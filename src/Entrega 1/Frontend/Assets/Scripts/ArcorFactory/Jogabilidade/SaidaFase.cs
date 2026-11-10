using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcorFactory
{
    // Saída da fase (portal, porta, elevador...)
    [RequireComponent(typeof(Collider))]
    public class SaidaFase : MonoBehaviour
    {
        public int metaRocklets = 0;
        public string titulo = "FASE CONCLUÍDA!";
        public float espera = 0.9f;
        [Tooltip("Partes que acendem quando a saída libera (opcional).")]
        public Renderer[] partesAcender;
        public Color corLiberada = new Color(0.35f, 1f, 0.6f);

        private bool usado;
        private bool liberada;
        private float proximoAviso;
        private InventarioJogador inventario;
        private MaterialPropertyBlock bloco;

        void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        void Start()
        {
            inventario = FindFirstObjectByType<InventarioJogador>();
            bloco = new MaterialPropertyBlock();
            if (metaRocklets <= 0) Liberar(false);
        }

        void Update()
        {
            if (!liberada && inventario != null && inventario.Rocklets >= metaRocklets)
            {
                Liberar(true);
            }
        }

        void Liberar(bool avisar)
        {
            liberada = true;
            Efeitos.Feixe(transform, corLiberada, 9f, 0.9f);
            Efeitos.Aura(transform, corLiberada, 2.2f, 18f);
            if (partesAcender != null)
            {
                foreach (Renderer r in partesAcender)
                {
                    if (r == null) continue;
                    r.GetPropertyBlock(bloco);
                    bloco.SetColor("_EmissionColor", corLiberada * 2f);
                    bloco.SetColor("_CorPiscar", new Color(corLiberada.r, corLiberada.g, corLiberada.b, 0.35f));
                    r.SetPropertyBlock(bloco);
                }
            }
            if (avisar)
            {
                Sons.Tocar("saida_liberada", 0.8f, 0f);
                HUDArcor.Notificar("SAÍDA LIBERADA", "Encontre o portal para concluir a fase", corLiberada, 3.5f);
            }
        }

        void OnTriggerEnter(Collider outro)
        {
            if (usado) return;
            InventarioJogador inv = outro.GetComponentInParent<InventarioJogador>();
            if (inv == null) return;

            if (!liberada)
            {
                if (Time.time >= proximoAviso)
                {
                    proximoAviso = Time.time + 2f;
                    int faltam = Mathf.Max(0, metaRocklets - inv.Rocklets);
                    HUDArcor.Notificar("SAÍDA TRANCADA", "Faltam " + faltam + " Rocklets", Cores.Vermelho, 2.2f);
                    Sons.Tocar("ui_tique", 0.6f, 0f);
                }
                return;
            }

            usado = true;
            InventarioJogador.CongelarJogador();
            HUDJogo hud = FindFirstObjectByType<HUDJogo>();
            if (hud != null) hud.PararTempo();
            Efeitos.Confete(transform.position + Vector3.up * 2f, 80, 1.4f);
            Efeitos.Anel(transform.position + Vector3.up * 0.3f, corLiberada, 6f);
            StartCoroutine(Concluir());
        }

        IEnumerator Concluir()
        {
            yield return new WaitForSeconds(espera);
            HUDArcor.MostrarResumoEContinuar(() =>
            {
                FactoryEscape.GameFlow.SyncLevelWithScene(SceneManager.GetActiveScene().name);
                FactoryEscape.GameFlow.Victory();
            });
        }
    }
}
