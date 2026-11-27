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
