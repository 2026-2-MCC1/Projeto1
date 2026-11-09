using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FactoryEscape
{
    // PauseMenu - menu de pausa das fases (criado sozinho pelo LevelHUD)
    public class PauseMenu : MonoBehaviour
    {
        public Font customFont;

        public static bool Pausado { get; private set; }

        // Cutscenes e a tela de fase concluída travam a pausa enquanto estão na tela
        public static bool Bloqueado;

        static readonly Color Cartao = new Color(0.03f, 0.19f, 0.47f, 0.97f);
        static readonly Color CartaoEscuro = new Color(0.01f, 0.1f, 0.28f, 1f);

        Image fundo;
        RectTransform cartao;
        CanvasGroup grupoCartao;
        RectTransform paginaPrincipal, paginaOpcoes;
        CanvasGroup grupoPrincipal, grupoOpcoes;
        Text volumeValor, telaCheiaValor, qualidadeValor;
        Image[] barrasVolume;
        Text titulo, subtitulo;
        Sprite pilula, borda;
        readonly List<BotaoPausa> botoesPrincipal = new List<BotaoPausa>();
        readonly List<BotaoPausa> botoesOpcoes = new List<BotaoPausa>();
        int selecionado;
        bool nasOpcoes;
        float animacao; // 0..1 da entrada do cartão
        float trocaPagina = 1f; // 0..1 da troca de página
        CursorLockMode travaAnterior;
        bool cursorVisivelAntes;
        GameObject tela;

        void Start()
        {
            FEKit.Init(customFont);
            FEKit.EnsureEventSystem();
            Pausado = false;
            Bloqueado = false;

            if (PlayerPrefs.HasKey("FE_Qualidade") && PlayerPrefs.GetInt("FE_Qualidade") < QualitySettings.names.Length)
                QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("FE_Qualidade"), true);

            pilula = Resources.Load<Sprite>("ArcorFactory/UI/UI_Cartao");
            borda = Resources.Load<Sprite>("ArcorFactory/UI/UI_CartaoBorda");

            var canvas = FEKit.CreateCanvas("Pause Canvas", 200);
            canvas.transform.SetParent(transform, false);

            // Fundo escuro que bloqueia os cliques no jogo
            var fundoGo = new GameObject("Pausa", typeof(RectTransform), typeof(Image));
            fundoGo.transform.SetParent(canvas.transform, false);
            FEKit.Stretch((RectTransform)fundoGo.transform);
            fundo = fundoGo.GetComponent<Image>();
            fundo.color = new Color(FEKit.Ink.r, FEKit.Ink.g, FEKit.Ink.b, 0f);
            fundo.raycastTarget = true;
            tela = fundoGo;

            // Cartão central
            cartao = Caixa("Cartao", fundoGo.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 860f), Cartao);
            grupoCartao = cartao.gameObject.AddComponent<CanvasGroup>();
            if (borda != null)
            {
                var b = Caixa("Borda", cartao, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(772f, 872f), FEKit.Sun, borda);
                b.GetComponent<Image>().raycastTarget = false;
            }
            var faixaTopo = Caixa("Topo", cartao, new Vector2(0.5f, 1f), new Vector2(0f, -100f), new Vector2(720f, 170f), new Color(0.3f, 0.6f, 1f, 0.12f));
            faixaTopo.GetComponent<Image>().raycastTarget = false;

            titulo = Texto(cartao, "PAUSADO", 96, FEKit.Sun, new Vector2(0f, -40f), FontStyle.Bold);
            FEKit.Bouncy(titulo, 0f);
            subtitulo = Texto(cartao, "", 30, FEKit.Dim, new Vector2(0f, -160f), FontStyle.Normal);

            // página principal
            paginaPrincipal = Pagina("Principal");
            grupoPrincipal = paginaPrincipal.gameObject.AddComponent<CanvasGroup>();
            float y = -235f;
            botoesPrincipal.Add(Botao(paginaPrincipal, "CONTINUAR", y, Continuar, true)); y -= 104f;
            botoesPrincipal.Add(Botao(paginaPrincipal, "REINICIAR FASE", y, Reiniciar)); y -= 104f;
            botoesPrincipal.Add(Botao(paginaPrincipal, "OPÇÕES", y, AbrirOpcoes)); y -= 104f;
            botoesPrincipal.Add(Botao(paginaPrincipal, "MENU PRINCIPAL", y, MenuPrincipal)); y -= 104f;
            botoesPrincipal.Add(Botao(paginaPrincipal, "SAIR DO JOGO", y, GameFlow.Quit));
            Texto(paginaPrincipal, "ESC  voltar ao jogo     W/S  escolher     ENTER  confirmar", 22, FEKit.Dim, new Vector2(0f, -800f), FontStyle.Normal);

            // página de opções
            paginaOpcoes = Pagina("Opcoes");
            grupoOpcoes = paginaOpcoes.gameObject.AddComponent<CanvasGroup>();
            MontarOpcoes();
            paginaOpcoes.gameObject.SetActive(false);

            AtualizarOpcoes();
            tela.SetActive(false);
        }

        // montagem
        RectTransform Caixa(string nome, Transform pai, Vector2 ancora, Vector2 pos, Vector2 tam, Color cor, Sprite s = null)
        {
            var go = new GameObject(nome, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(pai, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = ancora;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = tam;
            var img = go.GetComponent<Image>();
            img.sprite = s != null ? s : pilula;
            if (img.sprite != null) img.type = Image.Type.Sliced;
            img.color = cor;
            return rt;
        }

        Text Texto(Transform pai, string conteudo, int tamanho, Color cor, Vector2 pos, FontStyle estilo)
        {
            var t = FEKit.MakeText(pai, conteudo, tamanho, cor, new Vector2(0.5f, 1f), pos, TextAnchor.UpperCenter, estilo, 700f);
            return t;
        }

        RectTransform Pagina(string nome)
        {
            var go = new GameObject(nome, typeof(RectTransform));
            go.transform.SetParent(cartao, false);
            var rt = (RectTransform)go.transform;
            FEKit.Stretch(rt);
            return rt;
        }

        BotaoPausa Botao(RectTransform pai, string rotulo, float y, UnityAction acao, bool destaque = false, float largura = 520f, float x = 0f, int tamanho = 44)
        {
            var rt = Caixa("Botao_" + rotulo, pai, new Vector2(0.5f, 1f), new Vector2(x, y), new Vector2(largura, 84f), CartaoEscuro);
            var img = rt.GetComponent<Image>();
            var txt = FEKit.MakeText(rt, rotulo, tamanho, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0f, tamanho * 0.7f), TextAnchor.UpperCenter, FontStyle.Bold, largura);
            var b = rt.gameObject.AddComponent<Button>();
            b.targetGraphic = img;
            b.transition = Selectable.Transition.None;
            b.navigation = new Navigation { mode = Navigation.Mode.None };
            b.onClick.AddListener(() => FESfx.Play(FESfx.Click));
            b.onClick.AddListener(acao);
            var bp = rt.gameObject.AddComponent<BotaoPausa>();
            bp.Configurar(this, img, txt, destaque);
            return bp;
        }

        void MontarOpcoes()
        {
            Texto(paginaOpcoes, "OPÇÕES", 40, Color.white, new Vector2(0f, -215f), FontStyle.Bold);

            // Volume: rótulo, barra de 10 gomos e -/+
            Texto(paginaOpcoes, "VOLUME", 30, FEKit.Dim, new Vector2(0f, -290f), FontStyle.Bold);
            botoesOpcoes.Add(Botao(paginaOpcoes, "-", -375f, () => MudarVolume(-0.1f), false, 84f, -250f, 54));
            barrasVolume = new Image[10];
            for (int i = 0; i < 10; i++)
            {
                var g = Caixa("Gomo_" + i, paginaOpcoes, new Vector2(0.5f, 1f), new Vector2(-153f + i * 34f, -375f), new Vector2(26f, 30f + i * 3.5f), FEKit.Sun);
                g.GetComponent<Image>().raycastTarget = false;
                barrasVolume[i] = g.GetComponent<Image>();
            }
            botoesOpcoes.Add(Botao(paginaOpcoes, "+", -375f, () => MudarVolume(0.1f), false, 84f, 250f, 54));
            volumeValor = Texto(paginaOpcoes, "70%", 28, Color.white, new Vector2(0f, -420f), FontStyle.Bold);

            Texto(paginaOpcoes, "TELA CHEIA", 30, FEKit.Dim, new Vector2(0f, -480f), FontStyle.Bold);
            var bTela = Botao(paginaOpcoes, "SIM", -555f, AlternarTelaCheia, false, 300f, 0f, 38);
            telaCheiaValor = bTela.Rotulo;
            botoesOpcoes.Add(bTela);

            Texto(paginaOpcoes, "QUALIDADE", 30, FEKit.Dim, new Vector2(0f, -615f), FontStyle.Bold);
            var bQual = Botao(paginaOpcoes, "ALTA", -690f, MudarQualidade, false, 300f, 0f, 38);
            qualidadeValor = bQual.Rotulo;
            botoesOpcoes.Add(bQual);

            botoesOpcoes.Add(Botao(paginaOpcoes, "VOLTAR", -790f, FecharOpcoes, true, 360f));
        }

        // ciclo
        void Update()
        {
            if (FEKit.PausePressed() && !Bloqueado)
            {
                if (!Pausado) Pausar();
                else if (nasOpcoes) FecharOpcoes();
                else Continuar();
            }
            if (!Pausado || tela == null) return;

            float dt = Time.unscaledDeltaTime;
            animacao = Mathf.MoveTowards(animacao, 1f, dt / 0.28f);
            fundo.color = new Color(FEKit.Ink.r, FEKit.Ink.g, FEKit.Ink.b, 0.78f * animacao);
            float k = animacao;
            const float c1 = 1.70158f, c3 = c1 + 1f;
            float mola = 1f + c3 * Mathf.Pow(k - 1f, 3f) + c1 * Mathf.Pow(k - 1f, 2f);
            cartao.localScale = Vector3.one * Mathf.LerpUnclamped(0.85f, 1f, mola);
            grupoCartao.alpha = Mathf.Clamp01(k * 2f);

            // troca de página: a nova entra deslizando
            if (trocaPagina < 1f)
            {
                trocaPagina = Mathf.MoveTowards(trocaPagina, 1f, dt / 0.22f);
                float e = 1f - Mathf.Pow(1f - trocaPagina, 3f);
                var g = nasOpcoes ? grupoOpcoes : grupoPrincipal;
                var p = nasOpcoes ? paginaOpcoes : paginaPrincipal;
                g.alpha = e;
                p.anchoredPosition = new Vector2((1f - e) * (nasOpcoes ? 120f : -120f), 0f);
            }

            // teclado: W/S ou setas escolhem, ENTER/ESPAÇO confirma, A/D mexe no volume
            var lista = nasOpcoes ? botoesOpcoes : botoesPrincipal;
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null && lista.Count > 0)
            {
                if (kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame) Selecionar((selecionado + 1) % lista.Count, true);
                if (kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame) Selecionar((selecionado - 1 + lista.Count) % lista.Count, true);
                if (nasOpcoes && (kb.aKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame)) MudarVolume(-0.1f);
                if (nasOpcoes && (kb.dKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame)) MudarVolume(0.1f);
                if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)
                {
                    lista[Mathf.Clamp(selecionado, 0, lista.Count - 1)].GetComponent<Button>().onClick.Invoke();
                }
            }
            for (int i = 0; i < lista.Count; i++) lista[i].Selecionado = i == selecionado;
        }

        public void AoPassarMouse(BotaoPausa b)
        {
            var lista = nasOpcoes ? botoesOpcoes : botoesPrincipal;
            int i = lista.IndexOf(b);
            if (i >= 0 && i != selecionado) Selecionar(i, false);
        }

        void Selecionar(int i, bool comSom)
        {
            selecionado = i;
            if (comSom) FESfx.Play(FESfx.Hover);
        }

        public void Pausar()
        {
            if (Pausado || tela == null || Bloqueado) return;
            Pausado = true;
            Time.timeScale = 0f;
            AudioListener.pause = true; // para máquinas, música da fase etc

            travaAnterior = Cursor.lockState;
            cursorVisivelAntes = Cursor.visible;
            FEKit.LiberarMouse();

            subtitulo.text = "FASE " + GameFlow.Level + " de " + GameFlow.TotalLevels + "  •  " + GameFlow.LevelName(GameFlow.Level);
            nasOpcoes = false;
            paginaPrincipal.gameObject.SetActive(true);
            paginaOpcoes.gameObject.SetActive(false);
            grupoPrincipal.alpha = 1f;
            paginaPrincipal.anchoredPosition = Vector2.zero;
            trocaPagina = 1f;
            selecionado = 0;
            animacao = 0f;
            AtualizarOpcoes();
            tela.SetActive(true);
        }

        public void Continuar()
        {
            if (!Pausado) return;
            tela.SetActive(false);
            Pausado = false;
            Time.timeScale = 1f;
            AudioListener.pause = false;
            Cursor.lockState = travaAnterior;
            Cursor.visible = cursorVisivelAntes;
        }

        void AbrirOpcoes()
        {
            nasOpcoes = true;
            paginaPrincipal.gameObject.SetActive(false);
            paginaOpcoes.gameObject.SetActive(true);
            titulo.text = "PAUSADO";
            selecionado = botoesOpcoes.Count - 1;
            trocaPagina = 0f;
            grupoOpcoes.alpha = 0f;
            AtualizarOpcoes();
        }

        void FecharOpcoes()
        {
            nasOpcoes = false;
            paginaOpcoes.gameObject.SetActive(false);
            paginaPrincipal.gameObject.SetActive(true);
            selecionado = 2; // volta com o OPÇÕES selecionado
            trocaPagina = 0f;
            grupoPrincipal.alpha = 0f;
            FESfx.Play(FESfx.Click);
        }

        void Reiniciar()
        {
            Despausar();
            GameFlow.Retry();
        }

        void MenuPrincipal()
        {
            Despausar();
            GameFlow.GoMenu();
        }

        void Despausar()
        {
            Pausado = false;
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }

        // opções
        void MudarVolume(float delta)
        {
            GameFlow.Volume = Mathf.Clamp01(Mathf.Round((GameFlow.Volume + delta) * 10f) / 10f);
            FESfx.Play(FESfx.Click, 0.8f + GameFlow.Volume * 0.8f);
            AtualizarOpcoes();
        }

        void AlternarTelaCheia()
        {
            Screen.fullScreen = !Screen.fullScreen;
            AtualizarOpcoes(!Screen.fullScreen);
        }

        void MudarQualidade()
        {
            int n = QualitySettings.names.Length;
            if (n == 0) return;
            int q = (QualitySettings.GetQualityLevel() + 1) % n;
            QualitySettings.SetQualityLevel(q, true);
            PlayerPrefs.SetInt("FE_Qualidade", q);
            AtualizarOpcoes();
        }

        void AtualizarOpcoes() { AtualizarOpcoes(Screen.fullScreen); }

        void AtualizarOpcoes(bool telaCheia)
        {
            if (volumeValor != null) volumeValor.text = Mathf.RoundToInt(GameFlow.Volume * 100f) + "%";
            if (barrasVolume != null)
            {
                int cheios = Mathf.RoundToInt(GameFlow.Volume * 10f);
                for (int i = 0; i < barrasVolume.Length; i++)
                    barrasVolume[i].color = i < cheios ? FEKit.Sun : new Color(1f, 1f, 1f, 0.15f);
            }
            if (telaCheiaValor != null) telaCheiaValor.text = telaCheia ? "SIM" : "NÃO";
            if (qualidadeValor != null)
            {
                var nomes = QualitySettings.names;
                int q = QualitySettings.GetQualityLevel();
                qualidadeValor.text = nomes.Length > 0 ? nomes[Mathf.Clamp(q, 0, nomes.Length - 1)].ToUpperInvariant() : "-";
            }
        }

        void OnDestroy()
        {
            // Saindo da cena com o jogo pausado: não deixa o próximo começar congelado
            Bloqueado = false;
            if (Pausado)
            {
                Pausado = false;
                Time.timeScale = 1f;
                AudioListener.pause = false;
            }
        }
    }

    // Botão "pílula" do menu de pausa
    public class BotaoPausa : MonoBehaviour, IPointerEnterHandler
    {
        PauseMenu menu;
        Image fundo;
        bool destaque;
        public Text Rotulo { get; private set; }
        public bool Selecionado { get; set; }

        public void Configurar(PauseMenu m, Image f, Text t, bool d)
        {
            menu = m; fundo = f; Rotulo = t; destaque = d;
        }

        public void OnPointerEnter(PointerEventData e) { if (menu != null) menu.AoPassarMouse(this); }

        void OnDisable() { transform.localScale = Vector3.one; }

        void Update()
        {
            float k = 14f * Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * (Selecionado ? 1.07f : 1f), k);
            Color normal = destaque ? new Color(0.0f, 0.33f, 0.64f, 1f) : new Color(0.01f, 0.1f, 0.28f, 1f);
            Color alvo = Selecionado ? FEKit.Sun : normal;
            fundo.color = Color.Lerp(fundo.color, alvo, k);
            if (Rotulo != null) Rotulo.color = Color.Lerp(Rotulo.color, Selecionado ? FEKit.Ink : Color.white, k);
        }
    }
}
