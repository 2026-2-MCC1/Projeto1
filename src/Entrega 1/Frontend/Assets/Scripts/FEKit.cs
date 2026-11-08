using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FactoryEscape
{
    // =====================================================================
    //  FEKit - ferramentas compartilhadas por todas as cenas:
    //  paleta, construtores de UI, texturas geradas por código e entrada.
    // =====================================================================
    public static class FEKit
    {
        // ---------- Paleta ----------
        public static readonly Color ArcorBlue  = new Color(0.00f, 0.33f, 0.64f);
        public static readonly Color BrightBlue = new Color(0.30f, 0.65f, 1.00f);
        public static readonly Color Navy       = new Color(0.00f, 0.16f, 0.40f);
        public static readonly Color Sun        = new Color(1.00f, 0.82f, 0.20f);
        public static readonly Color Candy      = new Color(1.00f, 0.42f, 0.62f);
        public static readonly Color Oops       = new Color(1.00f, 0.45f, 0.40f);
        public static readonly Color Dim        = new Color(0.78f, 0.87f, 1.00f);
        public static readonly Color Ink        = new Color(0.01f, 0.09f, 0.22f);
        public static readonly Vector2 TL = new Vector2(0f, 1f);   // canto superior esquerdo

        public static Font CurrentFont;

        public static void Init(Font custom)
        {
            if (custom != null) CurrentFont = custom;
            else if (CurrentFont == null) CurrentFont = LoadDefaultFont();
        }

        static Font LoadDefaultFont()
        {
            Font f = null;
            try { f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
            if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return f;
        }

        // ---------- Entrada (funciona com Input antigo, novo ou os dois) ----------
        public static void EnsureEventSystem()
        {
#if UNITY_2023_1_OR_NEWER
            var es = Object.FindFirstObjectByType<EventSystem>();
#else
            var es = Object.FindObjectOfType<EventSystem>();
#endif
            if (es != null) return;
            var go = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
        }

        public static bool ConfirmPressed()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space);
#elif ENABLE_INPUT_SYSTEM
            var k = UnityEngine.InputSystem.Keyboard.current;
            return k != null && (k.enterKey.wasPressedThisFrame || k.spaceKey.wasPressedThisFrame);
#else
            return false;
#endif
        }

        public static bool GPressed()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.G);
#elif ENABLE_INPUT_SYSTEM
            var k = UnityEngine.InputSystem.Keyboard.current;
            return k != null && k.gKey.wasPressedThisFrame;
#else
            return false;
#endif
        }

        public static bool VPressed()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.V);
#elif ENABLE_INPUT_SYSTEM
            var k = UnityEngine.InputSystem.Keyboard.current;
            return k != null && k.vKey.wasPressedThisFrame;
#else
            return false;
#endif
        }

        // ---------- Canvas e painéis ----------
        public static Canvas CreateCanvas(string name, int sortingOrder)
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        public static RectTransform Panel(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            Stretch(rt);
            return rt;
        }

        public static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        // ---------- Texto, botão, linha ----------
        public static Text MakeText(Transform parent, string content, int size, Color color,
                                    Vector2 anchor, Vector2 pos, TextAnchor align,
                                    FontStyle style = FontStyle.Normal,
                                    float width = 900f, bool wrap = false)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.font = CurrentFont; t.text = content; t.fontSize = size; t.color = color;
            t.alignment = align; t.fontStyle = style; t.supportRichText = true;
            t.raycastTarget = false;
            t.horizontalOverflow = wrap ? HorizontalWrapMode.Wrap : HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.lineSpacing = 1.15f;

            // contorno azul-marinho = visual de desenho animado
            if (size >= 38)
            {
                var o = go.AddComponent<Outline>();
                o.effectColor = Navy;
                float d = size * 0.045f;
                o.effectDistance = new Vector2(d, -d);
            }

            var rt = t.rectTransform;
            int col = (int)align % 3, row = (int)align / 3;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(col * 0.5f, 1f - row * 0.5f);
            rt.anchoredPosition = pos;
            int lines = content.Split('\n').Length;
            rt.sizeDelta = new Vector2(width, size * 1.4f * lines);
            return t;
        }

        // Botão = somente texto (sem caixa). Fica amarelo, cresce e desliza no hover. PLIM no clique.
        public static Button MakeButton(Transform parent, string label, int size, Vector2 pos,
                                        UnityAction onClick, float width = 800f, bool clickSound = true)
        {
            var t = MakeText(parent, label, size, Color.white, TL, pos,
                             TextAnchor.UpperLeft, FontStyle.Bold, width);
            t.raycastTarget = true;

            var b = t.gameObject.AddComponent<Button>();
            b.targetGraphic = t;
            b.transition = Selectable.Transition.ColorTint;
            var c = b.colors;
            c.normalColor      = new Color(0.88f, 0.94f, 1f);
            c.highlightedColor = Sun;
            c.pressedColor     = Candy;
            c.selectedColor    = c.normalColor;
            c.fadeDuration     = 0.1f;
            b.colors = c;

            if (clickSound) b.onClick.AddListener(() => FESfx.Play(FESfx.Click));
            b.onClick.AddListener(onClick);

            t.gameObject.AddComponent<FEHoverNudge>();
            return b;
        }

        public static void MakeLine(Transform parent, Vector2 pos, float width)
        {
            var go = new GameObject("Line", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = Sun; img.raycastTarget = false;
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = TL; rt.pivot = TL;
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(width, 7f);
        }

        public static void Bouncy(Text t, float phase)
        {
            t.gameObject.AddComponent<FEBounce>().phase = phase;
        }

        // ---------- Texturas geradas por código (com cache) ----------
        static Texture2D factoryTex, gradientTex;
        static Sprite circleSprite;

        public static Texture2D FactoryTexture()
        {
            if (factoryTex != null) return factoryTex;
            const int S = 512;
            var tex = new Texture2D(S, S, TextureFormat.RGBA32, true);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            var px = new Color32[S * S];
            var rnd = new System.Random(7);

            for (int y = 0; y < S; y++)
            {
                for (int x = 0; x < S; x++)
                {
                    float u = x / (float)S, v = y / (float)S;

                    float n = 0.5f + 0.25f * Mathf.Sin(2 * Mathf.PI * (2 * u + v))
                                   + 0.25f * Mathf.Sin(2 * Mathf.PI * (3 * v - u));
                    Color c = new Color(0.10f + 0.04f * n, 0.28f + 0.06f * n, 0.55f + 0.08f * n);

                    // bolinhas (estilo azulejo de fábrica de doces)
                    float dx = (x % 64) - 32f, dy = (y % 64) - 32f;
                    if (dx * dx + dy * dy < 64f) c = Color.Lerp(c, BrightBlue, 0.18f);

                    // vigas verticais
                    int bx = x % 128;
                    if (bx < 12) c *= 0.7f;
                    else if (bx < 14) c = Color.Lerp(c, Color.white, 0.12f);

                    // cano azul com brilho + flanges amarelas
                    if (y >= 60 && y < 108)
                    {
                        float s = Mathf.Sin((y - 60) / 48f * Mathf.PI);
                        c = Color.Lerp(ArcorBlue * 0.8f, BrightBlue, s);
                        if (bx < 10) c = Color.Lerp(Sun * 0.7f, Sun, s);
                    }
                    else if (y >= 300 && y < 332)   // cano rosa
                    {
                        float s = Mathf.Sin((y - 300) / 32f * Mathf.PI);
                        c = Color.Lerp(Candy * 0.6f, Candy, s);
                    }

                    // faixa de aviso amarela e azul-marinho
                    if (y >= 200 && y < 224)
                    {
                        bool st = ((x + y) / 16) % 2 == 0;
                        c = st ? Sun * 0.95f : Navy;
                    }

                    // desgaste leve (a fábrica está abandonada...)
                    c *= 0.92f + 0.16f * (float)rnd.NextDouble();
                    c.a = 1f;
                    px[y * S + x] = c;
                }
            }
            tex.SetPixels32(px);
            tex.Apply();
            factoryTex = tex;
            return tex;
        }

        public static Texture2D Gradient()
        {
            if (gradientTex != null) return gradientTex;
            const int W = 256;
            var tex = new Texture2D(W, 1, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            for (int x = 0; x < W; x++)
            {
                float t = Mathf.Clamp01(x / (W * 0.75f));
                tex.SetPixel(x, 0, new Color(Ink.r, Ink.g, Ink.b, Mathf.Lerp(0.88f, 0.02f, t)));
            }
            tex.Apply();
            gradientTex = tex;
            return tex;
        }

        public static Sprite CircleSprite()
        {
            if (circleSprite != null) return circleSprite;
            const int S = 64;
            var tex = new Texture2D(S, S, TextureFormat.RGBA32, false);
            float r = S * 0.5f - 1f;
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(S * 0.5f, S * 0.5f));
                    float disc = Mathf.Clamp01((r - d) / 1.5f);
                    float rim = Mathf.SmoothStep(0.7f, 1f, d / r);   // borda mais forte (efeito bolha)
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, disc * (0.55f + 0.45f * rim)));
                }
            tex.Apply();
            circleSprite = Sprite.Create(tex, new Rect(0, 0, S, S), new Vector2(0.5f, 0.5f));
            return circleSprite;
        }
    }

    // =====================================================================
    //  FESfx - sons PLIM! gerados por código (e música que continua entre cenas)
    // =====================================================================
    public static class FESfx
    {
        public static float Volume = 0.8f;

        static AudioSource sfx, musicSrc;
        static AudioClip click, hover, win, lose;
        static AudioClip oClick, oHover, oWin, oLose;   // sons personalizados (opcional)

        public static AudioClip Click
        {
            get { if (oClick != null) return oClick;
                  if (click == null) click = Jingle(new[] { 1318.5f }, 0f, 0.45f, 0.9f);
                  return click; }
        }
        public static AudioClip Hover
        {
            get { if (oHover != null) return oHover;
                  if (hover == null) hover = Jingle(new[] { 1975.5f }, 0f, 0.12f, 0.35f);
                  return hover; }
        }
        public static AudioClip Win
        {
            get { if (oWin != null) return oWin;
                  if (win == null) win = Jingle(new[] { 523.25f, 659.25f, 783.99f, 1046.5f }, 0.13f, 0.5f, 0.8f);
                  return win; }
        }
        public static AudioClip Lose
        {
            get { if (oLose != null) return oLose;
                  if (lose == null) lose = Jingle(new[] { 392f, 349.23f, 293.66f, 196f }, 0.2f, 0.5f, 0.8f);
                  return lose; }
        }

        public static void SetOverrides(AudioClip c, AudioClip h, AudioClip w, AudioClip l, float vol)
        {
            if (c != null) oClick = c;
            if (h != null) oHover = h;
            if (w != null) oWin = w;
            if (l != null) oLose = l;
            Volume = vol;
        }

        // A fonte de áudio fica fora da cena (DontDestroyOnLoad):
        // o PLIM termina de tocar mesmo quando a próxima cena já carregou.
        static AudioSource MakeSource(string name)
        {
            var go = new GameObject(name);
            Object.DontDestroyOnLoad(go);
            var a = go.AddComponent<AudioSource>();
            a.playOnAwake = false;
            a.spatialBlend = 0f;
            return a;
        }

        public static void Play(AudioClip clip, float pitch = 1f)
        {
            if (clip == null) return;
            if (sfx == null) sfx = MakeSource("FE_SFX");
            sfx.pitch = pitch;
            sfx.PlayOneShot(clip, Volume);
        }

        public static void PlayMusic(AudioClip clip)
        {
            if (clip == null) return;
            if (musicSrc == null) { musicSrc = MakeSource("FE_MUSIC"); musicSrc.loop = true; }
            if (musicSrc.clip == clip && musicSrc.isPlaying) return;   // não reinicia entre cenas
            musicSrc.clip = clip;
            musicSrc.Play();
        }

        // Sininhos: cada nota é um "plim" que vai sumindo. Uma nota só = o PLIM do botão.
        public static AudioClip Jingle(float[] freqs, float step, float ring, float amp)
        {
            const int rate = 44100;
            int total = Mathf.CeilToInt((step * (freqs.Length - 1) + ring) * rate);
            var data = new float[total];
            for (int k = 0; k < freqs.Length; k++)
            {
                int start = Mathf.RoundToInt(step * k * rate);
                int len = Mathf.Min(Mathf.RoundToInt(ring * rate), total - start);
                float f = freqs[k];
                for (int i = 0; i < len; i++)
                {
                    float t = i / (float)rate;
                    float env = Mathf.Min(1f, t * 400f) * Mathf.Exp(-t * 6f / ring);
                    float s = Mathf.Sin(2f * Mathf.PI * f * t)
                            + 0.35f * Mathf.Sin(2f * Mathf.PI * f * 2f * t)
                            + 0.15f * Mathf.Sin(2f * Mathf.PI * f * 3.01f * t);
                    data[start + i] += s * env * amp * 0.35f;
                }
            }
            var clip = AudioClip.Create("sfx", total, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }

    // =====================================================================
    //  Fundo animado: textura andando + bolhas subindo + luz piscando
    // =====================================================================
    public class FEBackground : MonoBehaviour
    {
        RawImage img;
        Image flicker;
        Rect uv = new Rect(0, 0, 3f, 1.7f);
        Vector2 speed;
        RectTransform[] bub;
        float[] bubSpeed, bubPhase;

        public static FEBackground Create(Transform parent, Texture tex, Vector2 scrollSpeed, int bubbleCount)
        {
            var go = new GameObject("Background", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            FEKit.Stretch((RectTransform)go.transform);
            go.transform.SetAsFirstSibling();   // fica atrás de todo o resto
            var bg = go.AddComponent<FEBackground>();
            bg.Build(tex != null ? tex : FEKit.FactoryTexture(), scrollSpeed, bubbleCount);
            return bg;
        }

        void Build(Texture tex, Vector2 scrollSpeed, int bubbleCount)
        {
            speed = scrollSpeed;

            var scroll = new GameObject("Scroll", typeof(RectTransform), typeof(RawImage));
            scroll.transform.SetParent(transform, false);
            FEKit.Stretch((RectTransform)scroll.transform);
            img = scroll.GetComponent<RawImage>();
            img.texture = tex;
            img.raycastTarget = false;

            // bolhas
            int n = Mathf.Max(0, bubbleCount);
            bub = new RectTransform[n];
            bubSpeed = new float[n];
            bubPhase = new float[n];
            Sprite circle = FEKit.CircleSprite();
            Color[] cols = { Color.white, FEKit.BrightBlue, FEKit.Sun, FEKit.Candy };
            for (int i = 0; i < n; i++)
            {
                var go = new GameObject("Bubble", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(transform, false);
                var im = go.GetComponent<Image>();
                im.sprite = circle;
                im.raycastTarget = false;
                Color c = cols[i % cols.Length]; c.a = 0.30f;
                im.color = c;

                var rt = im.rectTransform;
                float size = Random.Range(40f, 150f);
                rt.anchorMin = rt.anchorMax = new Vector2(Random.Range(0.30f, 0.98f), 0f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(size, size);
                rt.anchoredPosition = new Vector2(0f, Random.Range(-150f, 1200f));
                bub[i] = rt;
                bubSpeed[i] = Random.Range(25f, 70f);
                bubPhase[i] = Random.Range(0f, 6.28f);
            }

            // degradê à esquerda para o texto ficar legível
            var shade = new GameObject("Shade", typeof(RectTransform), typeof(RawImage));
            shade.transform.SetParent(transform, false);
            FEKit.Stretch((RectTransform)shade.transform);
            var sh = shade.GetComponent<RawImage>();
            sh.texture = FEKit.Gradient();
            sh.raycastTarget = false;

            // luz piscando
            var fl = new GameObject("Flicker", typeof(RectTransform), typeof(Image));
            fl.transform.SetParent(transform, false);
            FEKit.Stretch((RectTransform)fl.transform);
            flicker = fl.GetComponent<Image>();
            flicker.raycastTarget = false;
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            uv.x = Mathf.Repeat(uv.x + speed.x * dt, 1f);
            uv.y = Mathf.Repeat(uv.y + speed.y * dt, 1f);
            if (img != null) img.uvRect = uv;

            for (int i = 0; i < bub.Length; i++)
            {
                Vector2 p = bub[i].anchoredPosition;
                p.y += bubSpeed[i] * dt;
                if (p.y > 1250f) p.y = -170f;
                p.x = Mathf.Sin(Time.unscaledTime * 0.8f + bubPhase[i]) * 30f;
                bub[i].anchoredPosition = p;
            }

            if (flicker != null)
            {
                float a = 0.04f + 0.04f * Mathf.PerlinNoise(Time.unscaledTime * 3f, 0f);
                if (Random.value > 0.985f) a = 0f;
                flicker.color = new Color(1f, 0.95f, 0.7f, a);
            }
        }
    }

    // Título que pula e balança de leve
    public class FEBounce : MonoBehaviour
    {
        public float phase;
        RectTransform rt; Vector2 basePos;
        void Awake() { rt = (RectTransform)transform; }
        void OnEnable() { basePos = rt.anchoredPosition; }
        void OnDisable()
        {
            if (rt == null) return;
            rt.anchoredPosition = basePos; rt.localRotation = Quaternion.identity;
        }
        void Update()
        {
            float t = Time.unscaledTime * 2.2f + phase;
            rt.anchoredPosition = basePos + new Vector2(0f, Mathf.Sin(t) * 9f);
            rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 0.7f) * 1.5f);
        }
    }

    // Botão: desliza para a direita, cresce e faz "tic" no hover
    public class FEHoverNudge : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        RectTransform rt; Vector2 basePos; bool hover;
        void Awake() { rt = (RectTransform)transform; }
        void OnEnable() { basePos = rt.anchoredPosition; hover = false; }
        void OnDisable()
        {
            if (rt == null) return;
            rt.anchoredPosition = basePos; rt.localScale = Vector3.one; hover = false;
        }
        public void OnPointerEnter(PointerEventData e) { hover = true; FESfx.Play(FESfx.Hover); }
        public void OnPointerExit(PointerEventData e) { hover = false; }
        void Update()
        {
            float k = 15f * Time.unscaledDeltaTime;
            Vector2 target = basePos + (hover ? new Vector2(24f, 0f) : Vector2.zero);
            rt.anchoredPosition = Vector2.Lerp(rt.anchoredPosition, target, k);
            rt.localScale = Vector3.Lerp(rt.localScale, hover ? Vector3.one * 1.08f : Vector3.one, k);
        }
    }
}
