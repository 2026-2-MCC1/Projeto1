
using UnityEngine;
using UnityEngine.UI;

namespace FactoryEscape
{
    // =====================================================================
    //  LevelHUD - coloque UM em cada cena de FASE do seu jogo.
    //  Mostra o nome da fase no início e faz o texto desaparecer sozinho.
    // =====================================================================
    public class LevelHUD : MonoBehaviour
    {
        // Fonte personalizada (opcional)
        public Font customFont;

        // Ativa ou desativa a apresentação do nome da fase
        public bool showBanner = true;

        // Tempo que o nome da fase permanece visível
        public float bannerSeconds = 2.8f;

        // Referência ao texto de apresentação
        Text banner;

        // Contador de tempo do banner
        float timer;

        // =================================================================
        //  INICIALIZAÇÃO
        // =================================================================
        void Awake()
        {
            // Garante que o jogo comece com o tempo normal
            Time.timeScale = 1f;

            // Aplica o volume configurado pelo jogador
            GameFlow.ApplyVolume();

            // Identifica automaticamente a fase atual
            GameFlow.SyncLevelWithScene(gameObject.scene.name);

            // Inicializa os elementos visuais
            FEKit.Init(customFont);

            // Cria o Canvas da interface
            var canvas = FEKit.CreateCanvas("HUD Canvas", 50);
            canvas.transform.SetParent(transform, false);

            // Cria o painel principal
            var root = FEKit.Panel(canvas.transform, "HUD");

            // Obtém o número da fase atual
            int lv = GameFlow.Level;

            // Exibe o nome da fase apenas no início
            if (showBanner)
            {
                banner = FEKit.MakeText(
                    root,
                    "FASE " + lv + "\n" + GameFlow.LevelName(lv),
                    110,
                    FEKit.Sun,
                    FEKit.TL,
                    new Vector2(120, -420),
                    TextAnchor.UpperLeft,
                    FontStyle.Bold,
                    1500f
                );

                // Inicia a contagem regressiva
                timer = bannerSeconds;
            }
        }

        // =================================================================
        //  ATUALIZAÇÃO DO BANNER
        // =================================================================
        void Update()
        {
            // Verifica se o banner ainda está visível
            if (banner != null && timer > 0f)
            {
                // Diminui o tempo de exibição
                timer -= Time.unscaledDeltaTime;

                // Controla a transparência do texto
                Color c = banner.color;
                c.a = Mathf.Clamp01(timer / 0.8f);
                banner.color = c;

                // Esconde o texto quando o tempo termina
                if (timer <= 0f)
                {
                    banner.gameObject.SetActive(false);
                }
            }
        }

        // =================================================================
        //  VITÓRIA E DERROTA
        // =================================================================

        // Método para finalizar a fase com vitória
        public void Win()
        {
            GameFlow.Victory();
        }

        // Método para finalizar a fase com derrota
        public void Lose()
        {
            GameFlow.GameOver();
        }
    }
}
