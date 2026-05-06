
namespace Huragok.Exceptions.ManagedBlam {
    internal class InvalidBlamProjectException : Exception {
        internal InvalidBlamProjectException() { }
        internal InvalidBlamProjectException(string message) : base(message) { }
        internal InvalidBlamProjectException(string message, Exception innerException) : base(message, innerException) { }
    }

    internal class MismatchedBlamProjectException : Exception {
        internal MismatchedBlamProjectException() { }
        internal MismatchedBlamProjectException(string message) : base(message) { }
        internal MismatchedBlamProjectException(string message, Exception innerException) : base(message, innerException) { }
    }
}