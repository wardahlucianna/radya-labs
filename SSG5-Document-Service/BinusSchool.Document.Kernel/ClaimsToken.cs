namespace BinusSchool.Document.Kernel;

public class ClaimsToken
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public IEnumerable<ClaimsRole> Roles { get; set; }
    public IEnumerable<ClaimsTenant> Tenants { get; set; }
}

public class ClaimsTenant
{
    public string Id { get; set; }
    public string Name { get; set; }
}

public class ClaimsRole
{
    public string Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public IEnumerable<ClaimsPermission> Permissions { get; set; }
}

public class ClaimsPermission
{
    public string Id { get; set; }
    public string Name { get; set; }
}