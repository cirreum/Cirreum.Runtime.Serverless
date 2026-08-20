# Cirreum.Runtime.Serverless 1.2.0 — remote clients behind an interface

## Why this release exists

`AddRemoteClient<TClient>` binds a concrete typed client to a configured `HttpClient`. Injecting
that client by interface — the shape that makes a caller substitutable in tests — meant adding a
forwarding registration by hand:

```csharp
builder.AddRemoteClient<OrdersApiClient>(o => { … });
builder.Services.AddTransient<IOrdersApi>(sp => sp.GetRequiredService<OrdersApiClient>());
```

Adequate, but a line every app writes and none should have to. `Cirreum.Runtime.Wasm` gained the
interface-typed form in the same wave; this release keeps the two hosts' remote-client surfaces
symmetric rather than letting them drift.

## What's new

**`AddRemoteClient<TClient, TImplementation>`**, mirroring both existing typed forms — the
configure delegate and the pre-built options object:

```csharp
builder.AddRemoteClient<IOrdersApi, OrdersApiClient>(o => {
    o.ServiceUri = new Uri("https://orders.internal/");
    o.CredentialType = CredentialType.ManagedIdentity;
});
```

`TImplementation` is constrained to `TClient`, so the pairing is checked at compile time. The
named-only overload registers no typed client and is unchanged.

**The client name defaults to `TImplementation`.** Naming it after the service type would rename
the client the moment an app moved from a concrete registration to an interface-typed one —
silently detaching any configuration, credential wiring, or header redaction keyed on the old
name. Defaulting to the concrete client keeps the name stable across that refactor. An explicit
`clientName` still wins.

## Compatibility

- Purely additive. Existing `AddRemoteClient<TClient>` calls are unaffected — same signatures,
  same client names, same behaviour.
- Internally, the typed-client registration is now supplied to the shared registration path as a
  delegate, so both forms run one copy of the validation, de-duplication, and credential wiring
  instead of a second copy that could drift. No observable change.

## See also

- `Cirreum.Runtime.Wasm 2.1.0` — the same surface for the browser host, where the motivating
  case is substituting a client in component tests.
