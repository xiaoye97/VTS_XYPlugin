using System;

namespace VTS_XYPlugin_Common
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class BindModelAttribute : Attribute
    {
        public string ModelName;

        public BindModelAttribute(string modelName)
        {
            ModelName = modelName;
        }
    }
}