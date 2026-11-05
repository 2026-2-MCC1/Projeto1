using UnityEngine;

// Faz o objeto balançar de um lado para o outro (no eixo Z), como um pêndulo.
// Coloque este script no ponto de cima (onde o pêndulo fica preso).
public class ObstaculoPendulo : MonoBehaviour
{
    public float anguloMaximo = 55f;   // até onde ele vai para cada lado
    public float velocidade = 66f;     // graus por segundo
    public float comecarEm = 0f;       // ângulo inicial (para os pêndulos não andarem juntos)

    private float angulo;
    private int sentido = 1;           // 1 = indo, -1 = voltando

    void Start()
    {
        angulo = comecarEm;
    }

    void Update()
    {
        angulo = angulo + velocidade * sentido * Time.deltaTime;

        // Chegou no ângulo máximo: inverte o sentido.
        if (angulo > anguloMaximo)
        {
            angulo = anguloMaximo;
            sentido = -1;
        }
        else if (angulo < -anguloMaximo)
        {
            angulo = -anguloMaximo;
            sentido = 1;
        }

        transform.localEulerAngles = new Vector3(0, 0, angulo);
    }
}
