using TMPro;
using UnityEngine;

namespace ArcorFactory
{
    // O asset fica em Resources/ArcorFactory/ConfigArcor e é preenchido pelo menu
    [CreateAssetMenu(menuName = "ArcorFactory/Config Arcor", fileName = "ConfigArcor")]
    public class ConfigArcor : ScriptableObject
    {
        [Header("Interface")]
        public TMP_FontAsset fonte;
        public Sprite painel;
        public Sprite aba;
        public Sprite faixa;
        public Sprite circulo;
        public Sprite brilho;

        [Header("Partículas")]
        public Material particulaAditiva;
        public Material particulaSuave;
        public Material particulaEstrela;
        public Material sombraJogador;

        [Header("Tela de resumo")]
        public Sprite estrelaCheia;
        public Sprite estrelaVazia;
        public Sprite raios;
        public Sprite confete;
        public Sprite cartao;
        public Sprite cartaoBorda;

        [Header("Itens")]
        public GameObject[] rocklets;

        static ConfigArcor instancia;

        public static ConfigArcor Instancia
        {
            get
            {
                if (instancia == null)
                {
                    instancia = Resources.Load<ConfigArcor>("ArcorFactory/ConfigArcor");
                    if (instancia == null)
                    {
                        instancia = CreateInstance<ConfigArcor>();
                        Debug.LogWarning("[ArcorFactory] ConfigArcor não encontrado em Resources/ArcorFactory. Rode ArcorFactory > Reformulação > 1 - Gerar assets.");
                    }
                }
                return instancia;
            }
        }

        public GameObject RockletAleatorio()
        {
            if (rocklets == null || rocklets.Length == 0) return null;
            return rocklets[Random.Range(0, rocklets.Length)];
        }
    }
}
