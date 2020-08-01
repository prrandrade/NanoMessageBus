namespace NanoMessageBus.Abstractions
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public class MessageIdAttribute : Attribute { }
}
