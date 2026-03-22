# Sentinel Assets API

> API REST para gestão e monitoramento de ativos de segurança da infraestrutura — rastreamento de vulnerabilidades, alertas em tempo real via AWS SNS e autenticação JWT.

[![.NET](https://img.shields.io/badge/.NET-7.0-512BD4?logo=dotnet)](https://dot.net)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15-336791?logo=postgresql)](https://postgresql.org)
[![AWS SNS](https://img.shields.io/badge/AWS-SNS-FF9900?logo=amazonaws)](https://aws.amazon.com/sns/)

---

## 📖 O que é o Sentinel Assets API?

O **Sentinel** funciona como um *porteiro inteligente* da sua infraestrutura: registra ativos de TI (servidores, dispositivos, aplicações), monitora status de vulnerabilidade e dispara alertas em tempo real sempre que novos ativos são adicionados. Ideal para portfólio, demonstração de conceitos cloud-native e integração com pipelines de segurança.

### Principais funcionalidades

| Recurso | Descrição |
|--------|-----------|
| **CRUD de Assets** | Criar, listar, buscar, atualizar e remover ativos de segurança |
| **Autenticação JWT** | Endpoints protegidos com token Bearer; emissão via login |
| **Notificações AWS SNS** | Disparo automático de alertas ao cadastrar novo ativo |
| **PostgreSQL** | Banco relacional com índice único em `IPAddress` |
| **Swagger** | Documentação interativa da API em ambiente de desenvolvimento |
| **Docker** | Um comando para subir API + PostgreSQL (`docker compose up -d`) |

---

## 🏗️ Arquitetura

```
┌─────────────────┐     JWT      ┌──────────────────────┐
│   Cliente       │◄────────────►│  Sentinel Assets API │
│  (Postman/etc)  │   Bearer     │  (ASP.NET Core 7)    │
└─────────────────┘              └──────────┬───────────┘
                                            │
                    ┌───────────────────────┼───────────────────────┐
                    ▼                       ▼                       ▼
            ┌───────────────┐      ┌───────────────┐      ┌─────────────────┐
            │  PostgreSQL   │      │   AWS SNS     │      │ Swagger UI      │
            │  (Docker)     │      │  (Notificações)│      │ (Documentação)  │
            └───────────────┘      └───────────────┘      └─────────────────┘
```

### Stack tecnológica

| Componente | Tecnologia |
|------------|------------|
| Runtime | .NET 7 |
| API | ASP.NET Core Web API |
| ORM | Entity Framework Core |
| Banco de dados | PostgreSQL (Npgsql) |
| Autenticação | JWT Bearer |
| Notificações | AWS Simple Notification Service (SNS) |
| Documentação | Swagger/OpenAPI |

---

## 🚀 Início rápido

### Pré-requisitos

- [Docker](https://www.docker.com/) (obrigatório para o modo completo)
- [.NET 7 SDK](https://dotnet.microsoft.com/download/dotnet/7.0) (opcional; só para desenvolvimento local)
- [AWS CLI](https://aws.amazon.com/cli/) (opcional; para notificações SNS)

---

### ⚡ Modo Docker (um comando)

Subir **PostgreSQL + API** com um único comando:

```bash
# Na raiz do projeto
docker compose up -d
```

- **API:** http://localhost:5000  
- **Swagger:** http://localhost:5000/swagger  
- Migrations são aplicadas automaticamente na inicialização

Para parar: `docker compose down`

---

### 🛠️ Modo desenvolvimento local (opcional)

Para rodar a API fora do Docker (útil para debug com breakpoints):

#### 1. Subir só o PostgreSQL

```bash
docker compose up -d postgres
```

#### 2. Aplicar migrations e rodar a API

```bash
cd src/SentinelAssetsAPI
dotnet restore
dotnet ef database update
dotnet run
```

> **Dica:** Se aparecer "arquivo bloqueado por outro processo", há uma instância da API rodando. Encerre-a com `Ctrl+C` ou `taskkill /IM SentinelAssetsAPI.exe /F`

### 4. Testar o fluxo completo

```bash
# 1. Obter token JWT
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"password"}'

# 2. Listar ativos (usar o token retornado)
curl -X GET http://localhost:5000/api/assets \
  -H "Authorization: Bearer SEU_TOKEN_AQUI"

# 3. Criar um ativo (persiste no PostgreSQL e dispara notificação SNS)
curl -X POST http://localhost:5000/api/assets \
  -H "Authorization: Bearer SEU_TOKEN_AQUI" \
  -H "Content-Type: application/json" \
  -d '{"name":"Servidor Web","ipAddress":"192.168.1.10","vulnerabilityStatus":"OK"}'
```

---

## 📡 Endpoints da API

| Método | Rota | Autenticação | Descrição |
|--------|------|--------------|-----------|
| `POST` | `/api/auth/login` | Não | Gera token JWT |
| `GET` | `/api/assets` | JWT | Lista todos os ativos |
| `GET` | `/api/assets/{id}` | JWT | Busca ativo por ID |
| `POST` | `/api/assets` | JWT | Cria ativo e envia notificação SNS |
| `PUT` | `/api/assets/{id}` | JWT | Atualiza ativo |
| `DELETE` | `/api/assets/{id}` | JWT | Remove ativo |

### Credenciais de login (desenvolvimento)

- **Usuário:** `admin`
- **Senha:** `password`

> ⚠️ **Limitação intencional (demonstração):** As credenciais estão hardcoded para facilitar o uso local. Em produção, seriam substituídas por: tabela de usuários no banco, senhas com hash (bcrypt/Argon2), possivelmente OAuth/OpenID Connect, refresh tokens e políticas de bloqueio após tentativas falhas.

---

## 📦 Modelo de dados

### Asset

| Campo | Tipo | Obrigatório | Descrição |
|-------|------|-------------|-----------|
| `id` | `int` | Sim (auto) | Identificador único |
| `name` | `string` (100) | Sim | Nome do ativo |
| `ipAddress` | `string` (45) | Não | Endereço IP (único) |
| `vulnerabilityStatus` | `string` (100) | Não | `OK`, `LOW`, `MEDIUM`, `HIGH`, `CRITICAL` (padrão: `OK`) |
| `createdAt` | `DateTime` | Sim | Data/hora de criação |
| `lastScanned` | `DateTime?` | Não | Última varredura de vulnerabilidade |

---

## ⚙️ Configuração

### appsettings.json

O `appsettings.json` do projeto deve usar **porta 5433** para o PostgreSQL em modo local (evita conflito com PostgreSQL instalado no host). O `docker-compose` sobrescreve isso para a API em container (usa `postgres:5432` internamente).

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5433;Database=sentinel_assets;Username=postgres;Password=postgres123"
  },
  "Jwt": {
    "Key": "CHAVE-MINIMO-32-CARACTERES",
    "Issuer": "SentinelAPI",
    "Audience": "SentinelAPI"
  },
  "AWS": {
    "Region": "us-east-1"
  }
}
```

### Segredos (JWT, senhas) — boas práticas

Os valores em `appsettings.json` são aceitáveis para **demonstração local**, mas não devem ser commitados em produção:

| Ambiente | Recomendação |
|----------|--------------|
| **Desenvolvimento** | `dotnet user-secrets` — segredos ficam fora do repositório |
| **Produção** | Variáveis de ambiente ou provedor externo (Azure Key Vault, AWS Secrets Manager) |

**Exemplo com user-secrets (dev):**

```bash
cd src/SentinelAssetsAPI
dotnet user-secrets set "Jwt:Key" "sua-chave-secreta-min-32-chars"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5433;..."
```

O ASP.NET Core carrega user-secrets automaticamente em Development, sobrescrevendo o `appsettings.json`. Em produção, use `Jwt__Key` e `ConnectionStrings__DefaultConnection` como variáveis de ambiente.

### AWS SNS

Para enviar notificações reais:

1. Crie um tópico SNS no AWS Console (ex.: `SentinelAssetsTopic`).
2. Configure credenciais AWS (`aws configure` ou variáveis de ambiente).
3. Atualize o `TopicArn` em `NotificationService.cs` com a ARN do seu tópico.

Política IAM sugerida em `infrastructure/iam-policy.json`:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": ["sns:Publish"],
      "Resource": "arn:aws:sns:*:*:SentinelAssetsTopic"
    }
  ]
}
```

---

## 📁 Estrutura do projeto

```
Sentinel Assets API/
├── src/
│   └── SentinelAssetsAPI/
│       ├── Controllers/
│       │   ├── AuthController.cs      # Login e emissão JWT
│       │   └── AssetsController.cs    # CRUD de ativos
│       ├── Data/
│       │   └── AppDbContext.cs        # DbContext EF Core
│       ├── Migrations/                 # Migrações PostgreSQL
│       ├── Models/
│       │   └── Asset.cs               # Entidade Asset
│       ├── Services/
│       │   └── NotificationService.cs # Integração AWS SNS
│       ├── Program.cs                 # Bootstrap e DI
│       └── appsettings.json
├── infrastructure/
│   └── iam-policy.json                # Política IAM para SNS
├── docker-compose.yml                 # PostgreSQL local
├── SentinelAssetsAPI.sln
└── README.md
```

---

## 🔄 Migração SQLite → PostgreSQL

O projeto foi migrado de SQLite para PostgreSQL para suportar cenários de produção:

- Maior concorrência e escalabilidade
- Suporte a índices e tipos avançados (ex.: JSONB)
- Integração direta com AWS RDS e ambientes cloud

Migrations antigas de SQLite foram removidas; a migration atual (`InitialPostgres`) cria a tabela `Assets` com índices em PostgreSQL.

---

## 🔧 Solução de problemas

### "password authentication failed for user postgres"

Isso ocorre quando há conflito de porta com PostgreSQL instalado no host. O projeto usa a porta **5433** para evitar isso. Se ainda assim falhar:

```powershell
# Resetar o banco (remove volume e recria)
.\scripts\reset-postgres.ps1
```

### Docker: aviso sobre `version` obsoleto

O atributo `version` foi removido do `docker-compose.yml` (obsoleto nas versões recentes do Docker Compose).

### "Arquivo bloqueado por outro processo" ao fazer build

A API já está em execução. Encerre a instância anterior:
- `Ctrl+C` no terminal onde a API está rodando, ou
- `taskkill /IM SentinelAssetsAPI.exe /F` (PowerShell/CMD)

---

## 📋 Próximos passos (roadmap)

- [ ] Testes unitários e de integração (xUnit)
- [ ] Dockerfile para a API e `docker-compose` full-stack
- [ ] CI/CD com GitHub Actions
- [ ] Integração com AWS RDS em produção
- [ ] Autenticação com usuários reais (banco + bcrypt)
- [ ] Paginação e filtros em `GET /api/assets`

---

## 📄 Licença

Projeto de demonstração/portfólio. Utilize conforme sua necessidade.
