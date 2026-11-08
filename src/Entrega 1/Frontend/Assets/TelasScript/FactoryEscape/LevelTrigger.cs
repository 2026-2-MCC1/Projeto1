using UnityEngine;

namespace FactoryEscape
{
    public enum TriggerResult { Victory, GameOver }

    // =====================================================================
    //  LevelTrigger - atalho sem código para a SUA fase.
    //  Coloque num objeto com Collider (marque "Is Trigger") ou Collider2D:
    //    - na porta de saída  -> Result = Victory
    //    - em máquinas/espinhos -> Result = GameOver
    //  O jogador precisa ter a tag "Player" e um Rigidbody (ou Rigidbody2D).
    // =====================================================================
    public class LevelTrigger : MonoBehaviour
    {
        public TriggerResult result = TriggerResult.Victory;
        public string playerTag = "Player";
        public float delay = 0.3f;     // pequena pausa antes de trocar de cena

        bool fired;

        void OnTriggerEnter(Collider other)     { Fire(other.gameObject); }
        void OnTriggerEnter2D(Collider2D other) { Fire(other.gameObject); }

        void Fire(GameObject who)
        {
            if (fired || !who.CompareTag(playerTag)) return;
            fired = true;
            Invoke(nameof(Go), delay);
        }

        void Go()
        {
            if (result == TriggerResult.Victory) GameFlow.Victory();
            else GameFlow.GameOver();
        }
    }
}
