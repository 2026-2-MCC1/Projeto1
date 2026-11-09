using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcorFactory
{
    public enum Raridade { Comum, Raro, Epico, Lendario }

    // Contadores da fase atual (zerados a cada cena carregada) e recordes salvos
    public static class EstatisticasFase
    {
        public static int MobsDerrotados { get; private set; }
        public static int CaixasQuebradas { get; private set; }
        public static int BausAbertos { get; private set; }
        public static int BausTotal { get; private set; }
        public static int SelosColetados { get; private set; }
        public static int SelosTotal { get; private set; }
        public static int DanosRecebidos { get; private set; }
        public static int RockletsBonus { get; private set; }
        public static Raridade MelhorBau { get; private set; }

        public static event Action AoMudar;
        public static event Action<Vector3> AoLevarDano;
        public static event Action<Raridade> AoAbrirBau;

        static string cena = "";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Registrar()
        {
            Zerar("");
            SceneManager.sceneLoaded -= AoCarregarCena;
            SceneManager.sceneLoaded += AoCarregarCena;
        }

        static void AoCarregarCena(Scene s, LoadSceneMode modo)
        {
            if (modo == LoadSceneMode.Single) Zerar(s.name);
        }

        static void Zerar(string nomeCena)
        {
            cena = nomeCena;
            MobsDerrotados = CaixasQuebradas = BausAbertos = BausTotal = SelosColetados = SelosTotal = DanosRecebidos = RockletsBonus = 0;
            MelhorBau = Raridade.Comum;
            AoMudar = null;
            AoLevarDano = null;
            AoAbrirBau = null;
        }

        public static string Cena { get { return string.IsNullOrEmpty(cena) ? SceneManager.GetActiveScene().name : cena; } }

        public static void RegistrarBau() { BausTotal++; Avisar(); }
        public static void RegistrarSelo() { SelosTotal++; Avisar(); }
        public static void MobDerrotado() { MobsDerrotados++; Avisar(); }
        public static void CaixaQuebrada() { CaixasQuebradas++; Avisar(); }
        public static void SeloColetado() { SelosColetados++; Avisar(); }
        public static void Bonus(int rocklets) { RockletsBonus += rocklets; Avisar(); }

        public static void BauAberto(Raridade r)
        {
            BausAbertos++;
            if (r > MelhorBau) MelhorBau = r;
            AoAbrirBau?.Invoke(r);
            Avisar();
        }

        public static void DanoRecebido(Vector3 origem)
        {
            DanosRecebidos++;
            AoLevarDano?.Invoke(origem);
            Avisar();
        }

        static void Avisar() { AoMudar?.Invoke(); }

        // estrelas e recordes
        public static int Estrelas()
        {
            int e = 1;
            bool metadeBaus = BausTotal == 0 || BausAbertos * 2 >= BausTotal;
            bool tudo = BausAbertos >= BausTotal && SelosColetados >= SelosTotal;
            if (metadeBaus) e = 2;
            if (tudo && (BausTotal + SelosTotal) > 0) e = 3;
            return e;
        }

        public static float MelhorTempo { get { return PlayerPrefs.GetFloat("arcor_tempo_" + Cena, 0f); } }
        public static int MelhoresEstrelas { get { return PlayerPrefs.GetInt("arcor_estrelas_" + Cena, 0); } }

        // Salva e devolve true se bateu o recorde de tempo
        public static bool SalvarResultado(float tempo)
        {
            bool recorde = false;
            float melhor = MelhorTempo;
            if (tempo > 0f && (melhor <= 0f || tempo < melhor))
            {
                PlayerPrefs.SetFloat("arcor_tempo_" + Cena, tempo);
                recorde = melhor > 0f;
            }
            int e = Estrelas();
            if (e > MelhoresEstrelas) PlayerPrefs.SetInt("arcor_estrelas_" + Cena, e);
            PlayerPrefs.Save();
            return recorde;
        }
    }
}
