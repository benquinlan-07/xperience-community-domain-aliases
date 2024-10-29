# Xperience Community: Domain Aliases

## Description

This package provides XbK administrators with an interface to assign additional domain aliases to a website channel. This package functions by intercepting the request in the middleware and rewriting the host to that of the assocaited website channel.

The extension was developed to meet requirement to be able to use a hosted development site that references the same database shared with the development team. Due to the current limitation of Website Channels to only support a single domain binding, the hosted development site was unable to render the content for review/testing.

![Xperience by Kentico Domain Aliases](https://raw.githubusercontent.com/benquinlan-07/xperience-community-domain-aliases/refs/heads/main/images/domain-aliases.jpeg)

## Requirements

### Library Version Matrix

| Xperience Version | Library Version |
| ----------------- | --------------- |
| >= 29.5.3         | 1.0.0           |

### Dependencies

- [ASP.NET Core 6.0](https://dotnet.microsoft.com/en-us/download)
- [Xperience by Kentico](https://docs.kentico.com)

## Package Installation

Add the package to your application using the .NET CLI

```
dotnet add package XperienceCommunity.DomainAliases
```

or via the Package Manager

```
Install-Package XperienceCommunity.DomainAliases
```

## Quick Start

1. Install the NuGet package.

1. Update your Program.cs to register the necessary services and execute the middleware.

```csharp
    using XperienceCommunity.DomainAliases;

    ...

    builder.Services.AddDomainAliasesExtensionServices();

    ...

    app.UseDomainAliasesExtension();
```

## Full Instructions

1. Start your XbyK website.

1. Log in to the administration site.

1. Edit your website channel.

1. Select domain aliases from the menu and proceed to add the required domain aliases for your environment.
![Xperience by Kentico Domain Aliases](https://raw.githubusercontent.com/benquinlan-07/xperience-community-domain-aliases/refs/heads/main/images/domain-aliases.jpeg)
