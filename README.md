# FCG.TechChallenge.Pagamentos

> Microsserviço de **Pagamentos** da plataforma **FIAP Cloud Games (FCG)** — evolução do MVP do repositório **Grupo49-TechChallenge**, agora em arquitetura de **microsserviços**, com **mensageria**, **CQRS/Outbox** e componentes serverless para tarefas assíncronas. Consulte também os serviços **Usuários** e **Jogos** para a experiência fim a fim.

- **Usuários** (este repositório): cadastro, autenticação, perfis, emissão de tokens  
  https://github.com/ajmarzola/FCG.TechChallenge.Usuarios
- **Jogos**: catálogo, compra, busca e integrações (Elasticsearch)  
  https://github.com/ajmarzola/FCG.TechChallenge.Jogos
- **Pagamentos**: orquestração de transações e status por eventos  
  https://github.com/ajmarzola/FCG.TechChallenge.Pagamentos

🔎 **Projeto anterior (base conceitual):**  
https://github.com/ajmarzola/Grupo49-TechChallenge

🧭 **Miro – Visão de Arquitetura:**  
<https://miro.com/welcomeonboard/VXBnOHN6d0hWOWFHZmxhbzlMenp2cEV3N0FPQm9lUEZwUFVnWC9qWnUxc2ZGVW9FZnZ4SjNHRW5YYVBRTUJEWkFaTjZPNmZMcXFyWUNONEg3eVl4dEdOZWozd0J3RzZld08xM3E1cGl2dTR6QUlJSUVFSkpQcFVSRko1Z0hFSXphWWluRVAxeXRuUUgwWDl3Mk1qRGVRPT0hdjE=?share_link_id=964446466388>

---

## Sumário

- [Visão Geral](#visão-geral)
- [Arquitetura](#arquitetura)
- [Tecnologias](#tecnologias)
- [Como Rodar (Rápido)](#como-rodar-rápido)
- [Configuração por Ambiente](#configuração-por-ambiente)
- [Executando com .NET CLI](#executando-com-net-cli)
- [Executando com Docker](#executando-com-docker)
- [Fluxo de Teste End-to-End](#fluxo-de-teste-end-to-end)
- [Coleções/API Docs](#coleçõesapi-docs)
- [Estrutura do Repositório](#estrutura-do-repositório)
- [CI/CD](#cicd)
- [Roadmap](#roadmap)
- [Licença](#licença)

---

## Visão Geral

Este microsserviço recebe **intents de pagamento** (ex.: após um pedido de compra no serviço de *Jogos*), processa de forma **assíncrona** via **mensageria** (Azure Service Bus) e publica **eventos de domínio** de volta para os demais serviços (ex.: `PagamentoAprovado`, `PagamentoRecusado`).  
O projeto é parte da **Fase 3 do Tech Challenge**, que pede a separação em três microsserviços (Usuários, Jogos, Pagamentos), uso de **serverless** para jobs assíncronos e **observabilidade** aprimorada.

Como evolução natural, aproveitamos os conceitos, endpoints e pipelines que você já montou no MVP anterior (**Grupo49-TechChallenge**) — ex.: autenticação JWT, operações de catálogo e biblioteca de jogos — e os distribuímos por serviços dedicados.

---

## Arquitetura

- **API Pagamentos** (ASP.NET Core) expõe endpoints para criação/consulta de pagamentos.
- **Mensageria** (Azure Service Bus): filas/tópicos para processar intents e publicar resultados.
- **CQRS + Outbox**: comandos gravam estado (WriteModel); *outbox* garante entrega de eventos.  
- **Read Model**: projeções para consultas rápidas de status (ex.: por `orderId`).
- **Jobs Serverless** (Azure Functions): processadores assíncronos (p. ex., *workers* do outbox / orquestração de steps).
- **Integração**:
  - **Jogos**: emite intent de pagamento; lê o resultado para concluir compra/licenciamento.  
  - **Usuários**: provê contexto de autenticação/claims (JWT) e dados do comprador.

---

## Tecnologias

- **.NET 8** (API e processos)  
- **Azure Service Bus** (mensageria)  
- **PostgreSQL / SQL Server** (persistência — ajuste conforme seu `appsettings`)  
- **Azure Functions** (jobs assíncronos/processamento)  
- **Docker** (containerização local/CI)  
- **Elasticsearch** (no serviço **Jogos**, para busca e recomendações)

---

## Monitoramento
- Instalação do stack de monitoramento via Helm — ver [values-monitoring.yaml](https://github.com/ajmarzola/Grupo49-TechChallenge/blob/main/infra/monitoring/values-monitoring.yaml)

---

## Como Rodar (Rápido)

Existem **duas formas** principais de rodar local:

1) **.NET CLI (sem Docker)** – ideal para desenvolvimento rápido.  
2) **Docker** – isolamento completo, próximo do deploy (há `docker-compose.yml` neste repo de **Pagamentos**).

Antes de tudo, **configure as variáveis** (veja seção [Configuração por Ambiente](#configuração-por-ambiente)).

### Pré-requisitos

- .NET SDK 8.x  
- Docker + Docker Compose (para opção 2)  
- Banco (PostgreSQL/SQL Server) acessível/local  
- Azure Service Bus (ou *Azurite Service Bus* / *Service Bus local emulator*)  
- (Opcional) Azure Functions Core Tools para rodar jobs localmente

---

## Configuração por Ambiente

Ajuste **`appsettings.Development.json`** ou use variáveis de ambiente (sugestão abaixo).  
> Dica: use **secrets do dotnet** em dev: `dotnet user-secrets set "Chave" "Valor"`

| Chave (Environment) | Exemplo / Descrição |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Development` |
| `ConnectionStrings__Default` | `Host=localhost;Port=5432;Database=fcg_payments;Username=dev;Password=dev` |
| `ServiceBus__ConnectionString` | `Endpoint=sb://...;SharedAccessKeyName=...;SharedAccessKey=...` |
| `ServiceBus__PaymentsTopic` | `payments` (ou `payments-commands`) |
| `ServiceBus__Subscriptions__PaymentsProcessor` | `payments-processor` |
| `Jwt__Authority` | URL do emissor (se usar B2C/IdP) |
| `Jwt__Audience` | `fcg-api` |
| `ReadModel__Provider` | `Postgres` (ou `SqlServer`) |
| `Outbox__Enabled` | `true` |
| `Observability__EnableTracing` | `true` |

> **Importante**: verifique nomes de fila/tópico/assinaturas conforme **seu** `appsettings` do projeto.

---

## Executando com .NET CLI

> A solução/Projetos:  
> `FCG.TechChallenge.Pagamentos.sln` com camadas **Application, Domain, Infrastructure, Presentation** e **Test** (vide lista no repo).

1. Restaurar & compilar
   ```bash
   dotnet restore
   dotnet build -c Debug
   ```

2. (Opcional) Aplicar **migrations** para o **WriteModel** e **ReadModel**  
   > Ajuste os nomes dos `DbContext`/projetos conforme seu código.
   ```bash
   dotnet ef database update -s FCG.TechChallenge.Pagamentos.Presentation -p FCG.TechChallenge.Pagamentos.Infrastructure
   ```

3. Executar a **API**
   ```bash
   dotnet run -c Debug --project FCG.TechChallenge.Pagamentos.Presentation
   ```
   - A API sobe, por padrão, em `http://localhost:5080` (ajuste a porta conforme seu `launchSettings.json`).

4. (Opcional) Executar **Azure Functions** (processadores/outbox)
   ```bash
   func start
   ```
   > Garanta que as variáveis de **Service Bus** e **ConnectionStrings** estejam presentes no terminal.

---

## Executando com Docker

> Este repo já possui **docker-compose.yml** para facilitar o *spin-up* local (API + dependências básicas). Abra na raiz do projeto.

1. Buildar imagens
   ```bash
   docker compose build
   ```

2. Subir os serviços
   ```bash
   docker compose up -d
   ```

3. Ver logs
   ```bash
   docker compose logs -f pagamentos-api
   ```

> Se preferir isolar bancos/mensageria em outro compose ou em containers já existentes, remova/ajuste os serviços do compose deste repo.

---

## Fluxo de Teste End-to-End

> **Cenário típico**: Usuário autenticado compra um jogo → Jogos emite *intent* → Pagamentos processa → Evento de aprovação/recusa retorna → Jogos confirma aquisição.

1) **Subir microsserviços** na máquina (ou via Docker):  
   - **Usuários** (tokens JWT) — repo tem README e workflows.  
     https://github.com/ajmarzola/FCG.TechChallenge.Usuarios
   - **Jogos** (catálogo/compra; integrações com Elasticsearch) — há `docker-compose.yml` e README.  
     https://github.com/ajmarzola/FCG.TechChallenge.Jogos  
   - **Pagamentos** (este repo).

2) **Obter token** (Usuários)  
   - Autentique e gere um JWT (endpoints/documentação no repo de **Usuários**).

3) **Criar intent de pagamento** (Pagamentos)
   ```bash
   curl -X POST http://localhost:5080/api/pagamentos/intent \
     -H "Authorization: Bearer <JWT>" \
     -H "Content-Type: application/json" \
     -d '{
           "orderId":"<id-do-pedido>",
           "userId":"<id-usuario>",
           "amount": 99.90,
           "currency":"BRL",
           "method":"credit_card",
           "metadata": { "jogoId":"<id-jogo>" }
         }'
   ```
   - A API valida, grava o comando e enfileira no **Service Bus** (Outbox → Dispatcher).

4) **Processamento assíncrono**  
   - *Worker/Function* consome a fila/tópico, chama provedor simulado/real, grava status e publica `PagamentoAprovado` ou `PagamentoRecusado`.

5) **Consultar status**
   ```bash
   curl http://localhost:5080/api/pagamentos/status/<orderId> \
     -H "Authorization: Bearer <JWT>"
   ```

> Dica: No **Jogos**, uma compra pode disparar a intent automaticamente, concluindo a jornada fim a fim quando o evento de pagamento chega.

---

## Coleções/API Docs

- **Swagger/OpenAPI**: acesse `http://localhost:<porta>/swagger` quando a API estiver de pé.
- (Opcional) **Postman**: adicione uma collection com as rotas acima.
- **Autorização**: inclua o JWT do serviço de **Usuários** no *Authorization*.

---

## Estrutura do Repositório

Padrão *clean/DDD* em camadas:

```
FCG.TechChallenge.Pagamentos/
├─ FCG.TechChallenge.Pagamentos.Application/
├─ FCG.TechChallenge.Pagamentos.Domain/
├─ FCG.TechChallenge.Pagamentos.Infrastructure/
├─ FCG.TechChallenge.Pagamentos.Presentation/
├─ FCG.TechChallenge.Pagamentos.Test/
├─ docker-compose.yml
└─ FCG.TechChallenge.Pagamentos.sln
```

> Ajuste os nomes/paths conforme a organização atual do repositório.

---

## CI/CD

- **GitHub Actions** com *build/test* e gatilhos para *deploy* (seguir o mesmo padrão adotado no MVP e nos outros serviços). No repositório **Grupo49-TechChallenge** há pipelines de **CI/CD** prontos que servem de referência de estrutura e gates.  
  https://github.com/ajmarzola/Grupo49-TechChallenge
- **Multi-stage** com *environments* (Dev → Homolog → Prod) e **aprovação manual** para Prod.
- Segredos via **GitHub Environments/Secrets** e, em cloud, **Key Vault**.

---

## Roadmap

- [ ] Implementar retries com *exponential backoff* no dispatcher do outbox  
- [ ] Adicionar *dead-letter* handling no Service Bus  
- [ ] Métricas de sucesso/latência (+ traces distribuídos)  
- [ ] Suporte a múltiplos PSPs (cartão, Pix, boleto) via *strategy*  
- [ ] Contratos de eventos versionados (`v1`, `v2`)  
- [ ] *Chaos testing* do pipeline de pagamento

---

## Licença

Este projeto é acadêmico e parte do **Tech Challenge FIAP**. Verifique termos aplicáveis a cada repositório.

## 👥 Integrantes do Grupo
• Anderson Marzola — RM360850 — Discord: aj.marzola

• Rafael Nicoletti — RM361308 — Discord: rafaelnicoletti_

• Valber Martins — RM3608959 — Discord: valberdev
