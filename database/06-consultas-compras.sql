-- =====================================================================
-- RetroGames — Módulo de Compras: consultas dos Repositories
-- Executar sobre um banco com 01-schema.sql, 02-dados.sql e
-- 05-compras.sql. Troque os valores das variáveis para outros dados.
--
-- Cada consulta lê uma única tabela, como no Repository da API
-- (AGENTS.md): a composição de pedido, itens, jogos e situação é feita
-- no PedidoService, com LINQ. As variáveis @... fazem o papel dos
-- parâmetros que a API envia.
--
-- As gravações (bloco 2) rodam dentro de uma transação desfeita com
-- ROLLBACK no fim: o script não deixa dados no banco.
-- =====================================================================

USE retrogames;

-- ---------------------------------------------------------------------
-- Parâmetros
-- ---------------------------------------------------------------------
SET @co_usuario = (SELECT MIN(co_seq_usuario) FROM tb_usuario);
SET @co_jogo    = (SELECT co_seq_jogo FROM tb_jogo WHERE no_jogo = 'Tetris');
SET @quantidade = 2;
-- Preço do jogo e último pedido do usuário, lidos antes (na API, o Service
-- já tem esses valores em memória).
SET @vl_preco   = (SELECT vl_preco FROM tb_jogo WHERE co_seq_jogo = @co_jogo);
SET @co_pedido_consulta = (SELECT MAX(co_seq_pedido) FROM tb_pedido WHERE co_usuario = @co_usuario);

-- ---------------------------------------------------------------------
-- 1. Leituras
-- ---------------------------------------------------------------------

-- SituacaoPedidoRepository.ListarAsync: todas as situações.
SELECT co_situacao_pedido, ds_situacao_pedido
FROM tb_situacao_pedido
ORDER BY co_situacao_pedido;

-- JogoRepository.ListarPorIdsAsync: os jogos de uma lista de ids, ativos
-- ou não (o PedidoService decide o que fazer com o inativo).
SELECT co_seq_jogo, no_jogo, ds_jogo, nu_ano_lancamento, vl_preco, qt_estoque,
       st_ativo, dt_cadastro, co_plataforma, co_condicao_jogo
FROM tb_jogo
WHERE co_seq_jogo IN (@co_jogo);

-- PedidoRepository.ListarPorUsuarioAsync (C05): os pedidos de um usuário,
-- do mais recente para o mais antigo.
SELECT co_seq_pedido, dt_pedido, vl_total, co_usuario, co_situacao_pedido
FROM tb_pedido
WHERE co_usuario = @co_usuario
ORDER BY dt_pedido DESC, co_seq_pedido DESC;

-- ItemPedidoRepository.ListarPorPedidosAsync: os itens de uma lista de pedidos.
SELECT co_seq_item_pedido, qt_item, vl_unitario, co_pedido, co_jogo
FROM tb_item_pedido
WHERE co_pedido IN (@co_pedido_consulta)
ORDER BY co_pedido, co_seq_item_pedido;

-- ---------------------------------------------------------------------
-- 2. Gravações do C04, na ordem do PedidoService (uma transação)
-- ---------------------------------------------------------------------
START TRANSACTION;

-- JogoRepository.BaixarEstoqueAsync: baixa só se houver estoque.
-- 1 linha afetada = baixou; 0 linhas = estoque insuficiente (o Service
-- lança ESTOQUE_INSUFICIENTE e a transação é desfeita).
UPDATE tb_jogo
SET qt_estoque = qt_estoque - @quantidade
WHERE co_seq_jogo = @co_jogo AND qt_estoque >= @quantidade;
SELECT ROW_COUNT() AS linhas_afetadas;

-- PedidoRepository.InserirAsync: o pedido nasce Pendente (1), data em UTC.
INSERT INTO tb_pedido (dt_pedido, vl_total, co_usuario, co_situacao_pedido)
VALUES (UTC_TIMESTAMP(), @vl_preco * @quantidade, @co_usuario, 1);
SET @co_pedido = LAST_INSERT_ID();

-- ItemPedidoRepository.InserirAsync: um INSERT com uma linha por item;
-- vl_unitario é o preço do banco no momento da compra.
INSERT INTO tb_item_pedido (qt_item, vl_unitario, co_pedido, co_jogo)
VALUES (@quantidade, @vl_preco, @co_pedido, @co_jogo);

-- Conferência (só no script).
SELECT co_seq_pedido, vl_total, co_situacao_pedido FROM tb_pedido WHERE co_seq_pedido = @co_pedido;
SELECT co_jogo, qt_item, vl_unitario FROM tb_item_pedido WHERE co_pedido = @co_pedido;

-- Desfaz tudo: o script só demonstra as gravações.
ROLLBACK;
