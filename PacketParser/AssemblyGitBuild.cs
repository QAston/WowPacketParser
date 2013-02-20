using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[AttributeUsage(AttributeTargets.Assembly)]
public class AssemblyGitBuild : Attribute
{
    public string gitBuild { get; private set; }
    public AssemblyGitBuild(string txt) { gitBuild = txt; }
}
