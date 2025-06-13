namespace Buf.Meldingsutveksler.SkjemaVerktoy.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        //var class1 = new Class1();

        var expected = "test";

        var actual = "test2";//class1.GetTest();

        Assert.Equal(expected, actual);
    }
}