using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FactoryEscape
{
    public enum FEScreen { Menu, Options, Credits, Tutorial, GameOver, Victory, End }

    // FactoryEscapeScreen - coloque UM destes em cada cena de tela
    public class FactoryEscapeScreen : MonoBehaviour
    {
        [Header("Qual tela é esta cena?")]
        public FEScreen screen = FEScreen.Menu;

        [Header("Visual (opcional)")]
        public Texture backgroundTexture; // sua imagem de fundo (Wrap Mode = Repeat)
        public Font customFont; // dica: uma fonte arredondada (ex.: Fredoka)
        public Vector2 scrollSpeed = new Vector2(0.03f, 0.004f);
        public int bubbleCount = 16;

        [Header("Sons (opcional: se vazio, o PLIM é gerado por código)")]
        public AudioClip music; // música de fundo (continua entre as cenas)
        public AudioClip clickSound;
        public AudioClip hoverSound;
        public AudioClip victorySound;
        public AudioClip gameOverSound;
        [Range(0f, 1f)] public float sfxVolume = 0.8f;

        [Header("Textos (edite à vontade)")]
        [TextArea(4, 10)]
        public string tutorialText =
            "MOVER ............ WASD / SETAS\n" +
            "PULAR ............ ESPAÇO\n" +
            "INTERAGIR ........ E\n\n" +
            "SUA MISSÃO: colete os doces e baus, desvie dos monstros\n" +
            "e chegue à saída antes que o alarme toque!\n" +
            "São 3 fases. Consegue escapar de todas?";

        [TextArea(4, 12)]
        public string creditsText =
            "FACTORY ESCAPE\n\n" +
            "Desenvolvido por\n\n" +
            "André Ananias\n" +
            "Antonie Gabriel\n" +
            "João Vitor\n" +
            "Karine Vicente\n\n" +
            "Projeto acadêmico de Ciência da Computação (Fecap)\n" +
            "ambientado em uma fábrica da Arcor.";

        Transform root;
        Text volumeValue, volumeBar;

        void Awake()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;   // libera o mouse (o jogo pode tê-lo travado)
            Cursor.visible = true;
            GameFlow.ApplyVolume();
            customFont = Resources.Load<Font>("Fonts/FredokaOne-Regular");

            FEKit.Init(customFont);
            FESfx.SetOverrides(clickSound, hoverSound, victorySound, gameOverSound, sfxVolume);
            FEKit.EnsureEventSystem();
            FEKit.LiberarMouse();

            var canvas = FEKit.CreateCanvas("FactoryEscapeCanvas", 100);
            canvas.transform.SetParent(transform, false);
            FEBackground.Create(canvas.transform, backgroundTexture, scrollSpeed, bubbleCount);
            root = FEKit.Panel(canvas.transform, screen.ToString());

            switch (screen)
            {
                case FEScreen.Menu:     BuildMenu();     break;
                case FEScreen.Options:  BuildOptions();  break;
                case FEScreen.Credits:  BuildCredits();  break;
                case FEScreen.Tutorial: BuildTutorial(); break;
                case FEScreen.GameOver: BuildGameOver(); break;
                case FEScreen.Victory:  BuildVictory();  break;
                case FEScreen.End:      BuildEnd();      break;
            }
        }

        void Start()
        {
            if (music != null) FESfx.PlayMusic(music);

            if (screen == FEScreen.GameOver) FESfx.Play(FESfx.Lose);
            if (screen == FEScreen.Victory || screen == FEScreen.End) FESfx.Play(FESfx.Win);
        }

        void Update()
        {
            // Mantém o mouse livre: se algum script do jogo (câmera, controle do jogador)
            // travar o cursor de novo, a tela destrava na hora.
            if (Cursor.lockState != CursorLockMode.None) Cursor.lockState = CursorLockMode.None;
            if (!Cursor.visible) Cursor.visible = true;

            if (screen == FEScreen.Tutorial && FEKit.ConfirmPressed())
                GameFlow.StartLevel();
        }

        // TELAS
        void BuildMenu()
        {
            FEKit.Bouncy(T("FACTORY", 150, Color.white, 120, -70), 0f);
            FEKit.Bouncy(T("ESCAPE", 150, FEKit.Sun, 120, -215), 0.8f);
            T("A fábrica da Arcor ficou maluca. Consegue fugir?", 30, FEKit.Dim, 126, -385, FontStyle.Normal);
            Line(126, -435);

            B("JOGAR",    56, 120, -490, GameFlow.NewGame);
            B("OPÇÕES",   56, 120, -575, GameFlow.Options);
            B("CRÉDITOS", 56, 120, -660, GameFlow.Credits);
            B("SAIR",     56, 120, -745, GameFlow.Quit);
        }

        void BuildOptions()
        {
            Header("OPÇÕES");
            T("VOLUME", 38, FEKit.Dim, 126, -300, FontStyle.Normal);
            B("-", 90, 120, -350, () => ChangeVolume(-0.1f), 90, false);
            volumeValue = T("70%", 80, Color.white, 230, -362);
            B("+", 90, 470, -350, () => ChangeVolume(+0.1f), 90, false);
            volumeBar = T("", 40, Color.white, 126, -500, FontStyle.Normal);
            RefreshVolume();

            B("VOLTAR", 56, 120, -640, GameFlow.GoMenu);
        }

        void BuildCredits()
        {
            Header("CRÉDITOS");

            string textoCreditos =
                "FACTORY ESCAPE\n\n" +
                "Programação ...... André Ananias, Antonie Gabriel, Karine Vicente e João Vitor\n" +
                "Arte ............. André Ananias, Antonie Gabriel e Karine Vicente\n" +
                "Som .............. André Ananias e Antonie Gabriel\n\n" +
                "Projeto acadêmico ambientado em uma fábrica da Arcor.";

            T(textoCreditos, 38, Color.white, 126, -300,
              FontStyle.Normal, 1000, true);

            B("VOLTAR", 56, 120, -860, GameFlow.GoMenu);
        }

        void BuildTutorial()
        {
            Header("COMO JOGAR");
            T(tutorialText, 38, Color.white, 126, -300, FontStyle.Normal, 1100, true);
            B("COMEÇAR!", 60, 120, -820, GameFlow.StartLevel);
            T("ou pressione ENTER", 26, FEKit.Dim, 126, -905, FontStyle.Normal);
        }

        void BuildGameOver()
        {
            FEKit.Bouncy(T("GAME", 150, FEKit.Oops, 120, -70), 0f);
            FEKit.Bouncy(T("OVER", 150, Color.white, 120, -215), 0.8f);
            T("Você não pode desistir!", 32, FEKit.Dim, 126, -385, FontStyle.Normal);
            Line(126, -435);
            B("TENTAR DE NOVO", 56, 120, -490, GameFlow.Retry);
            B("MENU PRINCIPAL", 56, 120, -575, GameFlow.GoMenu);
        }

        void BuildVictory()
        {
            int lv = GameFlow.Level;
            FEKit.Bouncy(T("VITÓRIA!", 150, FEKit.Sun, 120, -70), 0f);
            T("FASE " + lv + " CONCLUÍDA!", 48, Color.white, 126, -240);
            T("Uhul! Você escapou desta seção. Próxima: " + GameFlow.LevelName(lv + 1),
              30, FEKit.Dim, 126, -330, FontStyle.Normal);
            Line(126, -405);
            B("PRÓXIMA FASE", 56, 120, -460, GameFlow.NextLevel);
            B("MENU PRINCIPAL", 56, 120, -545, GameFlow.GoMenu);
        }

        void BuildEnd()
        {
            FEKit.Bouncy(T("FIM DE", 130, Color.white, 120, -70), 0f);
            FEKit.Bouncy(T("JOGO", 130, FEKit.Sun, 120, -200), 0.8f);
            T("Você completou as 3 fases e escapou da fábrica! Obrigado por jogar!",
              32, FEKit.Dim, 126, -360, FontStyle.Normal, 1400);
            Line(126, -415);
            B("JOGAR NOVAMENTE", 56, 120, -470, GameFlow.PlayAgain);
            B("MENU PRINCIPAL",  56, 120, -555, GameFlow.GoMenu);
            B("SAIR",            56, 120, -640, GameFlow.Quit);
        }

        void Header(string title)
        {
            FEKit.Bouncy(T(title, 100, FEKit.Sun, 120, -70), 0f);
            Line(126, -205);
        }

        // VOLUME
        void ChangeVolume(float delta)
        {
            float v = Mathf.Clamp01(Mathf.Round((GameFlow.Volume + delta) * 10f) / 10f);
            GameFlow.Volume = v;
            FESfx.Play(FESfx.Click, 0.8f + v * 0.8f); // plim mais agudo quanto maior o volume
            RefreshVolume();
        }

        void RefreshVolume()
        {
            float v = GameFlow.Volume;
            int filled = Mathf.RoundToInt(v * 20f);
            volumeValue.text = Mathf.RoundToInt(v * 100f) + "%";
            volumeBar.text = "<color=#FFD133>" + new string('|', filled) + "</color>" +
                             "<color=#3A5F94>" + new string('|', 20 - filled) + "</color>";
        }

        // ATALHOS DE CONSTRUÇÃO (x e y em unidades de 1920x1080, y negativo = para baixo)
        Text T(string s, int size, Color c, float x, float y,
               FontStyle st = FontStyle.Bold, float w = 900f, bool wrap = false)
        {
            return FEKit.MakeText(root, s, size, c, FEKit.TL, new Vector2(x, y),
                                  TextAnchor.UpperLeft, st, w, wrap);
        }

        Button B(string s, int size, float x, float y, UnityAction a,
                 float w = 800f, bool click = true)
        {
            return FEKit.MakeButton(root, s, size, new Vector2(x, y), a, w, click);
        }

        void Line(float x, float y) { FEKit.MakeLine(root, new Vector2(x, y), 420f); }
    }
}
