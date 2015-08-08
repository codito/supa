// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Arguments.cs">
//   Copyright (c) Supa Contributors. All rights reserved.
//   See license.md for license details.
// </copyright>
// <remark>
//   Entities in this file are untested. Please minimize logic.
// </remark>
// --------------------------------------------------------------------------------------------------------------------

namespace Supa
{
    using CommandLine;
    using CommandLine.Text;

    /// <summary>
    /// The source type.
    /// </summary>
    public enum SourceType
    {
        /// <summary>
        /// Represents an Exchange online source.
        /// </summary>
        Exchange
    }

    /// <summary>
    /// The sink type.
    /// </summary>
    public enum SinkType
    {
        /// <summary>
        /// Comma separated values file.
        /// </summary>
        Csv,

        /// <summary>
        /// Team foundation server work items store.
        /// </summary>
        Tfs,

        /// <summary>
        /// <c>Sqllite</c> database.
        /// </summary>
        Sql
    }

    /// <summary>
    /// Defines arguments for <c>supa.exe</c>.
    /// </summary>
    public class Arguments
    {
        /// <summary>
        /// Gets or sets a value indicating whether what if.
        /// </summary>
        [Option('w', "whatif", HelpText = "Don't perform any write actions, just show the operations.")]
        public bool WhatIf { get; set; }

        /// <summary>
        /// Gets or sets the source command.
        /// </summary>
        [VerbOption("source", HelpText = "List or Add issues source to supa.")]
        public SourceOptions SourceCommand { get; set; }

        /// <summary>
        /// Gets or sets the sink command.
        /// </summary>
        [VerbOption("sink", HelpText = "List or Add issues export target (sink) to supa.")]
        public SinkOptions SinkCommand { get; set; }

        /// <summary>
        /// Gets or sets the sync command.
        /// </summary>
        [VerbOption("sync", HelpText = "Sync issues source to an export target (sink).")]
        public SyncOptions SyncCommand { get; set; }

        /// <summary>
        /// The get usage.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }

        /// <summary>
        /// The get usage.
        /// </summary>
        /// <param name="verb">
        /// The verb.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            return HelpText.AutoBuild(this, verb);
        }
    }

    #region Source/Sink action and options

    /// <summary>
    /// The common source sink options.
    /// </summary>
    public abstract class CommonSourceSinkOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether list sub command is passed.
        /// </summary>
        [Option('l', "list", MutuallyExclusiveSet = "list", HelpText = "List registered issue sources.")]
        public bool List { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether add sub command is passed.
        /// </summary>
        [Option('a', "add", MutuallyExclusiveSet = "add", HelpText = "Add a new issue source.")]
        public bool Add { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [Option('n', "name", MutuallyExclusiveSet = "add", Required = true, HelpText = "Name of the source")]
        public string Name { get; set; }
    }

    /// <summary>
    /// The source options.
    /// </summary>
    public class SourceOptions : CommonSourceSinkOptions
    {
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        [Option('t', "type", MutuallyExclusiveSet = "add", Required = true, HelpText = "Type of the source. Available values:")]
        public SourceType Type { get; set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        [Option('u', "username", MutuallyExclusiveSet = "add", Required = true, HelpText = "Username for the exchange account. Use default to use current user's credentials.")]
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        [Option('p', "password", MutuallyExclusiveSet = "add", Required = true, HelpText = "Password for the exchange account. Leave blank for current user's credentials.")]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the folder.
        /// </summary>
        [Option('f', "folder", MutuallyExclusiveSet = "add", Required = true, HelpText = "Folder to look for issues. Must be a sub folder of Inbox.")]
        public string Folder { get; set; }
    }
    #endregion

    #region Sink action and options
    /// <summary>
    /// The sink options.
    /// </summary>
    public class SinkOptions
    {
    }
    #endregion

    /// <summary>
    /// The sync options.
    /// </summary>
    public class SyncOptions
    {
        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        [Option('i', "source", Required = true, HelpText = "Name of the source of issues. See source action to list all known sources.")]
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the sink.
        /// </summary>
        [Option('o', "sink", Required = true, HelpText = "Name of the target sink for issues. See sink action to list all known sources.")]
        public string Sink { get; set; }
    }
}