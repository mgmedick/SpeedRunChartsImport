using System;
using System.Collections.Generic;
using System.Text;

namespace SpeedRunAppImport.Interfaces
{
    public interface IProcessor
    {
        void Run();
        void Init();
        void RunProcesses();
    }
} 
