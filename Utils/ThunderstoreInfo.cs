using System.Collections.Generic;

namespace ModAge;

public class VersionInfo
{
    public string? name { get; set; }
    public string? full_name { get; set; }
    public string? description { get; set; }
    public string? icon { get; set; }
    public string? version_number { get; set; }
    public List<string>? dependencies { get; set; }
    public string? download_url { get; set; }
    public int downloads { get; set; }
    public string? date_created { get; set; }
    public string? website_url { get; set; }
    public bool is_active { get; set; }
    public string? uuid4 { get; set; }
    public int file_size { get; set; }
}

public class PackageInfo
{
    public string? name { get; set; }
    public string? full_name { get; set; } = string.Empty;
    public string? owner { get; set; }
    public string? package_url { get; set; }
    public string? donation_link { get; set; }
    public string? date_created { get; set; }
    public string? date_updated { get; set; }
    public string? uuid4 { get; set; }
    public int rating_score { get; set; }
    public bool is_pinned { get; set; }
    public bool is_deprecated { get; set; }
    public bool has_nsfw_content { get; set; }
    public List<string>? categories { get; set; }
    public List<VersionInfo>? versions { get; set; }
}

public class PreparedPackageInfo
{
    public string? name { get; set; }
    public string? clean_name { get; set; }
    public string? icon_url { get; set; }
    public string version { get; set; }
    public string? updated { get; set; }
    public bool? deprecated { get; set; }
    public string[]? urls { get; set; }
}