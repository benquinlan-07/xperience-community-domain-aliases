# XperienceCommunity.DomainAliases
## Description
This package provides XbK administrators with an interface to assign additional domain aliases to a website channel.

When editing a website channel, a new menu item will be shown for "Domain aliases".

## Installation
This package can be installed from nuget using the command:

    Install-Package XperienceCommunity.DomainAliases

or using the .NET CLI with the command:

    dotnet add package XperienceCommunity.DomainAliases


## Setup

Add the following line to your Program.cs to register the necessary services.

    using XperienceCommunity.DomainAliases;

    ...

    builder.Services.AddDomainAliasesExtensionServices();

    ...

    app.UseDomainAliasesExtension();


Ensure the call to register the middleware is added before `app.UseKentico()` to ensure that the middleware is able to execute before Kentico does.