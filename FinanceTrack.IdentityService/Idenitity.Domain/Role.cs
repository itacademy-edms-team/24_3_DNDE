using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Idenitity.Domain
{
    public class Role : IdentityRole<Guid>
    {
        public Role()
            : base() { }

        public Role(string roleName)
            : base(roleName) { }
    }
}
