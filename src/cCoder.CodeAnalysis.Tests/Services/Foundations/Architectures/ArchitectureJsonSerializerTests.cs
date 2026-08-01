// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Text.Json;
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Foundations.Architectures;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.Services.Foundations.Architectures;

public sealed class ArchitectureJsonSerializerTests
{
    [Fact]
    public void SerializeShouldWriteBrowserSafeProjectAndTypeReferences()
    {
        Architecture architecture = new()
        {
            Project = new ProjectMetadata
            {
                Id = "cCoder.Students",
                Name = "cCoder.Students",
                AssemblyName = "cCoder.Students",
            },
            Classes =
            [
                new Class
                {
                    Name = "cCoder.Students.StudentService",
                    StandardElementType = StandardElementType.FoundationService,
                    LineNumber = 12,
                    IsPublic = false,
                    Kind = ArchitectureTypeKind.Class,
                    BaseType = new TypeReference
                    {
                        Id = "cCoder.Framework:cCoder.Framework.Service",
                        FullName = "cCoder.Framework.Service",
                        Name = "Service",
                        Namespace = "cCoder.Framework",
                        AssemblyName = "cCoder.Framework",
                        Kind = ArchitectureTypeKind.Class,
                        IsInCurrentProject = false,
                        StandardElementType = StandardElementType.Dependency,
                    },
                    Interfaces =
                    [
                        new TypeReference
                        {
                            Id = "cCoder.Students:cCoder.Students.IStudentService",
                            FullName = "cCoder.Students.IStudentService",
                            Name = "IStudentService",
                            Namespace = "cCoder.Students",
                            AssemblyName = "cCoder.Students",
                            Kind = ArchitectureTypeKind.Interface,
                            IsInCurrentProject = true,
                            StandardElementType = StandardElementType.FoundationService,
                        },
                    ],
                },
            ],
        };

        string json = ArchitectureJsonSerializer.Serialize(architecture: architecture);
        using JsonDocument document = JsonDocument.Parse(json: json);
        JsonElement root = document.RootElement;
        JsonElement element = root.GetProperty(propertyName: "Classes")[0];
        JsonElement contract = element.GetProperty(propertyName: "Interfaces")[0];

        root.GetProperty(propertyName: "SchemaVersion").GetInt32().Should().Be(2, "");
        root.GetProperty(propertyName: "Project").GetProperty(propertyName: "AssemblyName")
            .GetString().Should().Be("cCoder.Students", "");
        element.GetProperty(propertyName: "LineNumber").GetInt32().Should().Be(12, "");
        element.GetProperty(propertyName: "IsPublic").GetBoolean().Should().BeFalse("");
        element.GetProperty(propertyName: "Kind").GetString().Should().Be("Class", "");
        element.GetProperty(propertyName: "BaseType").GetProperty(propertyName: "IsInCurrentProject")
            .GetBoolean().Should().BeFalse("");
        contract.GetProperty(propertyName: "Kind").GetString().Should().Be("Interface", "");
        contract.GetProperty(propertyName: "Id").GetString()
            .Should().Be("cCoder.Students:cCoder.Students.IStudentService", "");
    }

    [Fact]
    public void SerializeAndDeserializeShouldRoundTripAdditiveContractAndOmitAnalysisState()
    {
        Architecture expected = new()
        {
            Project = new ProjectMetadata
            {
                Id = "Students",
                Name = "Students",
                AssemblyName = "Students",
            },
            Classes =
            [
                new Class
                {
                    Name = "Students.Student",
                    LineNumber = 5,
                    IsPublic = true,
                    Methods =
                    [
                        new Method
                        {
                            Name = "GetStudent",
                            HasTryCatch = true,
                            IncomingExceptionTypes = ["Students.StudentServiceException"],
                            HttpResponses =
                            [
                                new HttpResponse
                                {
                                    StatusCode = 200,
                                    HasBody = true,
                                },
                            ],
                        },
                    ],
                    Interfaces =
                    [
                        new TypeReference
                        {
                            Id = "Students:Students.IStudent",
                            FullName = "Students.IStudent",
                            Name = "IStudent",
                            Namespace = "Students",
                            AssemblyName = "Students",
                            Kind = ArchitectureTypeKind.Interface,
                            IsInCurrentProject = true,
                        },
                    ],
                    AnalysisFilePath = "C:\\private\\Student.cs",
                    AnalysisSourceCode = "internal source",
                    AnalysisImplementedInterfaces = ["Students.IStudent"],
                },
            ],
        };

        string json = ArchitectureJsonSerializer.Serialize(architecture: expected);
        Architecture actual = ArchitectureJsonSerializer.Deserialize(json: json);

        actual.SchemaVersion.Should().Be(2, "");
        actual.Project.AssemblyName.Should().Be("Students", "");
        actual.Classes.Should().ContainSingle("");
        actual.Classes[0].Interfaces.Should().ContainSingle("");
        actual.Classes[0].Interfaces[0].Kind.Should().Be(ArchitectureTypeKind.Interface, "");
        actual.Classes[0].Methods.Single().HttpResponses.Single().HasBody.Should().BeTrue("");
        actual.Classes[0].Methods.Single().HasTryCatch.Should().BeTrue("");
        actual.Classes[0].Methods.Single().IncomingExceptionTypes.Should()
            .ContainSingle().Which.Should().Be("Students.StudentServiceException", "");
        json.Should().NotContain("AnalysisFilePath", "");
        json.Should().NotContain("private", "");
        json.Should().NotContain("AnalysisSourceCode", "");
        json.Should().NotContain("AnalysisImplementedInterfaces", "");
    }

    [Fact]
    public void DeserializeShouldApplySafeDefaultsToLegacyArchitectureJson()
    {
        const string legacyJson =
            """
            {
              "Classes": [
                {
                  "Name": "Students.Student",
                  "StandardElementType": "Model",
                  "Properties": [],
                  "Methods": []
                }
              ],
              "Links": [],
              "AnalysisItems": []
            }
            """;

        Architecture architecture = ArchitectureJsonSerializer.Deserialize(json: legacyJson);
        Class element = architecture.Classes.Single();

        architecture.SchemaVersion.Should().Be(2, "");
        architecture.Project.Should().NotBeNull("");
        architecture.Project.Id.Should().BeEmpty("");
        element.Kind.Should().Be(ArchitectureTypeKind.Class, "");
        element.LineNumber.Should().Be(0, "");
        element.IsPublic.Should().BeFalse("");
        element.BaseType.Should().BeNull("");
        element.Interfaces.Should().NotBeNull("").And.BeEmpty("");
    }
}