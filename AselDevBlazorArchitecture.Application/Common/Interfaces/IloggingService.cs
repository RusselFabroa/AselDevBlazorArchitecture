using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AselDevBlazorArchitecture.Application.Common.Interfaces
{
    public interface IAppLogger<T>
    {
        void LogInformation(string message);
        void LogError(string message, Exception ex);
    }
}
