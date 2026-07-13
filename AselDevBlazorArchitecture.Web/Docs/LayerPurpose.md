# Enterprise Clean Architecture -- Layer Purpose Definition

## Overview

This document defines the responsibility, boundaries, and allowed
dependencies of each layer in the solution.

This architecture follows strict Clean Architecture principles and is
designed to be:

-   Domain-agnostic
-   Database-flexible
-   Enterprise-ready
-   Testable
-   Maintainable
-   Scalable

------------------------------------------------------------------------

# Architecture Dependency Direction

Web → Application → Domain\
Infrastructure → Domain\
Web → Infrastructure (Dependency Injection only)

The Domain layer must not depend on any other layer.

------------------------------------------------------------------------

# 1️ Domain Layer

## Purpose

The Domain layer represents the core business model of the system.

## Responsibilities

-   Entities\
-   Value Objects\
-   Domain Exceptions\
-   Domain Events (optional)\
-   Repository Interfaces\
-   Unit of Work Interface\
-   Business rule enforcement

## Must NOT Contain

-   EF Core\
-   ASP.NET Core\
-   Blazor\
-   Logging frameworks\
-   Database code\
-   External libraries

------------------------------------------------------------------------

# 2️ Application Layer

## Purpose

The Application layer orchestrates use cases and business workflows.

## Responsibilities

-   Application services\
-   Use cases\
-   DTOs\
-   Validation logic\
-   Business authorization logic\
-   Logging abstraction\
-   Repository coordination\
-   Transaction orchestration

## Must NOT Contain

-   DbContext\
-   EF Core logic\
-   SQL queries\
-   Logging framework implementation\
-   HTTP-specific logic

------------------------------------------------------------------------

# 3️ Infrastructure Layer

## Purpose

Provides technical implementations for abstractions defined in Domain
and Application.

## Responsibilities

-   EF Core DbContext\
-   Repository implementations\
-   Unit of Work implementation\
-   Database provider configuration\
-   Logging implementation (e.g., Serilog)\
-   Identity\
-   External services\
-   Caching\
-   Background jobs\
-   File storage\
-   Email services

## Must NOT Contain

-   Business logic\
-   UI logic\
-   Domain rule definitions

Infrastructure must remain replaceable.

------------------------------------------------------------------------

# 4️ Web Layer (Blazor Server)

## Purpose

Handles presentation and user interaction.

## Responsibilities

-   Razor components\
-   Layouts\
-   Middleware\
-   Authorization policies\
-   Dependency Injection configuration\
-   Application service usage

## Must NOT Contain

-   Business logic\
-   Direct DbContext usage\
-   SQL queries\
-   Domain rule implementation

Blazor components must remain thin.

------------------------------------------------------------------------

# Repository Strategy

-   Defined in Domain\
-   Implemented in Infrastructure\
-   Used by Application\
-   Never accessed directly by Web\
-   Must not expose IQueryable\
-   Must expose intention-based methods

------------------------------------------------------------------------

# Unit of Work Strategy

-   Defined in Domain\
-   Implemented in Infrastructure\
-   Used by Application to commit transactions\
-   Not accessed directly by Web

------------------------------------------------------------------------

# Logging Strategy

-   Logging abstraction defined in Application\
-   Logging implementation defined in Infrastructure\
-   Structured logging required\
-   No business logging inside Razor components

------------------------------------------------------------------------

# Database Strategy

-   Provider configured in Infrastructure\
-   Switching provider must not affect Domain or Application\
-   No database-specific logic in Application\
-   Migrations handled in Infrastructure

------------------------------------------------------------------------

# Authorization Strategy

-   Policy enforcement in Web layer\
-   Business authorization in Application layer\
-   No authorization logic in Domain

------------------------------------------------------------------------

# Architectural Rules

1.  Domain must not reference Infrastructure.\
2.  Application must not reference EF Core.\
3.  Web must not reference DbContext directly.\
4.  Business logic must never live in Blazor components.\
5.  Infrastructure must remain replaceable.\
6.  Feature-based folder organization is preferred.

------------------------------------------------------------------------

# Long-Term Scalability

This architecture supports:

-   Multiple database providers\
-   Hybrid API + Blazor expansion\
-   CQRS adoption\
-   Microservice extraction\
-   Multi-tenant systems\
-   Distributed caching\
-   Background job processing\
-   Event-driven architecture

------------------------------------------------------------------------

# Final Reminder

If unsure where logic belongs:

Business logic → Domain or Application\
Technical implementation → Infrastructure\
UI behavior → Web

Never mix responsibilities.
