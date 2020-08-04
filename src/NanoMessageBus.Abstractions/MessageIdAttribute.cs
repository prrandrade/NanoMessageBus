namespace NanoMessageBus.Abstractions
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class MessageIdAttribute : Attribute { }
}
