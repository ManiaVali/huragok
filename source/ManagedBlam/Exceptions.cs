
namespace Huragok.Exceptions.ManagedBlam {
    internal class InvalidBlamProjectException : Exception {
        public InvalidBlamProjectException() { }
        public InvalidBlamProjectException(string message) : base(message) { }
        public InvalidBlamProjectException(string message, Exception innerException) : base(message, innerException) { }
    }

    internal class MismatchedBlamProjectException : Exception {
        public MismatchedBlamProjectException() { }
        public MismatchedBlamProjectException(string message) : base(message) { }
        public MismatchedBlamProjectException(string message, Exception innerException) : base(message, innerException) { }
    }
}