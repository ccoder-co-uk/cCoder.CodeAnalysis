namespace cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations;

internal sealed partial class InvalidIdentifierProcessingService : IInvalidIdentifierProcessingService
{
	public int Calculate(int id)
	{
		return TryCatch(operation:() => {
			Validate(inputs:[id]);
			int num = id * 5;
			if (num < 0)
			{
				num = 0;
			}
			return Format(value:num);
		});
	}

	private static int Format(int value) => value;
	private static int Normalize(int value)
	{
		return Format(value);
	}

	private static int Clamp(int value)
	{
		if (value < 0)
			return 0;

		return value;
	}

	private static int FormatWrapped(int value)
	{
		int formattedValue = Format(
			value:value);
		return formattedValue;
	}

	private static int Compare(int value)
	{
		// Deliberate production comment for STXFORMAT010.
		return Format(value:value).CompareTo(value:value);
	}
}
