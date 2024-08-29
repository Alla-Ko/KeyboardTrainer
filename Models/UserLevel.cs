using System;
using System.Collections.Generic;

namespace KeybordTrainer.Models;

public partial class UserLevel
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? TaskId { get; set; }

    public int? Speed { get; set; }

    public int? Fails { get; set; }

    public virtual User? User { get; set; }
}
