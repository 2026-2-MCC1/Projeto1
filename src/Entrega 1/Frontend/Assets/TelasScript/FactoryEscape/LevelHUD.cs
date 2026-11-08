using UnityEngine;
using UnityEngine.UI;

namespace FactoryEscape
{
    // =====================================================================
    //  LevelHUD - coloque UM em cada cena de FASE do seu jogo.
    //  Mostra "FASE 2 / 3 - NOME" no topo e uma faixa grande com o nome da
    //  fase que some sozinha. Também tem as teclas de teste G e V.
    // =====================================================================
    public class LevelHUD : MonoBehaviour
    {
        public Font customFont;
        public bool showBanner = true;
        public float bannerSeconds = 2.8f;
        public bool debugKeys = true;        // G = game over, V = vitória (desmarque ao entregar)

        Text banner;
        float timer;

        void Awake()
        {
            Time.timeScale = 1f;
            GameFlow.ApplyVolume();
            GameFlow.SyncLevelWithScene(gameObject.scene.name);   // Play direto na fase também funciona
            FEKit.Init(customFont);

            var canvas = FEKit.CreateCanvas("HUD Canvas", 50);
            canvas.transform.SetParent(transform, false);
            var root = FEKit.Panel(canvas.transform, "HUD");

            int lv = GameFlow.Level;
            FEKit.MakeText(root, "FASE " + lv + " / " + GameFlow.TotalLevels + "  -  " + GameFlow.LevelName(lv),
                           38, FEKit.Sun, FEKit.TL, new Vector2(40, -30),
                           TextAnchor.UpperLeft, FontStyle.Bold, 1400f);

            if (showBanner)
            {
                banner = FEKit.MakeText(root, "FASE " + lv + "\n" + GameFlow.LevelName(lv), 110, FEKit.Sun,
                                        FEKit.TL, new Vector2(120, -420),
                                        TextAnchor.UpperLeft, FontStyle.Bold, 1500f);
                timer = bannerSeconds;
            }

            if (debugKeys)
                FEKit.MakeText(root, "[G] game over   [V] vitória   (teste)", 22, FEKit.Dim,
                               Vector2.zero, new Vector2(40, 40), TextAnchor.LowerLeft);
        }

        void Update()
        {
            if (banner != null && timer > 0f)
            {
                timer -= Time.unscaledDeltaTime;
                Color c = banner.color;
                c.a = Mathf.Clamp01(timer / 0.8f);
                banner.color = c;
                if (timer <= 0f) banner.gameObject.SetActive(false);
            }

            if (debugKeys)
            {
                if (FEKit.GPressed()) GameFlow.GameOver();
                if (FEKit.VPressed()) GameFlow.Victory();
            }
        }

        // Para ligar em UnityEvents (botões, gatilhos, etc.)
        public void Win()  { GameFlow.Victory(); }
        public void Lose() { GameFlow.GameOver(); }
    }
}
