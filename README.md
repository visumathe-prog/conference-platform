# Conference Management Platform

## Production-Ready Microservices Architecture

### Prerequisites
- Docker & Docker Compose
- .NET 8 SDK
- Node.js 18+

### Getting API Keys (Required for real integration)

1. **Stripe** (Payments)
   - Register at https://stripe.com
   - Get keys from Dashboard → Developers → API keys
   - Webhook secret: Listen to `payment_intent.succeeded` event

2. **SendGrid** (Emails)
   - Register at https://sendgrid.com
   - Create API key with full access
   - Verify sender email

3. **AWS** (S3, SNS, SQS)
   - Create IAM user with programmatic access
   - Attach policies: AmazonS3FullAccess, AmazonSNSFullAccess
   - Copy Access Key & Secret

### Quick Start

# 1. Clone repository
git clone https://github.com/yourusername/conference-platform.git

# 2. Copy environment template
cp .env.example .env

# 3. Edit .env with your API keys (or leave empty for code review only)

# 4. Build and run with Docker
docker-compose up --build

# 5. Access services
# API Gateway: http://localhost:5000
# Swagger UI: http://localhost:5000/swagger
# Grafana: http://localhost:3000 (if configured)

Without API Keys:

The code is fully functional with validation. 
To run without keys, set DOCKER_ENV=demo to use mocks.

Architecture Highlights:

· Real Stripe integration with webhooks
· AWS S3 for certificate storage
· SendGrid for transactional emails
· Kafka for event-driven communication
· Distributed caching with Redis
· JWT authentication with refresh tokens
· OpenTelemetry + Prometheus + Grafana
· Horizontal Pod Autoscaling (K8s)
· Infrastructure as Code (Terraform)

Im Frühjahr 2026 wurde das Caching von Redis 7.0 auf Redis 7.2 (aktuellste stabile Version) migriert.

Author: Olha Bondarieva

P.S. bash

# 1. Клонирование
git clone https://github.com/yourname/conference-platform.git
cd conference-platform

# 2. Запуск инфраструктуры
docker-compose up -d postgres-identity redis kafka

# 3. Применение миграций
dotnet ef database update --project src/Services/Identity.Service

# 4. Запуск бэкенда
cd src/Services/Identity.Service
dotnet run

# 5. Фронтенд (новый терминал)
cd angular-app
npm install
ng serve

# 6. Открыть браузер
# http://localhost:4200

# 7. Компонент Как запустить
PostgreSQL docker-compose up -d postgres-identity
Redis docker-compose up -d redis
Kafka docker-compose up -d kafka
Identity Service dotnet run --project src/Services/Identity.Service
