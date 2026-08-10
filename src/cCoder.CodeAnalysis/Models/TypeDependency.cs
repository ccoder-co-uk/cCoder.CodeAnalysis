// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
#nullable disable
namespace cCoder.CodeAnalysis.Models;

public sealed class TypeDependency
{
    public string TypeName { get; set; }
    public StandardElementType StandardElementType { get; set; }
    public bool IsConfigurationModel { get; set; }

    public bool IsUtilityBroker
    {
        get
        {
            if (StandardElementType != StandardElementType.Broker)
            {
                return false;
            }

            string shortName = TypeName.Split(separator: ['.']).Last();

            return shortName is "LoggingBroker" or "ILoggingBroker"
                || shortName.StartsWith(value: "LoggingBroker<", comparisonType: StringComparison.Ordinal)
                || shortName.StartsWith(value: "ILoggingBroker<", comparisonType: StringComparison.Ordinal);
        }
    }
}
