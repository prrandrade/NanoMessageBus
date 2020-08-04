namespace NanoMessageBus.Abstractions.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class MessageIdAttribute : Attribute { }
}
