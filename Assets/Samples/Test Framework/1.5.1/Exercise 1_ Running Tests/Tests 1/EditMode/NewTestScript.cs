using System.Collections;
using MyExercise_1;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class NewTestScript
{
    // A Test behaves as an ordinary method
    [Test]
    public void AddTwoPositiveIntegers()
    {
        Assert.AreEqual(4, MyMath.Add(2, 2));
    }

    [Test]
    public void AddTwoNegativeIntegers()
    {
        Assert.AreEqual(-4, MyMath.Add(-2, -2));
    }

    [Test]
    public void SubtractTwoPositiveIntegers()
    {
        Assert.AreEqual(0, MyMath.Subtract(2, 2));
    }
}
