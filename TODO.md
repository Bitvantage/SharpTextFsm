* Build a web GUI
    * Debug and build templates
    * Generate records
* PowerShell cmdlets similar to how parser.py works
* Optimize the regular expression matching
    * There is a non-trivial amount of overhead whenever we ask for a Regex.Match().
    * It should be possible to combine all of the regular expressions in a given state into a single regular expression
    * The rule ID would need to be encoded into the group name so that the matching rule could be re-associated
    * Rules that have a '\-> Continue' without a state change would require additional merged regular expressions that start immediately after the '\->' continue rule
    * Global rules with matching state filters would be included
    * An encoded rule may look something like:
    \
    ```regex
    ((?>(?<textfsm_global_rule_1>^section))|(?>(?<textfsm_rule_1>^line))|(?>(?<textfsm_rule_2>^a|b))|(?>(?<textfsm_rule_3>^\d+)))
    ```
* Add external branching control to TextFSM templates
\
There are cases where it would be much cleaner to provide a single template that takes external input to influence the execution path
    * The template runner would take an array of strings
        * The array would be constructed such that the most relevant values are first
    * When the template is executing, it searches for states with names that match the parameters
    * The first matching state would override the actual state name
    * The state names would be identical to the ones they intend to overwrite; however with one of the following prefixes followed by the paramiter name
        * '%' to prepend rules to the state
        * '@' to replace the rules in the state
        * '+' to append rules to the state
        * For example MyState%VendorX
    * Presumably it would not make sense to allow prepend and append with replace
    * For loop prevention, all of the base state names would be merged together and evaluated just like they are today
    * Rename the Parse() function to Run()?
        * A TextFSM template is parsed, once parsed text is run through the template?
    * The process of converting a result set to a typed result set could be multi-threaded.