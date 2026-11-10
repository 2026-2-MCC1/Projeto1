
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FactoryEscape
{
    public static class GameFlow
    {
        // NOMES DAS CENAS
        public static string MenuScene = "MainMenu";
        public static string OptionsScene = "Opcoes";
        public static string CreditsScene = "Creditos";
        public static string TutorialScene = "Tutorial";
        public static string GameOverScene = "GameOver";
        public static string VictoryScene = "Vitoria";
        public static string EndScene = "FimDeJogo";

        // CENAS DAS FASES
        public static string[] LevelScenes =
        {
            "Fabrica_Parte1",
            "Fabrica_Parte2",
            "Fabrica_Parte3"
        };

        // NOMES DAS FASES
        public static string[] LevelNames =
        {
            "DEPÓSITO DE DOCES",
            "FABRICA SUSPENSA",
            "CAMINHO PARA O PORTAL"
        };

        public const int TotalLevels = 3;

        static int level = 1;

        public static int Level
        {
            get { return level; }
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration
        )]
        static void ResetStatics()
        {
            level = 1;
        }

        public static string LevelName(int l)
        {
            // Número de fase válido: devolve o nome do vetor LevelNames
            if (l >= 1 && l <= LevelNames.Length)
            {
                return LevelNames[l - 1];
            }

            // Número inválido: devolve texto vazio
            return "";
        }

        // FLUXO DO JOGO

        public static void NewGame()
        {
            level = 1;
            Load(TutorialScene);
        }

        public static void StartLevel()
        {
            Load(
                LevelScenes[
                    Mathf.Clamp(level, 1, TotalLevels) - 1
                ]
            );
        }

        public static void Retry()
        {
            StartLevel();
        }

        public static void PlayAgain()
        {
            level = 1;
            StartLevel();
        }

        // Avança para a próxima fase
        public static void NextLevel()
        {
            level = Mathf.Min(level + 1, TotalLevels);
            StartLevel();
        }

        // Decide qual tela será exibida ao concluir uma fase
        public static void Victory()
        {
            if (level >= TotalLevels)
            {
                // Última fase: tela de encerramento
                Load(EndScene);
            }
            else
            {
                // Fases 1 e 2: tela de Vitória
                Load(VictoryScene);
            }
        }

        public static void GameOver()
        {
            Load(GameOverScene);
        }

        public static void GoMenu()
        {
            Load(MenuScene);
        }

        public static void Options()
        {
            Load(OptionsScene);
        }

        public static void Credits()
        {
            Load(CreditsScene);
        }

        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // Identifica a fase atual pelo nome da cena
        public static void SyncLevelWithScene(string sceneName)
        {
            int found = -1;
            int count = 0;

            for (int i = 0; i < LevelScenes.Length; i++)
            {
                if (LevelScenes[i] == sceneName)
                {
                    found = i;
                    count++;
                }
            }

            if (count == 1)
                level = found + 1;
        }

        // CONTROLE DE VOLUME

        public static float Volume
        {
            get
            {
                return PlayerPrefs.GetFloat("fe_volume", 0.7f);
            }

            set
            {
                PlayerPrefs.SetFloat(
                    "fe_volume",
                    Mathf.Clamp01(value)
                );

                AudioListener.volume = Mathf.Clamp01(value);
            }
        }

        public static void ApplyVolume()
        {
            AudioListener.volume = Volume;
        }

        // CARREGAMENTO DE CENAS

        static void Load(string sceneName)
        {
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError(
                    "[Factory Escape] A cena \"" +
                    sceneName +
                    "\" não está no Build Settings. Confira o nome."
                );

                return;
            }

            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
        }
    }
}
