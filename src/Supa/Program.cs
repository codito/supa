// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs">
//   Copyright (c) Supa Contributors. All rights reserved.
//   See license.md for license details.
// </copyright>
// <remarks>
//  Untested code. Please don't add business logic.
// </remarks>
// --------------------------------------------------------------------------------------------------------------------
namespace Supa
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;

    using Serilog;

    /// <summary>
    /// Program entry point.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main method.
        /// </summary>
        /// <param name="args">
        /// Command line arguments.
        /// </param>
        public static void Main(string[] args)
        {
            while (true)
            {
                try
                {
                    // Setup logging
                    Log.Logger =
                        new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.ColoredConsole().CreateLogger();

                    // TODO Read the settings ini file
                    var app = new SupaApp();
                    app.ReadConfiguration("settings.json");
                    dynamic appConfig = app.Configuration;

                    var credential = new NetworkCredential(
                        appConfig.ExchangeSource.Username,
                        appConfig.ExchangeSource.Password,
                        appConfig.ExchangeSource.Domain);
                    var source = new ExchangeSource(
                        new Uri(appConfig.ExchangeSource.ServiceUri),
                        credential,
                        appConfig.ExchangeSource.FolderName);

                    IDictionary<string, object> workItemTemplate = appConfig.TfsSink.WorkItemTemplate;
                    var tfsSink = new TfsSink(
                        new Uri(appConfig.TfsSink.ServiceUri),
                        new NetworkCredential(appConfig.TfsSink.Username, appConfig.TfsSink.Password), 
                        appConfig.TfsSink.ParentWorkItem,
                        workItemTemplate);
                    tfsSink.Configure();
                    foreach (var issue in source.Issues)
                    {
                        tfsSink.UpdateWorkItem(issue);
                    }

                    Log.Logger.Information("Exit.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex}");
                }

                Thread.Sleep(15 * 60 * 1000);
            }
        }
    }
}