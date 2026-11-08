using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FactoryEscape.EditorTools
{
    // =====================================================================
    //  Cria as cenas das telas automaticamente e ajusta o Build Settings.
    //  ESTE ARQUIVO PRECISA FICAR DENTRO DE UMA PASTA CHAMADA "Editor".
    //  Menu da Unity:  Factory Escape > ...
    // =====================================================================
    public static class FactoryEscapeSceneCreator
    {
        const string Dir = "Assets/Scenes/FactoryEscape";

        [MenuItem("Factory Escape/1 - Criar cenas das telas")]
        static void CreateScreens()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var names = new[]
            {
                GameFlow.MenuScene, GameFlow.OptionsScene, GameFlow.CreditsScene, GameFlow.TutorialScene,
                GameFlow.GameOverScene, GameFlow.VictoryScene, GameFlow.EndScene
            };

            bool exists = false;
            foreach (var n in names) if (File.Exists(Dir + "/" + n + ".unity")) exists = true;
            if (exists && !EditorUtility.DisplayDialog("Factory Escape",
                    "Já existem cenas em " + Dir + ".\nDeseja substituí-las?", "Substituir", "Cancelar"))
                return;

            Directory.CreateDirectory(Dir);
            var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            string menuPath = Make(FEScreen.Menu,     GameFlow.MenuScene,     list, true);
            Make(FEScreen.Options,  GameFlow.OptionsScene,  list, false);
            Make(FEScreen.Credits,  GameFlow.CreditsScene,  list, false);
            Make(FEScreen.Tutorial, GameFlow.TutorialScene, list, false);
            Make(FEScreen.GameOver, GameFlow.GameOverScene, list, false);
            Make(FEScreen.Victory,  GameFlow.VictoryScene,  list, false);
            Make(FEScreen.End,      GameFlow.EndScene,      list, false);

            EditorBuildSettings.scenes = list.ToArray();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(menuPath);

            EditorUtility.DisplayDialog("Factory Escape",
                "7 cenas criadas em " + Dir + " e adicionadas ao Build Settings.\n" +
                "O MainMenu foi colocado como primeira cena (índice 0).\n\n" +
                "Próximo passo: Factory Escape > 2 - Adicionar cenas das fases ao Build.", "OK");
        }

        [MenuItem("Factory Escape/2 - Adicionar cenas das fases ao Build")]
        static void AddLevels()
        {
            var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            var missing = new List<string>();
            int added = 0;

            foreach (var name in new HashSet<string>(GameFlow.LevelScenes))
            {
                string found = null;
                foreach (var guid in AssetDatabase.FindAssets(name + " t:Scene"))
                {
                    string p = AssetDatabase.GUIDToAssetPath(guid);
                    if (Path.GetFileNameWithoutExtension(p) == name) { found = p; break; }
                }
                if (found == null) { missing.Add(name); continue; }
                if (!list.Exists(s => s.path == found))
                {
                    list.Add(new EditorBuildSettingsScene(found, true));
                    added++;
                }
            }

            EditorBuildSettings.scenes = list.ToArray();

            string msg = added + " cena(s) de fase adicionada(s) ao Build Settings.";
            if (missing.Count > 0)
                msg += "\n\nNÃO encontrei estas cenas: " + string.Join(", ", missing) +
                       "\nRenomeie suas cenas ou troque os nomes em GameFlow.cs (LevelScenes).";
            EditorUtility.DisplayDialog("Factory Escape", msg, "OK");
        }

        static string Make(FEScreen screen, string sceneName, List<EditorBuildSettingsScene> list, bool atStart)
        {
            string path = Dir + "/" + sceneName + ".unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var cam = Camera.main;
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = FEKit.Ink;
            }

            var go = new GameObject("FactoryEscapeScreen");
            go.AddComponent<FactoryEscapeScreen>().screen = screen;

            EditorSceneManager.SaveScene(scene, path);

            list.RemoveAll(s => s.path == path);
            var entry = new EditorBuildSettingsScene(path, true);
            if (atStart) list.Insert(0, entry); else list.Add(entry);
            return path;
        }
    }
}
