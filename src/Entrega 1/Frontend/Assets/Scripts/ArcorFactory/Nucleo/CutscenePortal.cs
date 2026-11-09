using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ArcorFactory
{
    // Cutscene do portal da Fase 2 para a Fase 3
    public class CutscenePortal : MonoBehaviour
    {
        public static bool Ativa { get; private set; }

        // Ligado pela entrada; a próxima fase toca a chegada
        public static bool ChegadaPendente;

        static readonly Color CorPortal = new Color(0.45f, 0.75f, 1f);

        CameraCrash camCrash;
        Camera cam;
        PlayerMovement movimento;
        CharacterController corpo;
        Image clarao;
        Canvas canvas;

        // Interface do jogo (HUD) some suavemente durante a cena e volta no fim
        readonly System.Collections.Generic.List<CanvasGroup> huds = new System.Collections.Generic.List<CanvasGroup>();
        float hudAlvo = 1f, hudAtual = 1f;

        void EsconderHud(bool esconder)
        {
            if (huds.Count == 0)
            {
                foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
                {
                    if (c == canvas || !c.isRootCanvas || c.renderMode == RenderMode.WorldSpace) continue;
                    var g = c.GetComponent<CanvasGroup>();
                    if (g == null) g = c.gameObject.AddComponent<CanvasGroup>();
                    huds.Add(g);
                }
            }
            hudAlvo = esconder ? 0f : 1f;
            if (!esconder) AplicarHud(1f); // volta na hora (o resumo da fase usa a HUD)
        }

        void AplicarHud(float a)
        {
            hudAtual = a;
            foreach (var g in huds) if (g != null) g.alpha = a;
        }

        // Pose de cinema que está sendo misturada com a câmera de jogo
        Vector3 posCine;
        Quaternion rotCine = Quaternion.identity;
        float pesoCine, fovCine = -1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reiniciar() { Ativa = false; ChegadaPendente = false; }

        // API
        public static void Entrar(Transform portal, Action depois)
        {
            if (Ativa) return;
            var go = new GameObject("[ArcorFactory] Cutscene Portal");
            var c = go.AddComponent<CutscenePortal>();
            c.StartCoroutine(c.SequenciaEntrada(portal, depois));
        }

        // Chamado pelo InicializadorArcor quando uma fase carrega
        public static void TentarChegada()
        {
            if (!ChegadaPendente) return;
            ChegadaPendente = false;
            if (FindFirstObjectByType<PlayerMovement>() == null) return;
            var go = new GameObject("[ArcorFactory] Cutscene Chegada");
            var c = go.AddComponent<CutscenePortal>();
            c.StartCoroutine(c.SequenciaChegada());
        }

        // comum
        bool Preparar()
        {
            cam = Camera.main;
            if (cam == null) return false;
            camCrash = cam.GetComponent<CameraCrash>();
            movimento = FindFirstObjectByType<PlayerMovement>();
            if (movimento == null) return false;
            corpo = movimento.GetComponent<CharacterController>();
            Ativa = true;
            FactoryEscape.PauseMenu.Bloqueado = true;
            movimento.controleTravado = true;
            movimento.direcaoCutscene = Vector3.zero;

            var cgo = new GameObject("Canvas_Cutscene", typeof(Canvas), typeof(CanvasScaler));
            cgo.transform.SetParent(transform, false);
            canvas = cgo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 300;
            var sc = cgo.GetComponent<CanvasScaler>();
            sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = new Vector2(1920, 1080);
            sc.matchWidthOrHeight = 0.5f;
            var cl = new GameObject("Clarao", typeof(RectTransform), typeof(Image));
            cl.transform.SetParent(cgo.transform, false);
            var rt = (RectTransform)cl.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
            clarao = cl.GetComponent<Image>();
            clarao.raycastTarget = false;
            clarao.color = new Color(1f, 1f, 1f, 0f);
            Barras(0f);
            return true;
        }

        void Devolver()
        {
            if (movimento != null)
            {
                movimento.enabled = true;
                movimento.direcaoCutscene = Vector3.zero;
                movimento.controleTravado = false;
            }
            if (camCrash != null) camCrash.LiberarCinema();
            hudAlvo = 1f; // a HUD volta aparecendo aos poucos
            FactoryEscape.PauseMenu.Bloqueado = false;
            Ativa = false;
        }

        // A câmera de jogo aplica a mistura depois de todos os Updates
        void LateUpdate()
        {
            if (camCrash != null)
            {
                if (pesoCine > 0f) camCrash.DefinirCinema(posCine, rotCine, pesoCine, fovCine);
                else camCrash.LiberarCinema();
            }
            else if (cam != null && pesoCine > 0f)
            {
                cam.transform.SetPositionAndRotation(Vector3.Lerp(cam.transform.position, posCine, pesoCine), Quaternion.Slerp(cam.transform.rotation, rotCine, pesoCine));
            }
            if (barraCima != null) AtualizarBarras();
            if (!Mathf.Approximately(hudAtual, hudAlvo)) AplicarHud(Mathf.MoveTowards(hudAtual, hudAlvo, Time.unscaledDeltaTime * 3f));
        }

        static float Suave(float k) { k = Mathf.Clamp01(k); return k * k * (3f - 2f * k); }
        static float MaisSuave(float k) { k = Mathf.Clamp01(k); return k * k * k * (k * (k * 6f - 15f) + 10f); }

        void Mirar(Vector3 de, Vector3 para)
        {
            posCine = de;
            Vector3 d = para - de;
            if (d.sqrMagnitude > 1e-6f) rotCine = Quaternion.LookRotation(d);
        }

        IEnumerator Clarear(float de, float ate, float duracao)
        {
            float t = 0f;
            while (t < duracao)
            {
                t += Time.unscaledDeltaTime;
                clarao.color = new Color(1f, 1f, 1f, Mathf.Lerp(de, ate, Suave(t / duracao)));
                yield return null;
            }
            clarao.color = new Color(1f, 1f, 1f, ate);
        }

        // Faixas pretas de cinema (entram e saem suavemente)
        RectTransform barraCima, barraBaixo;
        float barrasAlvo, barrasAtual;

        void Barras(float alvo)
        {
            barrasAlvo = alvo;
            if (barraCima != null) return;
            barraCima = Barra("BarraCima", new Vector2(0f, 1f));
            barraBaixo = Barra("BarraBaixo", new Vector2(0f, 0f));
        }

        RectTransform Barra(string nome, Vector2 ancora)
        {
            var go = new GameObject(nome, typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(canvas.transform, false);
            rt.SetAsFirstSibling();
            rt.anchorMin = ancora; rt.anchorMax = new Vector2(1f, ancora.y);
            rt.pivot = new Vector2(0.5f, ancora.y);
            rt.sizeDelta = new Vector2(0f, 0f);
            var img = go.GetComponent<Image>();
            img.color = Color.black;
            img.raycastTarget = false;
            return rt;
        }

        void AtualizarBarras()
        {
            barrasAtual = Mathf.MoveTowards(barrasAtual, barrasAlvo, Time.unscaledDeltaTime * 2.2f);
            float h = 110f * Suave(barrasAtual);
            barraCima.sizeDelta = new Vector2(0f, h);
            barraBaixo.sizeDelta = new Vector2(0f, h);
        }

        // ENTRADA (Fase 2)
        IEnumerator SequenciaEntrada(Transform gatilho, Action depois)
        {
            if (!Preparar()) { depois?.Invoke(); Destroy(gameObject); yield break; }
            Barras(1f);
            EsconderHud(true);

            // centro e frente do portal: o anel visível fica ao lado do gatilho
            Transform anel = gatilho;
            if (gatilho.parent != null)
            {
                var achado = gatilho.parent.Find("Anel_Portal");
                if (achado != null) anel = achado;
            }
            Vector3 centro = anel.position;
            Vector3 normal = anel.forward; normal.y = 0f;
            if (normal.sqrMagnitude < 0.01f) normal = Vector3.forward;
            normal.Normalize();
            Transform jog = movimento.transform;
            if (Vector3.Dot(jog.position - centro, normal) < 0f) normal = -normal; // normal aponta para o lado da tartaruga
            Transform nucleo = anel.Find("Nucleo");
            Transform disco = anel.Find("Disco");
            Vector3 escalaNucleo = nucleo != null ? nucleo.localScale : Vector3.one;

            // ponto no chão em frente ao portal, onde ela para
            Vector3 parada = centro + normal * 1.8f;
            parada.y = jog.position.y;

            Sons.TocarEm("portal_entrar", centro, 1f, 0f, 40f);

            var luzGo = new GameObject("Luz_Portal");
            luzGo.transform.position = centro + normal * 0.6f;
            var luz = luzGo.AddComponent<Light>();
            luz.type = LightType.Point; luz.color = CorPortal; luz.range = 9f; luz.intensity = 0f; luz.shadows = LightShadows.None;

            var pivo = new GameObject("Pivo_Vortice").transform;
            pivo.position = centro + normal * 0.15f;
            pivo.rotation = Quaternion.LookRotation(normal);
            var vortice = Efeitos.Vortice(pivo, CorPortal, 2.1f, 20f);
            var emV = vortice.emission;
            var aura = Efeitos.Aura(pivo, new Color(0.7f, 0.9f, 1f), 1.6f, 10f);

            // lado da câmera: o lado em que ela já está (não cruza a linha da cena)
            Vector3 lado = Vector3.Cross(Vector3.up, normal).normalized;
            if (Vector3.Dot(lado, cam.transform.position - centro) < 0f) lado = -lado;

            float fovBase = cam.fieldOfView;
            float t = 0f, pulso = 0f;
            const float dAndar = 1.6f, dEncarar = 0.55f, dSugar = 1.5f;
            float total = dAndar + dEncarar + dSugar;
            Vector3 escala0 = jog.localScale;
            Vector3 inicioSuccao = Vector3.zero;
            bool sugando = false;
            Quaternion rotSuccao = Quaternion.identity;
            ParticleSystem rastro = null;

            while (t < total)
            {
                float dt = Time.deltaTime;
                t += dt;
                float kTotal = t / total;

                // câmera: órbita lateral que se aproxima aos poucos
                Vector3 alvoOlhar = Vector3.Lerp(jog.position + Vector3.up * 0.8f, centro, 0.35f + 0.4f * MaisSuave(kTotal));
                float orbita = Mathf.Lerp(-22f, 10f, MaisSuave(kTotal));
                float dist = Mathf.Lerp(8.5f, 5.2f, MaisSuave(kTotal));
                Vector3 dirCam = Quaternion.AngleAxis(orbita, Vector3.up) * (lado * 0.82f + normal * 0.58f).normalized;
                Mirar(alvoOlhar + dirCam * dist + Vector3.up * Mathf.Lerp(2.4f, 1.3f, MaisSuave(kTotal)), alvoOlhar);
                pesoCine = MaisSuave(t / 1.3f);
                fovCine = Mathf.Lerp(fovBase, fovBase + 8f, MaisSuave((t - dAndar - dEncarar) / dSugar));

                // portal: gira cada vez mais rápido, pulsa (distorção) e acende
                float energia = Mathf.Clamp01(t / (dAndar + dEncarar)) + (sugando ? (t - dAndar - dEncarar) / dSugar : 0f);
                if (nucleo != null)
                {
                    nucleo.Rotate(Vector3.forward, (90f + 500f * energia) * dt, Space.Self);
                    float p = 1f + Mathf.Sin(t * (5f + 10f * energia)) * 0.025f * (1f + 2f * energia);
                    nucleo.localScale = new Vector3(escalaNucleo.x * p, escalaNucleo.y * p, escalaNucleo.z);
                }
                if (disco != null) disco.Rotate(Vector3.forward, -(30f + 160f * energia) * dt, Space.Self);
                luz.intensity = Mathf.Lerp(0f, 10f, Mathf.Clamp01(energia * 0.6f));
                emV.rateOverTime = Mathf.Lerp(20f, 160f, Mathf.Clamp01(energia * 0.6f));
                pulso -= dt;
                if (pulso <= 0f && t > 0.6f)
                {
                    pulso = Mathf.Lerp(0.5f, 0.18f, Mathf.Clamp01(energia * 0.5f));
                    Efeitos.Anel(centro + normal * 0.2f, new Color(0.6f, 0.85f, 1f, 0.6f), 4.6f - Mathf.Clamp01(energia * 0.5f) * 2f);
                }

                // tartaruga
                if (t < dAndar)
                {
                    // anda de verdade até a frente do portal (animação de andar no ritmo)
                    Vector3 falta = parada - jog.position; falta.y = 0f;
                    float vel = Mathf.Clamp01(falta.magnitude / 1.2f) * 0.75f;
                    movimento.direcaoCutscene = falta.sqrMagnitude > 0.01f ? falta.normalized * vel : Vector3.zero;
                }
                else if (t < dAndar + dEncarar)
                {
                    // para e vira de frente para o portal
                    movimento.direcaoCutscene = Vector3.zero;
                    Quaternion olhar = Quaternion.LookRotation(-normal);
                    jog.rotation = Quaternion.RotateTowards(jog.rotation, olhar, 360f * dt);
                }
                else
                {
                    if (!sugando)
                    {
                        // a partir daqui ela flutua: o movimento de jogo para (sem gravidade)
                        sugando = true;
                        movimento.enabled = false;
                        inicioSuccao = jog.position;
                        rotSuccao = jog.rotation;
                        rastro = Efeitos.Rastro(jog, new Color(0.6f, 0.85f, 1f, 0.8f));
                    }
                    float k = Mathf.Clamp01((t - dAndar - dEncarar) / dSugar);
                    float sobe = MaisSuave(Mathf.Clamp01(k / 0.35f)); // primeiro é erguida
                    float puxa = Mathf.Pow(Mathf.Clamp01((k - 0.15f) / 0.85f), 2.2f); // depois puxada, acelerando
                    float ang = puxa * 720f * Mathf.Deg2Rad;
                    Vector3 espiral = (lado * Mathf.Cos(ang) + Vector3.up * Mathf.Sin(ang)) * (1f - puxa) * 0.35f * sobe;
                    Vector3 erguida = inicioSuccao + Vector3.up * 0.6f * sobe;
                    jog.position = Vector3.Lerp(erguida, centro, puxa) + espiral;
                    jog.rotation = rotSuccao * Quaternion.Euler(0f, 1080f * puxa * puxa + 90f * sobe, 25f * Mathf.Sin(k * Mathf.PI));
                    jog.localScale = escala0 * Mathf.Lerp(1f, 0.04f, puxa);
                }
                yield return null;
            }

            // some num estalo de luz
            foreach (var r in jog.GetComponentsInChildren<Renderer>()) r.enabled = false;
            if (rastro != null) rastro.Stop();
            Efeitos.Brilhos(centro, Color.white, 40, 0.6f, 4f);
            Efeitos.Anel(centro + normal * 0.3f, Color.white, 7f);
            if (camCrash != null) camCrash.Tremer(0.1f, 0.35f);
            yield return Clarear(0f, 1f, 0.35f);

            ChegadaPendente = true;
            vortice.Stop();
            aura.Stop();
            yield return new WaitForSecondsRealtime(0.15f);

            // o resumo aparece por cima do branco
            Barras(0f);
            EsconderHud(false);
            StartCoroutine(Clarear(1f, 0f, 0.6f));
            FactoryEscape.PauseMenu.Bloqueado = false;
            Ativa = false;
            depois?.Invoke();
        }

        // CHEGADA (Fase 3)
        IEnumerator SequenciaChegada()
        {
            if (!Preparar()) { Destroy(gameObject); yield break; }
            clarao.color = Color.white;
            Barras(1f);
            yield return null; // deixa a fase terminar de montar (HUD, câmera)
            EsconderHud(true);
            AplicarHud(0f);

            Transform jog = movimento.transform;
            // só os renderers que estavam ligados
            var renderers = new System.Collections.Generic.List<Renderer>();
            foreach (var r in jog.GetComponentsInChildren<Renderer>())
            {
                if (!r.enabled) continue;
                renderers.Add(r);
                r.enabled = false;
            }
            movimento.enabled = false;
            Vector3 escala0 = jog.localScale;
            if (camCrash != null) camCrash.Encaixar();

            // alvo do voo: a saída mais longe da tartaruga
            Vector3 saida = jog.position + jog.forward * 30f;
            float melhor = -1f;
            foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (mb == null) continue;
                string n = mb.GetType().Name;
                if (n != "SaidaFase" && n != "PortaFinal" && n != "TransicaoFase" && n != "PortalFase") continue;
                float d = (mb.transform.position - jog.position).sqrMagnitude;
                if (d > melhor) { melhor = d; saida = mb.transform.position; }
            }

            // voo em curva (Bézier) do alto da saída até a câmera de jogo
            Vector3 ida = (saida - jog.position); ida.y = 0f;
            Vector3 dirIda = ida.sqrMagnitude > 1f ? ida.normalized : jog.forward;
            Vector3 lateral = Vector3.Cross(Vector3.up, dirIda);
            Vector3 p0 = saida - dirIda * 6f + lateral * 9f + Vector3.up * 16f;
            Vector3 p1 = Vector3.Lerp(saida, jog.position, 0.45f) + lateral * 12f + Vector3.up * 18f;
            Vector3 p2 = jog.position - dirIda * 4f + lateral * 6f + Vector3.up * 9f;
            Vector3 olhar0 = saida + Vector3.up * 1f;

            pesoCine = 1f;
            Mirar(p0, olhar0);
            StartCoroutine(Clarear(1f, 0f, 0.9f));

            var faixa = CriarFaixa();
            Sons.Tocar("fase_final", 0.9f, 0f);
            StartCoroutine(AnimarFaixa(faixa, true));

            float durVoo = 4.2f, t = 0f;
            bool faixaSaiu = false, portalAberto = false;
            Transform disco = null, pivo = null;
            ParticleSystem vortice = null;
            Light luz = null;
            Vector3 centroPortal = jog.position + Vector3.up * 0.9f;

            while (t < durVoo)
            {
                float vel = Pulou() && t > 0.4f ? 3f : 1f; // ENTER acelera, sem cortar
                t += Time.deltaTime * vel;
                float k = Mathf.Clamp01(t / durVoo);
                float e = MaisSuave(k);

                // ponto da curva e para onde olha (da saída para a tartaruga)
                Vector3 pc = Bezier(p0, p1, p2, Mathf.Clamp01(e / 0.82f));
                Vector3 olhar = Vector3.Lerp(olhar0, jog.position + Vector3.up * 1.1f, MaisSuave(Mathf.Clamp01((k - 0.25f) / 0.55f)));
                Mirar(pc, olhar);
                // nos últimos 30% a pose de cinema cede para a câmera de jogo
                pesoCine = 1f - MaisSuave(Mathf.Clamp01((k - 0.7f) / 0.3f));

                if (!faixaSaiu && k > 0.55f)
                {
                    faixaSaiu = true;
                    StartCoroutine(AnimarFaixa(faixa, false));
                }

                // o portal abre atrás da tartaruga quando a câmera começa a descer
                if (!portalAberto && k > 0.6f)
                {
                    portalAberto = true;
                    pivo = new GameObject("Portal_Chegada").transform;
                    pivo.position = centroPortal - jog.forward * 0.4f;
                    pivo.rotation = Quaternion.LookRotation(jog.forward);
                    disco = CriarDiscoPortal(pivo);
                    vortice = Efeitos.Vortice(pivo, CorPortal, 1.3f, 70f);
                    var luzGo = new GameObject("Luz");
                    luzGo.transform.SetParent(pivo, false);
                    luz = luzGo.AddComponent<Light>();
                    luz.type = LightType.Point; luz.color = CorPortal; luz.range = 7f; luz.intensity = 0f; luz.shadows = LightShadows.None;
                    Sons.TocarEm("portal_entrar", centroPortal, 0.6f, 0f);
                }
                if (portalAberto)
                {
                    float kp = Mathf.Clamp01((k - 0.6f) / 0.25f);
                    disco.localScale = Vector3.one * 2.6f * MaisSuave(kp);
                    disco.Rotate(Vector3.forward, 360f * Time.deltaTime, Space.Self);
                    luz.intensity = 6f * kp;
                }
                yield return null;
            }
            pesoCine = 0f;
            Barras(0f);

            // a tartaruga sai do portal com um pulinho
            Sons.Tocar("portal_chegada", 0.9f, 0f);
            foreach (var r in renderers) if (r != null) r.enabled = true;
            Efeitos.Brilhos(centroPortal, Color.white, 30, 0.5f, 3f);
            Efeitos.Anel(centroPortal, new Color(0.7f, 0.9f, 1f, 0.9f), 4f);
            Vector3 pBase = jog.position;
            t = 0f;
            while (t < 0.45f)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / 0.45f);
                const float c1 = 1.70158f, c3 = c1 + 1f;
                float mola = 1f + c3 * Mathf.Pow(k - 1f, 3f) + c1 * Mathf.Pow(k - 1f, 2f);
                jog.localScale = escala0 * Mathf.Max(0.02f, mola);
                jog.position = pBase + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.7f;
                if (disco != null) disco.Rotate(Vector3.forward, 360f * Time.deltaTime, Space.Self);
                yield return null;
            }
            jog.localScale = escala0;
            jog.position = pBase;
            Efeitos.Poeira(pBase + Vector3.down * 0.9f, 0.7f);
            Devolver();

            // o portal fecha sozinho
            if (vortice != null) vortice.Stop();
            t = 0f;
            while (t < 0.4f && disco != null)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / 0.4f);
                disco.localScale = Vector3.one * 2.6f * (1f - k * k);
                if (luz != null) luz.intensity = 6f * (1f - k);
                yield return null;
            }
            if (pivo != null) Destroy(pivo.gameObject, 1.2f);
            if (faixa != null) Destroy(faixa.gameObject, 0.6f);
            if (NivelAtual() >= FactoryEscape.GameFlow.TotalLevels) HUDArcor.Notificar("FASE FINAL", "Encontre a saída da fábrica!", Cores.Ouro, 3.6f);
            Destroy(gameObject, 1f);
        }

        static Vector3 Bezier(Vector3 a, Vector3 b, Vector3 c, float t)
        {
            float u = 1f - t;
            return u * u * a + 2f * u * t * b + t * t * c;
        }

        static int NivelAtual()
        {
            int i = Array.IndexOf(FactoryEscape.GameFlow.LevelScenes, UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            return i >= 0 ? i + 1 : FactoryEscape.GameFlow.Level;
        }

        static bool Pulou()
        {
            var k = Keyboard.current;
            return k != null && (k.enterKey.isPressed || k.spaceKey.isPressed || k.escapeKey.isPressed);
        }

        Transform CriarDiscoPortal(Transform pai)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(go.GetComponent<Collider>());
            go.name = "Disco";
            go.transform.SetParent(pai, false);
            go.transform.localScale = Vector3.zero;
            var r = go.GetComponent<Renderer>();
            r.sharedMaterial = Efeitos.MatAditiva;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var bloco = new MaterialPropertyBlock();
            bloco.SetColor("_BaseColor", new Color(0.5f, 0.8f, 1f, 1f));
            r.SetPropertyBlock(bloco);
            return go.transform;
        }

        // Faixa "FASE FINAL" no estilo da HUD (cartão azul com borda dourada)
        RectTransform CriarFaixa()
        {
            var cfg = ConfigArcor.Instancia;
            var go = new GameObject("Faixa_FaseFinal", typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(canvas.transform, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.76f);
            rt.sizeDelta = new Vector2(1000f, 220f);
            var fundo = go.AddComponent<Image>();
            fundo.sprite = cfg.cartao != null ? cfg.cartao : cfg.painel;
            if (fundo.sprite != null) fundo.type = Image.Type.Sliced;
            fundo.color = new Color(0.02f, 0.2f, 0.5f, 0.93f);
            fundo.raycastTarget = false;
            if (cfg.cartaoBorda != null)
            {
                var b = new GameObject("Borda", typeof(RectTransform), typeof(Image));
                var brt = (RectTransform)b.transform;
                brt.SetParent(rt, false);
                brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one; brt.offsetMin = new Vector2(-6, -6); brt.offsetMax = new Vector2(6, 6);
                var bi = b.GetComponent<Image>();
                bi.sprite = cfg.cartaoBorda; bi.type = Image.Type.Sliced; bi.color = Cores.Ouro; bi.raycastTarget = false;
            }
            int nivel = NivelAtual();
            bool final = nivel >= FactoryEscape.GameFlow.TotalLevels;
            Texto(rt, final ? "FASE FINAL" : "FASE " + nivel, 104f, Cores.Ouro, new Vector2(0f, 30f));
            Texto(rt, FactoryEscape.GameFlow.LevelName(nivel) + (final ? "  •  o último desafio" : ""), 36f, Color.white, new Vector2(0f, -62f));
            rt.localScale = Vector3.zero;
            return rt;
        }

        static void Texto(RectTransform pai, string s, float tam, Color cor, Vector2 pos)
        {
            var go = new GameObject("Texto", typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(pai, false);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(980f, tam * 1.3f);
            var t = go.AddComponent<TextMeshProUGUI>();
            if (ConfigArcor.Instancia.fonte != null) t.font = ConfigArcor.Instancia.fonte;
            t.text = s;
            t.fontSize = tam;
            t.color = cor;
            t.alignment = TextAlignmentOptions.Center;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.outlineWidth = 0.2f;
            t.outlineColor = new Color32(4, 24, 60, 255);
            t.raycastTarget = false;
        }

        IEnumerator AnimarFaixa(RectTransform rt, bool entrando)
        {
            if (rt == null) yield break;
            float t = 0f;
            const float d = 0.5f;
            while (t < d && rt != null)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / d);
                if (entrando)
                {
                    const float c1 = 1.70158f, c3 = c1 + 1f;
                    float mola = 1f + c3 * Mathf.Pow(k - 1f, 3f) + c1 * Mathf.Pow(k - 1f, 2f);
                    rt.localScale = Vector3.one * mola;
                    rt.localRotation = Quaternion.Euler(0f, 0f, (1f - k) * -6f);
                }
                else
                {
                    rt.localScale = Vector3.one * (1f + k * 0.12f);
                    foreach (var g in rt.GetComponentsInChildren<Graphic>())
                    {
                        var c = g.color; c.a = Mathf.Min(c.a, 1f - Suave(k)); g.color = c;
                    }
                }
                yield return null;
            }
        }
    }
}
