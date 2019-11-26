using System;

namespace CampusNetCheckerService
{
    public class CampusNetException : Exception
    {
        public string Title { get; }

        private CampusNetException(string title, string message) : base(message)
        {
            Title = title;
        }

        public static CampusNetException InvalidCredentials(string campusNetMessage) => new CampusNetException(
                "Invalid Credentials", $"The specified login credentials was invalid. Received message: \"{campusNetMessage}\"");
        public static CampusNetException UnknownError(string message) => new CampusNetException(
            "Unknown CampusNet Error", message);
        public static CampusNetException UnknownError(Exception ex) => UnknownError(
            $"Something went wrong, and we don't know what it was. We received this exception: \"{ex.Message}\"");
    }
}
