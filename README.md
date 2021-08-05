# Authorizer

Authorizer é um watcher job que receberá como entrada linhas em formato json por meio de um arquivo (operations) em um diretório de entrada e fornecer uma saída em formato json para cada uma das entradas em um diretório de saída.

## Resumo

#### Arquitetura

O projeto foi desenvolvido utilizando o modelo de arquitetura vertical slice apresentado por [Jimmy Bogard](https://www.youtube.com/watch?v=SUiWfhAhgQw) na [NDC Sydney](https://ndcsydney.com/) de 2018 e utilizando uma organização baseada features em single file , que eu gostaria de implementar para o teste assim como aplicar os conceitos do vertical slice na pratica.

#### Framework

A solução foi desenvolvida utilizando asp.net core, por ser um framework open source, multiplataforma no qual tenho maior conhecimento e experiência de desenvolvimento.

#### Testes

Para os testes foi utilizada a biblioteca xUnit, que foi desenvolvida pelos mesmos desenvolvedores do Nunit, que facilita e otimiza o desenvolvimento dos testes e dos casos de testes com .net.
Em conjunto com xUnit foi utilizado o Moq framework, para Mock de dados em teste, e a biblioteca fluentAssertion, para auxiliar na elaboração de asserts.

## Executar Aplicação

- Descompactar Authorizer.zip em um diretório de sua preferencia.
- Crie um arquivo com nome operations que contenha uma estrutura de json similar ao código abaixo:

```json
{"account": {"active-card": true, "available-limit": 10000}}
{"transaction": {"merchant": "Burger King", "amount": 20, "time": "2019-02-13T10:00:00.000Z"}}
{"transaction": {"merchant": "Habbib's", "amount": 90, "time": "2019-02-13T11:00:00.000Z"}}
{"transaction": {"merchant": "McDonald's", "amount": 30, "time": "2019-02-13T12:00:00.000Z"}}
```

#### Docker Container

- Instale o Docker conforme recomendado no site [docs.docker.com](https://docs.docker.com/get-docker/).
- Abra um terminal.
- Navegue para a o diretório raiz da aplicação onde se encontra o arquivo docker-compose.yaml.
- Execute o comando Abaixo:

```docker
docker-compose up
```

- Copie o arquivo operations criado para dentro do diretório **input** criado após a execução do container.
- O arquivo de entrada será removido do diretório de origem e um de resposta será criado no diretório de **output** criado após a execução do container.
