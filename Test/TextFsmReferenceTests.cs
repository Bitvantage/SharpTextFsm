/*
   Bitvantage.SharpTextFSM
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

using Bitvantage.SharpTextFSM;
using Bitvantage.SharpTextFSM.Exceptions;
using Bitvantage.SharpTextFSM.TemplateHelpers;
using System.Text.RegularExpressions;

namespace Test
{
    /*
     * These tests are inspired by the official TextFSM unit tests located at https://github.com/google/textfsm/blob/c8843d69daa9b565fea99a0283ad13c324d5b563/tests/textfsm_test.py
     */

    internal class TextFsmReferenceTests
    {
        [Test(Description = "Check basic line is parsed")]
        public void TestFsmValue01()
        {
            var template = new Template("""
                Value beer (\S+)

                Start
                 ^.* -> Error
                """);

            var valueDescriptors = template
                .Values
                .Where(item=>item.Options != Option.Regex)
                .ToList();

            Assert.That(valueDescriptors.Count, Is.EqualTo(1));
            Assert.That(valueDescriptors[0].Name, Is.EqualTo("beer"));
            Assert.That(valueDescriptors[0].Options, Is.EqualTo(Option.None));
            Assert.That(valueDescriptors[0].Regex.ToString(), Is.EqualTo(@"\S+"));
        }

        [Test(Description = "Test options")]
        public void TestFsmValue02()
        {
            var template = new Template("""
                Value Filldown,Required beer (\S+)

                Start
                 ^.* -> Error
                """);

            var valueDescriptors = template
                .Values
                .Where(item => item.Options != Option.Regex)
                .ToList();

            Assert.That(valueDescriptors.Count, Is.EqualTo(1));
            Assert.That(valueDescriptors[0].Name, Is.EqualTo("beer"));
            Assert.That(valueDescriptors[0].Options, Is.EqualTo(Option.Filldown | Option.Required));
            Assert.That(valueDescriptors[0].Regex.ToString(), Is.EqualTo(@"\S+"));
        }

        [Test(Description = "Multiple parenthesis")]
        public void TestFsmValue03()
        {
            var template = new Template("""
                Value Required beer (boo(hoo))

                Start
                 ^.* -> Error
                """);

            var valueDescriptors = template
                .Values
                .Where(item => item.Options != Option.Regex)
                .ToList();

            Assert.That(valueDescriptors.Count, Is.EqualTo(1));
            Assert.That(valueDescriptors[0].Name, Is.EqualTo("beer"));
            Assert.That(valueDescriptors[0].Options, Is.EqualTo(Option.Required));
            Assert.That(valueDescriptors[0].Regex.ToString(), Is.EqualTo(@"boo(hoo)"));
        }

        [Test(Description = "Regex must be bounded by parenthesis")]
        public void TestFsmValue04()
        {
            var exception = Assert.Throws<TemplateParseException>(() =>
                new Template("""
                    Value beer (boo(hoo)))boo

                    Start
                     ^.* -> Error
                    """));

            Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.SyntaxError));
        }

        [Test(Description = "Regex must be bounded by parenthesis")]
        public void TestFsmValue05()
        {
            var exception = Assert.Throws<TemplateParseException>(() =>
                new Template("""
                    Value beer boo(boo(hoo)))

                    Start
                     ^.* -> Error
                    """));

            Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.SyntaxError));
        }

        [Test(Description = "Regex must be bounded by parenthesis")]
        public void TestFsmValue06()
        {
            var exception = Assert.Throws<TemplateParseException>(() =>
                new Template("""
                    Value beer (boo)hoo)

                    Start
                     ^.* -> Error
                    """));

            Assert.That(exception.InnerException, Is.TypeOf<RegexParseException>());
            Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.InvalidValueRegularExpression));

        }

        [Test(Description = "Escaped parentheses do not count")]
        public void TestFsmValue07()
        {
            var exception = Assert.Throws<TemplateParseException>(() =>
                new Template("""
                    Value beer (boohoo\)

                    Start
                     ^.* -> Error
                    """));

            Assert.That(exception.InnerException, Is.TypeOf<RegexParseException>());
            Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.InvalidValueRegularExpression));

        }

        [Test(Description = "Escaped parentheses do not count")]
        public void TestFsmValue08()
        {
            var exception = Assert.Throws<TemplateParseException>(() =>
                new Template("""
                    Value beer (boo)hoo\)

                    Start
                     ^.* -> Error
                    """));

            Assert.That(exception.InnerException, Is.TypeOf<RegexParseException>());
            Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.InvalidValueRegularExpression));

        }

        [Test(Description = "Unbalanced parenthesis can exist if within square \"[]\" braces")]
        public void TestFsmValue09()
        {
            var template = new Template("""
                Value beer (boo[(]hoo)

                Start
                 ^.* -> Error
                """);

            var valueDescriptors = template
                .Values
                .Where(item => item.Options != Option.Regex)
                .ToList();

            Assert.That(valueDescriptors.Count, Is.EqualTo(1));
            Assert.That(valueDescriptors[0].Name, Is.EqualTo("beer"));
            Assert.That(valueDescriptors[0].Options, Is.EqualTo(Option.None));
            Assert.That(valueDescriptors[0].Regex.ToString(), Is.EqualTo(@"boo[(]hoo"));
        }

        [Test(Description = "Escaped braces don't count")]
        public void TestFsmValue10()
        {
            var exception = Assert.Throws<TemplateParseException>(() =>
                new Template("""
                    Value beer (boo\[)\]hoo)

                    Start
                     ^.* -> Error
                    """));

            Assert.That(exception.InnerException, Is.TypeOf<RegexParseException>());
            Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.InvalidValueRegularExpression));

        }

        [Test(Description = "String function")]
        public void TestFsmValue11()
        {
            var template = new Template("""
                Value Required beer (boo(hoo))

                Start
                 ^.* -> Error
                """);

            var valueDescriptors = template
                .Values
                .Where(item => item.Options != Option.Regex)
                .ToList();

            Assert.That(valueDescriptors.Count, Is.EqualTo(1));
            Assert.That(valueDescriptors[0].ToString(), Is.EqualTo("Value Required beer (boo(hoo))"));
        }

        [Test(Description = "String function")]
        public void TestFsmValue12()
        {
            var template = new Template("""
                Value Required,Filldown beer (bo\S+(hoo))

                Start
                 ^.* -> Error
                """);

            var valueDescriptors = template
                .Values
                .Where(item => item.Options != Option.Regex)
                .ToList();

            Assert.That(valueDescriptors.Count, Is.EqualTo(1));
            Assert.That(valueDescriptors[0].ToString(), Is.EqualTo("Value Filldown,Required beer (bo\\S+(hoo))"));
        }

        [Test(Description = "Basic line, no action")]
        public void TestFsmRules01()
        {
            var template = new Template("""
                Value beer (.*)

                Start
                 ^A beer called ${beer}
                 ^.* -> Error
                """);

            var states = template.States["Start"];

            Assert.That(states.Rules.Length, Is.EqualTo(2));
            Assert.That(states.Rules[0].RecordAction, Is.EqualTo(RecordAction.NoRecord));
            Assert.That(states.Rules[0].LineAction, Is.EqualTo(LineAction.Next));
            Assert.That(states.Rules[0].Regex.ToString(), Is.EqualTo("^A beer called (?<textfsm_beer>.*)"));
            Assert.That(states.Rules[0].Action, Is.EqualTo(null));
        }

        [Test(Description = "Multiple matches")]
        public void TestFsmRules02()
        {
            // this template was modified from the reference implementation
            // the reference implementation did not define $hi

            var template = new Template("""
                Value beer (.*)
                Value hi (.*)

                Start
                 ^A $hi called ${beer}
                 ^.* -> Error
                """);

            var states = template.States["Start"];

            Assert.That(states.Rules.Length, Is.EqualTo(2));
            Assert.That(states.Rules[0].RecordAction, Is.EqualTo(RecordAction.NoRecord));
            Assert.That(states.Rules[0].LineAction, Is.EqualTo(LineAction.Next));
            Assert.That(states.Rules[0].Regex.ToString(), Is.EqualTo("^A (?<textfsm_hi>.*) called (?<textfsm_beer>.*)"));
            Assert.That(states.Rules[0].Action, Is.EqualTo(null));
        }

        [Test(Description = "Line with action")]
        public void TestFsmRules03()
        {
            var template = new Template("""
                Value beer (.*)

                Start
                 ^A beer called ${beer} -> Next
                 ^.* -> Error
                """);

            var states = template.States["Start"];

            Assert.That(states.Rules.Length, Is.EqualTo(2));
            Assert.That(states.Rules[0].RecordAction, Is.EqualTo(RecordAction.NoRecord));
            Assert.That(states.Rules[0].LineAction, Is.EqualTo(LineAction.Next));
            Assert.That(states.Rules[0].Regex.ToString(), Is.EqualTo("^A beer called (?<textfsm_beer>.*)"));
            Assert.That(states.Rules[0].Action, Is.EqualTo(null));
        }

        [Test(Description = "Line with record")]
        public void TestFsmRules04()
        {
            var template = new Template("""
                Value beer (.*)

                Start
                 ^A beer called ${beer} -> Continue.Record
                 ^.* -> Error
                """);

            var states = template.States["Start"];

            Assert.That(states.Rules.Length, Is.EqualTo(2));
            Assert.That(states.Rules[0].RecordAction, Is.EqualTo(RecordAction.Record));
            Assert.That(states.Rules[0].LineAction, Is.EqualTo(LineAction.Continue));
            Assert.That(states.Rules[0].Regex.ToString(), Is.EqualTo("^A beer called (?<textfsm_beer>.*)"));
            Assert.That(states.Rules[0].Action, Is.EqualTo(null));
        }

        [Test(Description = "Line with new state")]
        public void TestFsmRules05()
        {
            var template = new Template("""
                Value beer (.*)

                Start
                 ^A beer called ${beer} -> Next.NoRecord End
                 ^.* -> Error
                """);

            var states = template.States["Start"];

            Assert.That(states.Rules.Length, Is.EqualTo(2));
            Assert.That(states.Rules[0].RecordAction, Is.EqualTo(RecordAction.NoRecord));
            Assert.That(states.Rules[0].LineAction, Is.EqualTo(LineAction.Next));
            Assert.That(states.Rules[0].Regex.ToString(), Is.EqualTo("^A beer called (?<textfsm_beer>.*)"));

            var action = states.Rules[0].Action as ChangeStateAction;
            Assert.That(action?.NewState.Name, Is.EqualTo("End"));
        }

        [Test(Description = "Bad syntax test")]
        public void TestFsmRules06()
        {
            var exception = Assert.Throws<TemplateParseException>(() =>
                new Template("""
                    Value beer (.*)

                    Start
                     ^A beer called ${beer} -> Next Next Next
                     ^.* -> Error
                    """));

            Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.SyntaxError));
        }

        [Test(Description = "Bad syntax test")]
        public void TestFsmRules07()
        {
            var exception = Assert.Throws<TemplateParseException>(() =>
                new Template("""
                    Value beer (.*)

                    Start
                     ^A beer called ${beer} -> Boo.hoo
                     ^.* -> Error
                    """));

            Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.SyntaxError));
        }

        [Test(Description = "Bad syntax test")]
        public void TestFsmRules08()
        {
            var exception = Assert.Throws<TemplateParseException>(() =>
                new Template("""
                    Value beer (.*)

                    Start
                     ^A beer called ${beer} -> Continue.Record $Hi
                     ^.* -> Error
                    """));

            Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.SyntaxError));
        }

        [Test(Description = "Bad syntax test")]
        public void TestRulePrefixes01()
        {
            var exception = Assert.Throws<TemplateParseException>(() =>
                new Template("""
                    Value beer (.*)

                    Start
                     A beer called ${beer} -> Continue.Record $Hi
                    """));
            Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.SyntaxError));
        }

        [Test(Description = "Bad syntax test")]
        public void TestRulePrefixes02()
        {
            var exception = Assert.Throws<TemplateParseException>(() =>
                new Template("""
                    Value beer (.*)

                    Start
                    .^A beer called ${beer} -> Continue.Record $Hi
                    """));

            Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.SyntaxError));
        }

        [Test(Description = "Bad syntax test")]
        public void TestRulePrefixes03()
        {
            var exception = Assert.Throws<TemplateParseException>(() =>
                new Template("""
                    Value beer (.*)

                    Start
                     	A beer called ${beer} -> Continue.Record $Hi
                    """));

            Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.SyntaxError));
        }

        [Test(Description = "Bad syntax test")]
        public void TestRulePrefixes04()
        {
            var exception = Assert.Throws<TemplateParseException>(() =>
                new Template("""
                    Value beer (.*)

                    Start
                    A beer called ${beer} -> Continue.Record $Hi
                    """));

            Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.SyntaxError));
        }

        [Test(Description = "Good syntax test")]
        public void TestRulePrefixes05()
        {
            var template = new Template("""
                Value beer (.*)

                Start
                 ^A simple string
                  ^A simple string
                	^A simple string
                 ^.* -> Error
                """);
        }

        [Test]
        public void TestImplicitDefaultRules01()
        {
            var template = new Template("""
                Value beer (.*)

                Start
                 ^A beer called ${beer} -> Record End
                 ^.* -> Error
                """);

            var states = template.States["Start"];

            Assert.That(states.Rules[0].ToString(), Is.EqualTo(" ^A beer called ${beer} -> Record End"));
        }

        [Test]
        public void TestImplicitDefaultRules02()
        {
            var template = new Template("""
                Value beer (.*)

                Start
                 ^A beer called ${beer} -> End
                 ^.* -> Error
                """);

            var states = template.States["Start"];

            Assert.That(states.Rules[0].ToString(), Is.EqualTo(" ^A beer called ${beer} -> End"));
        }

        [Test]
        public void TestImplicitDefaultRules03()
        {
            var template = new Template("""
                Value beer (.*)

                Start
                 ^A beer called ${beer} -> Next.NoRecord End
                 ^.* -> Error
                """);

            var states = template.States["Start"];

            Assert.That(states.Rules[0].ToString(), Is.EqualTo(" ^A beer called ${beer} -> End"));
        }

        [Test]
        public void TestImplicitDefaultRules04()
        {
            var template = new Template("""
                Value beer (.*)

                Start
                 ^A beer called ${beer} -> Clear End
                 ^.* -> Error
                """);

            var states = template.States["Start"];

            Assert.That(states.Rules[0].ToString(), Is.EqualTo(" ^A beer called ${beer} -> Clear End"));
        }

        [Test]
        public void TestImplicitDefaultRules05()
        {
            var template = new Template("""
                Value beer (.*)

                Start
                 ^A beer called ${beer} -> Error "Hello World"
                 ^.* -> Error
                """);

            var states = template.States["Start"];

            Assert.That(states.Rules[0].ToString(), Is.EqualTo(" ^A beer called ${beer} -> Error \"Hello World\""));
        }

        [Test]
        public void TestImplicitDefaultRules06()
        {
            var exception = Assert.Throws<TemplateParseException>(() =>
                new Template("""
                    Value beer (.*)

                    Start
                      ^A beer called ${beer} -> Next "Hello World"
                    """));

            Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.SyntaxError));
        }

        [Test]
        public void TestImplicitDefaultRules07()
        {
            var exception = Assert.Throws<TemplateParseException>(() =>
                new Template("""
                    Value beer (.*)

                    Start
                      ^A beer called ${beer} -> Record.Next
                    """));

            Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.SyntaxError));
        }

        [Test]
        public void TestImplicitDefaultRules08()
        {
            // this is an error in the reference textfsm implementation: Action 'Continue' with new state End specified.
            // however since this does not cause a loop; we allow it due to the relaxed continue with state change extension
            new Template("""
                Value beer (.*)

                Start
                  ^A beer called ${beer} -> Continue End
                """);
        }

        [Test]
        public void TestImplicitDefaultRules09()
        {
            var exception = Assert.Throws<TemplateParseException>(() =>
                new Template("""
                    Value beer (.*)

                    Start
                      ^A beer called ${beer} -> Beer End
                    """));

            Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.SyntaxError));
        }

        [Test]
        public void TestSpacesAroundAction01()
        {
            var template = new Template("""
                Value beer (.*)

                Start
                  ^Hello World -> Boo

                Boo
                 ^.* -> Error
                """);

            var states = template.States["Start"];

            Assert.That(states.Rules[0].ToString(), Is.EqualTo(" ^Hello World -> Boo"));
        }

        [Test]
        public void TestSpacesAroundAction02()
        {
            var template = new Template("""
                Value beer (.*)

                Start
                  ^Hello World ->  Boo

                Boo
                 ^.* -> Error
                """);

            var states = template.States["Start"];

            Assert.That(states.Rules[0].ToString(), Is.EqualTo(" ^Hello World -> Boo"));
        }

        [Test]
        public void TestSpacesAroundAction03()
        {
            var template = new Template("""
                Value beer (.*)

                Start
                  ^Hello World ->   Boo
                  
                Boo
                 ^.* -> Error
                """);

            var states = template.States["Start"];

            Assert.That(states.Rules[0].ToString(), Is.EqualTo(" ^Hello World -> Boo"));
        }

        [Test(Description = "# A '->' without a leading space is considered part of the matching line")]
        public void TestSpacesAroundAction04()
        {
            var template = new Template("""
                Value beer (.*)

                Start
                  ^A simple line-> Boo -> Next
                """);

            var states = template.States["Start"];

            Assert.That(states.Rules[0].ToString(), Is.EqualTo(" ^A simple line-> Boo"));
        }

        [Test(Description = "Trivial template to initiate object")]
        public void TestParseFsmVariables01()
        {
            var template = new Template("""
                Value unused (.)

                Start
                """);
        }

        [Test(Description = "Trivial entry")]
        public void TestParseFsmVariables02()
        {
            var template = new Template("""
                Value Filldown Beer (beer)

                Start
                """);
        }

        [Test(Description = "Single variable with commented header")]
        public void TestParseFsmVariables03()
        {
            var template = new Template("""
                # Headline
                Value Filldown Beer (beer)

                Start
                """);

            var valueDescriptors = template
                .Values
                .Where(item => item.Options != Option.Regex)
                .ToList();

            Assert.That(valueDescriptors.Count, Is.EqualTo(1));
            Assert.That(valueDescriptors[0].ToString(), Is.EqualTo("Value Filldown Beer (beer)"));
        }

        [Test(Description = "Multiple variables")]
        public void TestParseFsmVariables04()
        {
            var template = new Template("""
                # Headline
                Value Filldown Beer (beer)
                Value Required Spirits (whiskey)
                Value Filldown Wine (claret)

                Start
                """);

            var valueDescriptors = template
                .Values
                .Where(item => item.Options != Option.Regex)
                .ToList();

            Assert.That(valueDescriptors.Count, Is.EqualTo(3));
            Assert.That(template.Values["Beer"].ToString(), Is.EqualTo("Value Filldown Beer (beer)"));
            Assert.That(template.Values["Spirits"].ToString(), Is.EqualTo("Value Required Spirits (whiskey)"));
            Assert.That(template.Values["Wine"].ToString(), Is.EqualTo("Value Filldown Wine (claret)"));
        }

        [Test(Description = "Multiple variables")]
        public void TestParseFsmVariables05()
        {
            var template = new Template("""
                # Headline
                Value Filldown Beer (beer)
                 # A comment
                Value Spirits ()
                Value Filldown,Required Wine ((c|C)laret)

                Start
                """);

            var valueDescriptors = template
                .Values
                .Where(item => item.Options != Option.Regex)
                .ToList();
            
            Assert.That(valueDescriptors.Count, Is.EqualTo(3));
            Assert.That(template.Values["Beer"].ToString(), Is.EqualTo("Value Filldown Beer (beer)"));
            Assert.That(template.Values["Spirits"].ToString(), Is.EqualTo("Value Spirits ()"));
            Assert.That(template.Values["Wine"].ToString(), Is.EqualTo("Value Filldown,Required Wine ((c|C)laret)"));
        }

        [Test(Description = "Malformed variables")]
        public void TestParseFsmVariables06()
        {

            var exception = Assert.Throws<TemplateParseException>(() =>
                new Template("""
                    Value Beer (beer) beer

                    Start
                    """));

            Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.SyntaxError));
        }

        [Test(Description = "Malformed variables")]
        public void TestParseFsmVariables07()
        {

            var exception = Assert.Throws<TemplateParseException>(() =>
                new Template("""
                    Value Filldown, Required Spirits ()

                    Start
                    """));

            Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.SyntaxError));
        }

        [Test(Description = "Malformed variables")]
        public void TestParseFsmVariables08()
        {

            var exception = Assert.Throws<TemplateParseException>(() =>
                new Template("""
                    Value filldown,Required Wine ((c|C)laret)

                    Start
                    """));

            Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.SyntaxError));
        }

        [Test(Description = "Values that look bad but are okay")]
        public void TestParseFsmVariables09()
        {
            var template = new Template("""
                # Headline
                Value Filldown Beer (bee(r), (and) (M)ead$)
                # A comment
                Value Spirits,and,some ()
                Value Filldown,Required Wine ((c|C)laret)

                Start
                """);

            var valueDescriptors = template
                .Values
                .Where(item => item.Options != Option.Regex)
                .ToList();
            
            Assert.That(valueDescriptors.Count, Is.EqualTo(3));
            Assert.That(template.Values["Beer"].ToString(), Is.EqualTo("Value Filldown Beer (bee(r), (and) (M)ead$)"));
            Assert.That(template.Values["Spirits,and,some"].ToString(), Is.EqualTo("Value Spirits,and,some ()"));
            Assert.That(template.Values["Wine"].ToString(), Is.EqualTo("Value Filldown,Required Wine ((c|C)laret)"));
        }

        [Test(Description = "Variable name too long")]
        public void TestParseFsmVariables10()
        {

            var exception = Assert.Throws<TemplateParseException>(() =>
                new Template("""
                    Value Filldown nametoolong_nametoolong_nametoolo_nametoolong_nametoolong (beer)

                    Start
                    """));


            Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.InvalidValueName));
        }

        [Test(Description = "Fails when more then one 'Start' state")]
        public void TestParseFsmState01()
        {
            var exception = Assert.Throws<TemplateParseException>(() =>
                new Template("""
                    Value Beer (.)
                    Value Wine (\\w)

                    Start

                    Start
                    """));

            Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.DuplicateState));
        }

        [Test(Description = "Multiple states")]
        public void TestParseFsmState02()
        {
            var template = new Template("""
                # Headline
                Value Beer (.)

                Start
                  ^.
                  ^Hello World
                  ^Last-[Cc]ha$$nge
                """);

            var rules = template.States["Start"].Rules;
            Assert.That(rules.Count, Is.EqualTo(3));
            Assert.That(rules[0].ToString(), Is.EqualTo(" ^."));
            Assert.That(rules[1].ToString(), Is.EqualTo(" ^Hello World"));
            Assert.That(rules[2].ToString(), Is.EqualTo(" ^Last-[Cc]ha$$nge"));
        }

        [Test(Description = "Malformed states")]
        public void TestParseFsmState03()
        {
            var exception = Assert.Throws<TemplateParseException>(() =>
                new Template("""
                    Value Beer (.)

                    St%art
                    ^.
                    ^Hello World
                    """));


            Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.SyntaxError));
        }

        [Test(Description = "Malformed states")]
        public void TestParseFsmState04()
        {
            var exception = Assert.Throws<TemplateParseException>(() =>
                new Template("""
                    Value Beer (.)

                    Start
                    ^.
                    ^Hello World\n
                    """));


            Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.SyntaxError));
        }

        [Test(Description = "Malformed states")]
        public void TestParseFsmState05()
        {
            var exception = Assert.Throws<TemplateParseException>(() =>
                new Template("""
                    Value Beer (.)
                      Start
                      ^.
                      ^Hello World
                    """));

            Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.SyntaxError));
        }

        [Test(Description = "Multiple variables and substitution")]
        public void TestParseFsmState06()
        {
            var template = new Template("""
                # Headline
                Value Beer (.*)
                Value Wine (.*)

                Start
                  ^.${Beer}${Wine}.
                  ^Hello $Beer
                  ^Last-[Cc]ha$$nge
                """);

            var rules = template.States["Start"].Rules;
            Assert.That(rules.Count, Is.EqualTo(3));
            Assert.That(rules[0].ToString(), Is.EqualTo(" ^.${Beer}${Wine}."));
            Assert.That(rules[1].ToString(), Is.EqualTo(" ^Hello $Beer"));
            Assert.That(rules[2].ToString(), Is.EqualTo(" ^Last-[Cc]ha$$nge"));
        }

        [Test(Description = "State name too long")]
        public void TestParseFsmState07()
        {
            var exception = Assert.Throws<TemplateParseException>(() =>
                new Template("""
                    Value Beer (.)

                    Start

                    rnametoolongxnametoolongxnametoolongxnametoolongxnametoolo
                      ^.
                    """));

            Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.InvalidStateName));
        }

        [Test(Description = "'Continue' should not accept a destination")]
        public void TestParseFsmState08()
        {
            var exception = Assert.Throws<TemplateParseException>(() =>
                new Template("""
                    Value Beer (.)

                    Start
                     ^.* -> Continue Start
                    """));

            Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.StateLoop));
        }

        [Test(Description = "'Error' accepts a text string but 'next' state does not")]
        public void TestParseFsmState09()
        {
            var template = new Template("""
                Value Beer (.)

                Start
                 ^ -> Error "hi there"
                """);

            var rules = template.States["Start"].Rules;
            Assert.That(rules.Count, Is.EqualTo(1));
            Assert.That(rules[0].ToString(), Is.EqualTo(" ^ -> Error \"hi there\""));
        }

        [Test(Description = "'Error' accepts a text string but 'next' state does not")]
        public void TestParseFsmState10()
        {
            var exception = Assert.Throws<TemplateParseException>(() =>
                new Template("""
                    Value Beer (.)

                    Start
                     ^.* -> Next "Hello World"
                    """));

            Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.SyntaxError));
        }

        [Test]
        public void TestRuleStartsWithCarrot01()
        {
            var exception = Assert.Throws<TemplateParseException>(() =>
                new Template("""
                    Value Beer (.)
                    Value Wine (\\w)

                    Start
                      A Simple line
                    """));

            Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.SyntaxError));
        }

        [Test(Description = "No Values")]
        public void TestValidateFsm01()
        {
            var exception = Assert.Throws<TemplateParseException>(() =>
                new Template("""
                    Start
                      ^A Simple line
                    """));

            Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.SyntaxError));
        }

        [Test(Description = "No states")]
        public void TestValidateFsm02()
        {
            var exception = Assert.Throws<TemplateParseException>(() =>
                new Template("""
                    Value unused (.)
                    """));

            Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.NoStateDefinitions));
        }

        [Test(Description = "No 'Start' state")]
        public void TestValidateFsm03()
        {
            var exception = Assert.Throws<TemplateParseException>(() =>
                new Template("""
                    Value unused (.)

                    NotStart
                    """));

            Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.NoStartState));
        }

        [Test(Description = "Has 'Start' state with valid destination")]
        public void TestValidateFsm04()
        {
            var template = new Template("""
                Value unused (.)

                Start
                 ^.* -> Start
                """);
        }

        [Test(Description = "Invalid destination")]
        public void TestValidateFsm05()
        {
            var exception = Assert.Throws<TemplateParseException>(() =>
                new Template("""
                    Value unused (.)

                    Start
                     ^.* -> bogus
                    """));

            Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.UndefinedState));
        }

        [Test(Description = "Now valid again")]
        public void TestValidateFsm06()
        {
            var template = new Template("""
                Value unused (.)

                Start
                 ^.* -> bogus
                bogus
                 ^.* -> Start
                """);
        }

        [Test(Description = "Valid destination with options")]
        public void TestValidateFsm07()
        {
            var template = new Template("""
                Value unused (.)

                Start
                 ^.* -> bogus
                bogus
                 ^.* -> Start
                 ^.* -> Next.Record Start
                """);
        }

        [Test(Description = "Error with and without messages string")]
        public void TestValidateFsm08()
        {
            var template = new Template("""
                Value unused (.)

                Start
                 ^.* -> bogus
                bogus
                 ^.* -> Start
                 ^.* -> Next.Record Start
                 ^.* -> Error
                 ^.* -> Error "Boo hoo"
                """);
        }

        [Test(Description = "Trivial template")]
        public void TestTextFSM01()
        {
            var templateText = """
                Value Beer (.*)

                Start
                 ^\w
                """;
            var template = new Template(templateText);

            Assert.That(template.ToString(), Is.EqualTo(templateText));
        }

        [Test(Description = "Slightly more complex, multiple values")]
        public void TestTextFSM02()
        {
            var templateText = """
                Value A (.*)
                Value B (.*)

                Start
                 ^\w

                State1
                 ^.
                """;
            var template = new Template(templateText);

            Assert.That(template.ToString(), Is.EqualTo(templateText));
        }

        [Test(Description = "Trivial FSM, no records produced")]
        public void TestParseText01()
        {
            var template = new Template("""
                Value unused (.)

                Start
                 ^Trivial SFM
                """);

            var data = """
                Non-matching text
                line1
                line 2

                """;

            var results = template.Parse(data);

            Assert.That(results.Count, Is.EqualTo(0));
        }

        [Test(Description = "Simple FSM, One Variable no options")]
        public void TestParseText02()
        {
            var template = new Template("""
                Value boo (.*)

                Start
                 ^$boo -> Next.Record

                EOF
                """);

            var data = """
                Matching text
                Trivial SFM
                line 2

                """;

            var results = template.Parse(data);

            Assert.That(results.Count, Is.EqualTo(3));
            Assert.That(results[0]["boo"], Is.EqualTo("Matching text"));
            Assert.That(results[1]["boo"], Is.EqualTo("Trivial SFM"));
            Assert.That(results[2]["boo"], Is.EqualTo("line 2"));

        }

        [Test(Description = "Matching one line, tests 'Next' & 'Record' actions")]
        public void TestParseText03()
        {
            var template = new Template("""
                Value boo (.*)

                Start
                 ^$boo -> Next.Record

                EOF
                """);

            var data = """
                Matching text
                """;

            var results = template.Parse(data);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0]["boo"], Is.EqualTo("Matching text"));
        }

        [Test(Description = "Matching two lines")]
        public void TestParseText04()
        {
            var template = new Template("""
                Value boo (.*)

                Start
                 ^$boo -> Next.Record

                EOF
                """);

            var data = """
                Matching text
                And again
                """;

            var results = template.Parse(data);

            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results[0]["boo"], Is.EqualTo("Matching text"));
            Assert.That(results[1]["boo"], Is.EqualTo("And again"));
        }

        [Test(Description = "Two Variables and singular options. Matching two lines, only one records returned due to 'Required' flag")]
        public void TestParseText05()
        {
            var template = new Template("""
                Value Required boo (one)
                Value Filldown hoo (two)

                Start
                 ^$boo -> Next.Record
                 ^$hoo -> Next.Record

                EOF
                """);

            var data = """
                two
                one
                """;

            var results = template.Parse(data);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0]["boo"], Is.EqualTo("one"));
            Assert.That(results[0]["hoo"], Is.EqualTo("two"));
        }

        [Test(Description = "Two Variables and singular options. Matching two lines. Two records returned due to 'Filldown' flag")]
        public void TestParseText06()
        {
            var template = new Template("""
                Value Required boo (one)
                Value Filldown hoo (two)

                Start
                 ^$boo -> Next.Record
                 ^$hoo -> Next.Record

                EOF
                """);

            var data = """
                two
                one
                one
                """;

            var results = template.Parse(data);

            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results[0]["boo"], Is.EqualTo("one"));
            Assert.That(results[0]["hoo"], Is.EqualTo("two"));
            Assert.That(results[1]["boo"], Is.EqualTo("one"));
            Assert.That(results[1]["hoo"], Is.EqualTo("two"));
        }

        [Test(Description = "Multiple Variables and options")]
        public void TestParseText07()
        {
            var template = new Template("""
                Value Required,Filldown boo (one)
                Value Filldown,Required hoo (two)

                Start
                 ^$boo -> Next.Record
                 ^$hoo -> Next.Record

                EOF
                """);

            var data = """
                two
                one
                one
                """;

            var results = template.Parse(data);

            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results[0]["boo"], Is.EqualTo("one"));
            Assert.That(results[0]["hoo"], Is.EqualTo("two"));
            Assert.That(results[1]["boo"], Is.EqualTo("one"));
            Assert.That(results[1]["hoo"], Is.EqualTo("two"));
        }

        // testParseTextToDicts:520 - skipped since this was already tested above

        [Test(Description = "Simple FSM, One Variable no options")]
        public void TestParseNullText01()
        {
            var template = new Template("""
                Value boo (.*)

                Start
                 ^$boo -> Next.Record

                EOF
                """);

            var data = string.Empty;

            var results = template.Parse(data);

            Assert.That(results.Count, Is.EqualTo(0));
        }

        // testReset:598 - skipped since there is no reset

        [Test(Description = "Clear Filldown variable")]
        public void TestClear01()
        {
            var template = new Template("""
                Value Required boo (on.)
                Value Filldown,Required hoo (tw.)

                Start
                 ^$boo -> Next.Record
                 ^$hoo -> Next.Clear

                EOF
                """);

            var data = """
                one
                two
                onE
                twO
                """;

            var results = template.Parse(data);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0]["boo"], Is.EqualTo("onE"));
            Assert.That(results[0]["hoo"], Is.EqualTo("two"));
        }

        [Test(Description = "Clearall, with Filldown variable")]
        public void TestClear02()
        {
            var template = new Template("""
                Value Filldown boo (on.)
                Value Filldown hoo (tw.)

                Start
                 ^$boo -> Next.Clearall
                 ^$hoo
                """);

            var data = """
                one
                two
                """;

            var results = template.Parse(data);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0]["boo"], Is.EqualTo(""));
            Assert.That(results[0]["hoo"], Is.EqualTo("two"));
        }

        [Test]
        public void TestContinue01()
        {
            var template = new Template("""
                Value Required boo (on.)
                Value Filldown,Required hoo (on.)

                Start
                 ^$boo -> Continue
                 ^$hoo -> Continue.Record
                """);

            var data = """
                one
                on0
                """;

            var results = template.Parse(data);

            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results[0]["boo"], Is.EqualTo("one"));
            Assert.That(results[0]["hoo"], Is.EqualTo("one"));
            Assert.That(results[1]["boo"], Is.EqualTo("on0"));
            Assert.That(results[1]["hoo"], Is.EqualTo("on0"));
        }

        [Test]
        public void TestError01()
        {
            var template = new Template("""
                Value Required boo (on.)
                Value Filldown,Required hoo (on.)

                Start
                 ^$boo -> Continue
                 ^$hoo -> Error
                """);

            var data = """
                one
                """;

            var exception = Assert.Throws<TemplateErrorException>(() => template.Parse(data));
        }

        [Test]
        public void TestError02()
        {
            var template = new Template("""
                Value Required boo (on.)
                Value Filldown,Required hoo (on.)

                Start
                 ^$boo -> Continue
                 ^$hoo -> Error "Hello World"
                """);

            var data = """
                one
                """;

            var exception = Assert.Throws<TemplateErrorException>(() => template.Parse(data));
            Assert.That(exception.Text, Is.EqualTo("one"));
            Assert.That(exception.Error.Message, Is.EqualTo("Hello World"));
            Assert.That(exception.Line, Is.EqualTo(1));
        }

        [Test]
        public void KeyTest01()
        {
            var template = new Template("""
                Value Required boo (on.)
                Value Required,Key hoo (on.)

                Start
                 ^$boo -> Continue
                 ^$hoo -> Record
                """);

            Assert.That(template.Values["boo"].Options.HasFlag(Option.Key), Is.EqualTo(false));
            Assert.That(template.Values["hoo"].Options.HasFlag(Option.Key), Is.EqualTo(true));
        }

        [Test]
        public void TestList01()
        {
            var template = new Template("""
                Value List boo (on.)
                Value hoo (tw.)

                Start
                 ^$boo
                 ^$hoo -> Next.Record
                 
                EOF
                """);

            var data = """
                one
                two
                on0
                tw0
                """;

            var result = template.Parse(data);

            Assert.That(result.Count, Is.EqualTo(2));

            Assert.That(result[0]["boo"], Is.EqualTo(new[] { "one" }));
            Assert.That(result[0]["hoo"], Is.EqualTo("two"));

            Assert.That(result[1]["boo"], Is.EqualTo(new[] { "on0" }));
            Assert.That(result[1]["hoo"], Is.EqualTo("tw0"));
        }

        [Test]
        public void TestList02()
        {
            var template = new Template("""
                Value List,Filldown boo (on.)
                Value hoo (on.)

                Start
                 ^$boo -> Continue
                 ^$hoo -> Next.Record
                 
                EOF
                """);

            var data = """
                one
                on0
                on1
                """;

            var result = template.Parse(data);

            Assert.That(result.Count, Is.EqualTo(3));

            Assert.That(result[0]["boo"], Is.EqualTo(new[] { "one" }));
            Assert.That(result[0]["hoo"], Is.EqualTo("one"));

            Assert.That(result[1]["boo"], Is.EqualTo(new[] { "one", "on0" }));
            Assert.That(result[1]["hoo"], Is.EqualTo("on0"));

            Assert.That(result[2]["boo"], Is.EqualTo(new[] { "one", "on0", "on1" }));
            Assert.That(result[2]["hoo"], Is.EqualTo("on1"));
        }

        [Test]
        public void TestList03()
        {
            var template = new Template("""
                Value List,Required boo (on.)
                Value hoo (tw.)

                Start
                 ^$boo -> Continue
                 ^$hoo -> Next.Record
                 
                EOF
                """);

            var data = """
                one
                two
                tw2
                """;

            var result = template.Parse(data);

            Assert.That(result.Count, Is.EqualTo(1));

            Assert.That(result[0]["boo"], Is.EqualTo(new[] { "one" }));
            Assert.That(result[0]["hoo"], Is.EqualTo("two"));
        }

        // this test uses named capture groups, which are then implicitly merged into the result set as what appears to be a sub-list
        // supporting this feature would either break or significantly complicated a number of other features that rely on a concrete definition of the value lists.
        // For example the Value 'name' below could be either a list or a single string
        // Additionally having what are effectively dynamic 'Value' it becomes much more complicated to map values to POCOs
        // Additionally the named capture syntax for Python is (?P<namedCaptureGroup>expression) whereas the named capture group syntax for .NET is (?<namedCaptureGroup>expression)
        [Test(Description = "List-type values with nested regex capture groups are parsed correctly. Additionally, another value is used with the same group-name as one of the nested groups to ensure that there are no conflicts when the same name is used")]
        public void TestNestedMatching01()
        {
            // NOTE: The original test uses the Python specific (?P<namedCaptureGroup>expression) syntax for named capture groups. This test has been modified to use the .NET syntax of (?<namedCaptureGroup>expression)
            var template = new Template("""
                # A nested group is called "name"
                Value List foo ((?<name>\w+):\s+(?<age>\d+)\s+(?<state>\w{2})\s*)
                # A regular value is called "name"
                Value name (\w+)
                # "${name}" here refers to the Value called "name"

                Start
                 ^\s*${foo}
                 ^\s*${name}
                 ^\s*$$ -> Record
                """);

            var data = """
                 Bob: 32 NC
                 Alice: 27 NY
                 Jeff: 45 CA
                Julia


                """;

            var result = template.Parse(data);

            //FSM Table:
            //['foo', 'name']
            //[[{ 'name': 'Bob', 'age': '32', 'state': 'NC'}, { 'name': 'Alice', 'age': '27', 'state': 'NY'}, { 'name': 'Jeff', 'age': '45', 'state': 'CA'}], 'Julia']

            Assert.Inconclusive("Feature not supported");

            Assert.That(result.Count, Is.EqualTo(2));

            Assert.That(result[0]["name"], Is.EqualTo(new[] { "bob", " Alice", "Jeff" }));
            Assert.That(result[0]["age"], Is.EqualTo(new[] { "32", "27", "45" }));
            Assert.That(result[0]["age"], Is.EqualTo(new[] { "NC", "NY", "CA" }));
            Assert.That(result[1]["name"], Is.EqualTo("Julia"));
        }

        // testNestedNameConflict:781 - skipped since nested captures are not supported

        [Test(Description = "Explicit default")]
        public void TestGetValuesByAttrib01()
        {
            var template = new Template("""
                Value Required boo (on.)
                Value Required,List hoo (on.)

                Start
                 ^$boo -> Continue
                 ^$hoo -> Record
                """);

            var result = template
                .Values
                .Where(value => value.Options.HasFlag(Option.Required))
                .OrderBy(item => item.Name)
                .ToList();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Name, Is.EqualTo("boo"));
            Assert.That(result[1].Name, Is.EqualTo("hoo"));
        }

        [Test(Description = "Simple state change, no actions")]
        public void TestStateChange01()
        {
            var template = new Template("""
                Value boo (one)
                Value hoo (two)

                Start
                 ^$boo -> State1

                State1
                 ^$hoo -> Start
                 
                EOF
                """);

            var data = """
                one
                """;

            var result = template.Parse(data);

            Assert.That(template.States["Start"].Rules.Length, Is.EqualTo(1));
            Assert.That(template.States["Start"].Rules[0].Pattern, Is.EqualTo("^$boo"));

            Assert.That(template.States["State1"].Rules.Length, Is.EqualTo(1));
            Assert.That(template.States["State1"].Rules[0].Pattern, Is.EqualTo("^$hoo"));
            
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test(Description = "State change with actions")]
        public void TestStateChange02()
        {
            var template = new Template("""
                Value boo (one)
                Value hoo (two)
                
                Start
                 ^$boo -> Next.Record State1
                
                State1
                 ^$hoo -> Start
                
                EOF
                """);

            var data = """
                one
                """;

            var result = template.Parse(data);

            Assert.That(template.States["Start"].Rules.Length, Is.EqualTo(1));
            Assert.That(template.States["Start"].Rules[0].Pattern, Is.EqualTo("^$boo"));

            Assert.That(template.States["State1"].Rules.Length, Is.EqualTo(1));
            Assert.That(template.States["State1"].Rules[0].Pattern, Is.EqualTo("^$hoo"));

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0]["boo"], Is.EqualTo("one"));
        }

        [Test(Description = "Implicit EOF")]
        public void TestEof01()
        {
            var template = new Template("""
                Value boo (.*)
                
                Start
                 ^$boo -> Next
                """);

            var data = """
                Matching text
                """;

            var result = template.Parse(data);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0]["boo"], Is.EqualTo("Matching text"));
        }

        [Test(Description = "EOF explicitly suppressed in template")]
        public void TestEof02()
        {
            var template = new Template("""
                Value boo (.*)

                Start
                 ^$boo -> Next

                EOF
                """);

            var data = """
                Matching text
                """;

            var result = template.Parse(data);

            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test(Description = "Implicit EOF suppressed by argument")]
        public void TestEof03()
        {
            var template = new Template("""
                Value boo (.*)

                Start
                  ^$boo -> Next
                """);

            var data = """
                Matching text
                """;

            var result = template.Parse(data);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0]["boo"], Is.EqualTo("Matching text"));
        }

        [Test(Description = "End State, EOF is skipped")]
        public void TestEnd01()
        {
            var template = new Template("""
                Value boo (.*)

                Start
                  ^$boo -> End
                  ^$boo -> Record
                """);

            var data = """
                Matching text A
                Matching text B
                """;

            var result = template.Parse(data);

            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test(Description = "End State, EOF is skipped")]
        public void TestEnd02()
        {
            var template = new Template("""
                Value boo (.*)

                Start
                  ^$boo -> End
                  ^$boo -> Record
                """);

            var data = """
                Matching text A
                Matching text B
                """;

            var result = template.Parse(data);

            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test(Description = "End State, with explicit Record")]
        public void TestEnd03()
        {
            var template = new Template("""
                Value boo (.*)

                Start
                  ^$boo -> Record End
                """);

            var data = """
                Matching text A
                Matching text B
                """;

            var result = template.Parse(data);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0]["boo"], Is.EqualTo("Matching text A"));
        }

        [Test(Description = "EOF state transition is followed by implicit 'End' state")]
        public void TestEnd04()
        {
            var template = new Template("""
                Value boo (.*)

                Start
                  ^$boo -> EOF
                  ^$boo -> Record
                """);

            var data = """
                Matching text A
                Matching text B
                """;

            var result = template.Parse(data);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0]["boo"], Is.EqualTo("Matching text A"));
        }

        [Test]
        public void TestInvalidRegexp01()
        {
            Assert.Inconclusive("Feature not supported");
            var exception = Assert.Throws<TemplateParseException>(() =>
                new Template("""
                Value boo (.$*)

                Start
                  ^$boo -> Next
                """));

            Assert.That(exception.ErrorCode, Is.EqualTo(ParseError.SyntaxError));
        }

        [Test]
        public void TestValidRegexp01()
        {
            var template = new Template("""
                Value boo (fo*)

                Start
                  ^$boo -> Record
                """);

            var data = """
                f
                fo
                foo

                """;

            var result = template.Parse(data);

            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result[0]["boo"], Is.EqualTo("f"));
            Assert.That(result[1]["boo"], Is.EqualTo("fo"));
            Assert.That(result[2]["boo"], Is.EqualTo("foo"));
        }

        [Test]
        public void TestReEnteringState01()
        {
            var template = new Template("""
                Value boo (.*)

                Start
                  ^$boo -> Next Stop

                Stop
                  ^abc

                """);

            var data = """
                one
                two
                """;

            var result = template.Parse(data);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0]["boo"], Is.EqualTo("one"));
        }

        [Test]
        public void TestFillup01()
        {
            var template = new Template("""
                Value Required Col1 ([^-]+)
                Value Fillup Col2 ([^-]+)
                Value Fillup Col3 ([^-]+)
                
                Start
                  ^$Col1 -- -- -> Record
                  ^$Col1 $Col2 -- -> Record
                  ^$Col1 -- $Col3 -> Record
                  ^$Col1 $Col2 $Col3 -> Record
                """);

            var data = """
                1 -- B1
                2 A2 --
                3 -- B3
                """;

            var result = template.Parse(data);

            Assert.That(result.Count, Is.EqualTo(3));

            Assert.That(result[0]["Col1"], Is.EqualTo("1"));
            Assert.That(result[0]["Col2"], Is.EqualTo("A2"));
            Assert.That(result[0]["Col3"], Is.EqualTo("B1"));

            Assert.That(result[1]["Col1"], Is.EqualTo("2"));
            Assert.That(result[1]["Col2"], Is.EqualTo("A2"));
            Assert.That(result[1]["Col3"], Is.EqualTo("B3"));

            Assert.That(result[2]["Col1"], Is.EqualTo("3"));
            Assert.That(result[2]["Col2"], Is.EqualTo(""));
            Assert.That(result[2]["Col3"], Is.EqualTo("B3"));
        }

        [Test(Description = "Check basic line is parsed")]
        public void TestUnicodeFsmValue01()
        {
            var template = new Template("""
                Value beer (\S+Δ)
                
                Start
                """);

            Assert.That(template.Values["beer"].Name, Is.EqualTo("beer"));
            Assert.That(template.Values["beer"].Options, Is.EqualTo(Option.None));
            Assert.That(template.Values["beer"].Regex.ToString(), Is.EqualTo(@"\S+Δ"));

        }

        [Test(Description = "Basic line, no action")]
        public void TestUnicodeFsmValue02()
        {
            var template = new Template("""
                Value beer (\S+Δ)

                Start
                 ^A beer called ${beer}Δ
                """);

            Assert.That(template.States["Start"].Rules.Length, Is.EqualTo(1));
            Assert.That(template.States["Start"].Rules[0].Pattern, Is.EqualTo("^A beer called ${beer}Δ"));
            Assert.That(template.States["Start"].Rules[0].Action, Is.EqualTo(null));
            Assert.That(template.States["Start"].Rules[0].LineAction, Is.EqualTo(LineAction.Next));
            Assert.That(template.States["Start"].Rules[0].RecordAction, Is.EqualTo(RecordAction.NoRecord));
        }

        [Test(Description = "Complex template, multiple vars and states with comments (no var options)")]
        public void TestTemplateValue01()
        {
            var template = new Template("""
                # Header
                # Header 2
                Value Beer (.*)
                Value Wine (\\w+)
                
                # An explanation with a unicode character Δ
                Start
                  ^hi there ${Wine}. -> Next.Record State1
                
                State1
                 ^\\wΔ
                 ^$Beer .. -> Start
                  # Some comments
                 ^$$ -> Next
                 ^$$ -> End
                
                End
                # Tail comment.
                """);

            var templateText = """
                Value Beer (.*)
                Value Wine (\\w+)

                Start
                 ^hi there ${Wine}. -> Record State1

                State1
                 ^\\wΔ
                 ^$Beer .. -> Start
                 ^$$
                 ^$$ -> End
                """;

            Assert.That(template.ToString(), Is.EqualTo(templateText));
        }

        // line 892
    }
}
