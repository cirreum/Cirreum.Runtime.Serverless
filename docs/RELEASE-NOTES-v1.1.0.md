# Cirreum.Runtime.Serverless 1.1.0 — config-time errors stop the build

## Why this release exists

The serverless host has always *flushed* deferred logs at startup, but never *validated*
them: a configuration-time error — like Domain's dead-operations check, whose operations will
be denied on every dispatch — became a log line after the host was already running. The
server builder has always failed the build on exactly these entries; this release closes the
asymmetry (its WASM sibling closes the same gap in `Cirreum.Runtime.Wasm` 2.0.0, where the
entries weren't even flushed).

## What's new

**Build-time deferred-log validation.** `Build` now throws when configuration-time checks
wrote Warning-or-worse deferred entries, listing them in the exception. The
`FlushDeferredLogs` startup task keeps surfacing informational entries once the host runs.

## Compatibility

Additive surface, one deliberate behavior change: a host that was starting *with config-time
errors present* now fails fast at build with the errors spelled out, instead of running with
operations that deny on every dispatch. Correctly-configured hosts see no difference.

## See also

- `docs/CHANGELOG.md` — the enumerated changes
