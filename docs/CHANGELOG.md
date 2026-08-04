# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.1.0] - 2026-08-04

### Added

- **Build-time deferred-log validation.** `Build` now fails when configuration-time checks
  wrote Warning-or-worse deferred entries — e.g. Domain's dead-operations check, whose
  operations will be denied on every dispatch. The `FlushDeferredLogs` startup task still
  surfaces informational entries once the host runs; config-time errors now stop the build
  instead of becoming a log line after startup. Mirrors the check the server builder has
  always run. ⚠️ Hosts that were starting with such errors present will now fail fast at
  build — that is the point.

## [1.0.54] - 2026-07-31

### Updated

- Updated NuGet packages (Cirreum spine 4.0.1 wave: `Cirreum.Domain` 4.0.1 / `Cirreum.AuthenticationProvider` 2.0.3 / `Cirreum.Services.*` repins).

## [1.0.53] - 2026-07-30

### Updated

- Updated NuGet packages — picks up the `Cirreum.Domain` 3.0.0 authorization-enforcement wave
  (fail-open operation-authorization fix + `IPolicyAuthorizer` rename) through the re-pinned
  spine/service packages; see Cirreum.Domain `MIGRATION-v3.md`.

## [1.0.52] - 2026-07-27

### Updated

- Updated NuGet packages.

## [1.0.51] - 2026-07-24

### Updated

- Updated NuGet packages.

## [1.0.50] - 2026-07-22

### Updated

- Updated NuGet packages.

## [1.0.49] - 2026-07-20

### Updated

- Updated NuGet packages.

## [1.0.48] - 2026-07-19

### Updated

- Updated NuGet packages.

## [1.0.47] - 2026-07-08

### Updated

- Updated NuGet packages as part of the lower-layer changes.

## [1.0.46] - 2026-07-06

### Updated

- Updated NuGet packages.

## [1.0.45] - 2026-07-04

### Updated

- Updated NuGet packages.

## [1.0.44] - 2026-07-04

### Updated

- Updated NuGet packages.

## [1.0.43] - 2026-05-10

### Updated

- Updated NuGet packages.

## [1.0.41] - 2026-05-01

### Updated
- Updated NuGet packages.

