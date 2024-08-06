* Build a web GUI
    * Debug and build templates
    * Generate records
* PowerShell cmdlets similar to how parser.py works
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
    * The process of converting a result set to a typed result set could be multi-threaded.