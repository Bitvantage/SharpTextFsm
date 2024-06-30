/*
   Bitvantage.SharpTextFsm
   Copyright (C) 2024 Michael Crino
   
   This program is free software: you can redistribute it and/or modify
   it under the terms of the GNU Affero General Public License as published by
   the Free Software Foundation, either version 3 of the License, or
   (at your option) any later version.
   
   This program is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU Affero General Public License for more details.
   
   You should have received a copy of the GNU Affero General Public License
   along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using Bitvantage.SharpTextFsm;
using Bitvantage.SharpTextFsm.Exceptions;

namespace Test;

internal class TextFsmExtendedTests
{
    [Test(Description = "")]
    public void TestLineBreaks01()
    {
        var template = new Template("""
            Value List,Filldown boo (on.)
            Value Required test (on[0-9])
            Value hoo (on.)




            Start
             ^$boo -> Continue
             ^$test -> Continue
             ^$hoo -> Next.Record
             
            EOF
            """);

        var data = """
            one
            on0
            on1
            bob
            onx
            ony
            onk
            clear
            ong
            onf
            """;

        var result = template.Parse(data);

        Assert.That(result.Count, Is.EqualTo(2));

        Assert.That(result[0]["boo"], Is.EqualTo(new[] { "one", "on0" }));
        Assert.That(result[0]["hoo"], Is.EqualTo("on0"));
        Assert.That(result[0]["test"], Is.EqualTo("on0"));

        Assert.That(result[1]["boo"], Is.EqualTo(new[] { "one", "on0", "on1" }));
        Assert.That(result[1]["hoo"], Is.EqualTo("on1"));
        Assert.That(result[1]["test"], Is.EqualTo("on1"));
    }

    [Test(Description = "")]
    public void TestLineBreaks02()
    {
        var template = new Template("""
            Value List,Filldown boo (on.)
            Value Required test (on[0-9])
            Value hoo (on.)

            Start
             ^$boo -> Continue
             ^$test -> Continue
             ^$hoo -> Next.Record
             
            
            
            State2
             ^.*
             
            EOF
            """);

        var data = """
            one
            on0
            on1
            bob
            onx
            ony
            onk
            clear
            ong
            onf
            """;

        var result = template.Parse(data);

        Assert.That(result.Count, Is.EqualTo(2));

        Assert.That(result[0]["boo"], Is.EqualTo(new[] { "one", "on0" }));
        Assert.That(result[0]["hoo"], Is.EqualTo("on0"));
        Assert.That(result[0]["test"], Is.EqualTo("on0"));

        Assert.That(result[1]["boo"], Is.EqualTo(new[] { "one", "on0", "on1" }));
        Assert.That(result[1]["hoo"], Is.EqualTo("on1"));
        Assert.That(result[1]["test"], Is.EqualTo("on1"));
    }

    [Test]
    public void TestList02()
    {
        // a Value section is required
        // there must be a break between the value section and everything else

        var template = new Template("""
            Value List,Filldown boo (on.)
            Value Required test (on[0-9])
            Value hoo (on.)

            Start
             ^$boo -> Continue
             ^$test -> Continue
             ^$hoo -> Next.Record
             
            EOF
            """);

        var data = """
            one
            on0
            on1
            bob
            onx
            ony
            onk
            clear
            ong
            onf
            """;

        var result = template.Parse(data);

        Assert.That(result.Count, Is.EqualTo(2));

        Assert.That(result[0]["boo"], Is.EqualTo(new[] { "one", "on0" }));
        Assert.That(result[0]["hoo"], Is.EqualTo("on0"));
        Assert.That(result[0]["test"], Is.EqualTo("on0"));

        Assert.That(result[1]["boo"], Is.EqualTo(new[] { "one", "on0", "on1" }));
        Assert.That(result[1]["hoo"], Is.EqualTo("on1"));
        Assert.That(result[1]["test"], Is.EqualTo("on1"));
    }

    [Test]
    public void UndeclaredValue01()
    {
        Assert.Throws<TemplateParseException>(() => new Template("""
            Value boo (on.)
            Value hoo (on.)

            Start
             ^$boX -> Continue
             ^$hoo -> Next.Record
             
            EOF
            """));
    }

    [Test]
    public void UndeclaredValue02()
    {
        Assert.Throws<TemplateParseException>(() => new Template("""
            Value boo (on.)
            Value hoo (on.)

            Start
             ^${boX} -> Continue
             ^$hoo -> Next.Record
             
            EOF
            """));
    }

    [Test]
    public void DoubleDollarEscape01()
    {
        // a Value section is required
        // there must be a break between the value section and everything else

        var template = new Template("""
            Value boo (on.)
            Value hoo (on.)

            Start
             ^$boo -> Continue
             ^\$$$hoo -> Next.Record
             
            EOF
            """);

        var data = """
            one
            $on0
            """;

        var result = template.Parse(data);

        Assert.That(result.Count, Is.EqualTo(1));

        Assert.That(result[0]["boo"], Is.EqualTo("one"));
        Assert.That(result[0]["hoo"], Is.EqualTo("on0"));
    }

    [Test]
    public void UnmatchedHandlingNull01()
    {
        var template = new Template("""
            Value boo (one)
            Value hoo (two)

            Start
             ^${boo} -> Record
             ^${hoo} -> Record
             
            """, new TemplateOptions(UnmatchedHandling.Null));

        var data = """
            one
            two
            """;

        var result = template.Parse(data);

        Assert.That(result.Count, Is.EqualTo(2));

        Assert.That(result[0]["boo"], Is.EqualTo("one"));
        Assert.That(result[0]["hoo"], Is.EqualTo(null));

        Assert.That(result[1]["boo"], Is.EqualTo(null));
        Assert.That(result[1]["hoo"], Is.EqualTo("two"));
    }

    [Test]
    public void UnmatchedHandlingSkip01()
    {
        var template = new Template("""
            Value boo (one)
            Value hoo (two)

            Start
             ^${boo} -> Record
             ^${hoo} -> Record
             
            """, new TemplateOptions(UnmatchedHandling.Skip));

        var data = """
            one
            two
            """;

        var result = template.Parse(data);

        Assert.That(result.Count, Is.EqualTo(2));

        Assert.That(result[0]["boo"], Is.EqualTo("one"));
        Assert.That(result[0].ContainsKey("hoo"), Is.EqualTo(false));

        Assert.That(result[1].ContainsKey("boo"), Is.EqualTo(false));
        Assert.That(result[1]["hoo"], Is.EqualTo("two"));
    }

    [Test]
    public void UnmatchedHandlingEmpty01()
    {
        var template = new Template("""
            Value boo (one)
            Value hoo (two)

            Start
             ^${boo} -> Record
             ^${hoo} -> Record
             
            """, new TemplateOptions(UnmatchedHandling.Empty));

        var data = """
            one
            two
            """;

        var result = template.Parse(data);

        Assert.That(result.Count, Is.EqualTo(2));

        Assert.That(result[0]["boo"], Is.EqualTo("one"));
        Assert.That(result[0]["hoo"], Is.EqualTo(string.Empty));

        Assert.That(result[1]["boo"], Is.EqualTo(string.Empty));
        Assert.That(result[1]["hoo"], Is.EqualTo("two"));
    }

    [Test]
    public void Filldown01()
    {
        var template = new Template("""
            Value boo (one)
            Value hoo (two)
            Value Filldown test (\d+)

            Start
             ^${boo} -> Record
             ^${hoo} -> Record
             ^F${test}
             
            """);

        var data = """
            F37
            one
            two
            F99
            F42
            one
            F31
            one
            F32
            two
            F33
            two
            F34
            """;

        var result = template.Parse(data);

        Assert.That(result[0]["boo"], Is.EqualTo("one"));
        Assert.That(result[0]["hoo"], Is.EqualTo(string.Empty));
        Assert.That(result[0]["test"], Is.EqualTo("37"));

        Assert.That(result[1]["boo"], Is.EqualTo(string.Empty));
        Assert.That(result[1]["hoo"], Is.EqualTo("two"));
        Assert.That(result[1]["test"], Is.EqualTo("37"));

        Assert.That(result[2]["boo"], Is.EqualTo("one"));
        Assert.That(result[2]["hoo"], Is.EqualTo(string.Empty));
        Assert.That(result[2]["test"], Is.EqualTo("42"));

        Assert.That(result[3]["boo"], Is.EqualTo("one"));
        Assert.That(result[3]["hoo"], Is.EqualTo(string.Empty));
        Assert.That(result[3]["test"], Is.EqualTo("31"));

        Assert.That(result[4]["boo"], Is.EqualTo(string.Empty));
        Assert.That(result[4]["hoo"], Is.EqualTo("two"));
        Assert.That(result[4]["test"], Is.EqualTo("32"));

        Assert.That(result[5]["boo"], Is.EqualTo(string.Empty));
        Assert.That(result[5]["hoo"], Is.EqualTo("two"));
        Assert.That(result[5]["test"], Is.EqualTo("33"));

        Assert.That(result[6]["boo"], Is.EqualTo(string.Empty));
        Assert.That(result[6]["hoo"], Is.EqualTo(string.Empty));
        Assert.That(result[6]["test"], Is.EqualTo("34"));

        Assert.That(result.Count, Is.EqualTo(7));
    }

    [Test]
    public void Filldown02()
    {
        var template = new Template("""
            Value boo (one)
            Value hoo (two)
            Value Filldown,List test (\d+)

            Start
             ^${boo} -> Record
             ^${hoo} -> Record
             ^F${test}
            """);

        var data = """
            F37
            one
            two
            F99
            F42
            one
            F31
            one
            F32
            two
            F33
            two
            F34
            """;

        var result = template.Parse(data);

        Assert.That(result.Count, Is.EqualTo(7));

        Assert.That(result[0]["boo"], Is.EqualTo("one"));
        Assert.That(result[0]["hoo"], Is.EqualTo(string.Empty));
        Assert.That(result[0]["test"], Is.EqualTo(new[] { "37" }));

        Assert.That(result[1]["boo"], Is.EqualTo(string.Empty));
        Assert.That(result[1]["hoo"], Is.EqualTo("two"));
        Assert.That(result[1]["test"], Is.EqualTo(new[] { "37" }));

        Assert.That(result[2]["boo"], Is.EqualTo("one"));
        Assert.That(result[2]["hoo"], Is.EqualTo(string.Empty));
        Assert.That(result[2]["test"], Is.EqualTo(new[] { "37", "99", "42" }));

        Assert.That(result[3]["boo"], Is.EqualTo("one"));
        Assert.That(result[3]["hoo"], Is.EqualTo(string.Empty));
        Assert.That(result[3]["test"], Is.EqualTo(new[] { "37", "99", "42", "31" }));

        Assert.That(result[4]["boo"], Is.EqualTo(string.Empty));
        Assert.That(result[4]["hoo"], Is.EqualTo("two"));
        Assert.That(result[4]["test"], Is.EqualTo(new[] { "37", "99", "42", "31", "32" }));

        Assert.That(result[5]["boo"], Is.EqualTo(string.Empty));
        Assert.That(result[5]["hoo"], Is.EqualTo("two"));
        Assert.That(result[5]["test"], Is.EqualTo(new[] { "37", "99", "42", "31", "32", "33" }));

        Assert.That(result[6]["boo"], Is.EqualTo(string.Empty));
        Assert.That(result[6]["hoo"], Is.EqualTo(string.Empty));
        Assert.That(result[6]["test"], Is.EqualTo(new[] { "37", "99", "42", "31", "32", "33", "34" }));
    }

    [Test]
    public void Fillup01()
    {
        var template = new Template("""
            Value boo (one)
            Value hoo (two)
            Value Fillup test (\d+)

            Start
             ^${boo} -> Record
             ^${hoo} -> Record
             ^F${test}
             
            """);

        var data = """
            F37
            one
            two
            F99
            F42
            one
            F31
            one
            F32
            two
            F33
            two
            F34
            """;

        var result = template.Parse(data);

        Assert.That(result[0]["boo"], Is.EqualTo("one"));
        Assert.That(result[0]["hoo"], Is.EqualTo(string.Empty));
        Assert.That(result[0]["test"], Is.EqualTo("37"));

        Assert.That(result[1]["boo"], Is.EqualTo(string.Empty));
        Assert.That(result[1]["hoo"], Is.EqualTo("two"));
        Assert.That(result[1]["test"], Is.EqualTo("99"));

        Assert.That(result[2]["boo"], Is.EqualTo("one"));
        Assert.That(result[2]["hoo"], Is.EqualTo(string.Empty));
        Assert.That(result[2]["test"], Is.EqualTo("42"));

        Assert.That(result[3]["boo"], Is.EqualTo("one"));
        Assert.That(result[3]["hoo"], Is.EqualTo(string.Empty));
        Assert.That(result[3]["test"], Is.EqualTo("31"));

        Assert.That(result[4]["boo"], Is.EqualTo(string.Empty));
        Assert.That(result[4]["hoo"], Is.EqualTo("two"));
        Assert.That(result[4]["test"], Is.EqualTo("32"));

        Assert.That(result[5]["boo"], Is.EqualTo(string.Empty));
        Assert.That(result[5]["hoo"], Is.EqualTo("two"));
        Assert.That(result[5]["test"], Is.EqualTo("33"));

        Assert.That(result[6]["boo"], Is.EqualTo(string.Empty));
        Assert.That(result[6]["hoo"], Is.EqualTo(string.Empty));
        Assert.That(result[6]["test"], Is.EqualTo("34"));

        Assert.That(result.Count, Is.EqualTo(7));
    }

    [Test]
    public void Fillup02()
    {
        /*
            FSM Table:
            ['boo', 'hoo', 'test']
            ['one', '', ['37']]
            ['', 'two', '99']
            ['one', '', ['99', '42']]
            ['one', '', ['31']]
            ['', 'two', ['32']]
            ['', 'two', ['33']]
            ['', '', ['34']]
        */

        var template = new Template("""
            Value boo (one)
            Value hoo (two)
            Value Fillup,List test (\d+)

            Start
             ^${boo} -> Record
             ^${hoo} -> Record
             ^F${test}
            """);

        var data = """
            F37
            one
            two
            F99
            F42
            one
            F31
            one
            F32
            two
            F33
            two
            F34
            """;

        var result = template.Parse(data);

        Assert.That(result.Count, Is.EqualTo(7));

        Assert.That(result[0]["boo"], Is.EqualTo("one"));
        Assert.That(result[0]["hoo"], Is.EqualTo(string.Empty));
        Assert.That(result[0]["test"], Is.EqualTo(new[] { "37" }));

        Assert.That(result[1]["boo"], Is.EqualTo(string.Empty));
        Assert.That(result[1]["hoo"], Is.EqualTo("two"));
        Assert.That(result[1]["test"], Is.EqualTo(new[] { "99" }));

        Assert.That(result[2]["boo"], Is.EqualTo("one"));
        Assert.That(result[2]["hoo"], Is.EqualTo(string.Empty));
        Assert.That(result[2]["test"], Is.EqualTo(new[] { "99", "42" }));

        Assert.That(result[3]["boo"], Is.EqualTo("one"));
        Assert.That(result[3]["hoo"], Is.EqualTo(string.Empty));
        Assert.That(result[3]["test"], Is.EqualTo(new[] {"31" }));

        Assert.That(result[4]["boo"], Is.EqualTo(string.Empty));
        Assert.That(result[4]["hoo"], Is.EqualTo("two"));
        Assert.That(result[4]["test"], Is.EqualTo(new[] { "32" }));

        Assert.That(result[5]["boo"], Is.EqualTo(string.Empty));
        Assert.That(result[5]["hoo"], Is.EqualTo("two"));
        Assert.That(result[5]["test"], Is.EqualTo(new[] { "33" }));

        Assert.That(result[6]["boo"], Is.EqualTo(string.Empty));
        Assert.That(result[6]["hoo"], Is.EqualTo(string.Empty));
        Assert.That(result[6]["test"], Is.EqualTo(new[] {"34" }));
    }
}