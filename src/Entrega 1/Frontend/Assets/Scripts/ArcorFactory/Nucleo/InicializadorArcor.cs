using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcorFactory
{
    // sem precisar arrumar nada na cena: ataque, sombra e a HUD nova
    public static class InicializadorArcor
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void PrimeiraCena()
        {
            SceneManager.sceneLoaded -= AoCarregar;
            SceneManager.sceneLoaded += AoCarregar;
            Preparar();
        }

        static void AoCarregar(Scene s, LoadSceneMode modo)
        {
            if (modo == LoadSceneMode.Single) Preparar();
        }

        static void Preparar()
        {
            var jogador = Object.FindFirstObjectByType<PlayerMovement>();
            if (jogador == null) return; // menus, telas de vitória etc

            var go = jogador.gameObject;
            if (go.GetComponent<AtaqueJogador>() == null) go.AddComponent<AtaqueJogador>();
            if (go.GetComponent<SombraJogador>() == null) go.AddComponent<SombraJogador>();

            if (Object.FindFirstObjectByType<HUDArcor>() == null)
            {
                new GameObject("[ArcorFactory] HUD").AddComponent<HUDArcor>();
            }

            // chegando pelo portal da fase anterior: cutscene de chegada
            CutscenePortal.TentarChegada();
        }
    }
}
