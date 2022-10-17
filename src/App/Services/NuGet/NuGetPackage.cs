using App.Extensions;

namespace App.Services.NuGet;

public class NuGetPackage
{
    public NuGetPackage(string name, string version)
    {
        Name = name;
        Version = version;
    }

    public string Name { get; }
    public string Version { get; }
    
    public override bool Equals(object obj)
    {
        if (obj is not NuGetPackage item)
        {
            return false;
        }

        return Name.IgnoreCaseEquals(item.Name)
               && Version.IgnoreCaseEquals(item.Version);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Version);
    }
}

public class NotFoundNuGetPackage : NuGetPackage
{
    public NotFoundNuGetPackage(string name, string version, string reason) : base(name, version)
    {
        Reason = reason;
    }
    
    public string Reason { get; }
}