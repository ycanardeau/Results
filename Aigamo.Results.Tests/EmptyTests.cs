namespace Aigamo.Results.Tests;

public class EmptyTests
{
	[Fact]
	public void Ok_discards_value() =>
		Assert.Equal(Result.Ok<Unit, string>(Unit.Default), Result.Ok<int, string>(1).Empty());

	[Fact]
	public void Error_passes_through() =>
		Assert.Equal(Result.Error<Unit, string>("boom"), Result.Error<int, string>("boom").Empty());

	[Fact]
	public async Task TaskSource_discards_value() =>
		Assert.Equal(
			Result.Ok<Unit, string>(Unit.Default),
			await Task.FromResult(Result.Ok<int, string>(1)).Empty()
		);
}
