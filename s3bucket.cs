  public async Task < List < S3Services >> GetVersionsFromS3ForService(ClientService service, ListObjectsV2Request request) {
  var env = string.Empty;
  if (!string.IsNullOrWhiteSpace(m_AppConfiguration.EnvironmentBranch)) {
    env = $ "/{m_AppConfiguration.EnvironmentBranch}";
  }
  var request = new ListObjectsV2Request {
    BucketName = service.S3BucketName,
      Prefix = $ "{service.ProductName}{env}"
  };

  var items = await m_AmazonS3.ListObjectsV2Async(request);

  var versionStringComparer = new VersionStringComparer();
  var versions = (from a in items.S3Objects
    let version = GetVersionFromKey(a)
    where version != ""
    select version).OrderByDescending(t => t, versionStringComparer).Distinct().ToList();

  var s3Services = new List < S3Services > ();
  if (service.ProductName.ToLower().Contains("docFair") || service.ProductName.ToLower().Contains("mylo")) {
    var majorVersions = versions.Select(a => a.Substring(0, 5)).Distinct().ToList();

    foreach(var majorVersion in majorVersions) {
      var serviceVersions = versions.FindAll(a => a.StartsWith(majorVersion));

      s3Services.Add(new S3Services {
        ServiceName = service.ProductName,
          EhrVersion = majorVersion,
          ServiceVersions = service.ProductName.ToLower().Contains("rosetta") ? serviceVersions.Take(1).ToList() : serviceVersions
      });
    }
  } else {
    s3Services.Add(new S3Services {
      ServiceName = service.ProductName,
        EhrVersion = "",
        ServiceVersions = versions
    });
  }

  return s3Services;
}
