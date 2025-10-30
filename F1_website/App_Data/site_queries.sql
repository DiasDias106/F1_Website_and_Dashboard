create table paginas(
    id_pag int primary key identity(1,1),
    titulo varchar(50) not null,
    caminho varchar(255) not null
    );

insert into paginas (titulo, caminho) values ('Sobre','/Home/About');
