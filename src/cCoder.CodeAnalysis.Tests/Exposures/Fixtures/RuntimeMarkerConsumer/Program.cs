// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Exposures;

Console.WriteLine(typeof(RuntimeUtilityBroker).Assembly.GetName().Name);
Console.WriteLine(typeof(IUtilityBroker).Assembly.GetName().Name);

internal sealed class RuntimeUtilityBroker : IUtilityBroker;