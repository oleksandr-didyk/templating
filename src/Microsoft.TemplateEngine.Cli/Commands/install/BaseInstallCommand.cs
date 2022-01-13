﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using Microsoft.TemplateEngine.Abstractions;
using Microsoft.TemplateEngine.Edge.Settings;

namespace Microsoft.TemplateEngine.Cli.Commands
{
    internal abstract class BaseInstallCommand : BaseCommand<InstallCommandArgs>
    {
        internal BaseInstallCommand(
            NewCommand parentCommand,
            ITemplateEngineHost host,
            ITelemetryLogger logger,
            NewCommandCallbacks callbacks,
            string commandName)
            : base(host, logger, callbacks, commandName, SymbolStrings.Command_Install_Description)
        {
            ParentCommand = parentCommand;
            this.AddArgument(NameArgument);
            this.AddOption(InteractiveOption);
            this.AddOption(AddSourceOption);
        }

        internal Argument<IReadOnlyList<string>> NameArgument { get; } = new("package")
        {
            Description = SymbolStrings.Command_Install_Argument_Package,
            Arity = new ArgumentArity(1, 99)
        };

        internal virtual Option<bool> InteractiveOption { get; } = SharedOptionsFactory.CreateInteractiveOption();

        internal virtual Option<IReadOnlyList<string>> AddSourceOption { get; } = SharedOptionsFactory.CreateAddSourceOption();

        protected NewCommand ParentCommand { get; }

        protected override async Task<NewCommandStatus> ExecuteAsync(InstallCommandArgs args, IEngineEnvironmentSettings environmentSettings, InvocationContext context)
        {
            using TemplatePackageManager templatePackageManager = new TemplatePackageManager(environmentSettings);
            TemplatePackageCoordinator templatePackageCoordinator = new TemplatePackageCoordinator(
                TelemetryLogger,
                environmentSettings,
                templatePackageManager);

            //we need to await, otherwise templatePackageManager will be disposed.
            return await templatePackageCoordinator.EnterInstallFlowAsync(args, context.GetCancellationToken()).ConfigureAwait(false);
        }

        protected override InstallCommandArgs ParseContext(ParseResult parseResult)
        {
            return new InstallCommandArgs(this, parseResult);
        }
    }
}