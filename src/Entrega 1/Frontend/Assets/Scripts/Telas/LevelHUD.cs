using UnityEngine;

namespace FactoryEscape
{
    // LevelHUD - coloque UM em cada cena de FASE do seu jogo
    public class LevelHUD : MonoBehaviour
    {
        public Font customFont; // mantido para não perder a referência na cena
        public bool showBanner = false; // não é mais usado: o nome da fase não aparece
        public float bannerSeconds = 2.8f; // não é mais usado
        public bool debugKeys = true; // G = game over

        void Awake()
        {
            Time.timeScale = 1f;
            GameFlow.ApplyVolume();
            GameFlow.SyncLevelWithScene(gameObject.scene.name); // Play direto na fase também funciona

            // Menu de pausa (ESC)
            if (GetComponent<PauseMenu>() == null)
            {
                var pausa = gameObject.AddComponent<PauseMenu>();
                pausa.customFont = customFont;
            }
        }

        void Update()
        {
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
