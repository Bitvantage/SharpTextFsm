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
using Bitvantage.SharpTextFsm.TemplateHelpers;

namespace Test.Extensions;

internal class RelaxedContinueWithStateChange
{
    // after a state loop exception has occured use the following to view the graph
    // raw dot graph:       (string)exception.Data["digraph"]
    // url for dot graph:   ((Uri)exception.Data["digraphLink"]).ToString()
    // launch dot graph:    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(((Uri)exception.Data["digraphLink"]).ToString()) { UseShellExecute = true })

    [Test(Description = "Indirect state loop")]
    public void Test01()
    {
        var exception = Assert.Throws<TemplateParseException>(() => new Template("""
            Value X (.*)

            Start
             ^.* -> State1
             ^.* -> Continue State2
             ^.* -> State3
            State1
             ^1.* -> Continue State4
            State2
             ^1.* -> Continue State3
            State3
             ^.* -> Continue State4
            State4
             ^.* -> Continue State5
            State5
             ^.* -> Continue State6
            State6
             ^.* -> Continue State7
            State7
             ^.* -> Continue Start
            """));

        Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.StateLoop));

        Assert.That(((TemplateState[])exception.Data["loop"]!).Select(item => item.Name).ToArray(), Is.EqualTo(new[] { "Start", "State2", "State3", "State4", "State5", "State6", "State7", "Start" }));
        Assert.That(((Rule)exception.Data["rule"]!).State.Name, Is.EqualTo("State7"));
        Assert.That(((Rule)exception.Data["rule"]!).ToString(), Is.EqualTo(" ^.* -> Continue Start"));
    }

    [Test(Description = "Direct state loop")]
    public void Test02()
    {
        var exception = Assert.Throws<TemplateParseException>(() => new Template("""
            Value X (.*)

            Start
             ^.* -> State1
             ^.* -> Continue State2
             ^.* -> State3
            State1
             ^1.* -> Continue State3
            State2
             ^1.* -> Continue Start
            State3
             ^.* -> Start
            """));

        Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.StateLoop));

        Assert.That(((TemplateState[])exception.Data["loop"]!).Select(item => item.Name).ToArray(), Is.EqualTo(new[] { "Start", "State2", "Start" }));
        Assert.That(((Rule)exception.Data["rule"]!).State.Name, Is.EqualTo("State2"));
        Assert.That(((Rule)exception.Data["rule"]!).ToString(), Is.EqualTo(" ^1.* -> Continue Start"));
    }


    [Test(Description = "No state loop")]
    public void Test03()
    {
        var template = new Template("""
            Value X (.*)

            Start
             ^.* -> State1
             ^.* -> Continue State2
             ^.* -> State3
            State1
             ^1.* -> Continue State4
            State2
             ^1.* -> Continue State3
            State3
             ^.* -> Continue State4
            State4
             ^.* -> Continue State5
            State5
             ^.* -> Continue State6
            State6
             ^.* -> Continue State7
            State7
             ^.* -> Next Start
            """);
    }

    [Test(Description = "Indirect state loop")]
    public void Test04()
    {
        var exception = Assert.Throws<TemplateParseException>(() => new Template("""
            Value X (.*)

            Start
             ^.* -> StartX
            StartX
             ^.* -> Continue State1
             ^.* -> Continue State2
             ^.* -> Continue State3
            State1
             ^1.* -> Continue State4
            State2
             ^1.* -> Continue State4
            State3
             ^.* -> Continue State4
             ^.* -> Continue State5
            State4
             ^.* -> Continue State5
            State5
             ^.* -> Continue State6
            State6
             ^.* -> Continue StartX

            """));

        Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.StateLoop));

        Assert.That(((TemplateState[])exception.Data["loop"]!).Select(item => item.Name).ToArray(), Is.EqualTo(new[] { "StartX", "State3", "State5", "State6", "StartX" }));
        Assert.That(((Rule)exception.Data["rule"]!).State.Name, Is.EqualTo("State6"));
        Assert.That(((Rule)exception.Data["rule"]!).ToString(), Is.EqualTo(" ^.* -> Continue StartX"));
    }

    [Test(Description = "Indirect state loop")]
    public void Test05()
    {
        var exception = Assert.Throws<TemplateParseException>(() => new Template("""
            Value X (.*)

            Start
             ^.* -> Continue State1
             ^.* -> Continue State2
             ^.* -> Continue State3
            State1
             ^1.* -> Continue State2
            State2
             ^1.* -> Continue State4
            State3
             ^.* -> Continue State4
             ^.* -> Continue State5
            State4
             ^.* -> Continue State5
            State5
             ^.* -> Continue State2
            """));

        Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.StateLoop));

        Assert.That(((TemplateState[])exception.Data["loop"]!).Select(item => item.Name).ToArray(), Is.EqualTo(new[] { "State2", "State4", "State5", "State2" }));
        Assert.That(((Rule)exception.Data["rule"]!).State.Name, Is.EqualTo("State5"));
        Assert.That(((Rule)exception.Data["rule"]!).ToString(), Is.EqualTo(" ^.* -> Continue State2"));
    }

    [Test(Description = "~Global with state filter that does not cause a state loop")]
    public void Test06()
    {
        var template = new Template("""
            Value X (.*)

            ~Global
             [State7]
             ^.* -> Continue Start
            Start
             ^.* -> State1
             ^.* -> Continue State2
             ^.* -> State3
            State1
             ^1.* -> Continue State4
            State2
             ^1.* -> Continue State3
            State3
             ^.* -> Continue State4
            State4
             ^.* -> Continue State5
            State5
             ^.* -> Continue State6
            State6
             ^.* -> Continue State7
            State7
             ^.* -> Next Start
            """
        );

    }

    [Test(Description = "~Global without state filter that does not cause a state loop")]
    public void Test07()
    {
        var template = new Template("""
            Value X (.*)

            ~Global
             ^.* -> Continue Start
            Start
             ^.* -> State1
             ^.* -> Continue State2
             ^.* -> State3
            State1
             ^1.* -> Continue State4
            State2
             ^1.* -> Continue State3
            State3
             ^.* -> Continue State4
            State4
             ^.* -> Continue State5
            State5
             ^.* -> Continue State6
            State6
             ^.* -> Continue State7
            State7
             ^.* -> Next Start
            """
        );
    }

    [Test(Description = "State loop from same state")]
    public void Test08()
    {
        var exception = Assert.Throws<TemplateParseException>(() => new Template("""
            Value X (.*)

            Start
             ^.* -> State1
            State1
             ^1.* -> Continue State1
            """
        ));

        Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.StateLoop));

        Assert.That(((TemplateState[])exception.Data["loop"]!).Select(item => item.Name).ToArray(), Is.EqualTo(new[] { "State1", "State1" }));
        Assert.That(((Rule)exception.Data["rule"]!).State.Name, Is.EqualTo("State1"));
        Assert.That(((Rule)exception.Data["rule"]!).ToString().TrimStart(), Is.EqualTo("^1.* -> Continue State1"));
    }

    [Test(Description = "State loop from ~Global")]
    public void Test09()
    {
        var template = new Template("""
            Value X (.*)

            ~Global
             ^.* -> Continue State1
            Start
             ^.* -> State1
            State1
            """
        );
    }

    [Test(Description = "Empty Start state")]
    public void Test10()
    {
        var template = new Template("""
            Value X (.*)

            ~Global
             ^.* -> Continue State1
            Start
             ^.* -> Error
            State1
             ^.*
            """
        );
    }

    [Test(Description = "Empty Start state with loop")]
    public void Test11()
    {
        var exception = Assert.Throws<TemplateParseException>(() => new Template("""
            Value X (.*)

            ~Global
             ^.* -> Continue State1
            Start
            State1
             ^.* -> Continue State2
            State2
             ^.* -> Continue State1
            """
        ));

        Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.StateLoop));

        Assert.That(((TemplateState[])exception.Data["loop"]!).Select(item => item.Name).ToArray(), Is.EqualTo(new[] { "State1", "State2", "State1" }));
        Assert.That(((Rule)exception.Data["rule"]!).State.Name, Is.EqualTo("State2"));
        Assert.That(((Rule)exception.Data["rule"]!).ToString().TrimStart(), Is.EqualTo("^.* -> Continue State1"));
    }
}