using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface ISystemCtrl
{
    WindowUIType UIType { get; }

    void OpenView(WindowUIType type);
}