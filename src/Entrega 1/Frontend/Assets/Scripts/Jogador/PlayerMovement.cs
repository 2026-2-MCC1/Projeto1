using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    // Velocidade do jogador, sempre a mesma, como no Crash
    public float velocidadeAndar = 6.5f;

    // Força aplicada no pulo
    public float forcaPulo = 5f;

    // Valor usado para simular a gravidade
    public float gravidade = -9.8f;

    // Velocidade com que o jogador vira para a direção do movimento
    public float velocidadeGiro = 540f;

    // Rapidez com que o jogador ganha e perde velocidade
    public float aceleracao = 30f;

    // Guarda a velocidade vertical do jogador
    private float velocidadeVertical = 0f;

    // Guarda a velocidade horizontal atual, que muda aos poucos
    private Vector3 velocidadeHorizontal = Vector3.zero;

    // Referência ao CharacterController do jogador
    private CharacterController controller;

    // Referência à câmera principal
    private Transform cameraTransform;

    // Cutscenes
    [HideInInspector] public bool controleTravado = false;
    // Com o controle travado
    [HideInInspector] public Vector3 direcaoCutscene = Vector3.zero;
    // "Tempo de coiote": ainda dá para pular logo depois de sair da beirada
    public float tempoCoiote = 0.12f;
    // Guarda o aperto do pulo por um instante antes de tocar o chão
    public float memoriaPulo = 0.12f;
    private float tempoDesdeChao = 99f;
    private float tempoDesdePedidoPulo = 99f;
    private float maiorQueda = 0f;

    // Lança a tartaruga para cima (pulo em cima de mob, trampolim, baú)
    public void Quicar(float forca)
    {
        velocidadeVertical = forca;
        tempoDesdeChao = 99f;
    }

    // Tempo sem controle depois de levar um golpe e o quanto o empurrão freia
    public float tempoAtordoado = 0.4f;
    public float freioAtordoado = 12f;
    private float atordoado = 0f;

    // Chamado quando a tartaruga leva dano: vira para quem bateu, é jogada para trás
    public void Empurrar(Vector3 forca, float salto)
    {
        Vector3 olhar = -forca;
        olhar.y = 0f;
        if (olhar.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(olhar);
        }

        velocidadeHorizontal = forca;
        if (salto > 0f)
        {
            velocidadeVertical = salto;
        }
        atordoado = tempoAtordoado;
    }

    void Start()
    {
        // Busca o CharacterController do jogador
        controller = GetComponent<CharacterController>();

        // Guarda a posição e rotação da câmera principal
        cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        // Valores usados para controlar o movimento
        float x = 0f;
        float z = 0f;

        // Movimento para frente
        if (Keyboard.current.wKey.isPressed)
        {
            z = 1f;
        }

        // Movimento para trás
        if (Keyboard.current.sKey.isPressed)
        {
            z = -1f;
        }

        // Movimento para a esquerda
        if (Keyboard.current.aKey.isPressed)
        {
            x = -1f;
        }

        // Movimento para a direita
        if (Keyboard.current.dKey.isPressed)
        {
            x = 1f;
        }

        // Atordoado pelo golpe: ignora as teclas até passar
        bool podeControlar = atordoado <= 0f && !controleTravado;
        if (atordoado > 0f)
        {
            atordoado -= Time.deltaTime;
        }
        if (!podeControlar)
        {
            x = 0f;
            z = 0f;
        }

        // Mantém o jogador próximo ao chão
        if (controller.isGrounded && velocidadeVertical < 0)
        {
            velocidadeVertical = -2f;
        }

        // Pulo com tempo de coiote e memória do botão (fica mais justo e responsivo)
        if (controller.isGrounded)
        {
            if (tempoDesdeChao > 0.25f && maiorQueda < -7f)
            {
                ArcorFactory.Efeitos.Poeira(transform.position + Vector3.down * 0.9f, 0.6f);
                ArcorFactory.Sons.Tocar("aterrissar", 0.35f, 0.1f);
            }
            tempoDesdeChao = 0f;
            maiorQueda = 0f;
        }
        else
        {
            tempoDesdeChao += Time.deltaTime;
            maiorQueda = Mathf.Min(maiorQueda, velocidadeVertical);
        }
        tempoDesdePedidoPulo += Time.deltaTime;
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            tempoDesdePedidoPulo = 0f;
        }

        bool pediuPulo = tempoDesdePedidoPulo <= memoriaPulo;
        if (pediuPulo && tempoDesdeChao <= tempoCoiote && velocidadeVertical <= 0.5f && podeControlar)
        {
            velocidadeVertical = forcaPulo;
            tempoDesdeChao = 99f;
            tempoDesdePedidoPulo = 99f;
            ArcorFactory.Sons.Tocar("pulo", 0.3f, 0.08f);
        }

        // Aplica a gravidade ao jogador
        velocidadeVertical = velocidadeVertical + gravidade * Time.deltaTime;

        // Pega as direções da câmera
        Vector3 frenteCamera = cameraTransform.forward;
        Vector3 direitaCamera = cameraTransform.right;

        // Remove o movimento vertical da direção da câmera
        frenteCamera.y = 0f;
        direitaCamera.y = 0f;

        // Normaliza as direções
        frenteCamera.Normalize();
        direitaCamera.Normalize();

        // Calcula a direção do movimento em relação à câmera
        Vector3 direcao = frenteCamera * z + direitaCamera * x;

        // Cutscene conduzindo a tartaruga
        if (controleTravado)
        {
            direcao = new Vector3(direcaoCutscene.x, 0f, direcaoCutscene.z);
        }

        // Evita que o movimento diagonal seja mais rápido
        if (direcao.magnitude > 1f)
        {
            direcao.Normalize();
        }

        // Faz o jogador virar aos poucos para a direção em que está andando
        if (direcao != Vector3.zero)
        {
            Quaternion rotacaoAlvo = Quaternion.LookRotation(direcao);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                rotacaoAlvo,
                velocidadeGiro * Time.deltaTime
            );
        }

        // Aproxima a velocidade atual da velocidade desejada, sem mudança brusca
        velocidadeHorizontal = Vector3.MoveTowards(
            velocidadeHorizontal,
            direcao * velocidadeAndar,
            (podeControlar || controleTravado ? aceleracao : freioAtordoado) * Time.deltaTime
        );

        // Junta o movimento horizontal com o movimento vertical
        Vector3 movimento =
            velocidadeHorizontal +
            Vector3.up * velocidadeVertical;

        // Move o jogador
        controller.Move(movimento * Time.deltaTime);
    }
}
