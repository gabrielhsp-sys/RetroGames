-- =====================================================================
-- RetroGames — Módulo de Compras: consultas do detalhe (C06) e da
-- gestão de pedidos (C07)
-- Executar sobre um banco com 01-schema.sql, 02-dados.sql e
-- 05-compras.sql. Troque os valores das variáveis para outros dados.
--
-- Cada consulta lê uma única tabela, como no Repository da API
-- (AGENTS.md). A composição com itens, jogos, situação e cliente é feita
-- no PedidoService, com LINQ, reaproveitando as leituras do
-- 06-consultas-compras.sql. As variáveis @... fazem o papel dos
-- parâmetros que a API envia.
-- =====================================================================

USE retrogames;

-- ---------------------------------------------------------------------
-- Parâmetros
-- ---------------------------------------------------------------------
SET @co_pedido = (SELECT MAX(co_seq_pedido) FROM tb_pedido);

-- ---------------------------------------------------------------------
-- 1. Detalhe do pedido (C06)
-- ---------------------------------------------------------------------

-- PedidoRepository.BuscarPorIdAsync: um pedido pelo id. O PedidoService
-- confere se é do usuário logado (ou se ele é administrador).
SELECT co_seq_pedido, dt_pedido, vl_total, co_usuario, co_situacao_pedido
FROM tb_pedido
WHERE co_seq_pedido = @co_pedido;

-- ---------------------------------------------------------------------
-- 2. Gestão de pedidos (C07)
-- ---------------------------------------------------------------------
SET @co_situacao_filtro = 1;   -- Pendente; NULL lista todos
SET @co_situacao_nova   = 2;   -- Pago
-- Cliente do pedido, lido antes (na API, o Service já tem os pedidos em memória).
SET @co_usuario_pedido  = (SELECT co_usuario FROM tb_pedido WHERE co_seq_pedido = @co_pedido);

-- PedidoRepository.ListarAsync: todos os pedidos ou só os de uma situação,
-- do mais recente para o mais antigo.
SELECT co_seq_pedido, dt_pedido, vl_total, co_usuario, co_situacao_pedido
FROM tb_pedido
WHERE (@co_situacao_filtro IS NULL OR co_situacao_pedido = @co_situacao_filtro)
ORDER BY dt_pedido DESC, co_seq_pedido DESC;

-- UsuarioRepository.ListarPorIdsAsync: os clientes dos pedidos listados
-- (o PedidoService usa só o nome).
SELECT co_seq_usuario, no_usuario, ds_email, ds_senha_hash, st_ativo, dt_cadastro, co_perfil
FROM tb_usuario
WHERE co_seq_usuario IN (@co_usuario_pedido);

-- PedidoRepository.AlterarSituacaoAsync: troca a situação de um pedido.
-- Roda numa transação desfeita com ROLLBACK: o script não deixa alteração.
START TRANSACTION;

UPDATE tb_pedido
SET co_situacao_pedido = @co_situacao_nova
WHERE co_seq_pedido = @co_pedido;

SELECT co_seq_pedido, co_situacao_pedido FROM tb_pedido WHERE co_seq_pedido = @co_pedido;

ROLLBACK;
