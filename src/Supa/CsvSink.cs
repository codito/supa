// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CsvSink.cs">
//   Copyright (c) Supa Contributors. All rights reserved.
//   See license.md for license details.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Supa
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// Sink to dump issues in a <c>csv</c> file.
    /// </summary>
    public class CsvSink
    {
        private readonly string csvFilePath;

        private string csvHeader;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvSink"/> class.
        /// </summary>
        /// <param name="csvFilePath">
        /// Path to comma separated file.
        /// </param>
        public CsvSink(string csvFilePath)
        {
            if (string.IsNullOrEmpty(csvFilePath))
            {
                throw new ArgumentNullException("csvFilePath");
            }

            this.csvFilePath = csvFilePath;
        }

        /// <summary>
        /// Serializes a list of <see cref="Issue"/> to string.
        /// </summary>
        /// <param name="issues">
        /// List of <see cref="Issue"/>.
        /// </param>
        /// <returns>
        /// Serialized string.
        /// </returns>
        public string Serialize(IEnumerable<Issue> issues)
        {
            var serializedOutput = new StringBuilder();
            if (issues == null)
            {
                throw new ArgumentNullException("issues");
            }

            serializedOutput.AppendLine(this.GetCsvHeader());
            foreach (var issue in issues)
            {
                serializedOutput.AppendLine(issue.ToString());
            }

            return serializedOutput.ToString();
        }

        /// <summary>
        /// The get csv header.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetCsvHeader()
        {
            if (string.IsNullOrEmpty(this.csvHeader))
            {
                this.csvHeader = string.Join(",", typeof(Issue).GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name));
            }

            return this.csvHeader;
        }
    }
}