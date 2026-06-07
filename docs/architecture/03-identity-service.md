# Identity Service - Component Diagram

```mermaid
graph TB
    subgraph "Identity Service - Port 5001"
        
        subgraph "API Layer"
            AuthController[AuthController<br/>/api/auth/login<br/>/api/auth/register<br/>/api/auth/refresh]
            UserController[UserController<br/>/api/users<br/>/api/users/{id}<br/>/api/users/profile]
            RoleController[RoleController<br/>/api/roles]
        end
        
        subgraph "Application Layer - CQRS"
            Commands[Commands<br/>RegisterUserCommand<br/>UpdateUserCommand<br/>DeleteUserCommand]
            Queries[Queries<br/>GetUserQuery<br/>GetUsersListQuery<br/>GetUserRolesQuery]
            Handlers[Handlers<br/>+ Validation Pipeline<br/>+ Logging Pipeline<br/>+ Caching Pipeline]
        end
        
        subgraph "Domain Layer"
            Entities[Entities<br/>User<br/>Role<br/>Permission]
            ValueObjects[Value Objects<br/>Email<br/>Password<br/>PhoneNumber]
            Events[Domain Events<br/>UserRegisteredEvent<br/>UserRoleChangedEvent]
            Interfaces[Domain Interfaces<br/>IUserRepository<br/>IRoleRepository]
        end
        
        subgraph "Infrastructure Layer"
            EF[EF Core DbContext<br/>+ Repository Pattern<br/>+ Unit of Work]
            JWT[JWT Service<br/>+ Token Generation<br/>+ Refresh Tokens]
            Redis[Redis Cache<br/>+ Distributed Cache<br/>+ Session Storage]
            Kafka[Kafka Producer<br/>+ UserRegisteredEvent<br/>+ UserUpdatedEvent]
            Password[Password Service<br/>+ BCrypt Hashing<br/>+ Validation]
        end
        
        subgraph "Cross-Cutting"
            Logging[Serilog<br/>+ Structured Logging]
            Metrics[Prometheus<br/>+ Custom Metrics]
            Health[Health Checks<br/>+ DB + Cache + Kafka]
        end
        
        AuthController --> Commands
        AuthController --> Queries
        UserController --> Commands
        UserController --> Queries
        
        Commands --> Handlers
        Queries --> Handlers
        
        Handlers --> Entities
        Handlers --> Interfaces
        Handlers --> Events
        
        Interfaces --> EF
        Handlers --> JWT
        Handlers --> Redis
        Handlers --> Kafka
        Handlers --> Password
        
        AuthController --> Logging
        UserController --> Metrics
        Commands --> Health
    end
    
    Postgres[(PostgreSQL)] --> EF
    RedisServer[(Redis Server)] --> Redis
    KafkaBroker[(Kafka)] --> Kafka
    
    style Handlers fill:#ff9,stroke:#333,stroke-width:2px
    style Events fill:#f9f,stroke:#333,stroke-width:2px
```
