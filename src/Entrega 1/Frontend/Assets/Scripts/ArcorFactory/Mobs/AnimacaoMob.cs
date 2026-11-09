using UnityEngine;

namespace ArcorFactory
{
    // Tempos das ações dos mobs novos
    public static class TemposMobs
    {
        // Poosh
        public const float PooshVoar = 0.4f; // um bater de asas (loop)
        public const float PooshMirar = 0.8f; // aviso antes do mergulho
        public const float PooshMergulho = 0.24f; // loop das asas fechadas
        public const float PooshTonto = 1.9f;
        public const float PooshTontoLoop = 0.8f;

        // Chokko
        public const float ChokkoIdle = 2.4f;
        public const float ChokkoPasso = 1.2f; // ciclo de passos na velocidade base
        public const float ChokkoPuxarSoco = 0.42f;
        public const float ChokkoSoco = 0.16f;
        public const float ChokkoVoltarSoco = 0.5f;
        public const float ChokkoSocoTotal = ChokkoPuxarSoco + ChokkoSoco + ChokkoVoltarSoco;
        public const float ChokkoPreparo = 0.85f;
        public const float ChokkoPancada = 0.14f;
        public const float ChokkoCansado = 1.7f;
        public const float ChokkoAtingido = 0.5f; // estremecida ao levar golpe
        public const float ChokkoCansadoLoop = 1f;

        public const float Dano = 0.32f;
    }

    // Parâmetros dos Animators dos mobs novos (os mesmos nos dois controladores)
    public static class ParamMob
    {
        public static readonly int Fase = Animator.StringToHash("Fase"); // int: estado de animação do mob
        public static readonly int MultAndar = Animator.StringToHash("MultAndar"); // float: ritmo dos passos
    }

    // Estados de animação (valor do parâmetro "Fase") de cada mob
    public static class FasePoosh { public const int Voar = 0, Mirar = 1, Mergulho = 2, Tonto = 3; }
    public static class FaseChokko { public const int Idle = 0, Andar = 1, SocoE = 2, SocoD = 3, Preparar = 4, Pancada = 5, Cansado = 6, Atingido = 7; }
}
