// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Processings.Validations;

namespace cCoder.CodeAnalysis.Sample.Services.Foundations.Schools;

internal sealed partial class SchoolService
{
    private static void Validate(params object?[] inputs)
    {
        ValidationRulesEngine.Validate(inputs: inputs);
    }

    private static void ValidateSchoolOnGet(int schoolId) =>
        Validate(inputs: schoolId);

    private static void ValidateSchoolOnAdd(School newSchool) =>
        Validate(inputs: newSchool);

    private static void ValidateSchoolOnUpdate(School updatedSchool) =>
        Validate(inputs: updatedSchool);

    private static void ValidateSchoolOnDelete(int schoolId) =>
        Validate(inputs: schoolId);}