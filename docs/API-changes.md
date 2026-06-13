# API changes

This project aims to recreate the original plugin API closely, so as not to require too many changes to plugin code,
however it would be remiss not to make some improvements.

## General changes

- Nullability annotations have been applied where appropriate.
- Mutability is reduced across the board - it is no longer possible to reassign request properties etc.
- Callback collection parameters are generally exposed as interfaces rather than concrete types, to enable efficient
  proxying instead of sending copies.

## `IPluginFactory`

### `Create`

- Now takes an `IConfiguration` instead of `Dictionary<string, string>` for configuration. This allows you to easily
  bind to concrete options objects instead of hand-rolling parsing.
- No longer has `out errorMsg` parameter; if plugin creation fails an exception should be thrown instead.