using System;

namespace ModelCore.Universal
{
    public class CustomPath : Attribute
    {
        public string Path { get; private set; }

        public CustomPath(string path) => Path = path;
    }
        
    public class CustomId : Attribute
    {
        public string Id { get; private set; }

        public CustomId(string id) => Id = id;
    }
        
    public class Info : Attribute 
    {
        public string Value { get; private set; }
        public Info(string value) => Value = value;
    }
}