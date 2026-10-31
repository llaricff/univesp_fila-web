# Fila Web – Back-end

API do Fila Web em **.NET 9 (C#)**, com ASP.NET Core Minimal API. Recebe os cadastros da tela
(`frontend/`), guarda a fila, informa a posição de cada pessoa e permite à equipe da UPA chamar o próximo.

Esta é uma versão inicial, pensada para o grupo continuar. O que já funciona está listado abaixo,
e o que falta para produção está em **Próximos passos**.

## Como rodar

Requisito: [SDK do .NET 9](https://dotnet.microsoft.com/download/dotnet/9.0) (confira com `dotnet --version`, deve começar com `9.`).

```bash
cd backend/FilaWeb.Api
dotnet run
```

A API sobe em **http://localhost:5000**. Abra esse endereço no navegador: deve aparecer
`{"servico":"Fila Web API","status":"ok"}`.

No Visual Studio 2022: abra `backend/FilaWeb.sln` e aperte F5 (perfil `http`).

## Ligando o front-end

No `frontend/`, crie o arquivo `.env` (copie de `.env.example`) com:

```
VITE_API_URL=http://localhost:5000
```

Reinicie o `npm run dev`. O aviso "Modo de demonstração" some da tela e os cadastros passam a ser
gravados de verdade. O CORS já libera `http://localhost:5173` (configurável em `appsettings.json`, chave `Cors:Origens`).

## Endpoints

### Pacientes (públicos)

| Método | Rota | O que faz |
|---|---|---|
| `POST` | `/api/fila` | Entra na fila. Devolve `201` com `{ "id", "posicao" }` |
| `GET` | `/api/fila/{id}` | Situação e posição atual (para a tela "Acompanhar minha posição") |
| `POST` | `/api/fila/{id}/cancelar` | A pessoa desiste da vaga |

**Corpo do `POST /api/fila`** (é o que a tela já envia):

```json
{
  "nome": "Maria da Silva",
  "dataNascimento": "1990-05-21",
  "cpf": "52998224725",
  "telefone": "16912345678",
  "motivo": "sintomas"
}
```

`motivo` aceita `sintomas`, `lesao` ou `outro`. CPF e telefone podem vir com ou sem máscara: a API guarda só os dígitos.

**Erros** têm sempre este formato, e a tela mostra cada mensagem embaixo do campo certo:

```json
{ "mensagem": "Corrija os campos indicados.", "erros": { "cpf": "CPF inválido." } }
```

| Status | Quando |
|---|---|
| `400` | Campo inválido (`erros` traz os nomes: `nome`, `nascimento`, `cpf`, `telefone`, `motivo`) |
| `409` | CPF que já está aguardando na fila, ou cancelamento de quem já não está aguardando |
| `404` | Cadastro não encontrado |

> Observação: o README do front fala em `400` para CPF repetido. A API usa `409` (o mais correto
> para conflito). A tela trata qualquer status de erro do mesmo jeito, então nada quebra.

**Resposta do `GET /api/fila/{id}`** (não devolve dados pessoais, de propósito):

```json
{ "id": "…", "status": "aguardando", "posicao": 2, "totalAguardando": 5, "entradaEm": "2026-10-31T13:00:00+00:00" }
```

`status` é `aguardando`, `chamado` ou `cancelado`. `posicao` é `null` quando a pessoa não está mais aguardando.

### Equipe da UPA (exigem o cabeçalho `X-Api-Key`)

| Método | Rota | O que faz |
|---|---|---|
| `GET` | `/api/atendimento/fila` | Lista quem aguarda, em ordem (CPF mascarado: `***.982.247-**`) |
| `POST` | `/api/atendimento/proximo` | Chama a próxima pessoa e devolve nome e telefone dela |

A chave fica em `appsettings.json`, em `Fila:ChaveAtendimento`. **Vazia = área da equipe desligada** (responde 404).
Em desenvolvimento, `appsettings.Development.json` define `chave-local-de-teste`. Em produção, defina uma chave
longa e secreta por variável de ambiente (`Fila__ChaveAtendimento`), nunca no repositório.

Exemplos prontos de todas as chamadas estão em `FilaWeb.Api/FilaWeb.http`.

## Como o código está organizado

```
FilaWeb.Api/
  Program.cs                      configuração (CORS, registro dos serviços)
  Endpoints/FilaEndpoints.cs      as rotas acima
  Models/Atendimento.cs           a pessoa na fila (modelo de domínio)
  Models/Dtos.cs                  formatos de entrada e saída da API
  Services/CadastroValidator.cs   regras de validação (nome, data, CPF, telefone, motivo)
  Services/IFilaRepository.cs     "onde guardar a fila" (interface)
  Services/JsonFilaRepository.cs  implementação atual: memória + arquivo data/fila.json
```

**Regra da fila:** ordem de chegada (primeiro a entrar, primeiro a ser chamado). A mesma pessoa (CPF) só pode
estar uma vez aguardando; depois de chamada ou de cancelar, pode entrar de novo.

## Próximos passos (para o grupo decidir)

1. **Banco de dados de verdade.** Hoje a fila fica num arquivo (`data/fila.json`), que serve para
   desenvolvimento e para a demonstração, mas não para vários servidores ou volume real. Crie uma classe que
   implemente `IFilaRepository` (por exemplo com Entity Framework Core + PostgreSQL ou SQL Server) e troque o
   registro em `Program.cs`. Os endpoints não precisam mudar.
2. **Prioridade de emergência.** A tela promete que emergências mantêm prioridade, mas a API hoje só segue a
   ordem de chegada. É preciso definir com a equipe da UPA como a triagem classifica e reordena (por exemplo, um
   campo de prioridade e `ChamarProximoAsync` escolhendo por prioridade e depois por chegada).
3. **Autenticação da equipe.** A chave única (`X-Api-Key`) é o mínimo para demonstrar. Para uso real, troque por
   login de usuários (por exemplo ASP.NET Core Identity).
4. **Avisos por telefone** (SMS ou WhatsApp) quando a pessoa estiver perto da vez: a tela de confirmação já cita
   isso, mas não está implementado.
5. **LGPD.** A API guarda CPF e motivo da visita, que são dados pessoais e de saúde. Antes de ir para produção:
   base legal e consentimento, criptografia dos dados, prazo de retenção e controle de acesso. A pasta `data/`
   fica fora do Git (`.gitignore`) justamente por isso.
6. **HTTPS e limite de requisições** para uso público.

## Testes

Esta versão foi testada chamando a API de verdade (validações, ordem da fila, duplicidade de CPF, cancelamento,
chamada do próximo, CORS, persistência após reiniciar) e com a tela do `frontend/` ligada a ela. Ainda não há
projeto de testes automatizados no repositório: é um bom próximo passo (xUnit + `WebApplicationFactory`).
