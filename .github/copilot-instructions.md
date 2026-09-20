# Copilot Instructions

## Shared Instructions

Shared Copilot instructions, skills and prompts are maintained centrally in the
[account-level `.github` repository](https://github.com/f2calv/.github). They are deliberately not
copied here.

To load them, clone that repository and either add it to the VS Code workspace or link its
instruction, skill and prompt folders into `~/.copilot/`. If those files are unavailable, stop
rather than guessing the conventions.

Everything below is specific to this repository.

## Repository Purpose

This repository is a .NET Redis playground containing clients, a server process, web examples,
benchmarks and shared-library tests. Compose files provide standalone and replication examples.

- Keep Redis access and serialization behavior in `SharedLibrary`.
- Keep benchmark code out of correctness tests and application startup paths.
- Treat flushes, key deletion and volume removal as destructive operations requiring explicit
  approval.
- Preserve `UserSecretsId`; it identifies the local secret store and is not a credential.
