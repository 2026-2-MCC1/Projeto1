using UnityEngine;
using UnityEngine.SceneManagement;

namespace FactoryEscape
{
    // =====================================================================
    //  GameFlow - controla a navegação entre as CENAS do Factory Escape.
    //
    //  Menu > Tutorial > Fase 1 > (Vitória) > Fase 2 > (Vitória) > Fase 3 > Fim de Jogo
    //                       └── Game Over (tentar de novo / menu)
    //
    //  Use a partir do SEU jogo:
    //      GameFlow.Victory();    // jogador venceu a fase atual
    //      GameFlow.GameOver();   // jogador morreu
    //      GameFlow.Level;        // fase atual (1, 2 ou 3)
    // =====================================================================
    public static class GameFlow
    {
        // ---------- NOMES DAS CENAS ----------
        // Devem ser IGUAIS aos nomes dos arquivos .unity e estar no Build Settings.
        public static string MenuScene     = "MainMenu";
        public static string OptionsScene  = "Opcoes";
        public static string CreditsScene  = "Creditos";
        public static string TutorialScene = "Tutorial";
        public static string GameOverScene = "GameOver";
        public static string VictoryScene  = "Vitoria";
        public static string EndScene      = "FimDeJogo";

        // Cenas das 3 fases do SEU jogo (troque pelos nomes reais).
        // Se o seu jogo usa UMA cena só para as 3 fases, repita o mesmo nome 3 vezes
        // e leia GameFlow.Level para saber qual fase carregar.
        public static string[] LevelScenes = { "Fabrica_Parte1", "Fabrica_Parte2", "Fabrica_Parte3" };

        public static string[] LevelNames =
            { "A LINHA DE PRODUÇÃO", "O DEPÓSITO DOS DOCES", "A SAÍDA SECRETA" };

        public const int TotalLevels = 3;

        static int level = 1;
        public static int Level { get { return level; } }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { level = 1; }

        public static string LevelName(int l)
        {
            return (l >= 1 && l <= LevelNames.Length) ? LevelNames[l - 1] : "";
        }

        // ---------- FLUXO ----------
        public static void NewGame()   { level = 1; Load(TutorialScene); }
        public static void StartLevel(){ Load(LevelScenes[Mathf.Clamp(level, 1, TotalLevels) - 1]); }
        public static void Retry()     { StartLevel(); }
        public static void PlayAgain() { level = 1; StartLevel(); }

        public static void NextLevel()
        {
            level = Mathf.Min(level + 1, TotalLevels);
            StartLevel();
        }

        public static void Victory()
        {
            if (level >= TotalLevels) Load(EndScene);   // terminou a fase 3
            else Load(VictoryScene);
        }

        public static void GameOver() { Load(GameOverScene); }
        public static void GoMenu()   { Load(MenuScene); }
        public static void Options()  { Load(OptionsScene); }
        public static void Credits()  { Load(CreditsScene); }

        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // Se você apertar Play direto numa cena de fase, descobre sozinho qual fase é.
        public static void SyncLevelWithScene(string sceneName)
        {
            int found = -1, count = 0;
            for (int i = 0; i < LevelScenes.Length; i++)
                if (LevelScenes[i] == sceneName) { found = i; count++; }
            if (count == 1) level = found + 1;
        }

        // ---------- VOLUME (salvo entre execuções) ----------
        public static float Volume
        {
            get { return PlayerPrefs.GetFloat("fe_volume", 0.7f); }
            set
            {
                PlayerPrefs.SetFloat("fe_volume", Mathf.Clamp01(value));
                AudioListener.volume = Mathf.Clamp01(value);
            }
        }

        public static void ApplyVolume() { AudioListener.volume = Volume; }

        // ---------- CARREGAR CENA ----------
        static void Load(string sceneName)
        {
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError("[Factory Escape] A cena \"" + sceneName + "\" não está no Build Settings " +
                               "(File > Build Settings). Confira o nome em GameFlow.cs.");
                return;
            }
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
        }
    }
}
