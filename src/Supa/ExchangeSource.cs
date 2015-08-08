// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExchangeSource.cs">
//   Copyright (c) Supa Contributors. All rights reserved.
//   See license.md for license details.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Supa
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    using Serilog;

    using Supa.Platform;

    /// <summary>
    /// The outlook source.
    /// </summary>
    public class ExchangeSource
    {
        private readonly string folderName;
        private readonly IExchangeServiceProvider exchangeServiceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangeSource"/> class.
        /// </summary>
        /// <param name="serviceUrl">
        /// Url of the exchange server service.
        /// </param>
        /// <param name="credential">
        /// Credentials for the service.
        /// </param>
        /// <param name="folderName">
        /// Folder to monitor.
        /// </param>
        public ExchangeSource(Uri serviceUrl, NetworkCredential credential, string folderName)
            : this(folderName, new ExchangeServiceProvider(serviceUrl, credential))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangeSource"/> class.
        /// </summary>
        /// <param name="folderName">
        /// The folder name.
        /// </param>
        /// <param name="exchangeServiceProvider">
        /// The exchange service provider.
        /// </param>
        protected ExchangeSource(string folderName, IExchangeServiceProvider exchangeServiceProvider)
        {
            if (string.IsNullOrEmpty(folderName))
            {
                throw new ArgumentNullException(nameof(folderName));
            }

            this.folderName = folderName;
            this.exchangeServiceProvider = exchangeServiceProvider;
            // TODO
            //this.exchangeServiceProvider.Credential = credential;
        }

        /// <summary>
        /// Gets the issues.
        /// </summary>
        public IEnumerable<Issue> Issues
        {
            get
            {
                var mailThreads = this.exchangeServiceProvider.GetEmailThreads(this.folderName);
                foreach (var thread in mailThreads)
                {
                    Log.Logger.Information("Mail Thread: {0}, {1}", thread.Id, thread.Topic);
                    yield return new Issue { Id = thread.Id, Topic = thread.Topic, Description = thread.Mails.First().Body, Activity = thread.Mails.Count };
                }
            }
        }
    }
}