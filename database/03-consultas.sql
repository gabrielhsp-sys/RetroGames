-- =====================================================================
-- RetroGames — Etapa 5: consultas do catálogo
-- Executar sobre um banco criado por 01-schema.sql e 02-dados.sql.
-- Troque os valores das variáveis abaixo para consultar outros dados.
--
-- Observação: estas consultas usam JOIN para estudar o modelo no
-- MySQL. No Repository da API cada consulta lê uma única tabela e a
-- composição é feita no Service, com LINQ (AGENTS.md).
-- =====================================================================

USE retrogames;

-- A conexão passa a usar a mesma collation das colunas. Sem isto, a
-- variável @busca_like herda a collation padrão do cliente MySQL 8
-- (utf8mb4_0900_ai_ci) e o LIKE com no_jogo (utf8mb4_unicode_ci) falha
-- com o erro 1267 (Illegal mix of collations).
SET NAMES utf8mb4 COLLATE utf8mb4_unicode_ci;

-- ---------------------------------------------------------------------
-- Parâmetros
-- ---------------------------------------------------------------------
SET @co_jogo       = (SELECT co_seq_jogo   FROM tb_jogo   WHERE no_jogo   = 'Wonder Boy III: The Dragon''s Trap');
SET @co_genero     = (SELECT co_seq_genero FROM tb_genero WHERE no_genero = 'Plataforma');
SET @busca         = 'o';   -- parte do nome do jogo
SET @pagina        = 2;     -- 1 = primeira página
SET @tamanho_pagina = 5;

-- ---------------------------------------------------------------------
-- 1. Catálogo: jogos ativos com o nome da plataforma e a descrição da
--    condição, ordenados por nome.
-- ---------------------------------------------------------------------
SELECT j.co_seq_jogo,
       j.no_jogo,
       p.no_plataforma,
       c.ds_condicao_jogo,
       j.nu_ano_lancamento,
       j.vl_preco,
       j.qt_estoque
FROM tb_jogo j
JOIN tb_plataforma    p ON p.co_seq_plataforma = j.co_plataforma
JOIN tb_condicao_jogo c ON c.co_condicao_jogo  = j.co_condicao_jogo
WHERE j.st_ativo = TRUE
ORDER BY j.no_jogo;

-- ---------------------------------------------------------------------
-- 2. Gêneros de um jogo informado (@co_jogo).
-- ---------------------------------------------------------------------
SELECT g.co_seq_genero,
       g.no_genero
FROM tb_jogo_genero jg
JOIN tb_genero g ON g.co_seq_genero = jg.co_genero
WHERE jg.co_jogo = @co_jogo
ORDER BY g.no_genero;

-- ---------------------------------------------------------------------
-- 3. Jogos de um gênero informado (@co_genero).
-- ---------------------------------------------------------------------
SELECT j.co_seq_jogo,
       j.no_jogo,
       j.st_ativo
FROM tb_jogo_genero jg
JOIN tb_jogo j ON j.co_seq_jogo = jg.co_jogo
WHERE jg.co_genero = @co_genero
ORDER BY j.no_jogo;

-- ---------------------------------------------------------------------
-- 4. Quantidade de jogos por plataforma, incluindo as plataformas sem
--    jogos (LEFT JOIN + COUNT da coluna do jogo, que conta 0 quando
--    não há jogo associado).
-- ---------------------------------------------------------------------
SELECT p.co_seq_plataforma,
       p.no_plataforma,
       COUNT(j.co_seq_jogo) AS qt_jogos
FROM tb_plataforma p
LEFT JOIN tb_jogo j ON j.co_plataforma = p.co_seq_plataforma
GROUP BY p.co_seq_plataforma, p.no_plataforma
ORDER BY p.no_plataforma;

-- ---------------------------------------------------------------------
-- 5. Busca por parte do nome (@busca) entre os jogos ativos, com
--    @tamanho_pagina itens por página (@pagina).
--    LIMIT/OFFSET não aceitam variável @ direto no SQL; por isso a
--    consulta é preparada e recebe os valores como parâmetros (?).
-- ---------------------------------------------------------------------
SET @busca_like = CONCAT('%', @busca, '%');
SET @deslocamento = (@pagina - 1) * @tamanho_pagina;

-- total de resultados, para saber quantas páginas existem
SELECT COUNT(*) AS qt_resultados
FROM tb_jogo
WHERE st_ativo = TRUE
  AND no_jogo LIKE @busca_like;

PREPARE consulta_busca FROM
    'SELECT co_seq_jogo, no_jogo, vl_preco
     FROM tb_jogo
     WHERE st_ativo = TRUE
       AND no_jogo LIKE ?
     ORDER BY no_jogo, co_seq_jogo
     LIMIT ? OFFSET ?';
EXECUTE consulta_busca USING @busca_like, @tamanho_pagina, @deslocamento;
DEALLOCATE PREPARE consulta_busca;
