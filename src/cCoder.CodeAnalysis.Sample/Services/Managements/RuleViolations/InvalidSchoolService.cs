// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Coordinations.SchoolImports;

namespace cCoder.CodeAnalysis.Sample.Services.Managements.RuleViolations;

internal sealed partial class InvalidSchoolService(ISchoolImportCoordinationService importService, ISchoolImportValidationCoordinationService validationService) : IInvalidSchoolService
{
	public bool CanManage()
=>
	    TryCatch(operation:() => {
			School school = new School();
			return importService.CanImportSchool(school:school) && validationService.CanImportSchool(school:school);
		});}