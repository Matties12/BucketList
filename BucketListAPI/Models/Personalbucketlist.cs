using System;
using System.Collections.Generic;

namespace BucketListAPI.Models;

public partial class Personalbucketlist
{
    public int FkUser { get; set; }

    public int FkBucketListItem { get; set; }

    public bool? Executed { get; set; }

    public virtual Bucketlistitem FkBucketListItemNavigation { get; set; } = null!;

    public virtual User FkUserNavigation { get; set; } = null!;
}
