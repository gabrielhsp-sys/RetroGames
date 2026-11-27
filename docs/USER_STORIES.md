# Histórias de usuário do Módulo de Compras (C01 a C08)

Especificação do que foi construído no Módulo de Compras, versão completa (C01 a C08). O texto dos critérios de aceite vem do desafio **Módulo de Compras**. A implementação, os testes e os commits vêm do código no `main`. Este arquivo atende ao item do desafio "Atualize a especificação do repositório (endpoints, tabelas, regras e rastreabilidade, seção 7.1)".

Arquivos relacionados:

- [MER.md](MER.md): tabelas, colunas e relacionamentos, incluindo `tb_situacao_pedido`, `tb_pedido` e `tb_item_pedido`.
- [API.md](API.md): todos os endpoints, com acesso, status de sucesso e códigos de erro, e os catálogos `ResponseCodes` e `BusinessErrors`.
- [SPRINT-COMPRAS.md](SPRINT-COMPRAS.md): quadro da sprint, tarefas e revisão.

Conferência de 27/11/2026: `dotnet test` com 69 testes, 0 falhas.

---

## C01: Cadastro de cliente

**História:** Como visitante, quero criar minha conta de cliente, para poder comprar jogos na loja.

**Critérios de aceite (desafio):** nome, e-mail válido e senha. Cria usuário ativo com perfil Cliente e senha em hash (RS04). Após o cadastro, a sessão já começa. Tela Cadastro (`/cadastro`), com link na tela de Login.

**Como está implementado:**

| Item | Valor |
| --- | --- |
| Endpoint | `POST /api/Autenticacao/cadastrar`, `[AllowAnonymous]`, 201 |
| Erros | 400 (código 3) do validator; 409 (106 `EMAIL_JA_CADASTRADO`) |
| Regras | O Service grava `st_ativo = true`, `co_perfil = PerfilIds.Cliente`, `dt_cadastro` em UTC e só o hash da senha (`PasswordHasher`). O e-mail é gravado sem espaços e em minúsculas, uma escolha do código que o desafio não pede. O e-mail repetido é conferido antes do INSERT; se o `UNIQUE` do banco recusar (cadastros simultâneos), a `RegistroDuplicadoException` vira o mesmo 106. A action faz `SignInAsync` depois do cadastro. |
| Validação | Nome até 100 caracteres, e-mail válido até 256, senha com no mínimo 8 caracteres |
| Tabela | `tb_usuario` (já existia; nenhuma alteração) |

## C02: Cliente logado

**História:** Como cliente, quero ver no menu o Carrinho e os Meus pedidos, para chegar às minhas compras sem acessar a Administração.

**Critérios de aceite (desafio):** o menu mostra Carrinho e Meus pedidos; o cliente não vê a Administração.

**Como está implementado:** só front-end, em `components/Layout.jsx`. "Meus pedidos" aparece para quem está logado (cliente ou administrador). O menu Administração aparece só quando `ehAdministrador`. O link do Carrinho, com contador, aparece para todos; a rota `/carrinho` exige login. A proteção real é da API (RS03).

## C03: Carrinho

**História:** Como cliente, quero montar um carrinho com os jogos que escolhi, para revisar quantidades e total antes de comprar.

**Critérios de aceite (desafio):** adicionar a partir do detalhe do jogo (não para jogo esgotado), alterar quantidade (1 a 10), remover itens e ver o total. Fica no navegador (`localStorage`): não é token de sessão, então a RS01 continua valendo. Tela `/carrinho`, que exige login: sem sessão, vai para o Login e volta depois.

**Como está implementado:** só front-end. O carrinho fica no `localStorage`, na chave `retrogames.carrinho`, só com `idJogo` e `quantidade` (`contexts/CarrinhoProvider.jsx`, `contexts/carrinhoContext.js`, `QUANTIDADE_MAXIMA = 10`). Nome, preço e estoque são lidos da API. No detalhe do jogo, o botão fica desabilitado para jogo esgotado ou inativo. A rota `/carrinho` está dentro de `RotaProtegida`, que manda para `/login?voltarPara=…`. A API só recebe o carrinho na finalização (C04).

## C04: Finalizar compra

**História:** Como cliente, quero finalizar a compra do meu carrinho, para que o pedido seja registrado com o preço e o estoque reais.

**Critérios de aceite (desafio):** só com sessão (401, código 4). Pedido com ao menos um item e quantidade de 1 a 10 (400, código 3). Jogo inexistente ou inativo: 400, código 110. Estoque insuficiente: 409, código 111. O preço vem do banco, nunca do front. O pedido nasce Pendente, com data em UTC e total calculado no Service. Pedido, itens e baixa de estoque ficam em uma só transação.

**Como está implementado:**

| Item | Valor |
| --- | --- |
| Endpoint | `POST /api/Pedido`, política padrão (login), 201 com `Location` em `/api/Pedido/{id}` |
| Erros | 401 (4); 400 (3); 400 (110 `JOGO_INDISPONIVEL`); 409 (111 `ESTOQUE_INSUFICIENTE`) |
| Validação | `InserirPedidoRequestValidator`: ao menos um item, quantidade de 1 a 10, `idJogo` maior que zero e cada jogo uma só vez no pedido (o `UNIQUE (co_pedido, co_jogo)` também garante) |
| Regras no Service | `PedidoService.CriarAsync`: (1) lê os jogos do pedido; inexistente ou inativo, 110; (2) quantidade maior que o estoque, 111, antes de abrir a transação; (3) `ValorUnitario` de cada item recebe o `Preco` do banco (o ApiModel de entrada nem tem campo de preço); (4) `Data = DateTime.UtcNow`, `IdSituacaoPedido = SituacaoPedidoIds.Pendente`, `Total = Σ quantidade × valor unitário`; (5) num único `TransactionScope` (`ReadCommitted`): baixa de estoque de cada item, INSERT do pedido e INSERT dos itens, e `Complete()` no fim |
| Compra simultânea | `JogoRepository.BaixarEstoqueAsync` só baixa `WHERE qt_estoque >= @quantidade`. Se outra compra levou o estoque depois da conferência, a baixa devolve `false`, o Service lança 111 sem `Complete()` e a transação desfaz as baixas já feitas |
| Tabelas | `tb_situacao_pedido`, `tb_pedido` e `tb_item_pedido` (`database/05-compras.sql`); baixa em `tb_jogo.qt_estoque` |
| Tela | `/carrinho`: "Finalizar compra"; em 110 ou 111 a tela relê os jogos e diz pelo nome o que mudou |

## C05: Meus pedidos

**História:** Como cliente, quero ver a lista dos meus pedidos, para acompanhar o que comprei e a situação de cada compra.

**Critérios de aceite (desafio):** lista só os pedidos do cliente logado, do mais recente para o mais antigo, com situação, total e itens (jogo, quantidade e preço pago). Tela `/meus-pedidos`, que exige login.

**Como está implementado:**

| Item | Valor |
| --- | --- |
| Endpoint | `GET /api/Pedido/meus`, política padrão (login), 200 |
| Regras | O id do usuário vem da sessão (`IUsuarioAtualService`), nunca da requisição. `PedidoRepository.ListarPorUsuarioAsync` filtra e ordena no SQL (`ORDER BY dt_pedido DESC, co_seq_pedido DESC`). O Service compõe pedidos, itens, jogos e situações com LINQ, sempre com o mesmo número de consultas. Sem pedidos, devolve lista vazia sem consultar itens |
| Resposta | Cada pedido com `descricaoSituacao`, `total` e `itens` (`nomeJogo`, `quantidade`, `valorUnitario` = preço pago) |
| Tela | `/meus-pedidos`, dentro de `RotaProtegida` |

## C06: Detalhe do pedido

**História:** Como cliente, quero ver o detalhe de um pedido meu, para conferir situação, data, itens e total; como administrador, quero ver o detalhe de qualquer pedido, para atender os clientes.

**Critérios de aceite (desafio):** mostra situação, data, itens e total. O cliente vê só os seus pedidos: pedido de outro cliente, 403, código 113. O administrador vê qualquer pedido. Pedido inexistente: 404, código 109. Tela de detalhe a partir de Meus pedidos.

**Como está implementado:**

| Item | Valor |
| --- | --- |
| Endpoint | `GET /api/Pedido/{id:int}`, política padrão (login), 200 |
| Erros | 401 (4); 404 (109 `PEDIDO_NAO_ENCONTRADO`); 403 (113 `PEDIDO_DE_OUTRO_USUARIO`) |
| Regras | `PedidoService.ObterAsync` busca o pedido primeiro: inexistente é 404 para qualquer usuário. Depois, quem não é administrador e não é o dono recebe 113. Este é o 403 por regra de negócio citado no desafio, diferente do 403 de perfil (código 5) |
| Tela | `/pedidos/:id`, dentro de `RotaProtegida`, com link "Ver detalhes" em Meus pedidos e mensagens próprias para 403 e 404 |

## C07: Gestão de pedidos

**História:** Como administrador, quero listar todos os pedidos e mudar a situação de cada um, para acompanhar o pagamento e o envio das compras.

**Critérios de aceite (desafio):** o administrador lista todos os pedidos, com filtro por situação, e muda a situação de um pedido (Pendente, Pago, Enviado, Cancelado). Situação inexistente: 404, código 112. Pedido inexistente: 404, código 109. Tela `/admin/pedidos`, com filtro e troca de situação, no menu Administração.

**Como está implementado:**

| Item | Valor |
| --- | --- |
| Endpoints | `GET /api/Pedido?idSituacao=`, `PUT /api/Pedido/{id}/situacao` e `GET /api/SituacaoPedido`, todos com `[Authorize(Policy = Politicas.Administrador)]`, 200 |
| Erros | 401 (4); 403 (5); 400 (3, `idSituacao` não informado); 404 (109); 404 (112 `SITUACAO_NAO_ENCONTRADA`) |
| Regras | `PedidoService.ListarAsync`: com filtro, confere a situação na lista de `tb_situacao_pedido` (112) antes de listar; o filtro vai para o SQL (`FiltroPedido`); o nome do cliente vem de `UsuarioRepository.ListarPorIdsAsync`, combinado com LINQ. `PedidoService.AlterarSituacaoAsync`: confere o pedido (109), depois a situação (112), e grava com um só `UPDATE`, sem transação |
| Tela | `/admin/pedidos`: filtro por situação na URL e troca de situação; só a troca para Cancelado pede confirmação, avisando que o estoque não é devolvido |

**Pendente (decisão do professor):** os três pontos abaixo não são definidos pelo desafio. O código está como descrito, e nenhuma alternativa foi escolhida.

| Ponto em aberto | Como está hoje |
| --- | --- |
| Regra de transição entre situações | Qualquer troca é aceita (por exemplo, de Cancelado para Pago). |
| Devolução de estoque ao cancelar | O estoque não volta: mudar para Cancelado não altera `tb_jogo.qt_estoque`. |
| Paginação da lista do administrador | Sem paginação: `GET /api/Pedido` devolve todos os pedidos do filtro. |

## C08: Manutenção de gêneros

**História:** Como administrador, quero incluir, alterar e excluir gêneros, para manter o catálogo organizado.

**Critérios de aceite (desafio):** o administrador inclui, altera e exclui gêneros. Nome repetido: 409, código 107. Gênero com jogos: não exclui, 409, código 108. Mesmo comportamento da manutenção de plataformas. Tela `/admin/generos`, no menu Administração.

**Como está implementado:**

| Item | Valor |
| --- | --- |
| Endpoints | `GET /api/Genero` e `GET /api/Genero/{id}` públicos; `POST` (201), `PUT` (200) e `DELETE` (200) com `[Authorize(Policy = Politicas.Administrador)]` |
| Erros | 400 (3); 404 (105 `GENERO_NAO_ENCONTRADO`); 409 (107 `GENERO_NOME_DUPLICADO`); 409 (108 `GENERO_POSSUI_JOGOS`); 401 (4) e 403 (5) nas gravações |
| Regras | `GeneroService`, no mesmo formato do `PlataformaService`: nome repetido conferido antes da gravação (ignorando o próprio gênero na alteração) e também pela `RegistroDuplicadoException` do `UNIQUE`, ambos 107; exclusão recusada com 108 quando `JogoGeneroRepository.ExisteComGeneroAsync` acha vínculos; id inexistente, 105 |
| Validação | Nome obrigatório, até 50 caracteres (o mesmo tamanho de `no_genero`) |
| Tela | `/admin/generos`: inclusão, alteração e exclusão com confirmação |

---

## Regras do Módulo de Compras

O desafio traz as regras como critérios de aceite e não dá a elas um número RN. Por isso, elas não receberam números novos aqui: cada uma é identificada pela história. As regras RN01 a RN06 e RS01 a RS06 da especificação continuam valendo.

| História | Regra | Onde fica | Resultado da violação | Testes |
| --- | --- | --- | --- | --- |
| C01 | E-mail único | `AutenticacaoService.CadastrarAsync` (conferência e `RegistroDuplicadoException`); `UNIQUE` em `ds_email` | 409, 106 | `Cadastrar_EmailJaCadastrado_LancaEmailJaCadastradoENaoGrava`, `Cadastrar_BancoRecusaEmailDuplicado_LancaEmailJaCadastrado` |
| C01 | Cadastro público cria só Cliente ativo, com senha em hash (RS04) | `AutenticacaoService.CadastrarAsync` | — | `Cadastrar_DadosValidos_GravaClienteAtivoEDevolveComId`, `Cadastrar_SenhaDigitada_GravaSoOHashQueConfereComASenha`, `Cadastrar_EmailComEspacosEMaiusculas_GravaSemEspacosEMinusculo` |
| C03 | Quantidade de 1 a 10 no carrinho | `CarrinhoProvider.jsx` (usabilidade; a API confere de novo em C04) | — | — |
| C04 | Ao menos um item; quantidade de 1 a 10; jogo sem repetição | `InserirPedidoRequestValidator` | 400, 3 | `InserirPedidoRequestValidatorTests` (`Validar_QuantidadeNaFaixa_EhValido`, `Validar_QuantidadeForaDaFaixa_EhInvalido`, `Validar_SemItens_EhInvalido`, `Validar_MesmoJogoDuasVezes_EhInvalido`) |
| C04 | Só jogo existente e ativo é vendido | `PedidoService.CriarAsync` | 400, 110 | `Criar_JogoInexistente_LancaJogoIndisponivelENaoGrava`, `Criar_JogoInativo_LancaJogoIndisponivelENaoGrava` |
| C04 | Quantidade não passa do estoque | `PedidoService.CriarAsync` e `JogoRepository.BaixarEstoqueAsync` | 409, 111 | `Criar_QuantidadeUmAcimaDoEstoque_LancaEstoqueInsuficienteENaoGrava`, `Criar_QuantidadeIgualAoEstoque_GravaOPedido`, `Criar_BaixaRecusadaPorCompraSimultanea_LancaEstoqueInsuficienteENaoInserePedido` |
| C04 | Preço do banco; total, data UTC e situação Pendente definidos no Service | `PedidoService.CriarAsync` | — | `Criar_ItemComPrecoVindoDeFora_UsaOPrecoDoBanco`, `Criar_ItensValidos_GravaPedidoPendenteDoUsuarioComTotalCalculado`, `Criar_ItensValidos_DevolvePedidoComSituacaoEItensComNomeDoJogo` |
| C04 | Pedido, itens e baixa de estoque numa só transação | `PedidoService.CriarAsync` (`TransactionScope`) | nada é gravado | `Criar_ItensValidos_BaixaOEstoqueEGravaOsItensDoPedido`, `Criar_BaixaRecusadaPorCompraSimultanea_LancaEstoqueInsuficienteENaoInserePedido` (com repositórios simulados; o rollback no MySQL é needs-verification) |
| C05 | Só os pedidos do usuário logado, do mais recente ao mais antigo | `PedidoService.ListarMeusAsync`, `PedidoRepository.ListarPorUsuarioAsync` | — | `ListarMeus_UsuarioLogado_BuscaSoOsPedidosDele`, `ListarMeus_PedidosDoMaisRecenteAoMaisAntigo_MantemAOrdem`, `ListarMeus_ItensDeCadaPedido_ComNomeDoJogoEPrecoPago`, `ListarMeus_SemPedidos_DevolveListaVaziaSemConsultarItens` |
| C06 | Pedido existe | `PedidoService.ObterAsync` | 404, 109 | `Obter_PedidoInexistente_LancaPedidoNaoEncontrado` |
| C06 | Cliente só vê o próprio pedido; administrador vê qualquer um | `PedidoService.ObterAsync` | 403, 113 | `Obter_PedidoDeOutroCliente_LancaPedidoDeOutroUsuario`, `Obter_PedidoDoProprioCliente_DevolveSituacaoDataItensETotal`, `Obter_AdministradorEPedidoDeOutroUsuario_DevolveODetalhe` |
| C07 | Situação existe (filtro e troca) | `PedidoService.ListarAsync` e `AlterarSituacaoAsync` | 404, 112 | `Listar_FiltroComSituacaoInexistente_LancaSituacaoNaoEncontradaSemListar`, `AlterarSituacao_SituacaoInexistente_LancaSituacaoNaoEncontradaSemGravar` |
| C07 | Pedido existe (troca) | `PedidoService.AlterarSituacaoAsync` | 404, 109 | `AlterarSituacao_PedidoInexistente_LancaPedidoNaoEncontradoSemGravar` |
| C08 | Nome de gênero único | `GeneroService` (conferência e `RegistroDuplicadoException`); `UNIQUE` em `no_genero` | 409, 107 | `Inserir_NomeJaCadastrado_LancaNomeDuplicadoENaoInsere`, `Inserir_RepositoryLancaRegistroDuplicado_LancaNomeDuplicado`, `Alterar_NomeDeOutroGenero_LancaNomeDuplicadoENaoGrava`, `Alterar_MantendoOProprioNome_GravaAlteracao` |
| C08 | Gênero com jogos não é excluído | `GeneroService.ExcluirAsync` | 409, 108 | `Excluir_GeneroComJogos_LancaPossuiJogosENaoExclui`, `Excluir_GeneroSemJogos_ExcluiOGenero` |
| C08 | Gênero existe | `GeneroService` | 404, 105 | `BuscarPorId_IdInexistente_LancaNaoEncontrado`, `Alterar_IdInexistente_LancaNaoEncontradoENaoGrava`, `Excluir_IdInexistente_LancaNaoEncontradoENaoExclui` |

Os testes de C01 estão em `AutenticacaoServiceTests`, os de C04 a C07 em `PedidoServiceTests` (fora os de validator) e os de C08 em `GeneroServiceTests`.

---

## Rastreabilidade (Espec. 7.1)

Mesmo formato da tabela 7.1 da especificação, com a coluna de commits pedida no desafio ("A mensagem de cada commit cita a história"). O traço indica que não há teste automatizado específico.

### Módulo de Compras

| Funcionalidade | Endpoints | Regras | Classes principais | Testes | Commits |
| --- | --- | --- | --- | --- | --- |
| C01 Cadastro de cliente | `POST /api/Autenticacao/cadastrar` | C01; RS04 | `AutenticacaoController.Cadastrar`, `AutenticacaoService.CadastrarAsync`, `UsuarioRepository`, `CadastrarUsuarioRequestApiModel`, `CadastrarUsuarioRequestValidator`, `UsuarioMapper`; front `api/servicos.js` (`autenticacaoApi.cadastrar`), tela `pages/Cadastro/Cadastro.jsx` | `AutenticacaoServiceTests` (5 testes `Cadastrar_*`) | `4ce88fc`, `2d5cccf`, `1961c47` |
| C02 Cliente logado | — | RS03 (a proteção real é da API) | `components/Layout.jsx` | — | `8d76fdf`; ajuste `704fdc5` |
| C03 Carrinho | — (só navegador) | C03; RS01 | `contexts/CarrinhoProvider.jsx`, `contexts/carrinhoContext.js`, `hooks/useCarrinho.js`, `pages/Catalogo/DetalheJogo.jsx`, `pages/Carrinho/Carrinho.jsx`, `components/RotaProtegida.jsx` | — | `8b26aa5`, `6251e2f`; ajustes `704fdc5`, `5297081` |
| C04 Finalizar compra | `POST /api/Pedido` | C04 | `PedidoController.Inserir`, `PedidoService.CriarAsync`, `PedidoRepository`, `ItemPedidoRepository`, `SituacaoPedidoRepository`, `JogoRepository` (`ListarPorIdsAsync`, `BaixarEstoqueAsync`), `InserirPedidoRequestApiModel`, `InserirItemPedidoRequestApiModel`, `InserirPedidoRequestValidator`, `PedidoMapper`, `PedidoDetalheServiceModel`, `ItemPedidoDetalheServiceModel`; modelos `Pedido`, `ItemPedido`, `SituacaoPedido`; `database/05-compras.sql`, `06-consultas-compras.sql`; front `api/servicos.js` (`pedidoApi.criar`) e tela `pages/Carrinho/Carrinho.jsx` | `PedidoServiceTests` (9 testes `Criar_*`), `InserirPedidoRequestValidatorTests` (4 métodos, 6 casos) | `909429c`, `4ce88fc`, `8ff0a09`, `2460a1a`; ajustes `704fdc5`, `5297081` |
| C05 Meus pedidos | `GET /api/Pedido/meus` | C05 | `PedidoController.ListarMeus`, `PedidoService.ListarMeusAsync`, `PedidoRepository.ListarPorUsuarioAsync`, `ItemPedidoRepository.ListarPorPedidosAsync`; front `api/servicos.js` (`pedidoApi.listarMeus`), telas `pages/MeusPedidos/MeusPedidos.jsx`, `components/Pedido.jsx` | `PedidoServiceTests` (4 testes `ListarMeus_*`) | `df42b7c`, `df3cbff` |
| C06 Detalhe do pedido | `GET /api/Pedido/{id}` | C06 | `PedidoController.ListarPorId`, `PedidoService.ObterAsync`, `PedidoRepository.BuscarPorIdAsync`; `database/07-consultas-gestao-pedidos.sql`; front `api/servicos.js` (`pedidoApi.buscarPorId`), tela `pages/Pedidos/DetalhePedido.jsx` | `PedidoServiceTests` (4 testes `Obter_*`) | `4ce88fc` (códigos 109 e 113), `62f9b12`, `908d815`; ajuste `603a6d3` |
| C07 Gestão de pedidos | `GET /api/Pedido?idSituacao=`, `PUT /api/Pedido/{id}/situacao`, `GET /api/SituacaoPedido` | C07 | `PedidoController.Listar` e `AlterarSituacao`, `SituacaoPedidoController`, `PedidoService.ListarAsync` e `AlterarSituacaoAsync`, `SituacaoPedidoService`, `PedidoRepository` (`ListarAsync`, `AlterarSituacaoAsync`), `UsuarioRepository.ListarPorIdsAsync`, `FiltroPedido`, `ListarPedidoRequestApiModel`, `ListarTodosPedidoResponseApiModel`, `AlterarSituacaoPedidoRequestApiModel`, `AlterarSituacaoPedidoRequestValidator`, `SituacaoPedidoMapper`; `database/07-consultas-gestao-pedidos.sql`; front `api/servicos.js` (`pedidoApi.listar`, `pedidoApi.alterarSituacao`, `situacaoPedidoApi.listar`), tela `pages/Administracao/Pedidos.jsx` | `PedidoServiceTests` (3 testes `Listar_*` e 3 `AlterarSituacao_*`), `SituacaoPedidoServiceTests` (1), `AlterarSituacaoPedidoRequestValidatorTests` (2 métodos, 3 casos) | `4ce88fc` (códigos 109 e 112), `51ec209`, `827f112`; ajuste `603a6d3` |
| C08 Manutenção de gêneros | `GET`, `POST`, `PUT` e `DELETE` `/api/Genero` e `/api/Genero/{id}` | C08 | `GeneroController`, `GeneroService`, `GeneroRepository`, `JogoGeneroRepository.ExisteComGeneroAsync`, `InserirGeneroRequestApiModel`, `AlterarGeneroRequestApiModel`, `InserirGeneroRequestValidator`, `AlterarGeneroRequestValidator`, `GeneroMapper`; `database/08-consultas-generos.sql`; front `api/servicos.js` (`generoApi`), tela `pages/Auxiliares/Genero.jsx` | `GeneroServiceTests` (11 testes) | `4ce88fc` (códigos 107 e 108), `db52e7e`, `5a1dc51`, `c464c92` |

Os commits `704fdc5`, `5297081` e `603a6d3` têm prefixo `style:` e não citam história no título; alteram só as telas indicadas na tabela (conferido com `git show --stat`). O `4ce88fc` cita C01 e C04 no título, mas criou todos os códigos de 106 a 113 e a constante `SituacaoPedidoIds`.

Testes por história somados: C01 5, C04 15, C05 4, C06 4, C07 10, C08 11, total 49. Os outros 20 dos 69 testes são das aulas (tabela abaixo) e do administrador inicial (`AutenticacaoServiceTests.GarantirAdministradorInicial_*`).

### Catálogo e Administração (aulas), com os testes que existem hoje

A tabela 7.1 da especificação cita classes de teste que não existem neste repositório: `PlataformaControllerTests`, `PlataformaValidatorsTests`, `JogoValidatorsTests`, `AutenticacaoValidatorsTests` e `GlobalExceptionHandlerTests`. A tabela abaixo mostra os testes reais. A especificação, na seção 9, também cita controllers e o `GlobalExceptionHandler` como alvos de teste; aqui eles não têm testes automatizados.

| Funcionalidade | Endpoints | Regras | Classes principais | Testes no repositório |
| --- | --- | --- | --- | --- |
| Catálogo público de jogos | `GET /api/Jogo` | RN04 | `JogoController`, `JogoService`, `JogoRepository` | `JogoServiceTests` (`Listar_*`, 4 testes) |
| Detalhe de jogo | `GET /api/Jogo/{id}` | RN04 | `JogoController`, `JogoService` | `JogoServiceTests` (`BuscarPorId_*`, 2 testes) |
| Manutenção de plataformas | `GET`, `POST`, `PUT` e `DELETE` `/api/Plataforma` | RN01, RN02 | `PlataformaController`, `PlataformaService`, `PlataformaRepository`, validators de plataforma | `PlataformaServiceTests` (7 testes) |
| Manutenção de jogos | `POST`, `PUT` e `DELETE` `/api/Jogo` | RN03, RN05, RN06 | `JogoController`, `JogoService`, `JogoRepository`, `JogoGeneroRepository`, validators de jogo | `JogoServiceTests` (`Inserir_*` 3 e `Desativar_*` 2); RN06: — |
| Gêneros e condições de jogo | `GET /api/Genero`, `GET /api/CondicaoJogo` | — | `GeneroController`, `CondicaoJogoController` | `GeneroServiceTests` (`Listar_*`), `CondicaoJogoServiceTests` (1) |
| Autenticação e autorização | `/api/Autenticacao` (login, logout, eu) | RS01 a RS06 | `AutenticacaoController`, `AutenticacaoService`, `UsuarioRepository` | `AutenticacaoServiceTests` (`GarantirAdministradorInicial_*`, 1 teste) |
| Todas | — | Envelope (5.3) | `GlobalExceptionHandler`, `FluentValidationFilter` | — |

---

## O que ficou needs-verification

| Item | Por quê |
| --- | --- |
| Nome e local dos arquivos da especificação | O desafio pede a atualização "da especificação do repositório", mas não diz em quais arquivos. Os arquivos `docs/MER.md`, `docs/API.md` e `docs/USER_STORIES.md` foram uma escolha, também porque o `AGENTS.md` cita `docs/MER.md` e `docs/USER_STORIES.md`. Falta confirmar com o professor. |
| Numeração das regras de Compras | O desafio não numera essas regras. Falta confirmar se o professor quer números novos (RN07 em diante) ou se o código da história basta. |
| Rollback real no MySQL | Os testes unitários provam, com repositórios simulados, que o Service não chega ao `Complete()` quando falta estoque. Esta conferência não rodou a API com o banco, então não comprovou que o `TransactionScope` com o MySqlConnector desfaz a baixa no MySQL (item "Uma compra com estoque insuficiente não grava nada" do checklist do desafio). |
| Respostas 401 (4) e 403 (5) nas rotas de Compras | Vêm da configuração em `AutenticacaoConfig.cs` e `AutorizacaoConfig.cs`. Não há teste automatizado de controller, e esta conferência não chamou as rotas. |
| Versão completa num projeto individual | O desafio reserva a versão completa (C01 a C08) para grupos de 3 ou mais estudantes e a básica (C01 a C05) para 1 ou 2. Este repositório é individual e entrega a completa. Falta confirmar com o professor que isso é aceito. |
| Mensagens dos códigos 104 e 105 | O texto no `BusinessErrors` é diferente do texto da tabela 5.4 da especificação (detalhe em [API.md](API.md)). A especificação tem prioridade (`AGENTS.md`, seção 2), mas o código não foi alterado, porque esta tarefa não muda código. |
| Comportamento das telas | O que este arquivo diz das telas foi lido do código. Esta conferência não abriu o navegador: o fluxo completo e as larguras de 360 px e 1366 px não foram conferidos. |
