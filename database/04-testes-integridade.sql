-- =====================================================================
-- RetroGames — Etapa 5: testes de integridade
-- Executar sobre um banco criado por 01-schema.sql e 02-dados.sql.
--
-- Os testes 1 a 4 DEVEM falhar. Para o script seguir depois de cada
-- erro, execute-o com a opção --force do cliente mysql (no Workbench,
-- desmarque "Stop Script Execution on Errors" ou rode teste a teste).
-- Todo dado gravado por estes testes tem nome começando com 'TESTE ' e
-- é removido na seção "Limpeza", no fim do script.
-- =====================================================================

USE retrogames;

-- ---------------------------------------------------------------------
-- Teste 1 — inserir jogo com co_plataforma inexistente
-- Esperado: erro 1452 (chave estrangeira)
-- Obtido:   ERROR 1452 (23000): Cannot add or update a child row: a foreign
--           key constraint fails (CONSTRAINT `fk_jogo_plataforma`). Igual ao esperado.
-- ---------------------------------------------------------------------
INSERT INTO tb_jogo (no_jogo, ds_jogo, nu_ano_lancamento, vl_preco, qt_estoque, st_ativo, dt_cadastro, co_plataforma, co_condicao_jogo)
VALUES ('TESTE plataforma inexistente', NULL, 1990, 50.00, 1, TRUE, UTC_TIMESTAMP(), 999999, 1);

-- ---------------------------------------------------------------------
-- Teste 2 — inserir plataforma com nome já cadastrado
-- Esperado: erro 1062 (UNIQUE)
-- Obtido:   ERROR 1062 (23000): Duplicate entry 'Mega Drive' for key
--           'tb_plataforma.uk_plataforma_no_plataforma'. Igual ao esperado.
-- ---------------------------------------------------------------------
INSERT INTO tb_plataforma (no_plataforma, no_fabricante, nu_ano_lancamento)
VALUES ('Mega Drive', 'TESTE fabricante', 1988);

-- ---------------------------------------------------------------------
-- Teste 3 — excluir plataforma que possui jogos
-- Esperado: erro 1451 (chave estrangeira)
-- Obtido:   ERROR 1451 (23000): Cannot delete or update a parent row: a foreign
--           key constraint fails (CONSTRAINT `fk_jogo_plataforma`). Igual ao esperado.
-- ---------------------------------------------------------------------
DELETE FROM tb_plataforma
WHERE no_plataforma = 'Mega Drive';

-- ---------------------------------------------------------------------
-- Teste 4 — inserir jogo com no_jogo nulo
-- Esperado: erro 1048 (NOT NULL)
-- Obtido:   ERROR 1048 (23000): Column 'no_jogo' cannot be null. Igual ao esperado.
-- ---------------------------------------------------------------------
INSERT INTO tb_jogo (no_jogo, ds_jogo, nu_ano_lancamento, vl_preco, qt_estoque, st_ativo, dt_cadastro, co_plataforma, co_condicao_jogo)
VALUES (NULL, 'TESTE nome nulo', 1990, 50.00, 1, TRUE, UTC_TIMESTAMP(),
        (SELECT co_seq_plataforma FROM tb_plataforma WHERE no_plataforma = 'Mega Drive'), 1);

-- ---------------------------------------------------------------------
-- Teste 5 — inserir jogo com vl_preco igual a 0
-- Esperado: aceito. O banco não valida faixa de valores; a regra
--           "preço maior que zero" fica no Service (RN06).
-- Obtido:   aceito (1 linha gravada, vl_preco = 0.00). Igual ao esperado.
--           Observação: o INSERT que falhou no teste 1 consumiu um valor do
--           AUTO_INCREMENT, então o jogo de teste recebeu co_seq_jogo 22, não 21.
-- ---------------------------------------------------------------------
INSERT INTO tb_jogo (no_jogo, ds_jogo, nu_ano_lancamento, vl_preco, qt_estoque, st_ativo, dt_cadastro, co_plataforma, co_condicao_jogo)
VALUES ('TESTE preço zero', NULL, 1990, 0.00, 1, TRUE, UTC_TIMESTAMP(),
        (SELECT co_seq_plataforma FROM tb_plataforma WHERE no_plataforma = 'Mega Drive'), 1);

SELECT co_seq_jogo, no_jogo, vl_preco
FROM tb_jogo
WHERE no_jogo LIKE 'TESTE %';

-- ---------------------------------------------------------------------
-- Limpeza — remove tudo que os testes gravaram
-- (associações primeiro, por causa da chave estrangeira)
-- ---------------------------------------------------------------------
DELETE FROM tb_jogo_genero
WHERE co_jogo IN (SELECT co_seq_jogo FROM tb_jogo WHERE no_jogo LIKE 'TESTE %');

DELETE FROM tb_jogo
WHERE no_jogo LIKE 'TESTE %' OR ds_jogo LIKE 'TESTE %';

DELETE FROM tb_plataforma
WHERE no_plataforma LIKE 'TESTE %' OR no_fabricante LIKE 'TESTE %';

-- Conferência: os totais devem ser iguais aos da etapa 4
-- (3 condições, 2 perfis, 7 plataformas, 8 gêneros, 20 jogos,
--  29 associações jogo-gênero, 0 usuários).
SELECT (SELECT COUNT(*) FROM tb_condicao_jogo) AS condicoes,
       (SELECT COUNT(*) FROM tb_perfil)        AS perfis,
       (SELECT COUNT(*) FROM tb_plataforma)    AS plataformas,
       (SELECT COUNT(*) FROM tb_genero)        AS generos,
       (SELECT COUNT(*) FROM tb_jogo)          AS jogos,
       (SELECT COUNT(*) FROM tb_jogo_genero)   AS jogo_genero,
       (SELECT COUNT(*) FROM tb_usuario)       AS usuarios;
