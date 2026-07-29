// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Security.Cryptography;

namespace cCoder.CodeAnalysis.Sample.Dependencies;

internal sealed class ExternalStateDependency
{
    private readonly RandomNumberGenerator randomNumberGenerator =
        RandomNumberGenerator.Create();

    public byte[] CreateBytes(int length)
    {
        byte[] bytes = new byte[length];
        randomNumberGenerator.GetBytes(data: bytes);

        return bytes;
    }
}