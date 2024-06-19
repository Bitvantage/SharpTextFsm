# SharpTextFSM
SharpTextFSM is a .NET implementation of the [Google TextFSM Python module](https://github.com/google/textfsm). TextFSM templates match semi-formated line delimited text using an articulated regular expression state machine.
TextFSM templates are particularly well suited for parsing CLI output.

## Installing via NuGet Package Manager
```
PM> NuGet\Install-Package Bitvantage.SharpTextFSM
```

## TextFSM Resources
This documentation is focused on details that are specific to SharpTextFSM, for general information on TextFSM templates and regular expressions the following resources may be helpful:
* [Google TextFSM Wiki](https://github.com/google/textfsm/wiki/TextFSM)
* [Getting Started with TextFSM for Python](https://pyneng.readthedocs.io/en/latest/book/21_textfsm/README.html)
* [Awesome Regex Resources](https://github.com/Varunram/Awesome-Regex-Resources)
* [Network to Code Text FSM Templates](https://github.com/networktocode/ntc-templates) An extensive library of templates for many vendors' equipment that can be used directly by SharpTextFSM.

The templates are also available pre-packaged in the Bitvantage.ShartTextFSM.NtcTemplates project and include the template along with a pre-generated record object.


## Quick Start
A simple example of how to parse the output from the 'show ip arp' command from a Cisco IOS switch into a C# record.

### Combined C# Record Object and TextFSM Template 
```csharp
record ShowIpArp : ITemplate
{
    public string Protocol { get; set; }
    public IPAddress IpAddress { get; set; }
    [TemplateVariable(ThrowOnConversionFailure = false)]
    public long? Age { get; set; }
    public string MacAddress { get; set; }
    public string Type { get; set; }
    public string Interface { get; set; }

    string ITemplate.TextFsmTemplate =>
        """
        Value PROTOCOL (\S+)
        Value IP_ADDRESS (\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})
        Value AGE (-|\d+)
        Value MAC_ADDRESS ([a-f0-9]{4}\.[a-f0-9]{4}\.[a-f0-9]{4})
        Value TYPE (\S+)
        Value INTERFACE (\S+)
            
        Start
         ^Protocol\s+Address\s+Age\(min\)\s+Hardware Addr\s+Type\s+Interface -> Entry
         ^.* -> Error
            
        Entry
         ^${PROTOCOL}\s+${IP_ADDRESS}\s+${AGE}\s+${MAC_ADDRESS}\s+${TYPE}(\s+${INTERFACE})?$$ -> Record
         ^.* -> Error
        """;
}
```
### Example Usage
```csharp
class Example
{
    public void Test()
    {
        var data = """
            Protocol  Address              Age(min)       Hardware Addr     Type      Interface
            Internet  172.16.233.229       -              0000.0c59.f892    ARPA      Ethernet0/0
            Internet  172.16.233.218       -              0000.0c07.ac00    ARPA      Ethernet0/0
            Internet  172.16.233.19        -              0000.0c63.1300    ARPA      Ethernet0/0
            Internet  172.16.233.209       -              0000.0c36.6965    ARPA      Ethernet0/0
            Internet  172.16.168.11        -              0000.0c63.1300    ARPA      Ethernet0/0
            Internet  172.16.168.254       9              0000.0c36.6965    ARPA      Ethernet0/0
            Internet  10.0.0.0             -              aabb.cc03.8200    SRP-A
            """;

        var template = Template.FromType<ShowIpArp>();
        var results = template.Parse<ShowIpArp>(data).ToList();
    }
}
```
### Results
```text
{ShowIpArp { Protocol = Internet, IpAddress = 172.16.233.229,   Age = -, MacAddress = 0000.0c59.f892, Type = ARPA,  Interface = Ethernet0/0 }}
{ShowIpArp { Protocol = Internet, IpAddress = 172.16.233.218,   Age = -, MacAddress = 0000.0c07.ac00, Type = ARPA,  Interface = Ethernet0/0 }}
{ShowIpArp { Protocol = Internet, IpAddress = 172.16.233.19,    Age = -, MacAddress = 0000.0c63.1300, Type = ARPA,  Interface = Ethernet0/0 }}
{ShowIpArp { Protocol = Internet, IpAddress = 172.16.233.209,   Age = -, MacAddress = 0000.0c36.6965, Type = ARPA,  Interface = Ethernet0/0 }}
{ShowIpArp { Protocol = Internet, IpAddress = 172.16.168.11,    Age = -, MacAddress = 0000.0c63.1300, Type = ARPA,  Interface = Ethernet0/0 }}
{ShowIpArp { Protocol = Internet, IpAddress = 172.16.168.254,   Age = 9, MacAddress = 0000.0c36.6965, Type = ARPA,  Interface = Ethernet0/0 }}
{ShowIpArp { Protocol = Internet, IpAddress = 10.0.0.0,         Age = -, MacAddress = aabb.cc03.8200, Type = SRP-A, Interface =             }}
```
## 'Value' Bindings
Template TextFSM Values are automatically bound to similarly named public fields and properties within the type.

By default the case and the '_' are ignored. This behavior can be changed by decorating the class with the TemplateRecordAttribute.


| Flag Name     | Description                                                    |
| ----          | -----------                                                    |
| Disabled      | Do not automatically map. Mapping must be done explicitly      |
| Exact         | Exactly match the 'Value' name with the field or property name |
| IgnoreCase    | Perform a case-insensitive match                               |
| SnakeCase     | Ignore '_'                                                     |

For example to match using exact match logic and case-insensitive logic:
```csharp
[TemplateRecord(MappingStrategy.Exact | IgnoreCase)]
record Test
{
  ...
}
```

### Explicit 'Value' Binding
A field or property can be explicitly bound using the TemplateVariableAttribute. Explicitly bound fields and properties take precedence over automatically bound fields and properties.
```csharp
[TemplateVariable(Name = "MY_VALUE_NAME")]
public long MagicNumber { get; set; }
```

### Ignoring a Field or Property
A field or property can be ignored by setting the Ignore flag in the TemplateVariableAttribute.
```csharp
[TemplateVariable(Ignore = true)]
public long MagicNumber { get; set; }
```

## Type Conversion
If a type converter is not specified and the underlying type has a TryParse() or Parse() method, the built-in GenericTryParseConverter or GenericParseConverter type converter will automatically be configured.

### Setting an Explicit Type Converter
```csharp
[TemplateVariable(TypeConverter = typeof(MyTypeConverter)]
public long ValueField { get; set; }
```
### Type Conversion Failure
 If the type conversion fails, a TemplateTypeConversionException exception will be thrown. This behavior can be changed by setting the ThrowOnConversionFailure property to false.

When the ThrowOnConversionFailure property is set to false, values that fail to parse are set to the type's default value. This value can be changed by setting the DefaultValue property. The default value is specified as a string value and will be converted using the associated type converter.
```csharp
[TemplateVariable(ThrowOnConversionFailure=false)]
public long? Length { get; set; }

[TemplateVariable(ThrowOnConversionFailure=false)]
public long MagicNumber { get; set; }

[TemplateVariable(ThrowOnConversionFailure=false, DefaultValue="42")]
public long Answer { get; set; }
```
### Built-In Type Converters
| Name                      | Description                                        |
| ----                      | -----------                                         |
| AnyValueAsFalse           | Converts non-empty values to false                  |
| AnyValueAsTrue            | Converts non-empty values to true                   |
| EnumConverter             | Automatically used for enum types                   |
| GenericParseConverter     | Automatically used for types with a Parse method    |
| GenericTryParseConverter  | Automatically used for types with a TryParse method |
| StringConverter           | Automatically used for string types                 |

### Enum Conversion
By default, enums are automatically parsed using the built-in type converter EnumConverter. Member values are matched in a case-insensitive way, and aliases can be attached by using the EnumAliasAttribute.

```csharp
enum Animal
{
    None,
    Armadillo,
    Blobfish,
    Capybara,
    Fossa,
    [EnumAlias("Ghost Shark")]
    GhostShark,
    [EnumAlias("Goblin Shark")]
    GoblinShark,
    Hagfish,
    Hoatzin,
    Pangolin,
    Platypus,
    [EnumAlias("Sea Hog")]
    [EnumAlias("Sea Pig")]
    [EnumAlias("Sea Piggy")]
    [EnumAlias("Sea Swine")]
    SeaPig,
    Sloth,
    Tarsier,
    Uakari,
}
```
### Custom Type Converters
A custom type converter can be set in the Converter property.
```csharp
[TemplateVariable(Converter=typeof(MyConverter))]
public long MagicNumber { get; set; }
```
To create a custom type converter extend ValueConverter\<T\>.
```csharp
public class MyConverter : ValueConverter<long>
{
    public override bool TryConvert(string value, out long convertedValue)
    {
        if(!int.TryParse(value, out var parsedValue))
        {
            convertedValue = 0;
            return false;
        }

        convertedValue = 42 + parsedValue;
        return true;
    }
}
```

### List Conversion
When the TextFSM value definition has the 'List' option set, the underlying collection is an array, generic List\<T\>, or a ReadOnlyCollection\<T\>, and the ListCreator property is not set a list creator is automatically assigned. The list creator is responsible for creating an object of the target type from an array of typed values.

### Setting Explicit Type Converters
```csharp
[TemplateVariable(TypeConverter = typeof(ListCreator)]
public string Value { get; set; }
```
### Custom List Creator
To create a custom type converter extend from ListCreator\<TList, TItem\>.
```csharp
class CommaSeparatedList : ListCreator<string,string>
{
    public override string Create(string[] values)
    {
        return string.Join(",", values);
    }
}
```
### Built-in List Creators
| Name                      | Description                                       |
| ----                      | -----------                                       |
| ArrayConverter            | Converts TestFSM lists to an array                |
| GenericListCreator        | Converts TestFSM lists to a generic list          |
| ReadOnlyCollectionCreator | Converts TestFSM lists to a read-only collection  |


## Raw Rows
The TextFSM row that is used to generate the record can be included in the record. This can be useful for reference purposes or used by the internal logic of the class.

```csharp
[RawRow]
public Row ExampleRawRow { get; set; }
```

## Validation and Post Processing
When the ITemplateValidator interface is implemented, a custom method is called after the record is created. The record can be modified, and post-processing tasks can be performed. The raw row that was used to generate the instance of the record is provided for post-processing purposes.

If the function returns false, the record will not be added to the result set.

```csharp
internal class Test : ITemplate, ITemplateValidator
{
    public long MyProperty { get; set; }

    ...

    bool ITemplateValidator.Validate(Row row)
    {
        if(MyProperty == 0)
            return false;

        if(MyProperty == 5)
            MyProperty = int.Parse((string)row["MyUnboundValue"]);

        if(MyProperty > 10)
            MyProperty = 10;
           
        return true;
    }
}
```

## Regex Value Defenition
Regular expression values function similarly to regular values but do not implicitly capture. They can be used in both rules and other values, enhancing readability and reducing the need for repeating the same expressions.

```text
Value Regex CUTE_ANIMALS (dog|cat|panda|rabit)
Value Regex UGLY_ANIMALS (pug)
Value Regex ALL_ANIMALS (${CUTE_ANIMALS}|${UGLY_ANIMALS})
Value EVERYTHING (${ALL_ANIMALS})

Start
 ^${CUTE_ANIMALS}
 ^${UGLY_ANIMALS}
 ^${ALL_ANIMALS}
 ^${EVERYTHING}
```
\* This feature is not present in the reference implementation of TextFSM.

### Library Patterns
Common patterns are available in the built-in Regex library and may be referenced just like they were explicitly defined. Many of the patterns were either borrowed from or inspired by [Grok](https://github.com/logstash-plugins/logstash-patterns-core/blob/main/patterns/ecs-v1/grok-patterns).
```
Value MAC_ADDRESS (${_MAC_ADDRESS})

Start
 ^MAC_ADDRESS -> Record
```

| Name | Description |
| ---- | ----------- |
|_BASE_10_NUMBER|Matches decimal numbers|
|_BASE_16_FLOAT|Matches hexadecimal floating-point numbers|
|_BASE_16_NUMBER|Matches hexadecimal numbers|
|_DATA|Lazy match zero or more characters|
|_DATE_EU|Matches dates in the day-month-year, day/month/year or day.month.year format|
|_DATE_US|Matches dates in the month-day-year or month/day/year format|
|_DATE|Matches dates that are in the US or EU format|
|_DAY|Matches weekdays that are in the abbreviated or full-name format|
|_EMAIL_ADDRESS|Matches email addresses|
|_EMAIL_LOCAL_PART|Matches the characters before the at sign (@) in an email address. For example, in the email address 123456@alibaba.com, the matched content is 123456|
|_GREEDY_DATA|Greedy match zero or more characters|
|_HOST_AND_PORT|Matches IP addresses, hostnames, or positive integers|
|_HOSTNAME|Matches hostnames
|_HOUR|Matches hours|
|_INTEGER|Matches integers|
|_IP_OR_HOST|Matches IP addresses or hostnames|
|_IP|Matches IPv6 or IPv4 addresses|
|_IPV4|Matches IPv4 addresses|
|_IPV6|Matches IPv6 addresses|
|_MAC_ADDRESS|Matches any MAC addresses format |
|_MAC_ADDRESS_QUAD_DOT|Matches a MAC address in the 0102.03ab.cdef format |
|_MAC_ADDRESS_QUAD_COLON|Matches a MAC address in the 0102:03ab:cdef format |
|_MAC_ADDRESS_DOUBLE_DOT|Matches a MAC address in the 01.02.03.ab.cd.ef format |
|_MAC_ADDRESS_DOUBLE_DASH|Matches a MAC address in the 01-02-03-ab-cd-ef format |
|_MAC_ADDRESS_DOUBLE_COLON|Matches a MAC address in the 01:02:03:ab:cd:ef format |
|_MINUTE|Matches minutes|
|_MONTH_DAY|Matches days in a month|
|_MONTH_NUMBER|Matches months that are in the numeric format|
|_MONTH|Matches months that are in the numeric, abbreviated, or full-name format|
|_NON_NEGATIVE_INTEGER|Matches non-negative integers|
|_NOT_SPACE|Matches characters that are not spaces|
|_NUMBER|Matches numbers|
|_POSITIVE_INTEGER|Matches positive integers|
|_QUOTED_STRING|Matches quoted content. For example, in the I am "Iron Man" string, the matched content is Iron Man.
|_SECOND|Matches seconds|
|_SPACE|Matches spaces|
|_TIME|Matches time|
|_URN|Matches uniform resource names (URN)
|_UUID|Matches universally unique identifiers (UUIDs)
|_WORD|Matches letters, digits, and underscores (_)|
|_YEAR|Matches years|

## Loop Free Continue Line Action with State Change
The reference implementation of TextFSM does not allow combining the 'Continue' line action with a state transition to ensure a loop-free state machine. While this restriction is both reasonable and well intended, it complicates numerous common use cases and discourages using states to validate data.

SharpTextFSM relaxes this restriction by allowing 'Continue' line actions with a state transition provided that doing so cannot produce a loop.

A state machine has a loop if it CAN indefinitely process the same line without advancing to the next line. In practice, this means there cannot be a state with a rule that includes a continue line action and a state transition that jumps to another state that can return to the previous state, either directly or indirectly, through rules that specify the continue line action.

The following is an example of a valid SharpTextFSM template that uses both the Continue line action and a state change:
```
State1
 ^.* -> Continue State2
State2
 ^.* -> Continue State3
 ^.* -> State1
State3
 ^X.* -> Record
 ^Y.* -> State1
```

\* This feature is not present in the reference implementation of TextFSM. 

## ~Global State
Rules in the ~Global state are evaluated each time a new line is read and before the rules associated with the current state.

Placing rules in the ~Global state can be useful for handling content that appears in numerous states, such as comments, and for transitioning to states when the previous state ends without a clear trigger.

Since rules cannot transition to the ~Global state, rules in the ~Global state can not create a loop, therefore, using the 'Continue' line action with a state transition is always allowed.

```csharp
Value Test (.*)

~Global
 ^\s*#
 ^X -> Continue XState

Start
 ^.*

XState
 ^X${Test}
```

\* This feature is not present in the reference implementation of TextFSM.
### ~Global State Filters
State filters can be attached to rules in the ~Global state to limit the states that the rule applies to. The list of states can be negated by prefixing the state list with a '^'.

```csharp
~Global
 [State1,State2,State3]
 ^X -> Continue State4

 [^State1,State2]
 ^Y -> State1
```

## Metadata Value Option
Using the 'Metadata' value option, state machine data can be included in the result set.

Normally, the last value before a row is recorded will appear in the result set. However, if the List option is specified, an entry is added to the list each time a rule is matched.
```csharp
Value Metadata LAST_LINE_NUMBER (Line)
Value Metadata,List INPUT_TEXT (Text)
```
\*  This feature is not present in the reference implementation of TextFSM.
### Metadata Patterns
The metadata pattern is a value from the below list and not a regular expression.

| Name           | Description                        |
| ----           | -----------                        |
| Line           | The current line number            |
| Text           | The current text                   |
| State          | The current state                  |
| PreviousState  | The previous state                 |
| RuleIndex      | The index of the rule that matched |


## Parsing to a Non-Generic Result Set
To produce an untyped result set similar to the reference implementation of TextFSM.

```csharp
var results = template.Parse(string data)
```

The results will be a list of rows that contain either a string value or an array of string values.

## Parsing to a Dynamic Result Set
To produce an untyped dynamic result set.

```csharp
var results = template.Parse(string data).ToDynamic()
```
# Troubleshooting
## Explain() Function
The explain function explains in detail each step of the match process and can help to understand why a template is not working as expected.

```csharp
var template = Template.FromType<MyTemplate>().

var explain = template.Explain(data);
```

# Known Limitations
In general templates for the reference implementation of TextFSM should work with SharpTextFSM, and templates for SharpTextFSM should work with the reference implementation of TestFSM provided they do not use any extended features.

Known limitations include:
* The .NET regular expression engine is subtly different from the TextFSM regular expression engine, which can create incompatibility between the two implementations.
* Inline capture groups in 'Value' statements are not supported, for example, 'Value INLINE_CAPTURE_GROUP ((?P\<FirstGroup\>expression) (?P\<SecondGroup\>expression))'. In the reference implementation, a match would return a separate group inside of each existing 'Value' capture, in .NET regular expressions the syntax for a named capture group is different, and inline capture groups do not map cleanly to type mapping.