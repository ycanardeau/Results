namespace Aigamo.Results.Tests;

public class UnitTests
{
	[Fact]
	public void All_units_are_equal()
	{
		Assert.Equal(Unit.Default, default(Unit));
		Assert.True(Unit.Default == default(Unit));
		Assert.False(Unit.Default != default(Unit));
	}

	[Fact]
	public void Ok_of_unit_values_compare_equal()
		=> Assert.Equal(Result.Ok<Unit, string>(Unit.Default), Result.Ok<Unit, string>(default));
}
