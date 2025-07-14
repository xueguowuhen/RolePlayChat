using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class CustomizedController : SystemCtrlBase<DialogueController>, ISystemCtrl
{
    public WindowUIType UIType => WindowUIType.CustomWindow;

    public void OpenView(WindowUIType type)
    {

    }
}