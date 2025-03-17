using System;
using System.Collections.Generic;

namespace ClassicUO.Utility.Collections;

public class Comparer2<T> : Comparer<T>
{
	private readonly Comparison<T> _compareFunction;

	public Comparer2(Comparison<T> comparison)
	{
		_compareFunction = comparison ?? throw new ArgumentNullException("comparison");
	}

	public override int Compare(T arg1, T arg2)
	{
		return _compareFunction(arg1, arg2);
	}
}
