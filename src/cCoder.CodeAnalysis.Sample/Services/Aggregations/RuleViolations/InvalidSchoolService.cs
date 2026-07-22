// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Managements.SchoolImports;

namespace cCoder.CodeAnalysis.Sample.Services.Aggregations.RuleViolations;

internal sealed partial class InvalidSchoolService(ISchoolImportManagementService importService, ISchoolImportReadinessManagementService readinessService) : IInvalidSchoolService
{
	public bool CanAggregate()
=>
	    TryCatch(operation:() => {
			bool flag = readinessService.CanImportSchool(school:new School());
			bool flag2 = importService != null;
			return flag && flag2;
		});}