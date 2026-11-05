-- =====================================================================
-- RetroGames — Etapa 4: dados de exemplo
-- Executar logo depois de 01-schema.sql, num banco recém-criado.
--
-- Conteúdo: 3 condições e 2 perfis (chaves fixas), 7 plataformas
-- (a última, Neo Geo AES, fica sem jogos de propósito, para a contagem
-- por plataforma mostrar 0), 8 gêneros e 20 jogos, cada um com 1 a 3
-- gêneros. Um jogo inativo (Mortal Kombat) e um com estoque zero
-- (Chrono Trigger). Nenhum usuário: o administrador é criado pela API.
--
-- As chaves estrangeiras são buscadas pelo nome, para não depender da
-- ordem dos valores gerados por AUTO_INCREMENT.
-- =====================================================================

USE retrogames;

-- ---------------------------------------------------------------------
-- Domínios (chaves fixas)
-- ---------------------------------------------------------------------
INSERT INTO tb_condicao_jogo (co_condicao_jogo, ds_condicao_jogo) VALUES
    (1, 'Novo'),
    (2, 'Usado'),
    (3, 'Recondicionado');

INSERT INTO tb_perfil (co_perfil, ds_perfil) VALUES
    (1, 'Administrador'),
    (2, 'Cliente');

-- ---------------------------------------------------------------------
-- Plataformas
-- ---------------------------------------------------------------------
INSERT INTO tb_plataforma (no_plataforma, no_fabricante, nu_ano_lancamento) VALUES
    ('Atari 2600',                    'Atari',    1977),
    ('Nintendo Entertainment System', 'Nintendo', 1983),
    ('Master System',                 'Sega',     1986),
    ('Mega Drive',                    'Sega',     1988),
    ('Super Nintendo',                'Nintendo', 1990),
    ('PlayStation',                   'Sony',     1994),
    ('Neo Geo AES',                   'SNK',      1990);   -- sem jogos

-- ---------------------------------------------------------------------
-- Gêneros
-- ---------------------------------------------------------------------
INSERT INTO tb_genero (no_genero) VALUES
    ('Plataforma'),
    ('Ação'),
    ('Aventura'),
    ('RPG'),
    ('Corrida'),
    ('Luta'),
    ('Esporte'),
    ('Quebra-cabeça');

-- ---------------------------------------------------------------------
-- Jogos
-- Colunas: nome, descrição, ano, preço, estoque, ativo, cadastro (UTC),
--          plataforma, condição (1 Novo, 2 Usado, 3 Recondicionado)
-- ---------------------------------------------------------------------
INSERT INTO tb_jogo (no_jogo, ds_jogo, nu_ano_lancamento, vl_preco, qt_estoque, st_ativo, dt_cadastro, co_plataforma, co_condicao_jogo) VALUES
    -- Atari 2600
    ('Pitfall!', 'Pitfall Harry atravessa a selva desviando de crocodilos, buracos e escorpiões.', 1982, 149.90, 4, TRUE, UTC_TIMESTAMP(),
        (SELECT co_seq_plataforma FROM tb_plataforma WHERE no_plataforma = 'Atari 2600'), 2),
    ('River Raid', 'Avião de combate sobe um rio destruindo inimigos e reabastecendo combustível.', 1982, 129.90, 3, TRUE, UTC_TIMESTAMP(),
        (SELECT co_seq_plataforma FROM tb_plataforma WHERE no_plataforma = 'Atari 2600'), 2),
    ('Enduro', NULL, 1983, 119.90, 5, TRUE, UTC_TIMESTAMP(),
        (SELECT co_seq_plataforma FROM tb_plataforma WHERE no_plataforma = 'Atari 2600'), 3),

    -- Nintendo Entertainment System
    ('Super Mario Bros.', 'Mario atravessa o Reino dos Cogumelos para resgatar a princesa.', 1985, 299.90, 6, TRUE, UTC_TIMESTAMP(),
        (SELECT co_seq_plataforma FROM tb_plataforma WHERE no_plataforma = 'Nintendo Entertainment System'), 2),
    ('The Legend of Zelda', 'Link explora Hyrule em busca dos fragmentos da Triforce.', 1986, 349.90, 2, TRUE, UTC_TIMESTAMP(),
        (SELECT co_seq_plataforma FROM tb_plataforma WHERE no_plataforma = 'Nintendo Entertainment System'), 3),
    ('Mega Man 2', 'Mega Man enfrenta os oito robôs do Dr. Wily.', 1988, 279.90, 3, TRUE, UTC_TIMESTAMP(),
        (SELECT co_seq_plataforma FROM tb_plataforma WHERE no_plataforma = 'Nintendo Entertainment System'), 2),
    ('Tetris', 'Encaixe as peças que caem para completar linhas.', 1989, 159.90, 8, TRUE, UTC_TIMESTAMP(),
        (SELECT co_seq_plataforma FROM tb_plataforma WHERE no_plataforma = 'Nintendo Entertainment System'), 2),

    -- Master System
    ('Alex Kidd in Miracle World', 'Alex Kidd viaja pelo planeta Aries e decide disputas no jokenpô.', 1986, 189.90, 5, TRUE, UTC_TIMESTAMP(),
        (SELECT co_seq_plataforma FROM tb_plataforma WHERE no_plataforma = 'Master System'), 2),
    ('Phantasy Star', 'Alis parte em uma jornada pelo sistema Algol para vingar o irmão.', 1987, 449.90, 1, TRUE, UTC_TIMESTAMP(),
        (SELECT co_seq_plataforma FROM tb_plataforma WHERE no_plataforma = 'Master System'), 3),
    ('Wonder Boy III: The Dragon''s Trap', 'Amaldiçoado, o herói muda de forma enquanto procura a cura.', 1989, 229.90, 2, TRUE, UTC_TIMESTAMP(),
        (SELECT co_seq_plataforma FROM tb_plataforma WHERE no_plataforma = 'Master System'), 2),

    -- Mega Drive
    ('Sonic the Hedgehog', 'Sonic corre pela Green Hill Zone para deter o Dr. Robotnik.', 1991, 199.90, 7, TRUE, UTC_TIMESTAMP(),
        (SELECT co_seq_plataforma FROM tb_plataforma WHERE no_plataforma = 'Mega Drive'), 2),
    ('Streets of Rage 2', 'Briga de rua em rolagem lateral contra o sindicato do Mr. X.', 1992, 259.90, 4, TRUE, UTC_TIMESTAMP(),
        (SELECT co_seq_plataforma FROM tb_plataforma WHERE no_plataforma = 'Mega Drive'), 2),
    ('Mortal Kombat', 'Torneio de luta entre guerreiros de dois reinos.', 1993, 179.90, 3, FALSE, UTC_TIMESTAMP(),
        (SELECT co_seq_plataforma FROM tb_plataforma WHERE no_plataforma = 'Mega Drive'), 2),

    -- Super Nintendo
    ('Super Mario World', 'Mario e Yoshi atravessam Dinosaur Land.', 1990, 329.90, 5, TRUE, UTC_TIMESTAMP(),
        (SELECT co_seq_plataforma FROM tb_plataforma WHERE no_plataforma = 'Super Nintendo'), 2),
    ('The Legend of Zelda: A Link to the Past', 'Link alterna entre o Mundo da Luz e o Mundo das Trevas.', 1991, 389.90, 3, TRUE, UTC_TIMESTAMP(),
        (SELECT co_seq_plataforma FROM tb_plataforma WHERE no_plataforma = 'Super Nintendo'), 3),
    ('Super Mario Kart', 'Corrida de karts com itens e personagens da Nintendo.', 1992, 269.90, 4, TRUE, UTC_TIMESTAMP(),
        (SELECT co_seq_plataforma FROM tb_plataforma WHERE no_plataforma = 'Super Nintendo'), 2),
    ('Chrono Trigger', 'Viagem no tempo para impedir a destruição do planeta.', 1995, 899.90, 0, TRUE, UTC_TIMESTAMP(),
        (SELECT co_seq_plataforma FROM tb_plataforma WHERE no_plataforma = 'Super Nintendo'), 1),

    -- PlayStation
    ('Final Fantasy VII', 'Cloud e o grupo AVALANCHE enfrentam a corporação Shinra.', 1997, 249.90, 6, TRUE, UTC_TIMESTAMP(),
        (SELECT co_seq_plataforma FROM tb_plataforma WHERE no_plataforma = 'PlayStation'), 2),
    ('Gran Turismo', 'Simulador de corrida com centenas de carros licenciados.', 1997, 99.90, 9, TRUE, UTC_TIMESTAMP(),
        (SELECT co_seq_plataforma FROM tb_plataforma WHERE no_plataforma = 'PlayStation'), 2),
    ('ISS Pro Evolution', 'Futebol com seleções e controle tático das partidas.', 1999, 89.90, 5, TRUE, UTC_TIMESTAMP(),
        (SELECT co_seq_plataforma FROM tb_plataforma WHERE no_plataforma = 'PlayStation'), 3);

-- ---------------------------------------------------------------------
-- Gêneros de cada jogo (1 a 3 por jogo)
-- ---------------------------------------------------------------------
INSERT INTO tb_jogo_genero (co_jogo, co_genero)
SELECT j.co_seq_jogo, g.co_seq_genero
FROM tb_jogo j
JOIN (
              SELECT 'Pitfall!' AS no_jogo, 'Plataforma' AS no_genero
    UNION ALL SELECT 'Pitfall!', 'Aventura'
    UNION ALL SELECT 'River Raid', 'Ação'
    UNION ALL SELECT 'Enduro', 'Corrida'
    UNION ALL SELECT 'Super Mario Bros.', 'Plataforma'
    UNION ALL SELECT 'The Legend of Zelda', 'Ação'
    UNION ALL SELECT 'The Legend of Zelda', 'Aventura'
    UNION ALL SELECT 'Mega Man 2', 'Plataforma'
    UNION ALL SELECT 'Mega Man 2', 'Ação'
    UNION ALL SELECT 'Tetris', 'Quebra-cabeça'
    UNION ALL SELECT 'Alex Kidd in Miracle World', 'Plataforma'
    UNION ALL SELECT 'Phantasy Star', 'RPG'
    UNION ALL SELECT 'Wonder Boy III: The Dragon''s Trap', 'Plataforma'
    UNION ALL SELECT 'Wonder Boy III: The Dragon''s Trap', 'Ação'
    UNION ALL SELECT 'Wonder Boy III: The Dragon''s Trap', 'Aventura'
    UNION ALL SELECT 'Sonic the Hedgehog', 'Plataforma'
    UNION ALL SELECT 'Streets of Rage 2', 'Ação'
    UNION ALL SELECT 'Streets of Rage 2', 'Luta'
    UNION ALL SELECT 'Mortal Kombat', 'Luta'
    UNION ALL SELECT 'Super Mario World', 'Plataforma'
    UNION ALL SELECT 'The Legend of Zelda: A Link to the Past', 'Ação'
    UNION ALL SELECT 'The Legend of Zelda: A Link to the Past', 'Aventura'
    UNION ALL SELECT 'Super Mario Kart', 'Corrida'
    UNION ALL SELECT 'Chrono Trigger', 'RPG'
    UNION ALL SELECT 'Chrono Trigger', 'Aventura'
    UNION ALL SELECT 'Final Fantasy VII', 'RPG'
    UNION ALL SELECT 'Gran Turismo', 'Corrida'
    UNION ALL SELECT 'Gran Turismo', 'Esporte'
    UNION ALL SELECT 'ISS Pro Evolution', 'Esporte'
) AS a ON a.no_jogo = j.no_jogo
JOIN tb_genero g ON g.no_genero = a.no_genero;
