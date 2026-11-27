-- =====================================================================
-- RetroGames — Módulo de Compras: tabelas de pedido (C04 e C05)
-- Fonte: Desafio do Módulo de Compras ("Banco de dados") e
-- Especificação técnica, seções 4.1 (nomes) e 4.3 (restrições).
--
-- Executar UMA vez, sobre um banco criado por 01-schema.sql e
-- 02-dados.sql. Este script não apaga nada: se o 01-schema.sql for
-- executado de novo (ele recria o banco), execute este script depois.
--
-- O banco guarda só a estrutura: PK, FK com comportamento padrão, tipos,
-- NOT NULL e UNIQUE. Sem trigger, function, CHECK ou ON DELETE.
-- As regras (quantidade de 1 a 10, estoque, preço do banco, situação
-- inicial Pendente) ficam no PedidoService.
--
-- O e-mail do cliente (C01) já é único: uk_usuario_ds_email, no
-- 01-schema.sql. Não há ALTER em tb_usuario aqui.
-- =====================================================================

USE retrogames;

-- ---------------------------------------------------------------------
-- Domínio: situação do pedido (chave fixa, sem AUTO_INCREMENT)
-- ---------------------------------------------------------------------
CREATE TABLE tb_situacao_pedido (
    co_situacao_pedido INT         NOT NULL,
    ds_situacao_pedido VARCHAR(50) NOT NULL,
    CONSTRAINT pk_situacao_pedido PRIMARY KEY (co_situacao_pedido),
    CONSTRAINT uk_situacao_pedido_ds_situacao_pedido UNIQUE (ds_situacao_pedido)
);

-- Chaves fixas, iguais às de SituacaoPedidoIds (RetroGames.Model/Constantes).
INSERT INTO tb_situacao_pedido (co_situacao_pedido, ds_situacao_pedido) VALUES
    (1, 'Pendente'),
    (2, 'Pago'),
    (3, 'Enviado'),
    (4, 'Cancelado');

-- ---------------------------------------------------------------------
-- Pedido: um por compra finalizada
-- dt_pedido guarda data e hora em UTC (gravada pela aplicação).
-- vl_total é calculado no PedidoService com os preços do banco.
-- ---------------------------------------------------------------------
CREATE TABLE tb_pedido (
    co_seq_pedido      INT           NOT NULL AUTO_INCREMENT,
    dt_pedido          DATETIME      NOT NULL,
    vl_total           DECIMAL(10,2) NOT NULL,
    co_usuario         INT           NOT NULL,
    co_situacao_pedido INT           NOT NULL,
    CONSTRAINT pk_pedido PRIMARY KEY (co_seq_pedido),
    CONSTRAINT fk_pedido_usuario FOREIGN KEY (co_usuario)
        REFERENCES tb_usuario (co_seq_usuario),
    CONSTRAINT fk_pedido_situacao_pedido FOREIGN KEY (co_situacao_pedido)
        REFERENCES tb_situacao_pedido (co_situacao_pedido)
);

-- ---------------------------------------------------------------------
-- Item do pedido: vl_unitario é o preço do jogo no momento da compra
-- (mesmo tipo de tb_jogo.vl_preco). Um jogo aparece uma só vez em cada
-- pedido (UNIQUE).
-- ---------------------------------------------------------------------
CREATE TABLE tb_item_pedido (
    co_seq_item_pedido INT           NOT NULL AUTO_INCREMENT,
    qt_item            INT           NOT NULL,
    vl_unitario        DECIMAL(10,2) NOT NULL,
    co_pedido          INT           NOT NULL,
    co_jogo            INT           NOT NULL,
    CONSTRAINT pk_item_pedido PRIMARY KEY (co_seq_item_pedido),
    CONSTRAINT uk_item_pedido_co_pedido_co_jogo UNIQUE (co_pedido, co_jogo),
    CONSTRAINT fk_item_pedido_pedido FOREIGN KEY (co_pedido)
        REFERENCES tb_pedido (co_seq_pedido),
    CONSTRAINT fk_item_pedido_jogo FOREIGN KEY (co_jogo)
        REFERENCES tb_jogo (co_seq_jogo)
);
