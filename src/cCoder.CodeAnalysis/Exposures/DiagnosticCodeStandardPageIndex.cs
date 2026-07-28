// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;

namespace cCoder.CodeAnalysis.Exposures;

internal static class DiagnosticCodeStandardPageIndex
{
    private const string TheoryPage =
        "https://github.com/hassanhabib/The-Standard/blob/master/0.%20Introduction/0.0%20The%20Theory.md";

    private const string ModelingPage =
        "https://github.com/hassanhabib/The-Standard/blob/master/0.%20Introduction/0.1%20Purposing,%20Modeling%20&%20Simulation.md";

    private const string PrinciplesPage =
        "https://github.com/hassanhabib/The-Standard/blob/master/0.%20Introduction/0.2%20Principles.md";

    private const string BrokersPage =
        "https://github.com/hassanhabib/The-Standard/blob/master/1.%20Brokers/1.%20Brokers.md";

    private const string ServicesPage =
        "https://github.com/hassanhabib/The-Standard/blob/master/2.%20Services/2.%20Services.md";

    private const string FoundationsPage =
        "https://github.com/hassanhabib/The-Standard/blob/master/2.%20Services/2.1%20Foundations/2.1%20Foundations.md";

    private const string ProcessingsPage =
        "https://github.com/hassanhabib/The-Standard/blob/master/2.%20Services/2.2%20Processings/2.2%20Processings.md";

    private const string OrchestrationsPage =
        "https://github.com/hassanhabib/The-Standard/blob/master/2.%20Services/2.3%20Orchestrations/2.3%20Orchestrations.md";

    private const string AggregationsPage =
        "https://github.com/hassanhabib/The-Standard/blob/master/2.%20Services/2.4%20Aggregations/2.4%20Aggregations.md";

    private const string ExposersPage =
        "https://github.com/hassanhabib/The-Standard/blob/master/3.%20Exposers/3.%20Exposers.md";

    private const string RestfulApisPage =
        "https://github.com/hassanhabib/The-Standard/blob/master/3.%20Exposers/3.1%20Communication%20Protocols/3.1.1%20RESTful%20APIs/3.1.1%20RESTful%20APIs.md";

    private const string WebApplicationsPage =
        "https://github.com/hassanhabib/The-Standard/blob/master/3.%20Exposers/3.2%20User%20Interfaces/3.2.1%20Web%20Applications/3.2.1%20Web%20Applications.md";

    private static readonly DiagnosticCodeStandardPage[] diagnosticCodeStandardPages =
    [
        new(diagnosticCode: "STX0001", standardPageUri: $"{TheoryPage}#002-tri-nature"),
        new(diagnosticCode: "STX0002", standardPageUri: $"{ModelingPage}#01200-data-carrier-models"),
        new(diagnosticCode: "STX0003", standardPageUri: $"{ProcessingsPage}#22201-pass-through"),
        new(diagnosticCode: "STX0004", standardPageUri: $"{ServicesPage}#2025-flow-forward"),
        new(diagnosticCode: "STX0005", standardPageUri: $"{ServicesPage}#2020-do-or-delegate"),
        new(diagnosticCode: "STX0006", standardPageUri: $"{ServicesPage}#204-class-visibility-and-exposure-control"),
        new(diagnosticCode: "STX0007", standardPageUri: $"{ServicesPage}#2023-same-or-primitives-io-model"),
        new(diagnosticCode: "STX0008", standardPageUri: $"{FoundationsPage}#2131-validation"),
        new(diagnosticCode: "STX0009", standardPageUri: $"{FoundationsPage}#21321-exceptions-mappings"),
        new(diagnosticCode: "STX0010", standardPageUri: $"{FoundationsPage}#21321-exceptions-mappings"),
        new(diagnosticCode: "STX0011", standardPageUri: $"{FoundationsPage}#2131-validation"),
        new(diagnosticCode: "STX0012", standardPageUri: $"{FoundationsPage}#213112-rules--validations-collector"),
        new(
            diagnosticCode: "STX0013",
            firstStandardPageUri: $"{ServicesPage}#204-class-visibility-and-exposure-control",
            secondStandardPageUri: $"{FoundationsPage}#2130-abstraction"
        ),
        new(
            diagnosticCode: "STX0014",
            firstStandardPageUri: $"{BrokersPage}#192-file-name-conventions",
            secondStandardPageUri: $"{ServicesPage}#204-class-visibility-and-exposure-control"
        ),
        new(
            diagnosticCode: "STX0015",
            firstStandardPageUri: $"{ServicesPage}#204-class-visibility-and-exposure-control",
            secondStandardPageUri: $"{FoundationsPage}#2130-abstraction"
        ),
        new(diagnosticCode: "STX0016", standardPageUri: $"{FoundationsPage}#2122-business-language"),
        new(diagnosticCode: "STX0017", standardPageUri: $"{ServicesPage}#2034-naming-conventions"),
        new(diagnosticCode: "STX0018", standardPageUri: $"{ServicesPage}#2034-naming-conventions"),
        new(diagnosticCode: "STX0019", standardPageUri: $"{ServicesPage}#2034-naming-conventions"),
        new(diagnosticCode: "STX0020", standardPageUri: $"{ServicesPage}#2034-naming-conventions"),
        new(diagnosticCode: "STX0021", standardPageUri: $"{ServicesPage}#2034-naming-conventions"),
        new(
            diagnosticCode: "STX0022",
            firstStandardPageUri: $"{ServicesPage}#2034-naming-conventions",
            secondStandardPageUri: $"{FoundationsPage}#2122-business-language"
        ),
        new(diagnosticCode: "STX0023", standardPageUri: $"{FoundationsPage}#2131-validation"),
        new(diagnosticCode: "STXAPP001", standardPageUri: $"{ModelingPage}#01202-configuration-models"),
        new(diagnosticCode: "STXAPP002", standardPageUri: $"{ModelingPage}#01202-configuration-models"),
        new(
            diagnosticCode: "STXAPP003",
            firstStandardPageUri: $"{ModelingPage}#01202-configuration-models",
            secondStandardPageUri: $"{ServicesPage}#204-class-visibility-and-exposure-control"
        ),
        new(
            diagnosticCode: "STXAPP004",
            firstStandardPageUri: $"{ModelingPage}#01202-configuration-models",
            secondStandardPageUri: $"{ServicesPage}#204-class-visibility-and-exposure-control"
        ),
        new(
            diagnosticCode: "STXAPP006",
            firstStandardPageUri: $"{ModelingPage}#01202-configuration-models",
            secondStandardPageUri: $"{ExposersPage}#3011-user-interfaces"
        ),
        new(
            diagnosticCode: "STXAPP007",
            firstStandardPageUri: $"{ModelingPage}#01202-configuration-models",
            secondStandardPageUri: $"{ModelingPage}#012012-exposure-models-exposers"
        ),
        new(
            diagnosticCode: "STXAPP008",
            firstStandardPageUri: $"{ModelingPage}#01202-configuration-models",
            secondStandardPageUri: $"{PrinciplesPage}#0200-simplicity"
        ),
        new(
            diagnosticCode: "STXAPP009",
            firstStandardPageUri: $"{ModelingPage}#01202-configuration-models",
            secondStandardPageUri: $"{ServicesPage}#201-services-types"
        ),
        new(
            diagnosticCode: "STXAPP010",
            firstStandardPageUri: $"{ModelingPage}#01202-configuration-models",
            secondStandardPageUri: $"{ServicesPage}#204-class-visibility-and-exposure-control"
        ),
        new(
            diagnosticCode: "STXAPP011",
            firstStandardPageUri: $"{ModelingPage}#01202-configuration-models",
            secondStandardPageUri: $"{PrinciplesPage}#0200-simplicity"
        ),
        new(
            diagnosticCode: "STXAPP012",
            firstStandardPageUri: $"{ModelingPage}#01202-configuration-models",
            secondStandardPageUri: $"{PrinciplesPage}#0200-simplicity"
        ),
        new(
            diagnosticCode: "STXAPP013",
            firstStandardPageUri: $"{ModelingPage}#01202-configuration-models",
            secondStandardPageUri: $"{ServicesPage}#204-class-visibility-and-exposure-control"
        ),
        new(
            diagnosticCode: "STXAPP014",
            firstStandardPageUri: $"{ModelingPage}#01202-configuration-models",
            secondStandardPageUri: $"{PrinciplesPage}#0200-simplicity"
        ),
        new(
            diagnosticCode: "STXA001",
            firstStandardPageUri: $"{AggregationsPage}#2420-no-dependency-limitation",
            secondStandardPageUri: $"{AggregationsPage}#2426-pure-dependency-contracts"
        ),
        new(diagnosticCode: "STXA002", standardPageUri: AggregationsPage),
        new(diagnosticCode: "STXAPI001", standardPageUri: $"{RestfulApisPage}#31122-single-dependency"),
        new(diagnosticCode: "STXAPI002", standardPageUri: $"{RestfulApisPage}#31123-single-contract"),
        new(diagnosticCode: "STXAPI003", standardPageUri: $"{RestfulApisPage}#3113-organization"),
        new(diagnosticCode: "STXAPI004", standardPageUri: $"{RestfulApisPage}#31120-language"),
        new(
            diagnosticCode: "STXB001",
            firstStandardPageUri: $"{BrokersPage}#127-up--sideways",
            secondStandardPageUri: $"{BrokersPage}#128-one-resource-one-broker"
        ),
        new(diagnosticCode: "STXB002", standardPageUri: $"{BrokersPage}#121-no-flow-control"),
        new(diagnosticCode: "STXB003", standardPageUri: $"{BrokersPage}#121-no-flow-control"),
        new(diagnosticCode: "STXB004", standardPageUri: $"{BrokersPage}#120-implements-a-local-interface"),
        new(diagnosticCode: "STXB005", standardPageUri: $"{BrokersPage}#122-no-exception-handling"),
        new(diagnosticCode: "STXB006", standardPageUri: $"{BrokersPage}#127-up--sideways"),
        new(
            diagnosticCode: "STXB007",
            firstStandardPageUri: $"{BrokersPage}#126-language",
            secondStandardPageUri: $"{FoundationsPage}#2122-business-language"
        ),
        new(
            diagnosticCode: "STXC001",
            firstStandardPageUri: $"{ServicesPage}#2021-two-three-florance-pattern",
            secondStandardPageUri: $"{OrchestrationsPage}#2340-variants-levels"
        ),
        new(diagnosticCode: "STXC002", standardPageUri: $"{OrchestrationsPage}#234-variations"),
        new(diagnosticCode: "STXD001", standardPageUri: $"{TheoryPage}#0021-dependency"),
        new(diagnosticCode: "STXD002", standardPageUri: $"{TheoryPage}#0021-dependency"),
        new(diagnosticCode: "STXE001", standardPageUri: $"{ExposersPage}#3000-pure-mapping"),
        new(diagnosticCode: "STXE002", standardPageUri: $"{ExposersPage}#3000-pure-mapping"),
        new(diagnosticCode: "STXE003", standardPageUri: $"{ExposersPage}#302-single-point-of-contact"),
        new(diagnosticCode: "STXE004", standardPageUri: $"{ExposersPage}#3000-pure-mapping"),
        new(diagnosticCode: "STXE005", standardPageUri: $"{ExposersPage}#3000-pure-mapping"),
        new(diagnosticCode: "STXEX001", standardPageUri: $"{FoundationsPage}#21321-exceptions-mappings"),
        new(diagnosticCode: "STXEX002", standardPageUri: $"{FoundationsPage}#21321-exceptions-mappings"),
        new(diagnosticCode: "STXEX003", standardPageUri: $"{FoundationsPage}#21321-exceptions-mappings"),
        new(
            diagnosticCode: "STXF001",
            firstStandardPageUri: $"{FoundationsPage}#2120-pure-primitive",
            secondStandardPageUri: $"{FoundationsPage}#2121-single-entity-integration"
        ),
        new(diagnosticCode: "STXF002", standardPageUri: $"{FoundationsPage}#2121-single-entity-integration"),
        new(diagnosticCode: "STXF003", standardPageUri: $"{FoundationsPage}#2120-pure-primitive"),
        new(
            diagnosticCode: "STXFORMAT001",
            firstStandardPageUri: $"{PrinciplesPage}#029-readability-over-optimization",
            secondStandardPageUri: $"{BrokersPage}#151-asynchronization-abstraction"
        ),
        new(
            diagnosticCode: "STXFORMAT002",
            firstStandardPageUri: $"{PrinciplesPage}#029-readability-over-optimization",
            secondStandardPageUri: $"{BrokersPage}#151-asynchronization-abstraction"
        ),
        new(
            diagnosticCode: "STXFORMAT003",
            firstStandardPageUri: $"{BrokersPage}#151-asynchronization-abstraction",
            secondStandardPageUri: $"{FoundationsPage}#213120-testing-structural-validations"
        ),
        new(
            diagnosticCode: "STXFORMAT004",
            firstStandardPageUri: $"{PrinciplesPage}#029-readability-over-optimization",
            secondStandardPageUri: $"{BrokersPage}#151-asynchronization-abstraction"
        ),
        new(
            diagnosticCode: "STXFORMAT005",
            firstStandardPageUri: $"{PrinciplesPage}#023-level-0",
            secondStandardPageUri: $"{FoundationsPage}#213112-rules--validations-collector"
        ),
        new(
            diagnosticCode: "STXFORMAT006",
            firstStandardPageUri: $"{PrinciplesPage}#029-readability-over-optimization",
            secondStandardPageUri: $"{BrokersPage}#151-asynchronization-abstraction"
        ),
        new(
            diagnosticCode: "STXFORMAT007",
            firstStandardPageUri: $"{BrokersPage}#151-asynchronization-abstraction",
            secondStandardPageUri: $"{FoundationsPage}#213120-testing-structural-validations"
        ),
        new(
            diagnosticCode: "STXFORMAT008",
            firstStandardPageUri: $"{BrokersPage}#151-asynchronization-abstraction",
            secondStandardPageUri: $"{FoundationsPage}#213120-testing-structural-validations"
        ),
        new(
            diagnosticCode: "STXFORMAT009",
            firstStandardPageUri: $"{BrokersPage}#151-asynchronization-abstraction",
            secondStandardPageUri: $"{ServicesPage}#204-class-visibility-and-exposure-control"
        ),
        new(
            diagnosticCode: "STXFORMAT010",
            firstStandardPageUri: $"{BrokersPage}#151-asynchronization-abstraction",
            secondStandardPageUri: $"{ServicesPage}#204-class-visibility-and-exposure-control"
        ),
        new(
            diagnosticCode: "STXFORMAT011",
            firstStandardPageUri: $"{PrinciplesPage}#024-open-code",
            secondStandardPageUri: $"{PrinciplesPage}#0210-last-day"
        ),
        new(
            diagnosticCode: "STXFORMAT012",
            firstStandardPageUri: $"{PrinciplesPage}#029-readability-over-optimization",
            secondStandardPageUri: $"{BrokersPage}#151-asynchronization-abstraction"
        ),
        new(
            diagnosticCode: "STXFORMAT013",
            firstStandardPageUri: $"{PrinciplesPage}#029-readability-over-optimization",
            secondStandardPageUri: $"{BrokersPage}#151-asynchronization-abstraction"
        ),
        new(
            diagnosticCode: "STXMG001",
            firstStandardPageUri: $"{ServicesPage}#2021-two-three-florance-pattern",
            secondStandardPageUri: $"{OrchestrationsPage}#2340-variants-levels"
        ),
        new(diagnosticCode: "STXMG002", standardPageUri: $"{OrchestrationsPage}#234-variations"),
        new(diagnosticCode: "STXM001", standardPageUri: $"{ModelingPage}#01200-data-carrier-models"),
        new(diagnosticCode: "STXM002", standardPageUri: $"{ModelingPage}#01200-data-carrier-models"),
        new(
            diagnosticCode: "STXM003",
            firstStandardPageUri: $"{FoundationsPage}#2131-validation",
            secondStandardPageUri: $"{FoundationsPage}#213112-rules--validations-collector"
        ),
        new(
            diagnosticCode: "STXO001",
            firstStandardPageUri: $"{OrchestrationsPage}#23210-dependency-balance-florance-pattern",
            secondStandardPageUri: $"{OrchestrationsPage}#23211-two-three"
        ),
        new(diagnosticCode: "STXO002", standardPageUri: $"{OrchestrationsPage}#23202-class-level-language"),
        new(diagnosticCode: "STXP001", standardPageUri: $"{ProcessingsPage}#2222-one-foundation"),
        new(diagnosticCode: "STXP002", standardPageUri: $"{ProcessingsPage}#22202-class-level-language"),
        new(diagnosticCode: "STXP003", standardPageUri: $"{ProcessingsPage}#22202-class-level-language"),
        new(
            diagnosticCode: "STXSTRUCT001",
            firstStandardPageUri: $"{BrokersPage}#13-organization",
            secondStandardPageUri: $"{RestfulApisPage}#3113-organization",
            thirdStandardPageUri: $"{WebApplicationsPage}#321204-organization"
        ),
        new(
            diagnosticCode: "STXTEST001",
            firstStandardPageUri: $"{PrinciplesPage}#0200-simplicity",
            secondStandardPageUri: $"{RestfulApisPage}#31150-unit-tests"
        ),
        new(
            diagnosticCode: "STXTEST002",
            firstStandardPageUri: $"{PrinciplesPage}#020011-vertical-entanglement",
            secondStandardPageUri: $"{RestfulApisPage}#31150-unit-tests"
        ),
        new(
            diagnosticCode: "STXTEST003",
            firstStandardPageUri: $"{FoundationsPage}#213120-testing-structural-validations",
            secondStandardPageUri: $"{RestfulApisPage}#31150-unit-tests"
        ),
        new(
            diagnosticCode: "STXTEST004",
            firstStandardPageUri: $"{FoundationsPage}#213120-testing-structural-validations",
            secondStandardPageUri: $"{RestfulApisPage}#31150-unit-tests"
        ),
        new(
            diagnosticCode: "STXTEST005",
            firstStandardPageUri: $"{FoundationsPage}#213120-testing-structural-validations",
            secondStandardPageUri: $"{RestfulApisPage}#31150-unit-tests"
        ),
        new(diagnosticCode: "STXTEST006", standardPageUri: $"{RestfulApisPage}#31151-acceptance-tests"),
    ];

    public static IEnumerable<DiagnosticCodeStandardPage> GetDiagnosticCodeStandardPages() =>
        diagnosticCodeStandardPages;
}