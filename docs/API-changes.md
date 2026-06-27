# API changes

This project aims to recreate the original plugin API closely, so as not to require too many changes to plugin code,
however it would be remiss not to make some improvements.

## General changes

- Nullability annotations have been applied where appropriate.
- Mutability is reduced across the board - it is no longer possible to reassign request properties etc.
- Callback collection parameters are generally exposed as interfaces rather than concrete types, to enable efficient
  proxying instead of sending copies.
- Callbacks now return Task instead of void and can use async/await instead of callback-based async.
- Callbacks take a CancellationToken which expires after a pre-determined amount of time and can be used to ensure
  long-running tasks don't dangle forever.

## `IPluginFactory`

### `Create`

- Now takes an `IConfiguration` instead of `Dictionary<string, string>` for configuration. This allows you to easily
  bind to concrete options objects instead of hand-rolling parsing.
- No longer has `out errorMsg` parameter; if plugin creation fails an exception should be thrown instead.

## `PluginBase`

- `SetupInstance` has been removed in favour of putting setup logic in the constructor. Assigning fields like
  `IPluginHost host` in a secondary method is not very compatible with modern .NET nullable analysis rules; you either
  have to declare `host` as `IPluginHost?` or suppress warnings. Since users write their own `IPluginFactory` anyway,
  if more complex and failure-prone initialisation logic is required, it is suggested to write a static `Create` factory
  method that calls a private constructor.