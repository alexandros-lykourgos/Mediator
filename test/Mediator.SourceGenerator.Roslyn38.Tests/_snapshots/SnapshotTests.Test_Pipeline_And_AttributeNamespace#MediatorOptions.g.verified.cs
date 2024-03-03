﻿//HintName: MediatorOptions.g.cs
// <auto-generated>
//     Generated by the Mediator source generator.
// </auto-generated>

namespace Mediator
{
    /// <summary>
    /// Provide options for the Mediator source generator.
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCode("Mediator.SourceGenerator", "3.0.0.0")]
    public sealed class MediatorOptions
    {
        /// <summary>
        /// The namespace in which the Mediator implementation is generated.
        /// </summary>
        public string Namespace { get; set; } = "Mediator";

        /// <summary>
        /// The default lifetime of the services registered in the DI container by
        /// the Mediator source generator.
        /// Singleton by default.
        /// </summary>
        public global::Microsoft.Extensions.DependencyInjection.ServiceLifetime ServiceLifetime { get; set; } =
            global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton;
    }
}