using Microsoft.CodeAnalysis;


namespace Magnet.Syntax
{
    internal class InternalDiagnostics
    {

        internal static DiagnosticSeverity MapReportToSeverity(ReportDiagnostic severity)
        {
            switch (severity)
            {
                case ReportDiagnostic.Hidden:
                    return DiagnosticSeverity.Hidden;
                case ReportDiagnostic.Info:
                    return DiagnosticSeverity.Info;
                case ReportDiagnostic.Warn:
                    return DiagnosticSeverity.Warning;
                case ReportDiagnostic.Error:
                    return DiagnosticSeverity.Error;
                default:
                    return DiagnosticSeverity.Hidden;
            }
        }


        internal static readonly DiagnosticDescriptor InvalidScriptWarning1 = new DiagnosticDescriptor(
                id: "SW001",
                title: "Invalid Script Warning",
                messageFormat: "Script '{0}' is missing the [ScriptAttribute] attribute",
                category: "Inheritance",
                defaultSeverity: DiagnosticSeverity.Warning,
                isEnabledByDefault: true);



        internal static readonly DiagnosticDescriptor InvalidScriptWarning2 = new DiagnosticDescriptor(
                id: "SW002",
                title: "Invalid Script Warning",
                messageFormat: "The script '{0}' does not inherit an 'AbstractScript'",
                category: "Inheritance",
                defaultSeverity: DiagnosticSeverity.Warning,
                isEnabledByDefault: true);


        internal static readonly DiagnosticDescriptor ConfusingGlobalFieldDefinitionWarning = new DiagnosticDescriptor(
                id: "SW003",
                title: "Confusing Global Field Definition Warning",
                messageFormat: "If the field '{1} {0};' is a global variable, mark the [GlobalAttribute] attribute",
                category: "Inheritance",
                defaultSeverity: DiagnosticSeverity.Warning,
                isEnabledByDefault: true);


        internal static readonly DiagnosticDescriptor ConfusingGlobalPropertyDefinitionWarning = new DiagnosticDescriptor(
                id: "SW004",
                title: "Confusing Global Property Definition Warning",
                messageFormat: "If the property '{1} {0};' is a global variable, mark the [GlobalAttribute] attribute",
                category: "Inheritance",
                defaultSeverity: DiagnosticSeverity.Warning,
                isEnabledByDefault: true);


        internal static readonly DiagnosticDescriptor AsyncUsageNotAllowed = new DiagnosticDescriptor(
               id: "SE001",
               title: "Async usage not allowed",
               messageFormat: "async/await usage is prohibited in this context",
               category: "Usage",
               DiagnosticSeverity.Error,
               isEnabledByDefault: true);


        internal static readonly DiagnosticDescriptor IllegalNamespaces = new DiagnosticDescriptor(
               id: "SE002",
               title: "Invalid namespaces of use",
               messageFormat: "Namespace '{0}' has been disabled",
               category: "Usage",
               DiagnosticSeverity.Error,
               isEnabledByDefault: true);

        internal static readonly DiagnosticDescriptor IllegalTypes = new DiagnosticDescriptor(
               id: "SE003",
               title: "Invalid type of use",
               messageFormat: "Typed '{0}' has been disabled",
               category: "Usage",
               DiagnosticSeverity.Error,
               isEnabledByDefault: true);

        internal static readonly DiagnosticDescriptor IllegalConstructor = new DiagnosticDescriptor(
               id: "SE004",
               title: "Illegal implementation of constructors",
               messageFormat: "script '{0}' are not allowed to implement constructors",
               category: "Usage",
               DiagnosticSeverity.Error,
               isEnabledByDefault: true);

        internal static readonly DiagnosticDescriptor IllegalDestructor = new DiagnosticDescriptor(
               id: "SE005",
               title: "Illegal implementation of destructor",
               messageFormat: "script '{0}' are not allowed to implement destructors",
               category: "Usage",
               DiagnosticSeverity.Error,
               isEnabledByDefault: true);


        

    }
}
