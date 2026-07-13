using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AselDevBlazorArchitecture.Application.Features.Auth.DTOs
{
    public class LoginDto
    {
        public string UsernameOrEmployeeId { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
