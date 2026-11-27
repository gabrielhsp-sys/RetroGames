# Contrato da API

Endpoints da API RetroGames como estão hoje nos controllers de `RetroGames.WebApi/Controllers`. Complementa a seção 5 da especificação **RetroGames — Arquitetura e Stack**, que lista só os endpoints do Módulo de Catálogo e Administração; os do Módulo de Compras seguem o desafio. A rastreabilidade por história está em [USER_STORIES.md](USER_STORIES.md).

## Convenções

- Rotas no padrão `api/[controller]`, JSON em camelCase, id na rota e filtros na query string (Espec. 5.1).
- Toda resposta usa o envelope `{ "responseCode", "data", "errorMessage" }` (Espec. 5.3). Em falha de validação (código 3), `data` traz os erros por campo.
- Status usados: 200, 201, 400, 401, 403, 404, 409 e 500. Exclusões respondem 200 com o envelope; a API não usa 204.
- A API é segura por padrão (RS03): a `FallbackPolicy` em `Configuration/AutorizacaoConfig.cs` exige login em toda rota. Rota pública tem `[AllowAnonymous]` na action; gravação e gestão têm `[Authorize(Policy = Politicas.Administrador)]`.
- Sem sessão, a API responde 401 com código 4; com sessão e sem o perfil exigido, 403 com código 5 (`Configuration/AutenticacaoConfig.cs`).
- Em desenvolvimento, a descrição OpenAPI fica em `/openapi/v1.json` e a página de testes em `/swagger`, ambas públicas (`app.MapOpenApi().AllowAnonymous()` no `Program.cs`).

Na coluna "Acesso": **Público** = `[AllowAnonymous]`; **Logado** = nenhum atributo, vale a política padrão; **Admin** = `[Authorize(Policy = Politicas.Administrador)]`.

## Endpoints do Módulo de Compras

| Método | Rota | Acesso | Sucesso | Erros de negócio e de API | História | Action |
| --- | --- | --- | --- | --- | --- | --- |
| POST | `/api/Autenticacao/cadastrar` | Público | 201, com o usuário e a sessão iniciada; `Location` em `/api/Autenticacao/eu` | 400 (código 3); 409 (106) | C01 | `AutenticacaoController.Cadastrar` |
| POST | `/api/Pedido` | Logado | 201, com o pedido criado; `Location` em `/api/Pedido/{id}` | 401 (4); 400 (3); 400 (110); 409 (111) | C04 | `PedidoController.Inserir` |
| GET | `/api/Pedido/meus` | Logado | 200, lista do usuário logado, do mais recente ao mais antigo | 401 (4) | C05 | `PedidoController.ListarMeus` |
| GET | `/api/Pedido/{id}` | Logado (dono ou administrador) | 200, detalhe do pedido | 401 (4); 403 (113); 404 (109) | C06 | `PedidoController.ListarPorId` |
| GET | `/api/Pedido?idSituacao=` | Admin | 200, todos os pedidos (filtro opcional), do mais recente ao mais antigo, com o nome do cliente; sem paginação | 401 (4); 403 (5); 400 (3, `idSituacao` que não é número); 404 (112) | C07 | `PedidoController.Listar` |
| PUT | `/api/Pedido/{id}/situacao` | Admin | 200, com o pedido atualizado | 401 (4); 403 (5); 400 (3); 404 (109); 404 (112) | C07 | `PedidoController.AlterarSituacao` |
| GET | `/api/SituacaoPedido` | Admin | 200, as quatro situações | 401 (4); 403 (5) | C07 | `SituacaoPedidoController.Listar` |
| GET | `/api/Genero/{id}` | Público | 200 | 404 (105) | C08 | `GeneroController.ListarPorId` |
| POST | `/api/Genero` | Admin | 201; `Location` em `/api/Genero/{id}` | 401 (4); 403 (5); 400 (3); 409 (107) | C08 | `GeneroController.Inserir` |
| PUT | `/api/Genero/{id}` | Admin | 200 | 401 (4); 403 (5); 400 (3); 404 (105); 409 (107) | C08 | `GeneroController.Alterar` |
| DELETE | `/api/Genero/{id}` | Admin | 200 | 401 (4); 403 (5); 404 (105); 409 (108) | C08 | `GeneroController.Excluir` |

O desafio diz que, entre as rotas novas, só o `/cadastrar` recebe `[AllowAnonymous]`. O `GET /api/Genero/{id}` também é público: o desafio o define assim na tabela de endpoints da versão completa ("GET público"), e a tabela de acesso do `AGENTS.md` lista "`/api/Genero` (lista e por id)" como Público.

### Corpos e respostas (Compras)

| Endpoint | Entrada (ApiModel) | Saída em `data` (ApiModel) |
| --- | --- | --- |
| `POST /api/Autenticacao/cadastrar` | `CadastrarUsuarioRequestApiModel`: `nome`, `email`, `senha` | `UsuarioLogadoResponseApiModel` |
| `POST /api/Pedido` | `InserirPedidoRequestApiModel`: `itens[]` com `idJogo` e `quantidade`. Não há campo de preço. | `ListarPedidoResponseApiModel`: `id`, `data`, `total`, `idSituacaoPedido`, `descricaoSituacao`, `itens[]` (`ListarItemPedidoResponseApiModel`: `idJogo`, `nomeJogo`, `quantidade`, `valorUnitario`) |
| `GET /api/Pedido/meus` | — | lista de `ListarPedidoResponseApiModel` |
| `GET /api/Pedido/{id}` | — | `ListarPedidoResponseApiModel` |
| `GET /api/Pedido?idSituacao=` | `ListarPedidoRequestApiModel`: `idSituacao` opcional | lista de `ListarTodosPedidoResponseApiModel` (`ListarPedidoResponseApiModel` + `nomeUsuario`) |
| `PUT /api/Pedido/{id}/situacao` | `AlterarSituacaoPedidoRequestApiModel`: `idSituacao` | `ListarPedidoResponseApiModel` |
| `GET /api/SituacaoPedido` | — | lista de `ListarSituacaoPedidoResponseApiModel`: `id`, `descricao` |
| `POST` e `PUT /api/Genero` | `InserirGeneroRequestApiModel` / `AlterarGeneroRequestApiModel`: `nome` | `ListarGeneroResponseApiModel` (`id`, `nome`) no POST; `data` nulo no PUT |

### Validação de formato (código 3)

| Validator | Regras |
| --- | --- |
| `CadastrarUsuarioRequestValidator` | Nome obrigatório, até 100 caracteres; e-mail obrigatório, válido, até 256 caracteres; senha obrigatória, com no mínimo 8 caracteres. |
| `InserirPedidoRequestValidator` | Ao menos um item; cada jogo uma só vez; item não nulo; `idJogo` maior que zero; quantidade de 1 a 10. |
| `AlterarSituacaoPedidoRequestValidator` | `idSituacao` maior que zero. A existência da situação é conferida no Service (112). |
| `InserirGeneroRequestValidator` e `AlterarGeneroRequestValidator` | Nome obrigatório, até 50 caracteres. |

## Endpoints do Catálogo e Administração (aulas 4 a 6)

| Método | Rota | Acesso | Sucesso | Erros de negócio e de API |
| --- | --- | --- | --- | --- |
| POST | `/api/Autenticacao/login` | Público | 200, com o usuário e o cookie de sessão | 400 (3); 401 (6) |
| POST | `/api/Autenticacao/logout` | Logado | 200 | 401 (4) |
| GET | `/api/Autenticacao/eu` | Logado | 200, o usuário da sessão | 401 (4) |
| GET | `/api/Plataforma` | Público | 200 | — |
| GET | `/api/Plataforma/{id}` | Público | 200 | 404 (100) |
| POST | `/api/Plataforma` | Admin | 201 | 400 (3); 409 (101) |
| PUT | `/api/Plataforma/{id}` | Admin | 200 | 400 (3); 404 (100); 409 (101) |
| DELETE | `/api/Plataforma/{id}` | Admin | 200 | 404 (100); 409 (102) |
| GET | `/api/Genero` | Público | 200 | — |
| GET | `/api/CondicaoJogo` | Público | 200 | — |
| GET | `/api/Jogo` | Público | 200, página com `busca`, `idPlataforma`, `idGenero`, `pagina` (padrão 1), `tamanhoPagina` (padrão 12, de 1 a 50) e `incluirInativos` (só vale para o administrador, RN04) | 400 (3) |
| GET | `/api/Jogo/{id}` | Público | 200, com os gêneros | 404 (103); jogo inativo para quem não é administrador também responde 103 |
| POST | `/api/Jogo` | Admin | 201 | 400 (3); 404 (100, 104, 105) |
| PUT | `/api/Jogo/{id}` | Admin | 200 | 400 (3); 404 (103, 100, 104, 105) |
| DELETE | `/api/Jogo/{id}` | Admin | 200, desativa o jogo (RN03) | 404 (103) |

As gravações de plataforma e jogo também respondem 401 (4) sem sessão e 403 (5) sem o perfil de administrador.

## Catálogo de códigos

### `ResponseCodes` (`RetroGames.WebApi.ApiModels/ResponseCodes.cs`, 0 a 99)

| Código | Constante | Mensagem padrão | Status |
| --- | --- | --- | --- |
| 0 | `SUCESSO` | — | 200 / 201 |
| 1 | `ERRO` | Ocorreu um erro inesperado. Tente novamente mais tarde. | 500 |
| 2 | `MODEL_NULL` | O modelo veio nulo. | 400 |
| 3 | `MODEL_STATE_INVALIDO` | Há um erro com o modelo enviado. | 400 |
| 4 | `NAO_AUTENTICADO` | Faça login para continuar. | 401 |
| 5 | `ACESSO_NEGADO` | Você não tem permissão para esta operação. | 403 |
| 6 | `CREDENCIAIS_INVALIDAS` | E-mail ou senha inválidos. | 401 |

O Módulo de Compras não criou código novo neste catálogo.

### `BusinessErrors` (`RetroGames.Services/BusinessErrors.cs`, a partir de 100)

| Código | Status | Constante | Mensagem padrão | Origem |
| --- | --- | --- | --- | --- |
| 100 | 404 | `PLATAFORMA_NAO_ENCONTRADA` | Plataforma não encontrada. | Espec. (RN05) |
| 101 | 409 | `PLATAFORMA_NOME_DUPLICADO` | Já existe uma plataforma com este nome. | Espec. (RN01) |
| 102 | 409 | `PLATAFORMA_POSSUI_JOGOS` | Não é possível excluir: existem jogos desta plataforma. | Espec. (RN02) |
| 103 | 404 | `JOGO_NAO_ENCONTRADO` | Jogo não encontrado. | Espec. |
| 104 | 404 | `CONDICAO_NAO_ENCONTRADA` | Condição de jogo não encontrada. | Espec. (RN05) |
| 105 | 404 | `GENERO_NAO_ENCONTRADO` | Gênero não encontrado. | Espec. (RN05); usado também em C08 |
| 106 | 409 | `EMAIL_JA_CADASTRADO` | Já existe uma conta com este e-mail. | Desafio (C01) |
| 107 | 409 | `GENERO_NOME_DUPLICADO` | Já existe um gênero com este nome. | Desafio (C08) |
| 108 | 409 | `GENERO_POSSUI_JOGOS` | Não é possível excluir: existem jogos com este gênero. | Desafio (C08) |
| 109 | 404 | `PEDIDO_NAO_ENCONTRADO` | Pedido não encontrado. | Desafio (C06, C07) |
| 110 | 400 | `JOGO_INDISPONIVEL` | Um ou mais jogos não estão disponíveis para venda. | Desafio (C04) |
| 111 | 409 | `ESTOQUE_INSUFICIENTE` | Estoque insuficiente. | Desafio (C04) |
| 112 | 404 | `SITUACAO_NAO_ENCONTRADA` | Situação de pedido não encontrada. | Desafio (C07) |
| 113 | 403 | `PEDIDO_DE_OUTRO_USUARIO` | Você não tem acesso a este pedido. | Desafio (C06) |

Os códigos 106 a 113 e as mensagens são os da tabela "Códigos de erro" do desafio, sem diferença de texto. As mensagens de 100 a 105 são as da tabela 5.4 da especificação, também sem diferença de texto.
