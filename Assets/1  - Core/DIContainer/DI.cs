using System;

namespace DIContainer
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class DI : Attribute 
    {
        public string Id { get; }
        public DI() => Id = "";
        public DI(string id = "") => Id = id;
    }
}