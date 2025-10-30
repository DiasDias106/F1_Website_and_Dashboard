CREATE TABLE Pista (
    Id_pista INT IDENTITY(1,1) PRIMARY KEY,
    nome NVARCHAR(100) NOT NULL,
    localização NVARCHAR(100) NOT NULL,
    pais NVARCHAR(100) NOT NULL,
    comprimento_km DECIMAL(5,2) NOT NULL, -- Ex: 5.89 km
    unidade_comprimento NVARCHAR(10) DEFAULT 'km',
    imagem NVARCHAR(255) NOT NULL,
    descricao NVARCHAR(255) NOT NULL
);

CREATE TABLE Calendario (
    Id_evento INT IDENTITY(1,1) PRIMARY KEY,
    pista INT NOT NULL,
    nome_evento NVARCHAR(100) NOT NULL,
    data_inicio DATETIME NOT NULL,
    data_fim DATETIME NOT NULL,
    duracao AS DATEDIFF(DAY, data_inicio, data_fim),
    descricao NVARCHAR(255) NOT NULL,
    CONSTRAINT FK_Calendario_Pista FOREIGN KEY (pista) REFERENCES Pista(Id_pista)
);

CREATE TABLE Equipa (
    id_equipa INT IDENTITY(1,1) PRIMARY KEY,
    nome NVARCHAR(100) NOT NULL,
    descricao NVARCHAR(255) NULL,
    dono NVARCHAR(100) NULL,          -- pode ser o nome do dono ou um FK para utilizador depois
    imagem NVARCHAR(255) NULL,        -- caminho ou URL da imagem da equipa
    titulos INT DEFAULT 0              -- número de títulos da equipa
);

CREATE TABLE Piloto (
    Id_piloto INT IDENTITY(1,1) PRIMARY KEY,
    nome NVARCHAR(100) NOT NULL,
    data_nascimento DATE,
    nacionalidade NVARCHAR(100),
    titulos INT DEFAULT 0,
    equipa_id INT,
    CONSTRAINT FK_Piloto_Equipa FOREIGN KEY (equipa_id) REFERENCES Equipa(Id_equipa)
);

CREATE TABLE Componente (
    id_componente INT IDENTITY(1,1) PRIMARY KEY,
    nome NVARCHAR(100) NOT NULL,
    descricao NVARCHAR(255) NULL,
    equipa_id INT NOT NULL,
    preco DECIMAL(10,2) NOT NULL,
    quantidade INT DEFAULT 0,
    foto NVARCHAR(255) NULL, -- caminho ou URL da imagem
    CONSTRAINT FK_Componente_Equipa FOREIGN KEY (equipa_id) REFERENCES Equipa(id_equipa)
);

CREATE TABLE Mecanico (
    id_mecanico INT IDENTITY(1,1) PRIMARY KEY,
    nome NVARCHAR(100) NOT NULL,
    descricao NVARCHAR(255) NULL,
    cargo NVARCHAR(50) NOT NULL, -- ex: Chefe, Assistente, etc.
    equipa_id INT NOT NULL,
    salario DECIMAL(10,2) NOT NULL,
    em_funcoes BIT DEFAULT 1, -- 1 = em funções, 0 = inativo
    CONSTRAINT FK_Mecanico_Equipa FOREIGN KEY (equipa_id) REFERENCES Equipa(id_equipa)
);
CREATE TABLE Membro (
    id_membro INT IDENTITY(1,1) PRIMARY KEY,
    nome NVARCHAR(100) NOT NULL,
    equipa_id INT NOT NULL,
    mecanico_associado_id INT NULL,
    salario DECIMAL(10,2) NOT NULL,
    em_funcoes BIT DEFAULT 1, -- 1 = em funções, 0 = inativo
    funcao NVARCHAR(100) NOT NULL,
    descricao NVARCHAR(255) NULL,
    CONSTRAINT FK_Membro_Equipa FOREIGN KEY (equipa_id) REFERENCES Equipa(id_equipa),
    CONSTRAINT FK_Membro_Mecanico FOREIGN KEY (mecanico_associado_id) REFERENCES Mecanico(id_mecanico)
);

