# Legacy Code - READ ONLY

## Purpose
This directory contains the legacy OPT application source code and database scripts. It serves as **reference material** for migration — **NEVER modify files here**.

## Structure

```
old/
├── Fuente/    # Legacy source code
└── BD/        # Legacy database scripts and schemas
```

## Rules

- **NEVER modify** any file in this directory
- **ALWAYS read** from here to understand existing business logic
- When migrating functionality:
  1. Read the legacy code to understand current behavior
  2. Document key business rules in `.agents/decisions/`
  3. Implement new code in `src/`
  4. Note any discrepancies or improvements in migration decisions

## Migration Notes

When analyzing legacy code, look for:
- Business rules embedded in UI code
- Database procedures and triggers
- Hardcoded values that should become configuration
- Deprecated patterns or libraries
- Security vulnerabilities to fix during migration
