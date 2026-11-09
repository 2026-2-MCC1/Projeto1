using UnityEngine;
using UnityEngine.InputSystem;

namespace ArcorFactory
{
    // Fica no jogador (é adicionado sozinho pelo InicializadorArcor)
    public class AtaqueJogador : MonoBehaviour
    {
        public static AtaqueJogador Instancia { get; private set; }

        public enum EstadoGiro { Pronto, Girando }

        public Key teclaGiro = Key.Q;
        public float duracaoGiro = 0.42f;
        public float alcanceGiro = 2.1f;

        // Avisado uma vez por giro aceito
        public event System.Action AoComecarGiro;

        // Preenchido pelo TartarugaAnimador
        [System.NonSerialized] public bool animacaoGiroAtiva;
        // Preenchido pelo TartarugaAnimador
        [System.NonSerialized] public bool animacaoBloqueada;

        CharacterController corpo;
        PlayerMovement movimento;
        InventarioJogador inventario;
        float giroAte = -10f, giroComecou = -10f;
        int idGiro;
        EstadoGiro estadoGiro = EstadoGiro.Pronto;

        public CharacterController Corpo { get { return corpo; } }
        public InventarioJogador Inventario { get { return inventario; } }
        public PlayerMovement Movimento { get { return movimento; } }
        public bool Girando { get { return estadoGiro == EstadoGiro.Girando; } }
        public EstadoGiro Estado { get { return estadoGiro; } }
        public int IdGiro { get { return idGiro; } } // muda a cada giro
        public Vector3 Centro { get { return corpo != null ? corpo.bounds.center : transform.position; } }

        void Awake()
        {
            Instancia = this;
            corpo = GetComponent<CharacterController>();
            movimento = GetComponent<PlayerMovement>();
            inventario = GetComponent<InventarioJogador>();
        }

        void OnDestroy()
        {
            if (Instancia == this) Instancia = null;
        }

        public void DefinirDuracaoGiro(float segundos)
        {
            if (segundos > 0.05f) duracaoGiro = segundos;
        }

        void Update()
        {
            // 1) giro em andamento: tecla ignorada; só libera quando o tempo do clipe acabou
            if (estadoGiro == EstadoGiro.Girando)
            {
                bool tempoAcabou = Time.time >= giroAte;
                bool teto = Time.time >= giroComecou + duracaoGiro * 2.5f + 0.25f;
                if ((tempoAcabou && !animacaoGiroAtiva) || teto) estadoGiro = EstadoGiro.Pronto;
                return;
            }

            // 2) pronto: um aperto (borda de descida) começa um giro, no chão ou no ar
            var teclado = Keyboard.current;
            if (teclado == null || !teclado[teclaGiro].wasPressedThisFrame) return;
            if (!PodeGirar()) return;
            ComecarGiro();
        }

        bool PodeGirar()
        {
            if (corpo == null || animacaoBloqueada) return false;
            if (movimento != null && (!movimento.enabled || movimento.controleTravado)) return false;
            if (FactoryEscape.PauseMenu.Pausado || Time.timeScale <= 0f) return false;
            return true;
        }

        void ComecarGiro()
        {
            estadoGiro = EstadoGiro.Girando; // trava antes de qualquer outra coisa
            giroComecou = Time.time;
            giroAte = Time.time + duracaoGiro;
            idGiro++;
            Sons.Tocar("giro", 0.45f, 0.08f);
            AoComecarGiro?.Invoke();
        }

        // Golpe ou morte durante o giro: encerra na hora (a animação de dano assume)
        public void CancelarGiro()
        {
            estadoGiro = EstadoGiro.Pronto;
            giroAte = Time.time;
        }

        // O giro alcança o alvo? (distância no chão e diferença de altura)
        public bool GiroAcerta(Bounds alvo, float alcanceExtra = 0f, float alturaMax = 1.4f)
        {
            if (!Girando || corpo == null) return false;
            Vector3 d = alvo.center - corpo.bounds.center;
            float altura = Mathf.Abs(d.y) - alvo.extents.y;
            d.y = 0f;
            float raioAlvo = Mathf.Max(alvo.extents.x, alvo.extents.z);
            return d.magnitude <= alcanceGiro + raioAlvo * 0.5f + alcanceExtra && altura <= alturaMax;
        }

        // A tartaruga está caindo e os pés chegaram ao topo do alvo
        public bool PisouEm(Bounds alvo)
        {
            if (corpo == null || corpo.velocity.y > -0.5f) return false;
            Bounds b = corpo.bounds;
            Vector3 d = alvo.center - b.center;
            d.y = 0f;
            float raio = Mathf.Max(alvo.extents.x, alvo.extents.z) + corpo.radius * 0.6f;
            float pes = b.min.y - alvo.max.y;
            return d.magnitude <= raio && pes >= -0.55f && pes <= 0.3f;
        }

        // Encostou no corpo do alvo (de lado), para dano por contato
        public bool Encostou(Bounds alvo, float folga = 0f)
        {
            if (corpo == null) return false;
            Bounds b = corpo.bounds;
            b.Expand(folga * 2f);
            return b.Intersects(alvo);
        }

        public void Quicar(float forca)
        {
            if (movimento != null) movimento.Quicar(forca);
        }

        public void Empurrar(Vector3 de, float forca, float salto)
        {
            if (movimento == null) return;
            Vector3 dir = transform.position - de;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f) dir = -transform.forward;
            movimento.Empurrar(dir.normalized * forca, salto);
        }
    }
}
