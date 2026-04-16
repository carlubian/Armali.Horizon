namespace Armali.Horizon.Megnir.Model;

public class MegnirBackup
{
    public MegnirJob[] Jobs { get; set; } = [];
}

public class MegnirJob
{
    public string Name { get; set; } = string.Empty;
    public string[] Files { get; set; } = [];
    public string[] Directories { get; set; } = [];
}
