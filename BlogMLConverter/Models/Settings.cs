using BlogMLConverter.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlogMLConverter.Models
{
    /// <summary>
    /// Class Settings.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Gets or sets the tool action.
        /// </summary>
        /// <value>The tool action.</value>
        public ToolAction ToolAction { get; set; }

        /// <summary>
        /// Gets or sets the name of the blog ml file.
        /// </summary>
        /// <value>The name of the blog ml file.</value>
        public string BlogMLFileName { get; set; }

        /// <summary>
        /// Gets or sets the name of the WRX file.
        /// </summary>
        /// <value>The name of the WRX file.</value>
        public string WRXFileName { get; set; }

        /// <summary>
        /// Gets or sets the name of the qa source file.
        /// </summary>
        /// <value>The name of the qa source file.</value>
        public string QASourceFileName { get; set; }

        /// <summary>
        /// Gets or sets the name of the qa target file.
        /// </summary>
        /// <value>The name of the qa target file.</value>
        public string QATargetFileName { get; set; }

        /// <summary>
        /// Gets or sets the source base URL.
        /// </summary>
        /// <value>The source base URL.</value>
        public string SourceBaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the target base URL.
        /// </summary>
        /// <value>The target base URL.</value>
        public string TargetBaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the name of the qa report file.
        /// </summary>
        /// <value>The name of the qa report file.</value>
        public string QAReportFileName { get; set; }

        /// <summary>
        /// Gets or sets the sorce image URL.
        /// </summary>
        /// <value>The sorce image URL.</value>
        public string SourceImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the destination image URL.
        /// </summary>
        /// <value>The destination image URL.</value>
        public string DestinationImageUrl { get; set; }
    }
}