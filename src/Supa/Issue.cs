// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Issue.cs">
//   Copyright (c) Supa Contributors. All rights reserved.
//   See license.md for license details.
// </copyright>
// <summary>
//   Defines the IssueSource type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Supa
{
    using System;

    public enum IssueSource
    {
        Internal,

        External
    }

    public class Issue
    {
        public string Id { get; set; }

        public string Topic { get; set; }

        public string Description { get; set; }

        ////public IssueSource Source { get; set; }

        ////public string SourceDetail { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime LastUpdatedDate { get; set; }

        public override string ToString()
        {
            return string.Format("{0}, {1}, {2}", this.Id, this.Topic, this.LastUpdatedDate);
        }

        public int Activity { get; set; }
    }
}