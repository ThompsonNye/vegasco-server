# Vegasco Server

Backend for the vegasco (***VE***hicle ***GAS*** ***CO***nsumption) application.

## Getting Started

### Configuration

| Configuration | Description | Default | Required |
| --- | --- | --- | --- |
| JWT:Authority | The authority of the JWT token. | - | true |
| JWT:Audience | The audience of the JWT token. | - | true |
| JWT:Issuer | The issuer of the JWT token. | - | true |
| JWT:NameClaimType | The type of the name claim. | `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name` (C# constant `ClaimTypes.Name` | false |

The application uses the prefix `Vegasco_` for environment variable names. The prefix is removed when the application reads the environment variables and duplicate entries are overwritten by the environment variables. 

Example:

- `foo=bar1`
- `Vegasco_foo=bar2`

Results in:

- `foo=bar2`
- `Vegasco_foo=bar2`

Configuration hierarchy in environment variables is usually denoted using a colon (`:`). But because on some systems the colon character is a reserved character, you can use a double underscore (`__`) as an alternative. The application will replace the double underscore with a colon when reading the environment variables.

Example:

The environment variable `foo__bar=value` (as well as `Vegasco_foo__bar=value`) will be converted to `foo:bar=value` in the application.

### Configuration examples

As environment variables:

```env
Vegasco_JWT__Authority=https://example.authority.com
Vegasco_JWT__Audience=example-audience
Vegasco_JWT__Issuer=https://example.authority.com/realms/example-realm/
Vegasco_JWT__NameClaimType=preferred_username
```

As appsettings.json (or a environment specific appsettings.*.json):

**Note: the `Vegasco_` prefix is only for environment variables**

```json
{
  "JWT": {
    "Authority": "https://example.authority.com/realms/example-realm",
    "Audience": "example-audience",
    "Issuer": "https://example.authority.com/realms/example-realm/",
    "NameClaimType": "preferred_username"
  }
}
```
