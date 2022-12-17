using Client.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Exceptions
{
    public class ServerConnectionException : Exception
    {
        public InfoBarData? ExceptionInfoBarData { get; set; } = null;

        public string Message { get; set; } = string.Empty;

        public ServerConnectionException()
        {

        }

        public ServerConnectionException(InfoBarData infoBarData)
        {
            if (infoBarData == null)
                throw new ArgumentNullException(nameof(infoBarData));

            ExceptionInfoBarData = infoBarData;
        }

        public ServerConnectionException(string message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            Message = message;
        }

    }
}
