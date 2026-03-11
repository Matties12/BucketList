using System;
using System.Collections.Generic;

namespace BucketListAPI.Models;

public partial class Bucketlistitem
{
    public int IdBucketListItem { get; set; }

    public string NameBucketListItem { get; set; } = null!;

    public string DescriptionBucketListItem { get; set; } = null!;

    public virtual ICollection<Personalbucketlist> Personalbucketlists { get; set; } = new List<Personalbucketlist>();
}
