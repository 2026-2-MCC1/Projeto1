using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ArcorFactory
{
    // notificações deslizantes (baú aberto, selo, recorde...)
    public class HUDArcor : MonoBehaviour
    {
        public static HUDArcor Instancia { get; private set; }

        ConfigArcor cfg;
        Canvas canvas;
        RectTransform raiz, pilhaNotificacoes, painelContadores, dica;
        TextMeshProUGUI textoBaus, textoSelos, textoDicaTecla, textoDica;
        GameObject linhaBaus, linhaSelos;
        Image vinheta;
        float vinhetaForca;
        object donoDica;
        readonly List<RectTransform> notificacoes = new List<RectTransform>();
        bool resumoAberto;

        public static bool ResumoAberto { get { return Instancia != null && Instancia.resumoAberto; } }

        // API estática
        public static void Notificar(string titulo, string subtitulo, Color cor, float duracao = 3.4f)
        {
            if (Instancia != null) Instancia.CriarNotificacao(titulo, subtitulo, cor, duracao);
        }

        public static void MostrarDica(object dono, string tecla, string texto)
        {
            if (Instancia == null) return;
            Instancia.donoDica = dono;
            Instancia.textoDicaTecla.text = tecla;
            Instancia.textoDica.text = texto;
            Instancia.dica.gameObject.SetActive(true);
        }

        public static void EsconderDica(object dono)
        {
            if (Instancia == null || Instancia.donoDica != dono) return;
            Instancia.donoDica = null;
            Instancia.dica.gameObject.SetActive(false);
        }

        // Mostra o resumo e chama "continuar" quando o jogador confirmar
        public static void MostrarResumoEContinuar(Action continuar)
        {
            if (Instancia == null || Instancia.resumoAberto) { continuar?.Invoke(); return; }
            Instancia.StartCoroutine(Instancia.Resumo(continuar));
        }

        public static TMP_FontAsset Fonte { get { return ConfigArcor.Instancia.fonte; } }

        // montagem
        void Awake()
        {
            Instancia = this;
            cfg = ConfigArcor.Instancia;

            var go = new GameObject("Canvas_HUDArcor", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(transform, false);
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 40;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            raiz = (RectTransform)go.transform;

            CriarVinheta();
            CriarPilhaNotificacoes();
            CriarContadores();
            CriarDica();
        }

        void Start()
        {
            EstatisticasFase.AoMudar += AtualizarContadores;
            EstatisticasFase.AoLevarDano += AoDano;
            AtualizarContadores();
        }

        void OnDestroy()
        {
            EstatisticasFase.AoMudar -= AtualizarContadores;
            EstatisticasFase.AoLevarDano -= AoDano;
            if (Instancia == this) Instancia = null;
        }

        RectTransform Ret(string nome, Transform pai, Vector2 ancora, Vector2 pivo, Vector2 pos, Vector2 tam)
        {
            var go = new GameObject(nome, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(pai, false);
            rt.anchorMin = rt.anchorMax = ancora;
            rt.pivot = pivo;
            rt.anchoredPosition = pos;
            rt.sizeDelta = tam;
            return rt;
        }

        Image Img(RectTransform rt, Sprite s, Color cor)
        {
            var im = rt.gameObject.AddComponent<Image>();
            im.sprite = s;
            im.color = cor;
            im.raycastTarget = false;
            if (s != null && s.border.sqrMagnitude > 0f) im.type = Image.Type.Sliced;
            return im;
        }

        TextMeshProUGUI Txt(RectTransform rt, string texto, float tamanho, Color cor, TextAlignmentOptions al)
        {
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            if (Fonte != null) t.font = Fonte;
            t.text = texto;
            t.fontSize = tamanho;
            t.color = cor;
            t.alignment = al;
            t.raycastTarget = false;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.overflowMode = TextOverflowModes.Ellipsis;
            t.outlineWidth = 0.18f;
            t.outlineColor = new Color32(4, 24, 60, 255);
            return t;
        }

        static RectTransform Esticar(RectTransform rt, float margem = 0f)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(margem, margem);
            rt.offsetMax = new Vector2(-margem, -margem);
            return rt;
        }

        void CriarVinheta()
        {
            var rt = Esticar(Ret("Vinheta_Dano", raiz, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero));
            vinheta = rt.gameObject.AddComponent<Image>();
            vinheta.raycastTarget = false;
            var tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            for (int y = 0; y < 128; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    float dx = (x - 63.5f) / 63.5f, dy = (y - 63.5f) / 63.5f;
                    float d = Mathf.Sqrt(dx * dx * 0.8f + dy * dy * 0.8f);
                    float a = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.55f, 1.15f, d));
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            tex.Apply();
            vinheta.sprite = Sprite.Create(tex, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));
            vinheta.color = new Color(0.9f, 0.08f, 0.05f, 0f);
        }

        void CriarPilhaNotificacoes()
        {
            pilhaNotificacoes = Ret("Notificacoes", raiz, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-28f, -250f), new Vector2(560f, 600f));
        }

        void CriarContadores()
        {
            painelContadores = Ret("Contadores", raiz, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-30f, -168f), new Vector2(330f, 70f));
            var fundo = Img(painelContadores, cfg.faixa != null ? cfg.faixa : cfg.painel, new Color(1f, 1f, 1f, 0.95f));
            if (cfg.faixa == null && cfg.painel == null) fundo.color = new Color(0.02f, 0.16f, 0.4f, 0.8f);

            linhaBaus = Ret("Baus", painelContadores, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(22f, 0f), new Vector2(150f, 50f)).gameObject;
            var iconeBau = Ret("Icone", (RectTransform)linhaBaus.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(34f, 28f));
            Img(iconeBau, null, Cores.Laranja);
            var tampa = Ret("Tampa", iconeBau, new Vector2(0.5f, 1f), new Vector2(0.5f, 0f), new Vector2(0f, -10f), new Vector2(38f, 10f));
            Img(tampa, null, new Color(0.75f, 0.4f, 0.1f));
            var fecho = Ret("Fecho", iconeBau, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 2f), new Vector2(8f, 10f));
            Img(fecho, null, Cores.Ouro);
            textoBaus = Txt(Ret("Texto", (RectTransform)linhaBaus.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(44f, 0f), new Vector2(110f, 50f)), "0/0", 34f, Color.white, TextAlignmentOptions.MidlineLeft);

            linhaSelos = Ret("Selos", painelContadores, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(178f, 0f), new Vector2(150f, 50f)).gameObject;
            var moeda = Ret("Icone", (RectTransform)linhaSelos.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(34f, 34f));
            Img(moeda, cfg.circulo, Cores.Ouro);
            var miolo = Ret("Miolo", moeda, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(20f, 20f));
            Img(miolo, cfg.circulo, Cores.AzulArcor);
            textoSelos = Txt(Ret("Texto", (RectTransform)linhaSelos.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(44f, 0f), new Vector2(110f, 50f)), "0/0", 34f, Color.white, TextAlignmentOptions.MidlineLeft);
        }

        void CriarDica()
        {
            dica = Ret("Dica", raiz, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 150f), new Vector2(520f, 76f));
            Img(dica, cfg.faixa != null ? cfg.faixa : cfg.painel, Color.white);
            var tecla = Ret("Tecla", dica, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(18f, 0f), new Vector2(58f, 54f));
            Img(tecla, cfg.aba, Color.white);
            textoDicaTecla = Txt(Esticar(Ret("Letra", tecla, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero)), "E", 36f, new Color(0.02f, 0.12f, 0.35f), TextAlignmentOptions.Center);
            textoDicaTecla.outlineWidth = 0f;
            textoDica = Txt(Ret("Texto", dica, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(92f, 0f), new Vector2(410f, 60f)), "", 34f, Color.white, TextAlignmentOptions.MidlineLeft);
            dica.gameObject.SetActive(false);
        }

        // atualização
        void AtualizarContadores()
        {
            if (painelContadores == null) return;
            int bt = EstatisticasFase.BausTotal, st = EstatisticasFase.SelosTotal;
            linhaBaus.SetActive(bt > 0);
            linhaSelos.SetActive(st > 0);
            painelContadores.gameObject.SetActive(bt > 0 || st > 0);
            textoBaus.text = EstatisticasFase.BausAbertos + "/" + bt;
            textoSelos.text = EstatisticasFase.SelosColetados + "/" + st;
            var rtSelo = (RectTransform)linhaSelos.transform;
            rtSelo.anchoredPosition = new Vector2(bt > 0 ? 178f : 22f, 0f);
            painelContadores.sizeDelta = new Vector2((bt > 0 && st > 0) ? 330f : 175f, 70f);
        }

        void AoDano(Vector3 origem)
        {
            vinhetaForca = 0.75f;
        }

        void Update()
        {
            if (vinhetaForca > 0f)
            {
                vinhetaForca = Mathf.Max(0f, vinhetaForca - Time.unscaledDeltaTime * 1.4f);
            }
            Color c = vinheta.color;
            c.a = vinhetaForca;
            vinheta.color = c;
        }

        // notificações
        void CriarNotificacao(string titulo, string subtitulo, Color cor, float duracao)
        {
            var rt = Ret("Notificacao", pilhaNotificacoes, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(600f, 0f), new Vector2(540f, 104f));
            Img(rt, cfg.painel, Color.white);
            if (cfg.painel == null) rt.GetComponent<Image>().color = new Color(0.0f, 0.25f, 0.6f, 0.95f);
            var faixa = Ret("Cor", rt, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(14f, 0f), new Vector2(14f, 70f));
            Img(faixa, null, cor);
            var t1 = Txt(Ret("Titulo", rt, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(42f, -12f), new Vector2(470f, 46f)), titulo, 36f, cor, TextAlignmentOptions.TopLeft);
            t1.fontStyle = FontStyles.Bold;
            Txt(Ret("Sub", rt, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(42f, 12f), new Vector2(470f, 40f)), subtitulo, 26f, Color.white, TextAlignmentOptions.BottomLeft);
            var grupo = rt.gameObject.AddComponent<CanvasGroup>();
            notificacoes.Insert(0, rt);
            while (notificacoes.Count > 4)
            {
                var velha = notificacoes[notificacoes.Count - 1];
                notificacoes.RemoveAt(notificacoes.Count - 1);
                if (velha != null) Destroy(velha.gameObject);
            }
            Sons.Tocar("notificacao", 0.5f, 0.03f);
            StartCoroutine(AnimarNotificacao(rt, grupo, duracao));
        }

        IEnumerator AnimarNotificacao(RectTransform rt, CanvasGroup grupo, float duracao)
        {
            float t = 0f;
            while (rt != null && t < duracao + 0.8f)
            {
                t += Time.unscaledDeltaTime;
                int indice = notificacoes.IndexOf(rt);
                float yAlvo = -Mathf.Max(0, indice) * 114f;
                float entrada = Mathf.Clamp01(t / 0.35f);
                float voltar = 1f + 1.70158f;
                float e = 1f + voltar * Mathf.Pow(entrada - 1f, 3f) + 1.70158f * Mathf.Pow(entrada - 1f, 2f);
                float xAlvo = Mathf.Lerp(600f, 0f, e);
                if (t > duracao) xAlvo = Mathf.Lerp(0f, 80f, (t - duracao) / 0.8f);
                Vector2 p = rt.anchoredPosition;
                rt.anchoredPosition = new Vector2(xAlvo, Mathf.Lerp(p.y, yAlvo, Time.unscaledDeltaTime * 12f));
                grupo.alpha = t > duracao ? 1f - (t - duracao) / 0.8f : 1f;
                yield return null;
            }
            if (rt != null)
            {
                notificacoes.Remove(rt);
                Destroy(rt.gameObject);
            }
        }

        IEnumerator Pular(RectTransform alvo, float escala)
        {
            float t = 0f;
            while (alvo != null && t < 0.3f)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Sin(Mathf.Clamp01(t / 0.3f) * Mathf.PI);
                alvo.localScale = Vector3.one * Mathf.Lerp(1f, escala, k);
                yield return null;
            }
            if (alvo != null) alvo.localScale = Vector3.one;
        }

        // tela de fase concluída
        IEnumerator Resumo(Action continuar)
        {
            resumoAberto = true;
            FactoryEscape.PauseMenu.Bloqueado = true;
            EsconderDica(donoDica);
            var hud = FindFirstObjectByType<HUDJogo>();
            var inventario = FindFirstObjectByType<InventarioJogador>();
            float tempo = hud != null ? hud.TempoDecorrido : Time.timeSinceLevelLoad;
            int rocklets = inventario != null ? inventario.Rocklets : 0;
            float melhorAntes = EstatisticasFase.MelhorTempo;
            bool recorde = EstatisticasFase.SalvarResultado(tempo);
            int estrelas = Mathf.Clamp(EstatisticasFase.Estrelas(), 0, 3);

            // fundo escurecendo
            var fundoRt = Esticar(Ret("Resumo", raiz, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero));
            var fundo = fundoRt.gameObject.AddComponent<Image>();
            fundo.color = new Color(0.01f, 0.05f, 0.14f, 0f);
            fundo.raycastTarget = true;
            var grupo = fundoRt.gameObject.AddComponent<CanvasGroup>();

            // raios girando atrás de tudo
            RectTransform raios = null;
            Image imgRaios = null;
            if (cfg.raios != null)
            {
                raios = Ret("Raios", fundoRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), new Vector2(1300f, 1300f));
                imgRaios = Img(raios, cfg.raios, new Color(1f, 0.85f, 0.35f, 0f));
            }

            // cartão
            var painel = Ret("Cartao", fundoRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(900f, 700f));
            Img(painel, cfg.cartao != null ? cfg.cartao : cfg.painel, new Color(0.03f, 0.2f, 0.5f, 0.97f));
            if (cfg.cartaoBorda != null)
            {
                var borda = Esticar(Ret("Borda", painel, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero), -6f);
                Img(borda, cfg.cartaoBorda, Cores.Ouro);
            }
            // faixa clara no topo do cartão (dá volume)
            var brilhoTopo = Ret("BrilhoTopo", painel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -10f), new Vector2(860f, 200f));
            Img(brilhoTopo, cfg.cartao, new Color(0.3f, 0.6f, 1f, 0.16f));

            // faixa do título (aba) saindo por cima do cartão
            var aba = Ret("Faixa", painel, new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, 6f), new Vector2(680f, 112f));
            Img(aba, cfg.aba, Color.white);
            var titulo = Txt(Esticar(Ret("Titulo", aba, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero)), "FASE CONCLUÍDA!", 60f, new Color(0.02f, 0.12f, 0.35f), TextAlignmentOptions.Center);
            titulo.outlineWidth = 0f;
            Txt(Ret("NomeFase", painel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -72f), new Vector2(800f, 46f)),
                FactoryEscape.GameFlow.LevelName(FactoryEscape.GameFlow.Level), 32f, Cores.Ouro, TextAlignmentOptions.Center);

            // estrelas: três vagas vazias + as cheias que caem dentro delas
            var vagas = new RectTransform[3];
            var cheias = new RectTransform[3];
            for (int i = 0; i < 3; i++)
            {
                float x = (i - 1) * 190f;
                float y = -215f + (i == 1 ? 26f : 0f);
                float tam = i == 1 ? 175f : 145f;
                float giro = (i - 1) * -10f;
                vagas[i] = Ret("Vaga_" + i, painel, new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(x, y), new Vector2(tam, tam));
                vagas[i].localRotation = Quaternion.Euler(0f, 0f, giro);
                Img(vagas[i], cfg.estrelaVazia != null ? cfg.estrelaVazia : cfg.circulo, cfg.estrelaVazia != null ? Color.white : new Color(0.1f, 0.2f, 0.4f, 0.9f));
                cheias[i] = Ret("Estrela_" + i, vagas[i], new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(tam, tam));
                Img(cheias[i], cfg.estrelaCheia != null ? cfg.estrelaCheia : cfg.circulo, cfg.estrelaCheia != null ? Color.white : Cores.Ouro);
                cheias[i].gameObject.SetActive(false);
                vagas[i].localScale = Vector3.zero;
            }

            // estatísticas (o valor numérico sobe contando)
            string Tempo(float seg) { int t = Mathf.FloorToInt(seg); return (t / 60).ToString("00") + ":" + (t % 60).ToString("00"); }
            var linhas = new List<(string nome, int valor, string extra, bool ehTempo)>
            {
                ("Tempo", Mathf.FloorToInt(tempo), recorde ? "  <color=#FFD033>RECORDE!</color>" : (melhorAntes > 0f ? "  <size=22><color=#9FC4FF>melhor " + Tempo(melhorAntes) + "</color></size>" : ""), true),
                ("Rocklets", rocklets, "", false),
            };
            if (EstatisticasFase.BausTotal > 0) linhas.Add(("Baús abertos", EstatisticasFase.BausAbertos, " / " + EstatisticasFase.BausTotal, false));
            if (EstatisticasFase.SelosTotal > 0) linhas.Add(("Selos Arcor", EstatisticasFase.SelosColetados, " / " + EstatisticasFase.SelosTotal, false));
            linhas.Add(("Inimigos derrotados", EstatisticasFase.MobsDerrotados, "", false));

            var objLinhas = new List<RectTransform>();
            var valores = new List<TextMeshProUGUI>();
            for (int i = 0; i < linhas.Count; i++)
            {
                var l = Ret("Linha_" + i, painel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -340f - i * 56f), new Vector2(740f, 50f));
                Img(l, cfg.cartao, new Color(0f, 0.08f, 0.25f, i % 2 == 0 ? 0.35f : 0.18f));
                Txt(Ret("Nome", l, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(22f, 0f), new Vector2(400f, 50f)), linhas[i].nome, 30f, Cores.Creme, TextAlignmentOptions.MidlineLeft);
                valores.Add(Txt(Ret("Valor", l, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-22f, 0f), new Vector2(440f, 50f)), "", 34f, Color.white, TextAlignmentOptions.MidlineRight));
                l.gameObject.AddComponent<CanvasGroup>().alpha = 0f;
                objLinhas.Add(l);
            }

            // botão-dica de continuar
            var rodape = Ret("Continuar", painel, new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(420f, 74f));
            Img(rodape, cfg.cartao, Cores.Ouro);
            var textoRodape = Txt(Esticar(Ret("Texto", rodape, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero)), "CONTINUAR  <size=24>(ENTER)</size>", 32f, new Color(0.02f, 0.12f, 0.35f), TextAlignmentOptions.Center);
            textoRodape.outlineWidth = 0f;
            rodape.localScale = Vector3.zero;

            // animação de entrada
            Sons.Tocar("fase_concluida", 0.9f, 0f);
            float t0 = 0f;
            while (t0 < 0.5f)
            {
                t0 += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t0 / 0.5f);
                fundo.color = new Color(0.01f, 0.05f, 0.14f, 0.78f * k);
                painel.localScale = Vector3.one * VoltaComMola(k);
                painel.anchoredPosition = new Vector2(0f, Mathf.Lerp(-140f, -40f, 1f - Mathf.Pow(1f - k, 3f)));
                yield return null;
            }
            painel.localScale = Vector3.one;

            // vagas das estrelas aparecem em sequência
            for (int i = 0; i < 3; i++) StartCoroutine(Escalar(vagas[i], 0f, 1f, 0.28f, i * 0.07f));
            yield return new WaitForSecondsRealtime(0.45f);

            // estrelas ganhas caem do alto, girando, e "carimbam" a vaga
            for (int i = 0; i < estrelas; i++)
            {
                var e = cheias[i];
                e.gameObject.SetActive(true);
                float t = 0f;
                while (t < 0.32f)
                {
                    t += Time.unscaledDeltaTime;
                    float k = Mathf.Clamp01(t / 0.32f);
                    float e2 = k * k;
                    e.localScale = Vector3.one * Mathf.Lerp(3.2f, 1f, e2);
                    e.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(-200f, 0f, 1f - Mathf.Pow(1f - k, 2f)));
                    e.GetComponent<Image>().color = new Color(1f, 1f, 1f, Mathf.Clamp01(k * 2.5f));
                    yield return null;
                }
                e.localRotation = Quaternion.identity;
                var somEstrela = Sons.Tocar("estrela", 0.85f, 0f);
                if (somEstrela != null) somEstrela.pitch = 1f + i * 0.12f;
                StartCoroutine(Pular(e, 1.28f));
                StartCoroutine(Tremer(painel, 9f + i * 4f, 0.18f));
                Faiscas(painel, vagas[i].anchoredPosition, 16 + i * 6);
                if (imgRaios != null && i == 0) StartCoroutine(AcenderRaios(imgRaios));
                yield return new WaitForSecondsRealtime(0.22f);
            }
            if (estrelas == 3) StartCoroutine(Confete(fundoRt, 90));

            // linhas entram deslizando e o número sobe contando
            for (int i = 0; i < objLinhas.Count; i++)
            {
                StartCoroutine(EntrarLinha(objLinhas[i]));
                StartCoroutine(Contar(valores[i], linhas[i].valor, linhas[i].extra, linhas[i].ehTempo, 0.45f));
                Sons.Tocar("ui_tique", 0.4f, 0.05f);
                yield return new WaitForSecondsRealtime(0.12f);
            }
            yield return new WaitForSecondsRealtime(0.35f);
            if (recorde) Notificar("NOVO RECORDE!", "Melhor tempo nesta fase", Cores.Ouro);

            StartCoroutine(Escalar(rodape, 0f, 1f, 0.3f, 0f));

            // espera o jogador (ou segue sozinho depois de um tempo)
            float espera = 0f;
            while (espera < 9f)
            {
                espera += Time.unscaledDeltaTime;
                float pulso = 1f + Mathf.Sin(espera * 4f) * 0.035f;
                if (espera > 0.35f) rodape.localScale = Vector3.one * pulso;
                if (raios != null) raios.localRotation = Quaternion.Euler(0f, 0f, -espera * 14f);
                var k = Keyboard.current;
                if (espera > 0.5f && k != null && (k.enterKey.wasPressedThisFrame || k.spaceKey.wasPressedThisFrame || k.eKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame)) break;
                var m = Mouse.current;
                if (espera > 0.5f && m != null && m.leftButton.wasPressedThisFrame) break;
                yield return null;
            }
            Sons.Tocar("ui_fechar", 0.6f, 0f);

            // saída: cartão sobe e tudo some
            t0 = 0f;
            while (t0 < 0.35f)
            {
                t0 += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t0 / 0.35f);
                grupo.alpha = 1f - k;
                painel.anchoredPosition = new Vector2(0f, -40f + k * k * 120f);
                yield return null;
            }
            FactoryEscape.PauseMenu.Bloqueado = false;
            continuar?.Invoke();
        }

        static float VoltaComMola(float k)
        {
            const float c1 = 1.70158f, c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(k - 1f, 3f) + c1 * Mathf.Pow(k - 1f, 2f);
        }

        IEnumerator Escalar(RectTransform alvo, float de, float ate, float duracao, float atraso)
        {
            if (atraso > 0f) yield return new WaitForSecondsRealtime(atraso);
            float t = 0f;
            while (alvo != null && t < duracao)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duracao);
                alvo.localScale = Vector3.one * Mathf.LerpUnclamped(de, ate, VoltaComMola(k));
                yield return null;
            }
            if (alvo != null) alvo.localScale = Vector3.one * ate;
        }

        IEnumerator Tremer(RectTransform alvo, float forca, float duracao)
        {
            Vector2 base_ = alvo.anchoredPosition;
            float t = 0f;
            while (t < duracao)
            {
                t += Time.unscaledDeltaTime;
                float f = forca * (1f - t / duracao);
                alvo.anchoredPosition = base_ + new Vector2(UnityEngine.Random.Range(-f, f), UnityEngine.Random.Range(-f, f));
                yield return null;
            }
            alvo.anchoredPosition = base_;
        }

        IEnumerator AcenderRaios(Image img)
        {
            float t = 0f;
            while (t < 0.6f)
            {
                t += Time.unscaledDeltaTime;
                img.color = new Color(1f, 0.85f, 0.35f, Mathf.Clamp01(t / 0.6f) * 0.55f);
                yield return null;
            }
        }

        IEnumerator EntrarLinha(RectTransform l)
        {
            var g = l.GetComponent<CanvasGroup>();
            Vector2 alvo = l.anchoredPosition;
            float t = 0f;
            while (t < 0.3f)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / 0.3f);
                float e = 1f - Mathf.Pow(1f - k, 3f);
                l.anchoredPosition = alvo + new Vector2((1f - e) * -90f, 0f);
                g.alpha = e;
                yield return null;
            }
            l.anchoredPosition = alvo;
            g.alpha = 1f;
        }

        IEnumerator Contar(TextMeshProUGUI txt, int valor, string extra, bool ehTempo, float duracao)
        {
            float t = 0f;
            while (t < duracao)
            {
                t += Time.unscaledDeltaTime;
                int v = Mathf.RoundToInt(Mathf.Lerp(0f, valor, 1f - Mathf.Pow(1f - Mathf.Clamp01(t / duracao), 2f)));
                txt.text = Formatar(v, ehTempo) + extra;
                yield return null;
            }
            txt.text = Formatar(valor, ehTempo) + extra;
        }

        static string Formatar(int v, bool ehTempo)
        {
            return ehTempo ? (v / 60).ToString("00") + ":" + (v % 60).ToString("00") : v.ToString();
        }

        // faíscas de interface saindo da estrela
        void Faiscas(RectTransform pai, Vector2 centro, int quantidade)
        {
            for (int i = 0; i < quantidade; i++)
            {
                var f = Ret("Faisca", pai, new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), centro, new Vector2(18f, 18f));
                Img(f, cfg.estrelaCheia != null ? cfg.estrelaCheia : cfg.circulo, i % 3 == 0 ? Color.white : new Color(1f, 0.85f, 0.3f));
                float ang = UnityEngine.Random.value * Mathf.PI * 2f;
                Vector2 vel = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * UnityEngine.Random.Range(260f, 620f);
                StartCoroutine(Particula(f, vel, 900f, UnityEngine.Random.Range(0.45f, 0.75f), UnityEngine.Random.Range(-500f, 500f)));
            }
        }

        IEnumerator Confete(RectTransform pai, int quantidade)
        {
            Color[] cores = { Cores.Ouro, Cores.Rosa, Cores.AzulClaro, Cores.Verde, Cores.Laranja, Color.white };
            for (int i = 0; i < quantidade; i++)
            {
                var c = Ret("Confete", pai, new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),
                            new Vector2(UnityEngine.Random.Range(-960f, 960f), UnityEngine.Random.Range(20f, 140f)), new Vector2(26f, 13f));
                Img(c, cfg.confete != null ? cfg.confete : null, cores[i % cores.Length]);
                Vector2 vel = new Vector2(UnityEngine.Random.Range(-120f, 120f), UnityEngine.Random.Range(-260f, -80f));
                StartCoroutine(Particula(c, vel, 380f, UnityEngine.Random.Range(2.4f, 3.6f), UnityEngine.Random.Range(-420f, 420f), true));
                if (i % 6 == 0) yield return null;
            }
        }

        IEnumerator Particula(RectTransform rt, Vector2 vel, float gravidade, float vida, float giro, bool balanca = false)
        {
            var img = rt.GetComponent<Image>();
            Color cor = img.color;
            float t = 0f, fase = UnityEngine.Random.value * 6f;
            while (rt != null && t < vida)
            {
                float dt = Time.unscaledDeltaTime;
                t += dt;
                vel.y -= gravidade * dt;
                if (balanca) { vel.y = Mathf.Max(vel.y, -320f); vel.x = Mathf.Sin(t * 3f + fase) * 140f; }
                rt.anchoredPosition += vel * dt;
                rt.localRotation = Quaternion.Euler(0f, balanca ? Mathf.Sin(t * 6f + fase) * 70f : 0f, rt.localEulerAngles.z + giro * dt);
                img.color = new Color(cor.r, cor.g, cor.b, cor.a * Mathf.Clamp01((vida - t) / 0.4f));
                yield return null;
            }
            if (rt != null) Destroy(rt.gameObject);
        }
    }
}
