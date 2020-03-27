using System;
using System.Collections.Generic;
using System.Text;

namespace BlogMLConverter.Enums
{
    public enum ToolAction
    {
        Unknown,
        RemoveComments,
        ExportToWRX,
        QATarget,
        QASource,
        NewWRXWithOnlyFailedPosts
    }
}
