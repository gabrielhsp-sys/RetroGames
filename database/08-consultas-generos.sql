-- =====================================================================
-- RetroGames — Módulo de Compras: consultas da manutenção de gêneros (C08)
-- Executar sobre um banco com 01-schema.sql e 02-dados.sql.
-- Troque os valores das variáveis para consultar outros dados.
--
-- Nenhuma tabela nova: tb_genero e tb_jogo_genero já existem (aula 1).
-- Cada consulta lê uma única tabela, como no Repository da API
-- (AGENTS.md). As variáveis @... fazem o papel dos parâmetros que a API
-- envia. As gravações rodam numa transação desfeita com ROLLBACK: o
-- script não deixa alteração no banco.
-- =====================================================================

USE retrogames;

-- Mesma collation das colunas (ver 03-consultas.sql): sem isto, comparar
-- no_genero com uma variável pode falhar com o erro 1267.
SET NAMES utf8mb4 COLLATE utf8mb4_unicode_ci;

-- ---------------------------------------------------------------------
-- Parâmetros
-- ---------------------------------------------------------------------
SET @co_genero  = (SELECT co_seq_genero FROM tb_genero WHERE no_genero = 'RPG');
SET @no_genero  = 'Simulação'; -- nome de um gênero novo
SET @id_ignorar = NULL;        -- na alteração, o id do próprio gênero

-- ---------------------------------------------------------------------
-- 1. Leitura
-- ---------------------------------------------------------------------

-- GeneroRepository.ListarAsync: todos os gêneros, por nome.
SELECT co_seq_genero, no_genero
FROM tb_genero
ORDER BY no_genero;

-- GeneroRepository.BuscarPorIdAsync: um gênero pelo id.
SELECT co_seq_genero, no_genero
FROM tb_genero
WHERE co_seq_genero = @co_genero;

-- ---------------------------------------------------------------------
-- 2. Regras conferidas pelo GeneroService
-- ---------------------------------------------------------------------

-- GeneroRepository.ExisteComNomeAsync: já há gênero com o nome?
-- Na alteração, @id_ignorar recebe o id do próprio gênero (manter o
-- próprio nome não é duplicidade). A collation _ci não diferencia
-- maiúsculas: 'rpg' e 'RPG' são o mesmo nome. Maior que 0: 409, código 107.
SELECT COUNT(*) AS qt_mesmo_nome
FROM tb_genero
WHERE no_genero = @no_genero
  AND (@id_ignorar IS NULL OR co_seq_genero <> @id_ignorar);

-- JogoGeneroRepository.ExisteComGeneroAsync: algum jogo tem o gênero?
-- Maior que 0: não exclui, 409, código 108.
SELECT COUNT(*) AS qt_jogos_do_genero
FROM tb_jogo_genero
WHERE co_genero = @co_genero;

-- ---------------------------------------------------------------------
-- 3. Gravações (desfeitas no fim)
-- ---------------------------------------------------------------------
START TRANSACTION;

-- GeneroRepository.InserirAsync
INSERT INTO tb_genero (no_genero) VALUES (@no_genero);
SET @co_genero_novo = LAST_INSERT_ID();

-- GeneroRepository.AlterarAsync
UPDATE tb_genero
SET no_genero = CONCAT(@no_genero, ' (alterado)')
WHERE co_seq_genero = @co_genero_novo;

SELECT co_seq_genero, no_genero FROM tb_genero WHERE co_seq_genero = @co_genero_novo;

-- GeneroRepository.ExcluirAsync (só depois que o Service confere que não há jogos)
DELETE FROM tb_genero WHERE co_seq_genero = @co_genero_novo;

ROLLBACK;
