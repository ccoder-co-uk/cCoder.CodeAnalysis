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
    public bool IsInCurrentProject { get; set; } = true;
    public bool IsPublicInterface { get; set; }

    public bool IsUtilityBroker { get; set; }
}
