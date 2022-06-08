using System;

namespace VTS_XYPlugin_Common
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Class)]
    public class ExScriptAttribute : Attribute
    {
        public string Name;
        public string Description;
        public string Author;
        public string Version;

        public ExScriptAttribute(string name, string description, string author, string version)
        {
            Name = name;
            Description = description;
            Author = author;
            Version = version;
        }
    }
}