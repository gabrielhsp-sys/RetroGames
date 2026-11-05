-- =====================================================================
-- RetroGames — Etapa 3: tabelas do banco
-- Fonte: Especificação técnica, seções 4.1 (nomes), 4.2 (dicionário) e
-- 4.3 (restrições). Tamanhos dos VARCHAR: modelo-relacional.png
-- (o dicionário 4.2 não define tamanhos de texto).
--
-- O banco guarda só a estrutura: PK, FK com comportamento padrão, tipos,
-- NOT NULL e UNIQUE. Sem trigger, function, CHECK ou ON DELETE.
-- Somente ds_jogo aceita NULL; as demais colunas são obrigatórias.
--
-- Re-executável: apaga e recria o banco local retrogames.
-- =====================================================================

DROP DATABASE IF EXISTS retrogames;
CREATE DATABASE retrogames CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE retrogames;

-- ---------------------------------------------------------------------
-- Domínio: condição de conservação do jogo (chave fixa, sem AUTO_INCREMENT)
-- ---------------------------------------------------------------------
CREATE TABLE tb_condicao_jogo (
    co_condicao_jogo INT         NOT NULL,
    ds_condicao_jogo VARCHAR(50) NOT NULL,
    CONSTRAINT pk_condicao_jogo PRIMARY KEY (co_condicao_jogo),
    CONSTRAINT uk_condicao_jogo_ds_condicao_jogo UNIQUE (ds_condicao_jogo)
);

-- ---------------------------------------------------------------------
-- Domínio: perfil de acesso (chave fixa, sem AUTO_INCREMENT)
-- ---------------------------------------------------------------------
CREATE TABLE tb_perfil (
    co_perfil INT         NOT NULL,
    ds_perfil VARCHAR(50) NOT NULL,
    CONSTRAINT pk_perfil PRIMARY KEY (co_perfil),
    CONSTRAINT uk_perfil_ds_perfil UNIQUE (ds_perfil)
);

-- ---------------------------------------------------------------------
-- Cadastro: plataforma (console)
-- ---------------------------------------------------------------------
CREATE TABLE tb_plataforma (
    co_seq_plataforma INT         NOT NULL AUTO_INCREMENT,
    no_plataforma     VARCHAR(80) NOT NULL,
    no_fabricante     VARCHAR(80) NOT NULL,
    nu_ano_lancamento INT         NOT NULL,
    CONSTRAINT pk_plataforma PRIMARY KEY (co_seq_plataforma),
    CONSTRAINT uk_plataforma_no_plataforma UNIQUE (no_plataforma)
);

-- ---------------------------------------------------------------------
-- Cadastro (somente leitura): gênero
-- ---------------------------------------------------------------------
CREATE TABLE tb_genero (
    co_seq_genero INT         NOT NULL AUTO_INCREMENT,
    no_genero     VARCHAR(50) NOT NULL,
    CONSTRAINT pk_genero PRIMARY KEY (co_seq_genero),
    CONSTRAINT uk_genero_no_genero UNIQUE (no_genero)
);

-- ---------------------------------------------------------------------
-- Principal: jogo à venda
-- dt_cadastro guarda data e hora em UTC (gravada pela aplicação).
-- ---------------------------------------------------------------------
CREATE TABLE tb_jogo (
    co_seq_jogo       INT            NOT NULL AUTO_INCREMENT,
    no_jogo           VARCHAR(150)   NOT NULL,
    ds_jogo           VARCHAR(1000)  NULL,
    nu_ano_lancamento INT            NOT NULL,
    vl_preco          DECIMAL(10,2)  NOT NULL,
    qt_estoque        INT            NOT NULL,
    st_ativo          BOOLEAN        NOT NULL DEFAULT TRUE,
    dt_cadastro       DATETIME       NOT NULL,
    co_plataforma     INT            NOT NULL,
    co_condicao_jogo  INT            NOT NULL,
    CONSTRAINT pk_jogo PRIMARY KEY (co_seq_jogo),
    CONSTRAINT fk_jogo_plataforma FOREIGN KEY (co_plataforma)
        REFERENCES tb_plataforma (co_seq_plataforma),
    CONSTRAINT fk_jogo_condicao_jogo FOREIGN KEY (co_condicao_jogo)
        REFERENCES tb_condicao_jogo (co_condicao_jogo),
    INDEX ix_jogo_no_jogo (no_jogo)
);

-- ---------------------------------------------------------------------
-- Associativa: jogo x gênero (chave primária composta)
-- ---------------------------------------------------------------------
CREATE TABLE tb_jogo_genero (
    co_jogo   INT NOT NULL,
    co_genero INT NOT NULL,
    CONSTRAINT pk_jogo_genero PRIMARY KEY (co_jogo, co_genero),
    CONSTRAINT fk_jogo_genero_jogo FOREIGN KEY (co_jogo)
        REFERENCES tb_jogo (co_seq_jogo),
    CONSTRAINT fk_jogo_genero_genero FOREIGN KEY (co_genero)
        REFERENCES tb_genero (co_seq_genero)
);

-- ---------------------------------------------------------------------
-- Cadastro: usuário (e-mail único, senha guardada só como hash)
-- ---------------------------------------------------------------------
CREATE TABLE tb_usuario (
    co_seq_usuario INT          NOT NULL AUTO_INCREMENT,
    no_usuario     VARCHAR(100) NOT NULL,
    ds_email       VARCHAR(256) NOT NULL,
    ds_senha_hash  VARCHAR(500) NOT NULL,
    st_ativo       BOOLEAN      NOT NULL DEFAULT TRUE,
    dt_cadastro    DATETIME     NOT NULL,
    co_perfil      INT          NOT NULL,
    CONSTRAINT pk_usuario PRIMARY KEY (co_seq_usuario),
    CONSTRAINT uk_usuario_ds_email UNIQUE (ds_email),
    CONSTRAINT fk_usuario_perfil FOREIGN KEY (co_perfil)
        REFERENCES tb_perfil (co_perfil)
);
