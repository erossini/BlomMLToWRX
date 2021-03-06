using System;
using System.Xml.Serialization;

namespace BlogML.Core
{
    public enum BlogPostTypes : short
    {
        [System.Xml.Serialization.XmlEnumAttribute("normal")]
        Normal = 1,
        [System.Xml.Serialization.XmlEnumAttribute("article")]
        Article = 2,
    }
}
