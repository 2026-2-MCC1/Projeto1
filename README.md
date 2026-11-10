# FECAP - Fundação de Comércio Álvares Penteado

<p align="center">
<a href= "https://www.fecap.br/"><img src="https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRhZPrRa89Kma0ZZogxm0pi-tCn_TLKeHGVxywp-LXAFGR3B1DPouAJYHgKZGV0XTEf4AE&usqp=CAU" alt="FECAP - Fundação de Comércio Álvares Penteado" border="0"></a>
</p>

# Factory Escape.

## Jogo de plataforma 3D inspirado na fábrica da Arcor

## Integrantes: <a href="https://www.linkedin.com/in/andrepczx/">André Ananias</a>, <a href="https://www.linkedin.com/in/antonie-gabriel-204b9533b/">Antonie Gabriel</a>, <a href="https://www.linkedin.com/in/jo%C3%A3o-ferreira-a9b71839b?utm_source=share_via&utm_content=profile&utm_medium=member_ios">João Vitor</a>, <a href="https://www.linkedin.com/in/karine-vicente-87311b231?utm_source=share_via&utm_content=profile&utm_medium=member_ios">Karine Vicente </a>

## Professores Orientadores: <a href="https://www.linkedin.com/in/#/">Eduardo Savino Gomes</a>, <a href="https://www.linkedin.com/in/#/">Luis Fernando dos Santos Pires</a>, <a href="https://www.linkedin.com/in/#/">Renata Muniz Do Nascimento</a>, <a href="https://www.linkedin.com/in/#/">Robson de Oliveira Cardoso</a>, <a href="https://www.linkedin.com/in/#/">Victor Bruno Alexander Rosetti de Quiroz</a>


<p align="center">
<img src="imagens/banner_FactoryEscape.png" alt="Banner Factory Escape" width="100%">
<br>
Game by <a href="https://www.linkedin.com/in/andrepczx/">André Ananias</a>,
<a href="https://www.linkedin.com/in/antonie-gabriel-204b9533b/">Antonie Gabriel</a>,
<a href="https://www.linkedin.com/in/jo%C3%A3o-ferreira-a9b71839b/">João Vitor</a>,
<a href="https://www.linkedin.com/in/karine-vicente-87311b231/">Karine Vicente</a>.
<a href="https://creativecommons.org/licenses/by-sa/3.0/">CC BY-SA 3.0</a>
</p>

## Descrição

O **Factory Escape** é um jogo de plataforma 3D desenvolvido para aplicação prática dos conceitos de desenvolvimento de jogos. O projeto foca na utilização de mecânicas de movimentação, física, saltos e colisões, tudo ambientado em um cenário industrial temático inspirado na fábrica da Arcor.
<br><br>
A experiência é projetada para ser curta, progressiva e dinâmica, sendo dividida em três fases principais: um tutorial inicial, um desafio de parkour utilizando embalagens gigantes da 7Belo e uma intensa sequência de fuga com câmera frontal. A premissa coloca o jogador em seu primeiro dia de trabalho em uma avaliação de rotina que acaba resultando em um desastre após o colapso da caldeira principal, exigindo que o protagonista pule, desvie e corra para sobreviver e escapar da fábrica.
<br><br>

## 📁 Estrutura de pastas

```text
Raiz/
├── documentos/
│   ├── Entrega 1/
│   │   ├── Algoritmos e Lógica de Programação/
│   │   ├── Calculo 1/
│   │   ├── Ética e Pensamento Computacional/
│   │   ├── Jogos Digitais e Sistemas Digitais Interativos/
│   │   └── Projeto Interdisciplinar - Jogos Digitais/   (GDD)
│   └── Entrega 2/
│       ├── Algoritmos e Lógica de Programação/
│       ├── Cálculo 1/
│       ├── Ética e Pensamento Computacional/
│       ├── Jogos Digitais e Sistemas Digitais Interativos/
│       └── Projeto Interdisciplinar - Jogos Digitais/
├── imagens/                                             (capa e imagens do README)
├── src/
│   ├── Entrega 1/
│   │   └── Frontend/                                    (projeto Unity)
│   │       ├── Assets/
│   │       │   ├── Scenes/    (fases e telas)
│   │       │   ├── Scripts/   (código C# do jogo)
│   │       │   ├── Prefabs, Modelos, Materiais, Texturas, Animacoes
│   │       │   ├── Audio, Resources, UI, Shaders
│   │       │   └── ThirdParty/ (pacotes de terceiros)
│   │       ├── Packages/
│   │       └── ProjectSettings/
│   └── Entrega 2/
│       └── Frontend/
│           └── Executáveis/                             (jogo para Windows)
├── .gitignore
└── README.md
```

- **documentos**: toda a documentação do projeto, separada por entrega e por disciplina.
- **imagens**: imagens usadas no README.
- **src**: código-fonte do jogo (projeto Unity em `Entrega 1/Frontend`) e a versão executável (`Entrega 2/Frontend/Executáveis`).

## 🛠 Instalação

A versão executável para Windows está em `src/Entrega 2/Frontend/Executáveis`.

1. Baixe ou clone o repositório.
2. Abra a pasta `src/Entrega 2/Frontend/Executáveis`.
3. Execute o arquivo `ArcorFactory.exe` (mantenha as pastas `ArcorFactory_Data`, `MonoBleedingEdge` e `D3D12` junto dele).

## 💻 Configuração para desenvolvimento

O jogo foi desenvolvido na Unity, com programação em C# no Visual Studio.

Para abrir e editar o projeto é necessário:

- <a href="https://unity.com/download">Unity Hub</a> e Unity 6.3 LTS
- Visual Studio (com suporte a desenvolvimento de jogos com Unity)

Como rodar o projeto:

1. Clone este repositório.
2. Abra o Unity Hub, clique em "Open" e selecione a pasta `src/Entrega 1/Frontend`.
3. Aguarde a importação dos assets e pacotes.
4. Abra a cena `MainMenu` em `Assets/Scenes/Telas` e clique em Play.

## 🎮 Progresso do jogo

### Entrega 1

Primeira versão jogável de Factory Escape, já com menu, tutorial, três fases e telas de vitória e derrota.

Funcionalidades implementadas:

**Jogador e controles**
- Movimentação com WASD e corrida com Shift
- Pulo com *coyote time* (pequena tolerância para pular logo após sair da plataforma)
- Ataque giratório da tartaruga para derrotar inimigos
- Câmera em terceira pessoa controlada pelo mouse, com ajuste para não atravessar tetos e corredores
- Sistema de vidas, dano e invencibilidade temporária
- Respawn em caso de queda e checkpoint na fase 2
- O jogador é congelado ao entrar no portal de saída, sem poder se mover, tomar dano ou cair em Game Over

**Fases e jogabilidade**
- Tutorial inicial
- Três fases (`Fabrica_Parte1`, `Fabrica_Parte2` e `Fabrica_Parte3`) ligadas por portais de saída
- Coleta de Rocklets com meta por fase (50, 30 e 110) exibida no objetivo da tela
- Obstáculos fixos e móveis, com movimento horizontal e vertical
- Área de parkour
- Baús interativos, cartão de acesso e porta final que verifica o cartão
- Inimigos e efeitos visuais de ataque e dano

**Telas e fluxo do jogo**
- Menu principal, Opções, Créditos, Game Over, Vitória e Fim de Jogo
- HUD com vidas, Rocklets e fase atual
- Menu de pausa
- Mensagens de interação e sistema de objetivos
- Fluxo entre cenas controlado pela classe `GameFlow`

**Código**
- Scripts organizados em pastas em português (`Jogador`, `Fase`, `Telas`, entre outras) e comentados

### Entrega 2

- Telas e cenas secundárias programadas: Menu principal, Opções, Créditos, Game Over, Vitória, Fim de Jogo e menu de pausa
- Jogo finalizado, com as 3 fases ligadas por portais
- Versão executável para Windows (`src/Entrega 2/Frontend/Executáveis`)
- Documentos da entrega em `documentos/Entrega 2` (Algoritmos e Lógica de Programação e Ética e Pensamento Computacional)
- README atualizado com a estrutura do projeto, as instruções de instalação e os créditos

## 🎨 Créditos dos assets

Assets de terceiros usados no jogo (pasta `Assets/ThirdParty` e `Assets/Resources/Fonts`):

| Recurso | Autor | Onde foi usado |
|---|---|---|
| Low Poly Industrial Pack (`Donthrukat/Lowpolyindustrialpack`) | Donthrukat | Cenário industrial: barris, caixas, caixotes, tanques, tubulações, estantes, paletes e paredes. [Unity Asset Store](https://assetstore.unity.com/packages/3d/props/industrial/low-poly-industrial-pack-169683) |
| Low Poly Industrial Pack Lite (`Tyuris/Low_Poly_Industrial_Pack_Lite`) | Tyuris | Passarelas (catwalks), silo, tanque horizontal, galpão, fornalha e empilhadeira |
| Fredoka One | Milena Brandão (Google Fonts) | Fonte dos textos e menus (licença SIL Open Font License) |
| Matcha Home | Khurasan | Fonte de títulos (distribuída como gratuita para uso pessoal e comercial) |
| Super Waffles | Hadley Fisher | Fonte de destaques (distribuída para uso comercial) |
| Unity Engine, URP, TextMesh Pro, Input System e ProBuilder | Unity Technologies | Motor e pacotes do projeto |

As marcas e personagens inspirados na Arcor (7Belo, Plutonita, Poosh!, Chokko, Selo de Qualidade) pertencem à Arcor e são usados apenas em um projeto acadêmico da FECAP. Os demais recursos do jogo (modelos dos personagens e mobs, efeitos, interface, sons e músicas) foram feitos pela equipe, com o apoio das ferramentas de IA descritas na declaração de uso de IA do projeto.

## 📋 Licença/License
<a href="https://github.com/2026-2-MCC1/Projeto1">Factory Escape</a> © 2026 by <a href="https://github.com/2026-2-MCC1/Projeto1">André Ananias, Antonie Gabriel, João Vitor e Karine Vicente</a> is licensed under <a href="https://creativecommons.org/licenses/by/4.0/">CC BY 4.0</a><img src="https://mirrors.creativecommons.org/presskit/icons/cc.svg" alt="" style="max-width: 1em;max-height:1em;margin-left: .2em;"><img src="https://mirrors.creativecommons.org/presskit/icons/by.svg" alt="" style="max-width: 1em;max-height:1em;margin-left: .2em;">

## 🎓 Referências

1. <https://github.com/fecaphub>
2. Arcor: <https://www.arcor.com>
