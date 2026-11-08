using UnityEngine;

// Câmera no estilo Crash: fica sempre atrás e acima do jogador, olhando para o caminho à frente.
// O mouse não gira a câmera; andar para trás faz o jogador vir na direção dela.
public class CameraCrash : MonoBehaviour
{
    // Objeto que a câmera deve seguir, neste caso o jogador
    public Transform alvo;

    // Posição da câmera em relação ao jogador
    public float distanciaAtras = 8f;
    public float alturaCamera = 5.4f;

    // Para onde a câmera olha: acima dos pés e um pouco à frente do jogador
    public float alturaOlhar = 1.1f;
    public float olharAFrente = 3f;

    // Tempo para alcançar o jogador; na altura é maior, para não pular junto
    public float suavizacao = 0.12f;
    public float suavizacaoAltura = 0.3f;

    // Direção do caminho, em graus; pode ser alterada para acompanhar uma curva da fase
    public float direcaoCaminho = 0f;
    public float velocidadeGiro = 90f;

    // Acima desta distância a câmera vai direto, como depois de uma queda
    public float distanciaSalto = 15f;

    private Vector3 pontoSeguido;
    private Vector3 velocidadeLateral;
    private float velocidadeAltura;
    private float direcaoAtual;

    void Start()
    {
        // Trava o cursor no centro da tela e deixa ele invisível
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        direcaoAtual = direcaoCaminho;
        if (alvo != null)
        {
            pontoSeguido = alvo.position;
        }
    }

    void LateUpdate()
    {
        // Impede erros caso o alvo não esteja disponível
        if (alvo == null)
        {
            return;
        }

        if (Vector3.Distance(pontoSeguido, alvo.position) > distanciaSalto)
        {
            pontoSeguido = alvo.position;
            velocidadeLateral = Vector3.zero;
            velocidadeAltura = 0f;
        }

        // Segue no chão e na altura em ritmos diferentes
        Vector3 noChao = new Vector3(pontoSeguido.x, 0f, pontoSeguido.z);
        Vector3 alvoNoChao = new Vector3(alvo.position.x, 0f, alvo.position.z);
        noChao = Vector3.SmoothDamp(noChao, alvoNoChao, ref velocidadeLateral, suavizacao);
        float altura = Mathf.SmoothDamp(pontoSeguido.y, alvo.position.y, ref velocidadeAltura, suavizacaoAltura);
        pontoSeguido = new Vector3(noChao.x, altura, noChao.z);

        direcaoAtual = Mathf.MoveTowardsAngle(direcaoAtual, direcaoCaminho, velocidadeGiro * Time.deltaTime);
        Quaternion rumo = Quaternion.Euler(0f, direcaoAtual, 0f);

        transform.position = pontoSeguido + rumo * new Vector3(0f, alturaCamera, -distanciaAtras);
        transform.LookAt(pontoSeguido + Vector3.up * alturaOlhar + rumo * Vector3.forward * olharAFrente);
    }
}