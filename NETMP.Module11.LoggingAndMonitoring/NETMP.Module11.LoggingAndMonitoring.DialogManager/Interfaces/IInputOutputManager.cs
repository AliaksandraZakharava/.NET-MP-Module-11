using System;

namespace NETMP.Module11.LoggingAndMonitoring.DialogManager.Interfaces
{
    public interface IInputOutputManager
    {
        string ReadMessage();

        void DisplayMessage(string message);

        void DisplayException(Exception exception);

        void WaitForInput();
    }
}
