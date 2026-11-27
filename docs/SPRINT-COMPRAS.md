# Sprint do Módulo de Compras

Quadro da sprint do desafio do Módulo de Compras (DCE701). Versão do grupo: **completa** (C01 a C08), grupo de 4 estudantes.

## Objetivo da sprint

O cliente compra e acompanha os pedidos; o administrador gerencia pedidos e gêneros.

## Datas

| Item | Data |
| --- | --- |
| Início da sprint | 26/11/2026 (commit `909429c`, primeiro commit da sprint) |
| Fim da sprint (entrega) | 29/11/2026 |
| Apresentação | 01/12/2026 |
| Criação deste documento e das issues | 27/11/2026 |

O quadro foi montado em 27/11/2026. O trabalho feito antes disso não tem histórico de cartões: ele está comprovado pelos commits listados em cada história, cujo título começa com o código da história.

## Histórias

| Código | História | Situação | Responsável | Commits |
| --- | --- | --- | --- | --- |
| C01 | Cadastro de cliente | Concluído | Gabriel | `4ce88fc`, `2d5cccf`, `1961c47` |
| C02 | Cliente logado | Concluído | Gabriel | `8d76fdf`, `704fdc5` |
| C03 | Carrinho | Concluído | Gabriel | `8b26aa5`, `6251e2f`, `704fdc5`, `5297081` |
| C04 | Finalizar compra | Concluído | Gabriel | `909429c`, `4ce88fc`, `8ff0a09`, `2460a1a`, `704fdc5`, `5297081` |
| C05 | Meus pedidos | Concluído | Gabriel | `df42b7c`, `df3cbff` |
| C06 | Detalhe do pedido | Concluído | Gabriel | `4ce88fc`, `62f9b12`, `908d815`, `603a6d3` |
| C07 | Gestão de pedidos | Concluído | Gabriel | `4ce88fc`, `51ec209`, `827f112`, `603a6d3` |
| C08 | Manutenção de gêneros | Concluído | Gabriel | `4ce88fc`, `db52e7e`, `5a1dc51`, `c464c92` |

Os commits `704fdc5`, `5297081` e `603a6d3` (prefixo `style:`) ajustam a responsividade e a acessibilidade das telas de compra e de pedidos; não citam história no título e aparecem aqui nas histórias cujas telas alteram. O `d96877c` (`style:`) leva à tela de Plataformas, que não é do módulo de Compras, as mesmas correções de rolagem e foco feitas na de gêneros em `c464c92`.

### Critério para marcar uma tarefa como feita

Uma tarefa só tem `[x]` quando um commit da sprint criou ou alterou o arquivo que a implementa e o arquivo existe hoje no `main`. Em 27/11/2026, depois de C08, `dotnet test` passou com 69 testes e 0 falhas, e `npm run build` do `retrogames.web` terminou sem erro. Tarefas sem commit ficam `[ ]`.

---

## C01: Cadastro de cliente

**História:** Como visitante, quero criar minha conta de cliente, para poder comprar jogos na loja.

**Critérios de aceite (desafio):**

- Nome, e-mail válido e senha.
- Cria usuário ativo com perfil Cliente e senha em hash (RS04).
- Após o cadastro, a sessão já começa.
- E-mail já cadastrado: 409, código 106 (`EMAIL_JA_CADASTRADO`).
- Endpoint `POST /api/Autenticacao/cadastrar`, acesso Público.
- Tela Cadastro (`/cadastro`), com link na tela de Login.

**Tarefas:**

- [x] Tabela: nenhuma nova; usa `tb_usuario` da aula 6.
- [x] Erro 106 `EMAIL_JA_CADASTRADO` em `BusinessErrors` (`4ce88fc`).
- [x] Service: `AutenticacaoService` cadastra cliente ativo, perfil Cliente, senha em hash, e-mail normalizado (`2d5cccf`).
- [x] Validator e ApiModel: `CadastrarUsuarioRequestValidator`, `CadastrarUsuarioRequestApiModel` (`2d5cccf`).
- [x] Testes: `AutenticacaoServiceTests` (cadastro válido, hash, e-mail duplicado, duplicidade recusada pelo banco) (`2d5cccf`).
- [x] Endpoint: `POST /api/Autenticacao/cadastrar` com `[AllowAnonymous]`, iniciando a sessão (`2d5cccf`).
- [x] Tela: `/cadastro`, com link na tela de Login (`1961c47`).

**Commits:** `4ce88fc`, `2d5cccf`, `1961c47`.

## C02: Cliente logado

**História:** Como cliente, quero ver no menu o Carrinho e os Meus pedidos, para chegar às minhas compras sem acessar a Administração.

**Critérios de aceite (desafio):**

- O menu mostra Carrinho e Meus pedidos.
- O cliente não vê a Administração.

**Tarefas:**

- [x] Tela: menu do cliente com Carrinho (com contador) e Meus pedidos; Administração só para o administrador (`8d76fdf`; ajuste responsivo em `704fdc5`).

Só front-end: não há tabela, Repository, Service, teste nem endpoint novo nesta história.

**Commits:** `8d76fdf`, `704fdc5`.

## C03: Carrinho

**História:** Como cliente, quero montar um carrinho com os jogos que escolhi, para revisar quantidades e total antes de comprar.

**Critérios de aceite (desafio):**

- Adicionar a partir do detalhe do jogo (não para jogo esgotado).
- Alterar quantidade (1 a 10), remover itens e ver o total.
- Fica no navegador (`localStorage`): não é token de sessão, então a RS01 continua valendo.
- Tela Carrinho (`/carrinho`), que exige login: sem sessão, vai para o Login e volta depois.

**Tarefas:**

- [x] Contexto do carrinho no `localStorage`, só com `idJogo` e `quantidade`, limitado a 1 a 10 (`8b26aa5`).
- [x] Tela: botão de adicionar no detalhe do jogo, desabilitado para jogo esgotado (`6251e2f`; ajuste em `5297081`).
- [x] Tela: `/carrinho` com alterar quantidade, remover e total, protegida por login (`6251e2f`; ajustes em `704fdc5` e `5297081`).

Só front-end: o carrinho não chega à API até a finalização (C04).

**Commits:** `8b26aa5`, `6251e2f`, `704fdc5`, `5297081`.

## C04: Finalizar compra

**História:** Como cliente, quero finalizar a compra do meu carrinho, para que o pedido seja registrado com o preço e o estoque reais.

**Critérios de aceite (desafio):**

- Só com sessão: 401, código 4.
- Pedido com ao menos um item e quantidade de 1 a 10: 400, código 3.
- Jogo inexistente ou inativo: 400, código 110 (`JOGO_INDISPONIVEL`).
- Estoque insuficiente: 409, código 111 (`ESTOQUE_INSUFICIENTE`).
- O preço vem do banco, nunca do front.
- O pedido nasce Pendente, com data em UTC e total calculado no Service.
- Pedido, itens e baixa de estoque ficam em uma só transação.
- Endpoint `POST /api/Pedido`, acesso Usuário logado.

**Tarefas:**

- [x] Tabelas: `tb_situacao_pedido` (1 Pendente, 2 Pago, 3 Enviado, 4 Cancelado), `tb_pedido`, `tb_item_pedido` com `UNIQUE` de jogo por pedido, no script `database/05-compras.sql`; consultas em `database/06-consultas-compras.sql` (`909429c`).
- [x] Erros 110 e 111 em `BusinessErrors` e constantes `SituacaoPedidoIds` (`4ce88fc`).
- [x] Modelos: `Pedido`, `ItemPedido`, `SituacaoPedido` (`8ff0a09`).
- [x] Repository: `PedidoRepository`, `ItemPedidoRepository`, `SituacaoPedidoRepository` e baixa de estoque no `JogoRepository` (`8ff0a09`).
- [x] Service: `PedidoService.CriarAsync` com `TransactionScope`, situação Pendente, `DateTime.UtcNow`, preço do banco e total calculado (`8ff0a09`).
- [x] Validator: `InserirPedidoRequestValidator` (ao menos um item, quantidade 1 a 10, jogo sem repetição) (`8ff0a09`).
- [x] Testes: `PedidoServiceTests` (`Criar_*`: caminho válido, preço do banco, 110, 111, compra simultânea) e `InserirPedidoRequestValidatorTests` (`8ff0a09`).
- [x] Endpoint: `POST /api/Pedido`, com login exigido pela política padrão (`8ff0a09`).
- [x] Tela: finalizar compra no `/carrinho`, tratando 110 e 111 (`2460a1a`; ajustes em `704fdc5` e `5297081`).

**Commits:** `909429c`, `4ce88fc`, `8ff0a09`, `2460a1a`, `704fdc5`, `5297081`.

## C05: Meus pedidos

**História:** Como cliente, quero ver a lista dos meus pedidos, para acompanhar o que comprei e a situação de cada compra.

**Critérios de aceite (desafio):**

- Lista só os pedidos do cliente logado.
- Do mais recente para o mais antigo.
- Com situação, total e itens (jogo, quantidade e preço pago).
- Endpoint `GET /api/Pedido/meus`, acesso Usuário logado.
- Tela MeusPedidos (`/meus-pedidos`), que exige login: sem sessão, vai para o Login e volta depois.

**Tarefas:**

- [x] Repository: `PedidoRepository.ListarPorUsuarioAsync` com `ORDER BY dt_pedido DESC` e `ItemPedidoRepository.ListarPorPedidosAsync` (`df42b7c`).
- [x] Service: `PedidoService.ListarMeusAsync`, compondo pedidos, itens, jogos e situações com LINQ (`df42b7c`).
- [x] Testes: `PedidoServiceTests` (`ListarMeus_*`: só do usuário, ordem, itens com preço pago, lista vazia) (`df42b7c`).
- [x] Endpoint: `GET /api/Pedido/meus` (`df42b7c`).
- [x] Tela: `/meus-pedidos`, protegida por login (`df3cbff`).

**Commits:** `df42b7c`, `df3cbff`.

## C06: Detalhe do pedido

**História:** Como cliente, quero ver o detalhe de um pedido meu, para conferir situação, data, itens e total; como administrador, quero ver o detalhe de qualquer pedido, para atender os clientes.

**Critérios de aceite (desafio):**

- Mostra situação, data, itens e total.
- O cliente vê só os seus pedidos: pedido de outro cliente, 403, código 113 (`PEDIDO_DE_OUTRO_USUARIO`).
- O administrador vê qualquer pedido.
- Pedido inexistente: 404, código 109 (`PEDIDO_NAO_ENCONTRADO`).
- Endpoint `GET /api/Pedido/{id}`, acesso Usuário logado (dono do pedido ou administrador).
- Tela Detalhe do pedido, a partir de Meus pedidos.

**Tarefas:**

- [x] Erros 109 e 113 em `BusinessErrors` (`4ce88fc`).
- [x] Repository: `PedidoRepository.BuscarPorIdAsync`; consulta em `database/07-consultas-gestao-pedidos.sql` (`62f9b12`).
- [x] Service: `PedidoService.ObterAsync` obtém o pedido, com 109 e 113 e acesso do administrador a qualquer pedido (`62f9b12`).
- [x] Testes: `PedidoServiceTests` (`Obter_*`: dono, administrador, outro cliente 113, inexistente 109) (`62f9b12`).
- [x] Endpoint: `GET /api/Pedido/{id:int}`, que também torna válido o `Location` do `POST /api/Pedido` (`62f9b12`).
- [x] Tela: `/pedidos/:id` (qualquer usuário logado), com link "Ver detalhes" em Meus pedidos e estados de 403 e 404 (`908d815`; ajustes em `603a6d3`).

As tabelas já existem (C04, `909429c`).

**Commits:** `4ce88fc` (só os códigos de erro).

## C07: Gestão de pedidos

**História:** Como administrador, quero listar todos os pedidos e mudar a situação de cada um, para acompanhar o pagamento e o envio das compras.

**Critérios de aceite (desafio):**

- O administrador lista todos os pedidos, com filtro por situação.
- Muda a situação de um pedido (Pendente, Pago, Enviado, Cancelado).
- Situação inexistente: 404, código 112 (`SITUACAO_NAO_ENCONTRADA`).
- Pedido inexistente: 404, código 109 (`PEDIDO_NAO_ENCONTRADO`).
- Endpoints `GET /api/Pedido?idSituacao=`, `PUT /api/Pedido/{id}/situacao` e `GET /api/SituacaoPedido`, acesso Administrador.
- Tela Pedidos (`/admin/pedidos`, com filtro e troca de situação), no menu Administração.

**Tarefas:**

- [x] Erros 109 e 112 em `BusinessErrors` (`4ce88fc`).
- [x] Repository: `PedidoRepository.ListarAsync` (filtro `FiltroPedido` no SQL) e `AlterarSituacaoAsync`; `UsuarioRepository.ListarPorIdsAsync` para o nome do cliente. A existência da situação é conferida no Service sobre `SituacaoPedidoRepository.ListarAsync`, sem consulta nova. Consultas em `database/07-consultas-gestao-pedidos.sql` (`51ec209`).
- [x] Service: `PedidoService.ListarAsync` e `AlterarSituacaoAsync`, com 109 (conferido antes) e 112; `SituacaoPedidoService.ListarAsync` (`51ec209`).
- [x] Validator: `AlterarSituacaoPedidoRequestValidator` (situação maior que zero) (`51ec209`).
- [x] Testes: `PedidoServiceTests` (`Listar_*` com e sem filtro e filtro inexistente 112; `AlterarSituacao_*` sucesso, 109 e 112), `SituacaoPedidoServiceTests` e `AlterarSituacaoPedidoRequestValidatorTests` (`51ec209`).
- [x] Endpoints: `GET /api/Pedido?idSituacao=`, `PUT /api/Pedido/{id}/situacao` e `GET /api/SituacaoPedido`, com `[Authorize(Policy = Politicas.Administrador)]` (`51ec209`).
- [x] Tela: `/admin/pedidos` com filtro na URL, troca de situação (confirmação só ao cancelar) e item "Pedidos" no menu Administração (`827f112`; ajustes em `603a6d3`).

As tabelas já existem (C04, `909429c`).

**Commits:** `4ce88fc` (só os códigos de erro).

## C08: Manutenção de gêneros

**História:** Como administrador, quero incluir, alterar e excluir gêneros, para manter o catálogo organizado.

**Critérios de aceite (desafio):**

- O administrador inclui, altera e exclui gêneros.
- Nome repetido: 409, código 107 (`GENERO_NOME_DUPLICADO`).
- Gênero com jogos: não exclui, 409, código 108 (`GENERO_POSSUI_JOGOS`).
- Mesmo comportamento da manutenção de plataformas.
- Endpoints `GET`, `POST`, `PUT` e `DELETE` em `/api/Genero` e `/api/Genero/{id}`: GET público; os demais, Administrador.
- Tela Genero (`/admin/generos`), no menu Administração.

**Tarefas:**

- [x] Erros 107 e 108 em `BusinessErrors` (`4ce88fc`).
- [x] Repository: `GeneroRepository.BuscarPorIdAsync`, `ExisteComNomeAsync`, `InserirAsync`, `AlterarAsync` e `ExcluirAsync` (gravações com `ExecutarGravacaoAsync`); `JogoGeneroRepository.ExisteComGeneroAsync` verifica os jogos do gênero na tabela de vínculo. Consultas em `database/08-consultas-generos.sql` (`db52e7e`).
- [x] Service: `GeneroService` com 107 (conferido antes e na `RegistroDuplicadoException`), 108 e 105 (`GENERO_NAO_ENCONTRADO`) (`db52e7e`).
- [x] Validator e ApiModels: `InserirGeneroRequestApiModel`, `AlterarGeneroRequestApiModel` e seus validators (nome obrigatório, até 50 caracteres) (`db52e7e`).
- [x] Testes: `GeneroServiceTests`, 11 testes (lista, 105, 107 na inclusão e na alteração, 108 e exclusão) (`db52e7e`).
- [x] Endpoints: `GET /api/Genero/{id}` público; `POST`, `PUT` e `DELETE` com `[Authorize(Policy = Politicas.Administrador)]` (`db52e7e`).
- [x] Tela: `/admin/generos` com inclusão, alteração e exclusão (com confirmação), e item "Gêneros" no menu Administração (`5a1dc51`; ajustes de rolagem e foco em `c464c92`).

A tabela `tb_genero` já existe (aula 1).

**Commits:** `4ce88fc` (códigos de erro), `db52e7e`, `5a1dc51` e `c464c92`.

---

## Revisão da sprint

Situação em 27/11/2026, dois dias antes da entrega.

**Pronto:**

- Versão básica completa: C01 a C05, com back-end, testes e telas. Responsável: Gabriel.
- Banco do módulo (`database/05-compras.sql` e `06-consultas-compras.sql`) e todos os códigos de erro 106 a 113 no catálogo `BusinessErrors`, inclusive os da versão completa.
- C06, C07 e C08: back-end, testes e telas. Responsável: Gabriel.
- 69 testes unitários passando (`dotnet test`, 27/11/2026) e build do front sem erro.
- Especificação do repositório (endpoints, tabelas, regras e rastreabilidade, seção 7.1 do desafio), pronta em 27/11/2026:
  - [`docs/USER_STORIES.md`](USER_STORIES.md): histórias C01 a C08 com critérios, regras, códigos de erro e a rastreabilidade história → endpoints → classes → testes → commits;
  - [`docs/API.md`](API.md): endpoints com acesso, status e códigos, e os catálogos `ResponseCodes` e `BusinessErrors`;
  - [`docs/MER.md`](MER.md): tabelas e relacionamentos, incluindo `tb_situacao_pedido`, `tb_pedido` e `tb_item_pedido`.
- `README.md` atualizado com o estado do projeto e como rodar.

**Ainda não pronto:**

- Decisões em aberto de C07, para o grupo e o professor: o desafio não define regra de transição entre situações (hoje qualquer troca é aceita) nem devolução de estoque ao cancelar (hoje o estoque não volta). A lista do administrador não tem paginação. Continuam pendentes: nenhuma alternativa foi escolhida.
- O desafio não diz em quais arquivos a especificação deve ficar; os três acima são uma escolha a confirmar com o professor (`needs-verification` em `docs/USER_STORIES.md`).

Esta seção será atualizada ao fim da sprint com o que foi entregue até 29/11/2026.
