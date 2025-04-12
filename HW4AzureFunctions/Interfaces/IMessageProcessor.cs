using HW4AzureFunctions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW4AzureFunctions.Interfaces
{
    /// <summary>
    /// Interface for processing messages from the zip request queue.
    /// </summary>
    public interface IMessageProcessor
    {
        /// <summary>
        /// Process the message.
        /// </summary>
        /// <param name="message">The message to process.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Process(ZipQueueMessage message);
    }
}
