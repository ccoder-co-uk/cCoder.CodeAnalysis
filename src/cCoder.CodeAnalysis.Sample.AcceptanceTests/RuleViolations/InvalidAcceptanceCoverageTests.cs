// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using FluentAssertions;

namespace cCoder.CodeAnalysis.Sample.AcceptanceTests.RuleViolations;

public sealed class InvalidStudentControllerAcceptanceTests
{
	[Fact]
	public void GetStudentReturnsStudent()
	{
		bool studentExists = true;
		bool actualStudentExists = studentExists;
		actualStudentExists.Should().BeTrue("");
	}
}